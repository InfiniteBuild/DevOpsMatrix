using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevOpsSoapInterface
{
    public class SoapPayloadDevOpsSettings : SoapPayloadBase
    {
        public static string CommandName = SoapCmdNames.DevOpsSettings;

        public string ServerUri { get; }
        public string ProjectName { get; }
        public string AccessToken { get; }

        public SoapPayloadDevOpsSettings(string serverUri, string projectName, string accessToken)
        {
            ServerUri = serverUri;
            ProjectName = projectName;
            AccessToken = accessToken;
        }
    }
}
