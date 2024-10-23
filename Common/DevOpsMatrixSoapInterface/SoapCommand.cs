namespace DevOpsMatrix.Tfs.Soap.Interface
{
    public class SoapCommand
    {
        public string Command { get; set; }
        public string CmdHeader { get; set; }
        public byte[] Data { get; set; }

        public SoapCommand()
        {
            Command = string.Empty;
            Data = null;
        }

        public SoapCommand(string command)
        {
            Command = command;
            Data = null;
        }
    }
}
