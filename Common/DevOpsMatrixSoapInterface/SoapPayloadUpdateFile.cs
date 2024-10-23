
namespace DevOpsMatrix.Tfs.Soap.Interface
{
    public class SoapPayloadUpdateFile : SoapPayloadBase
    {
        public static string CommandName = SoapCmdNames.UpdateFile;

        public string EncodingName { get; set; }

        public SoapPayloadUpdateFile()
        {
        }

        public SoapPayloadUpdateFile(string serverPath, string encodingName) : base(serverPath)
        {
            EncodingName = encodingName;
        }
    }
}
