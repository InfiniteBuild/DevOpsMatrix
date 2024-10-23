using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevOpsSoapInterface
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
