using DevOpsSoapInterface;
using System;
using Newtonsoft.Json;
using Microsoft.TeamFoundation.VersionControl.Client;
using System.IO;
using System.Threading;
using LoggingLibInterface;

namespace TfsSoapApiExecutor
{
    internal class SoapApiCmdDeleteFile : SoapApiCmdBase
    {
        public SoapApiCmdDeleteFile()
        {
        }

        public override CmdResult Execute(SoapCommand command)
        {
            CmdResult result = new CmdResult();
            result.Result = "success";

            // Parse the payload
            SoapPayloadDeleteFile payloadObj = JsonConvert.DeserializeObject<SoapPayloadDeleteFile>(command.CmdHeader);

            Feedback.LogUserMessage("Command: Delete " + payloadObj.ItemServerPath);

            Workspace area = WorkContext.Instance.GetWorkspace(payloadObj.ItemServerPath);
            
            if (area == null)
            {
                result.Result = "failed";
                result.Message = "Workspace not found.";
                return result;
            }

            string localPath = area.GetLocalItemForServerItem(payloadObj.ItemServerPath);

            if (!FileSystemItemExists(localPath))
            {
                // the item or parent was already deleted
                return result;
            }

            RecursionType recurse = RecursionType.None;
            if (Directory.Exists(localPath))
            {
                recurse = RecursionType.Full;
            }

            bool retry = true;
            string errorMsg = string.Empty;
            int retryCount = 0;
            while ((retry) && (retryCount < 5))
            {
                try
                {
                    int ret = area.PendDelete(new string[] { localPath }, recurse, LockLevel.None, true, false);
                    retry = false;
                    errorMsg = string.Empty;

                    if (ret == 0)
                    {
                        errorMsg = "No items were deleted.";
                    }
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
