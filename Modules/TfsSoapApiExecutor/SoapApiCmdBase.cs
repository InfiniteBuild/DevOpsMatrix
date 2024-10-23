using DevOpsMatrix.Tfs.Soap.Interface;
using System.IO;

namespace DevOpsMatrix.Tfs.Soap.ApiExecutor
{
    public abstract class SoapApiCmdBase
    {
        public abstract CmdResult Execute(SoapCommand command);

        protected bool FileSystemItemExists(string itemPath)
        {
            if (Directory.Exists(itemPath))
                return true;

            if (File.Exists(itemPath))
                return true;

            return false;
        }
    }
}
