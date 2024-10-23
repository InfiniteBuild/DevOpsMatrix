using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using DevOpsSoapInterface;
using Newtonsoft.Json;
using System.Text;
using System.Collections.Generic;
using System.IO;
using LoggingLibInterface;

namespace TfsSoapApiExecutor
{
    internal class IPCServer
    {
        private TcpListener _tcpListener;
        private Thread _serverThread;
        private ManualResetEvent _stopEvent = new ManualResetEvent(false);
        List<Thread> requestThreads = new List<Thread>();

        public bool IsRunning
        {
            get { return _serverThread.IsAlive; }
        }

        public IPCServer()
        {
            _tcpListener = new TcpListener(IPAddress.Loopback, 65002);
        }

        public void Start()
        {
            Feedback.LogInfo("Server started.");
            _serverThread = new Thread(new ThreadStart(StartListening));
            _serverThread.Start();
        }

        private void StartListening()
        {
            _tcpListener.Start();
            while (_stopEvent.WaitOne(100) == false)
            {
                if (_tcpListener.Pending())
                {
                    //Console.WriteLine("Client connected.");
                    TcpClient client = _tcpListener.AcceptTcpClient();
                    NetworkStream stream = client.GetStream();

                    Thread procReq = new Thread(() => {
                        try { ProcessRequest(stream); }
                        catch (Exception exc) 
                        {
                            Feedback.LogError($"Error processing client request: {exc.ToString()}");
                        }
                        finally { stream.Close(); client.Close(); }
                    });
                    requestThreads.Add(procReq);
                    procReq.Start();
                }

                // Clean up any threads that have finished
                for (int i = requestThreads.Count - 1; i >= 0; i--)
                {
                    if (requestThreads[i].IsAlive == false)
                    {
                        requestThreads.RemoveAt(i);
                    }
                }
            }

            Feedback.LogInfo("Server stopped.");
            _tcpListener.Stop();
        }

        public SoapResult ExecuteCommand(SoapCommand command)
        {
            SoapResult result = null;

            if (command.Command == SoapCmdNames.Ping)
            {
                result = new SoapResult();
                result.Result = "success";
                result.SetData("Pong!");

                return result;
            }

            if (command.Command == SoapCmdNames.Quit)
            {
                Stop();
                return null;
            }

            SoapApiCmdBase cmd = SoapApiCommandFactory.Instance.CreateCommand(command.Command);
            if (cmd != null)
            {
                result = new SoapResult();

                CmdResult cmdresult = cmd.Execute(command);

                result.Result = cmdresult.Result;
                result.Message = cmdresult.Message;
                if (cmdresult.Data != null)
                    result.SetData(cmdresult.Data);

                return result;
            }

            result = new SoapResult();
            result.Result = "error";
            result.Message = "Unknown command";
            return result;
        }

        public void Stop()
        {
            _stopEvent.Set();
        }

        private SoapCommand ReadIncomingCommand(NetworkStream stream)
        {
            SoapCommand soapCommand = new SoapCommand();

            // Read the length of the incoming command name
            byte[] lengthBuffer = new byte[4]; // The length is sent as an int (4 bytes)
            int lengthBytesRead = stream.Read(lengthBuffer, 0, lengthBuffer.Length);
            int commandLength = BitConverter.ToInt32(lengthBuffer, 0);

            // Command Name
            byte[] cmdNameBuffer = new byte[commandLength];
            stream.Read(cmdNameBuffer, 0, commandLength);
            soapCommand.Command = Encoding.UTF8.GetString(cmdNameBuffer);

            // Command Header Data
            stream.Read(lengthBuffer, 0, lengthBuffer.Length); // size of the command header data
            commandLength = BitConverter.ToInt32(lengthBuffer, 0);
            if (commandLength > 0)
            {
                byte[] cmdHeaderBuffer = new byte[commandLength];
                stream.Read(cmdHeaderBuffer, 0, commandLength);
                soapCommand.CmdHeader = Encoding.UTF8.GetString(cmdHeaderBuffer);
            }

            // Data Stream
            stream.Read(lengthBuffer, 0, lengthBuffer.Length); // size of the data stream
            commandLength = BitConverter.ToInt32(lengthBuffer, 0);

            if (commandLength > 0)
            {
                // Use a MemoryStream to handle the incoming data stream
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    byte[] buffer = new byte[8192]; // 8KB buffer
                    int totalBytesRead = 0;
                    while (totalBytesRead < commandLength)
                    {
                        int bytesRead = stream.Read(buffer, 0, Math.Min(buffer.Length, commandLength - totalBytesRead));
                        if (bytesRead == 0)
                        {
                            // The connection was closed before all data was received
                            throw new InvalidOperationException("Connection closed before all data was received.");
                        }
                        memoryStream.Write(buffer, 0, bytesRead);
                        totalBytesRead += bytesRead;
                    }

                    memoryStream.Seek(0, SeekOrigin.Begin);
                    soapCommand.Data = memoryStream.ToArray();
                }
            }

            return soapCommand;
        }

        private void SendResponse(NetworkStream stream, SoapResult result)
        {
            string response = JsonConvert.SerializeObject(result);

            // Convert the command to bytes
            byte[] commandBytes = Encoding.UTF8.GetBytes(response);
            // Convert the length of the command to bytes
            byte[] lengthBytes = BitConverter.GetBytes(commandBytes.Length);

            // Send the length of the command first
            stream.Write(lengthBytes, 0, lengthBytes.Length);
            // Then send the command
            stream.Write(commandBytes, 0, commandBytes.Length);

            //Console.WriteLine($"Sent response: {response}");
        }

        private void ProcessRequest(NetworkStream stream)
        {
            try
            {
                SoapCommand soapCommand = ReadIncomingCommand(stream);

                // Execute the command and send response back to the client
                SoapResult result = null;
                try
                {
                    result = ExecuteCommand(soapCommand);
                }
                catch (Exception ex)
                {
                    Feedback.LogError($"Error executing command: {ex.Message}");
                    result = new SoapResult();
                    result.Result = "error";
                    result.Message = ex.Message;
                }

                SendResponse(stream, result);
            }
            catch (Exception ex)
            {
                Feedback.LogError($"Error processing client request: {ex.ToString()}");
                SoapResult result = new SoapResult();
                result.Result = "error";
                result.Message = ex.Message;
                SendResponse(stream, result);
            }
        }
    }

}

