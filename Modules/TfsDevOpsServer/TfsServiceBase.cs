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
            VssCredentials creds = new WindowsCredential(true);

            if ((!string.IsNullOrWhiteSpace(settings.Username)) && (!string.IsNullOrWhiteSpace(settings.Password)))
            {
                creds = new VssBasicCredential(settings.Username, settings.Password);
            }

            if (!string.IsNullOrWhiteSpace(settings.AccessToken))
            {
                if (string.IsNullOrWhiteSpace(settings.Username))
                {
                    creds = new VssBasicCredential(string.Empty, settings.AccessToken);
                }
                else
                {
                    creds = new VssBasicCredential(settings.Username, settings.AccessToken);
                }
            }

            connection = new VssConnection(settings.ServerUri, creds);
            return connection;
        }
    }
}
