using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevOpsSoapInterface
{
    public class SoapPayloadUndeleteFile : SoapPayloadBase
    {
        public static string CommandName = SoapCmdNames.UndeleteFile;

        public SoapPayloadUndeleteFile(string serverItem) : base(serverItem)
        {
            
        }
    }
}
