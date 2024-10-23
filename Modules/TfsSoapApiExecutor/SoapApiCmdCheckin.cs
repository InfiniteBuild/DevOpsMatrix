using DevOpsMatrix.Tfs.Soap.Interface;
using Lumberjack.Interface;
using Newtonsoft.Json;
using System;
using System.Threading;

namespace DevOpsMatrix.Tfs.Soap.ApiExecutor
{
    internal class SoapApiCmdCheckin : SoapApiCmdBase
    {
        public SoapApiCmdCheckin()
        {
        }

        public override CmdResult Execute(SoapCommand command)
        {
            CmdResult result = new CmdResult();
            result.Result = "success";

            // Parse the payload
            SoapPayloadCheckin payloadObj = JsonConvert.DeserializeObject<SoapPayloadCheckin>(command.CmdHeader);

            Logging48.LogUserMessage("Command: Checkin " + payloadObj.Comment);

            int changeset = -1;
            
            bool retry = true;
            string errorMsg = string.Empty;
            int retryCount = 0;
            while ((retry) && (retryCount < 5))
            {
                try
                {
                    changeset = WorkContext.Instance.CommitChanges(payloadObj.Comment);
                    result.Data = changeset;
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
