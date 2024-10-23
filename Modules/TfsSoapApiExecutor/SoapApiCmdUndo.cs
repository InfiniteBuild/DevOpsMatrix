using DevOpsSoapInterface;
using LoggingLibInterface;
using Newtonsoft.Json;
using System;
using System.Threading;

namespace TfsSoapApiExecutor
{
    internal class SoapApiCmdUndo : SoapApiCmdBase
    {
        public SoapApiCmdUndo()
        {
        }

        public override CmdResult Execute(SoapCommand command)
        {
            CmdResult result = new CmdResult();
            result.Result = "success";

            Feedback.LogUserMessage("Command: UndoChanges");

            bool retry = true;
            string errorMsg = string.Empty;
            int retryCount = 0;
            while ((retry) && (retryCount < 5))
            {
                try
                {
                    WorkContext.Instance.UndoChanges();
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
