namespace DevOpsMatrix.Tfs.Soap.Interface
{
    public class SoapPayloadDeleteFile : SoapPayloadBase
    {
        public static string CommandName = SoapCmdNames.DeleteFile;

        public SoapPayloadDeleteFile(string serverItem) : base(serverItem)
        {

        }
    }
}
