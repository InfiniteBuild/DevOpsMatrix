using DevOpsMatrix.Tfs.Soap.Interface;
using System;
using Newtonsoft.Json;
using Microsoft.TeamFoundation.VersionControl.Client;
using System.IO;
using System.Threading;
using Lumberjack.Interface;

namespace DevOpsMatrix.Tfs.Soap.ApiExecutor
{
    internal class SoapApiCmdAddFile : SoapApiCmdBase
    {
        public SoapApiCmdAddFile()
        {
        }

        public override CmdResult Execute(SoapCommand command)
        {
            CmdResult result = new CmdResult();
            result.Result = "success";

            // Parse the payload
            SoapPayloadAddFile payloadObj = JsonConvert.DeserializeObject<SoapPayloadAddFile>(command.CmdHeader);

            if (!string.IsNullOrWhiteSpace(payloadObj.EncodingName))
                Logging48.LogUserMessage("Command: Add (" + payloadObj.EncodingName + ") " + payloadObj.ItemServerPath);
            else
                Logging48.LogUserMessage("Command: Add " + payloadObj.ItemServerPath);

            Workspace area = WorkContext.Instance.GetWorkspace(payloadObj.ItemServerPath);

            if (area == null)
            {
                result.Result = "failed";
                result.Message = "Workspace not found.";
                return result;
            }

            string localpath = area.GetLocalItemForServerItem(payloadObj.ItemServerPath);
            string parentDir = Path.GetDirectoryName(localpath);
            if (!Directory.Exists(parentDir))
            {
                Directory.CreateDirectory(parentDir);
            }

            FileStream fileStream = null;
            if (File.Exists(localpath))
                fileStream = new FileStream(localpath, FileMode.Truncate, FileAccess.Write);
            else
                fileStream = new FileStream(localpath, FileMode.CreateNew, FileAccess.Write);

            // Adding a file without data is a valid scenario
            if (command.Data != null)
                fileStream.Write(command.Data, 0, command.Data.Length);
            else
                Logging48.LogWarning("No data provided for file: " + payloadObj.ItemServerPath);

            fileStream.Close();

            bool retry = true;
            string errorMsg = string.Empty;
            int retryCount = 0;
            while ((retry) && (retryCount < 5))
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(payloadObj.EncodingName))
                        area.PendAdd(localpath);
                    else
                        area.PendAdd(new string[] { localpath }, false, payloadObj.EncodingName, LockLevel.None);

                    retry = false;
                    errorMsg = string.Empty;
                }
                catch (Exception ex)
                {
                    Logging48.LogWarning("Error: " + ex.Message);
                    Logging48.LogWarning("Retrying...");
                    errorMsg = ex.ToString();
                    retryCount++;
                    Thread.Sleep(1000);
                }
            }

            if (!string.IsNullOrWhiteSpace(errorMsg))
            {
                result.Result = "failed";
                result.Message = errorMsg;
            }

            return result;
        }
    }
}
