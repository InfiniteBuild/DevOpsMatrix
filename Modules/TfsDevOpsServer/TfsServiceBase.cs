using DevOpsMatrix.Interface;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace DevOpsMatrix.Tfs.Server
{
    public static class TfsServiceTools
    {
        public static VssConnection CreateConnection(IDevOpsSettings settings)
        {
            VssConnection connection;
            VssCredentials creds;

            if (!string.IsNullOrWhiteSpace(settings.AccessToken))
            {
                creds = new VssBasicCredential(string.Empty, settings.AccessToken);
            }
            else
            {
                creds = new WindowsCredential(true);
            }
            
            connection = new VssConnection(settings.ServerUri, creds);
            return connection;
        }
    }
}
