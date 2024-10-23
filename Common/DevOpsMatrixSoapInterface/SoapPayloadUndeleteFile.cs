namespace DevOpsMatrix.Tfs.Soap.Interface
{
    public class SoapPayloadUndeleteFile : SoapPayloadBase
    {
        public static string CommandName = SoapCmdNames.UndeleteFile;

        public SoapPayloadUndeleteFile(string serverItem) : base(serverItem)
        {
            
        }
    }
}
