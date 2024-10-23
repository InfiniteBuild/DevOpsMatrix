using System;
using Newtonsoft.Json;
using Microsoft.TeamFoundation.VersionControl.Client;
using System.Threading;
using DevOpsMatrix.Tfs.Soap.Interface;
using Lumberjack.Interface;

namespace DevOpsMatrix.Tfs.Soap.ApiExecutor
{
    internal class SoapApiCmdRenameFile : SoapApiCmdBase
    {
        public SoapApiCmdRenameFile()
        {
        }

        public override CmdResult Execute(SoapCommand command)
        {
            CmdResult result = new CmdResult();
            result.Result = "success";

            // Parse the payload
            SoapPayloadRenameFile payloadObj = JsonConvert.DeserializeObject<SoapPayloadRenameFile>(command.CmdHeader);

            Logging48.LogUserMessage("Command: Rename " + payloadObj.ItemServerPath + " -> " + payloadObj.NewServerPath);

            Workspace area = WorkContext.Instance.GetWorkspace(payloadObj.ItemServerPath);

            if (area == null)
            {
                result.Result = "failed";
                result.Message = "Workspace not found.";
                return result;
            }

            string localpath = area.GetLocalItemForServerItem(payloadObj.ItemServerPath);
            string newLocalpath = area.GetLocalItemForServerItem(payloadObj.NewServerPath);

            if (!FileSystemItemExists(localpath))
            {
                if (FileSystemItemExists(newLocalpath))
                {
                    result.Result = "success";
                    result.Message = "Item already renamed and exists at new path";
                    return result;
                }
            }

            bool retry = true;
            string errorMsg = string.Empty;
            int retryCount = 0;
            while ((retry) && (retryCount < 5))
            {
                try
                {
                    area.PendRename(localpath, newLocalpath);
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
