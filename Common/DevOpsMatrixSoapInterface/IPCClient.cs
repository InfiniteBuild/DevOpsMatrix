using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DevOpsMatrix.Tfs.Soap.Interface
{
    internal class IPCClient
    {
        private string remoteHost = string.Empty;
        private int remotePort = 65002;

        public IPCClient()
        {
            
        }

        private TcpClient Connect()
        {
            TcpClient tcpClient = new TcpClient();
            tcpClient.Connect(IPAddress.Loopback, remotePort);
            return tcpClient;
        }

        public SoapResult SendCommand(SoapCommand command)
        {
            TcpClient _tcpClient = null;
            try { _tcpClient = Connect(); }
            catch 
            {
                SoapResult error = new SoapResult();
                error.Result = "Error";
                error.Message = "Failed to connect to the server.";
                return error;
            }

            NetworkStream stream = _tcpClient.GetStream();

            // Send the command name
            byte[] cmdBytes = Encoding.UTF8.GetBytes(command.Command);
            byte[] cmdBytesLength = BitConverter.GetBytes(cmdBytes.Length);
            stream.Write(cmdBytesLength, 0, cmdBytesLength.Length);
            stream.Write(cmdBytes, 0, cmdBytes.Length);

            // Send the command data
            if (!string.IsNullOrWhiteSpace(command.CmdHeader))
            {
                byte[] cmdHeaderBytes = Encoding.UTF8.GetBytes(command.CmdHeader);
                byte[] cmdHeaderBytesLength = BitConverter.GetBytes(cmdHeaderBytes.Length);
                stream.Write(cmdHeaderBytesLength, 0, cmdHeaderBytesLength.Length);
                stream.Write(cmdHeaderBytes, 0, cmdHeaderBytes.Length);
            }
            else
            {
                byte[] cmdHeaderBytesLength = BitConverter.GetBytes((int)0);
                stream.Write(cmdHeaderBytesLength, 0, cmdHeaderBytesLength.Length);
            }

            // Send the Data
            if (command.Data != null)
            {
                byte[] dataBytesLenght = BitConverter.GetBytes(command.Data.Length);
                stream.Write(dataBytesLenght, 0, dataBytesLenght.Length);
                stream.Write(command.Data, 0, command.Data.Length);
            }
            else
            {
                byte[] dataBytesLenght = BitConverter.GetBytes((int)0);
                stream.Write(dataBytesLenght, 0, dataBytesLenght.Length);
            }

            SoapResult result = null;
            try
            {
                // Process response data
                // Read the length of the incoming command first
                byte[] lengthBuffer = new byte[4]; // Assuming the length is sent as an int (4 bytes)
                int lengthBytesRead = stream.Read(lengthBuffer, 0, lengthBuffer.Length);
                int responseLength = BitConverter.ToInt32(lengthBuffer, 0);

                // Now read the command using the length
                byte[] commandBuffer = new byte[responseLength];
                int totalBytesRead = 0;
                while (totalBytesRead < responseLength)
                {
                    int bytesRead = stream.Read(commandBuffer, totalBytesRead, responseLength - totalBytesRead);
                    if (bytesRead == 0)
                    {
                        // The connection was closed before all data was received
                        throw new InvalidOperationException("Connection closed before all data was received.");
                    }
                    totalBytesRead += bytesRead;
                }

                string response = Encoding.UTF8.GetString(commandBuffer, 0, totalBytesRead);
                result = Newtonsoft.Json.JsonConvert.DeserializeObject<SoapResult>(response);
            }
            catch (System.IO.IOException)
            {
                // Handle the exception
            }

            _tcpClient.Close();
            return result;
        }

    }

}
