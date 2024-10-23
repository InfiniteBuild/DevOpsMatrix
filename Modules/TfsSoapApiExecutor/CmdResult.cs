using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevOpsMatrix.Tfs.Soap.ApiExecutor
{
    public class CmdResult
    {
        public string Result { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }

        public CmdResult()
        {
        }

        public CmdResult(string result, string message, object data)
        {
            Result = result;
            Message = message;
            Data = data;
        }
    }
}
