
namespace DevOpsSoapInterface
{
    public class SoapPayloadRenameFile : SoapPayloadBase
    {
        public static string CommandName = SoapCmdNames.RenameFile;

        public string NewServerPath { get; private set; }

        public SoapPayloadRenameFile(string serverpath, string newserverpath) : base(serverpath)
        {
            NewServerPath = newserverpath;
        }
    }
}
