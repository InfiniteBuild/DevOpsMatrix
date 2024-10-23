using DevOpsSoapInterface;
using LoggingLibInterface;
using Newtonsoft.Json;
using System;
using System.Threading;

namespace TfsSoapApiExecutor
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

            Feedback.LogUserMessage("Command: Checkin " + payloadObj.Comment);

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
