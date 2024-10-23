using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;


namespace DevOpsMatrix.Tfs.Soap.Interface
{
    public class SoapExecutor : IDisposable
    {
        private Process m_ApiProcess = null;
        private IPCClient m_ipcClient = null;

        private string m_serverUri = string.Empty;
        private string m_projectName = string.Empty;
        private string m_accessToken = string.Empty;

        public string Status
        {
            get
            {
                if ((m_ApiProcess != null) && (!m_ApiProcess.HasExited))
                    return "running";
                else
                    return "stopped";
            }
        }

        public SoapExecutor()
        {
        }

        public SoapExecutor(string serverUri, string projectName, string accessToken) : this()
        {
            m_serverUri = serverUri;
            m_projectName = projectName;
            m_accessToken = accessToken;
        }

        public int CheckIn(string comment)
        {
            SoapCommand cmd = new SoapCommand(SoapPayloadCheckin.CommandName);
            cmd.CmdHeader = JsonConvert.SerializeObject(new SoapPayloadCheckin(comment));

            SoapResult result = m_ipcClient.SendCommand(cmd);

            if (result.Result != "success")
            {
                string message = "Failed to check in the changes.  ";
                if (result.Message != null)
                    message += result.Message;
                throw new InvalidOperationException(message);
            }

            int changeset = result.GetData<int>();
            return changeset;
        }

        public SoapResult Undo()
        {
            SoapCommand cmd = new SoapCommand(SoapCmdNames.Undo);
            SoapResult result = m_ipcClient.SendCommand(cmd);
            return result;
        }

        public SoapResult UpdateFile(string serverpath, byte[] content, string encodingName)
        {
            SoapCommand cmd = new SoapCommand(SoapPayloadUpdateFile.CommandName);
            cmd.CmdHeader = JsonConvert.SerializeObject(new SoapPayloadUpdateFile(serverpath, encodingName));
            cmd.Data = content;

            SoapResult result = m_ipcClient.SendCommand(cmd);
            return result;
        }

        public SoapResult AddFile(string serverpath, byte[] content, string encodingName)
        {
            SoapCommand cmd = new SoapCommand(SoapPayloadAddFile.CommandName);
            cmd.CmdHeader = JsonConvert.SerializeObject(new SoapPayloadUpdateFile(serverpath, encodingName));
            cmd.Data = content;

            SoapResult result = m_ipcClient.SendCommand(cmd);
            return result;
        }

        public SoapResult DeleteFile(string serverpath)
        {
            SoapCommand cmd = new SoapCommand(SoapPayloadDeleteFile.CommandName);
            cmd.CmdHeader = JsonConvert.SerializeObject(new SoapPayloadDeleteFile(serverpath));

            SoapResult result = m_ipcClient.SendCommand(cmd);
            return result;
        }

        public SoapResult UndeleteFile(string serverpath, byte[] content)
        {
            SoapCommand cmd = new SoapCommand(SoapPayloadUndeleteFile.CommandName);
            SoapPayloadUndeleteFile undelete = new SoapPayloadUndeleteFile(serverpath);

            cmd.CmdHeader = JsonConvert.SerializeObject(undelete);
            if (content != null)
                cmd.Data = content;

            SoapResult result = m_ipcClient.SendCommand(cmd);
            return result;
        }

        public SoapResult RenameFile(string originalPath, string itemPath)
        {
            SoapCommand cmd = new SoapCommand(SoapPayloadRenameFile.CommandName);
            cmd.CmdHeader = JsonConvert.SerializeObject(new SoapPayloadRenameFile(originalPath, itemPath));

            SoapResult result = m_ipcClient.SendCommand(cmd);
            return result;
        }

        public string Ping()
        {
            SoapCommand cmd = new SoapCommand(SoapCmdNames.Ping);
            SoapResult result = m_ipcClient.SendCommand(cmd);
            if (result.Result == "success")
                return result.GetData<string>();
            else
                throw new InvalidOperationException("Failed to ping the API executor process.");
        }

        public void Startup()
        {
            m_ApiProcess = StartSoapAPIExecutor();
            if ((m_ApiProcess == null) || (m_ApiProcess.HasExited))
                throw new InvalidOperationException("Failed to start the API executor process.");

            m_ipcClient = new IPCClient();

            SoapCommand cmd = new SoapCommand(SoapPayloadDevOpsSettings.CommandName);
            cmd.CmdHeader = JsonConvert.SerializeObject(new SoapPayloadDevOpsSettings(m_serverUri, m_projectName, m_accessToken));

            SoapResult result = m_ipcClient.SendCommand(cmd);
            if ((result == null) || (result.Result != "success"))
                throw new InvalidOperationException("Failed to set the DevOps settings.  ");
        }

        public void Shutdown()
        {
            if ((m_ApiProcess != null) && (!m_ApiProcess.HasExited))
            {
                SoapCommand cmd = new SoapCommand(SoapCmdNames.Quit);
                m_ipcClient.SendCommand(cmd);
            }
        }

        public void Dispose()
        {
            if ((m_ApiProcess != null) && (!m_ApiProcess.HasExited))
            {
                SoapCommand cmd = new SoapCommand(SoapCmdNames.Quit);
                m_ipcClient.SendCommand(cmd);
            }
        }

        private Process StartSoapAPIExecutor()
        {
            string asmPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string soapExecutorPath = string.Empty;
            if (File.Exists(Path.Combine(asmPath, "TfsSoapApiExecutor.exe")))
                soapExecutorPath = Path.Combine(asmPath, "TfsSoapApiExecutor.exe");
            else
                soapExecutorPath = Path.Combine(asmPath, "..", "TfsSoap", "TfsSoapApiExecutor.exe");

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = soapExecutorPath;
            startInfo.Arguments = "";
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;

            Process proc = Process.Start(startInfo);
            return proc;
        }
    }
}
