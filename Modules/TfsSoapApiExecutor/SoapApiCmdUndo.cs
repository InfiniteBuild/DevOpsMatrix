using DevOpsMatrix.Tfs.Soap.Interface;
using Lumberjack.Interface;
using System;
using System.Threading;

namespace DevOpsMatrix.Tfs.Soap.ApiExecutor
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

            Logging48.LogUserMessage("Command: UndoChanges");

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
