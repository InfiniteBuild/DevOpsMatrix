using DevOpsSoapInterface;
using System;
using Newtonsoft.Json;
using Microsoft.TeamFoundation.VersionControl.Client;
using System.IO;
using System.Threading;
using LoggingLibInterface;

namespace TfsSoapApiExecutor
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
                Feedback.LogUserMessage("Command: Add (" + payloadObj.EncodingName + ") " + payloadObj.ItemServerPath);
            else
                Feedback.LogUserMessage("Command: Add " + payloadObj.ItemServerPath);

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
            fileStream.Write(command.Data, 0, command.Data.Length);
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
                    Feedback.LogWarning("Error: " + ex.Message);
                    Feedback.LogWarning("Retrying...");
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
