using DevOpsSoapInterface;
using System;
using Newtonsoft.Json;
using Microsoft.TeamFoundation.VersionControl.Client;
using System.IO;
using System.Threading;
using LoggingLibInterface;

namespace TfsSoapApiExecutor
{
    internal class SoapApiCmdUndeleteFile : SoapApiCmdBase
    {
        public SoapApiCmdUndeleteFile()
        {
        }

        public override CmdResult Execute(SoapCommand command)
        {
            CmdResult result = new CmdResult();
            result.Result = "success";

            // Parse the payload
            SoapPayloadUndeleteFile payloadObj = JsonConvert.DeserializeObject<SoapPayloadUndeleteFile>(command.CmdHeader);

            Feedback.LogUserMessage("Command: Undelete " + payloadObj.ItemServerPath);

            Workspace area = WorkContext.Instance.GetWorkspace(payloadObj.ItemServerPath);
            
            if (area == null)
            {
                result.Result = "failed";
                result.Message = "Workspace not found.";
                return result;
            }

            Item svrItem = area.VersionControlServer.GetItem(payloadObj.ItemServerPath, VersionSpec.Latest, DeletedState.Deleted, GetItemsOptions.None);

            string localPath = area.GetLocalItemForServerItem(svrItem.ServerItem);
            int deleteId = svrItem.DeletionId;

            bool retry = true;
            string errorMsg = string.Empty;
            int retryCount = 0;
            while ((retry) && (retryCount < 5))
            {
                try
                {
                    area.PendUndelete(localPath, deleteId);
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

            if (command.Data != null)
            {
                area.PendEdit(localPath);
                using (FileStream fileStream = new FileStream(localPath, FileMode.Truncate, FileAccess.Write))
                {
                    fileStream.Write(command.Data, 0, command.Data.Length);
                    fileStream.Close();
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
