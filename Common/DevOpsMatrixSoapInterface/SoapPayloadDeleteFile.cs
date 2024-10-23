using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevOpsSoapInterface
{
    public class SoapPayloadDeleteFile : SoapPayloadBase
    {
        public static string CommandName = SoapCmdNames.DeleteFile;

        public SoapPayloadDeleteFile(string serverItem) : base(serverItem)
        {

        }
    }
}
