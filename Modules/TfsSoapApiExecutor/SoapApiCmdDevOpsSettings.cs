using DevOpsSoapInterface;
using Newtonsoft.Json;
using System;

namespace TfsSoapApiExecutor
{
    internal class SoapApiCmdDevOpsSettings : SoapApiCmdBase
    {
        public SoapApiCmdDevOpsSettings()
        {
        }

        public override CmdResult Execute(SoapCommand command)
        {
            CmdResult result = new CmdResult();
            result.Result = "success";

            // Parse the payload
            SoapPayloadDevOpsSettings payloadObj = JsonConvert.DeserializeObject<SoapPayloadDevOpsSettings>(command.CmdHeader);

            // Create a new work context
            try
            {
                WorkContext.CreateContext(payloadObj.ServerUri, payloadObj.ProjectName, payloadObj.AccessToken);
            }
            catch (Exception ex)
            {
                result.Result = "failed";
                result.Message = ex.Message;
            }

            return result;
        }
    }
}
