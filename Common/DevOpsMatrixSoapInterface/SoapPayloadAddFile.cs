
namespace DevOpsMatrix.Tfs.Soap.Interface
{
    public class SoapPayloadAddFile : SoapPayloadBase
    {
        public static string CommandName = SoapCmdNames.AddFile;

        public string EncodingName { get; set; }

        public SoapPayloadAddFile()
        {
        }

        public SoapPayloadAddFile(string serverPath, string encodingName) : base(serverPath)
        {
            EncodingName = encodingName;
        }
    }
}
