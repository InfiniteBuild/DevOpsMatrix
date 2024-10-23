using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevOpsSoapInterface
{
    public class SoapResult
    {
        public string Result { get; set; }
        public string Message { get; set; }
        public string Data { get; set; }

        public SoapResult()
        {
        }

        public SoapResult(string result, string message, string data)
        {
            Result = result;
            Message = message;
            Data = data;
        }

        public T GetData<T>()
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(Data);
        }

        public void SetData<T>(T data)
        {
            Data = Newtonsoft.Json.JsonConvert.SerializeObject(data);
        }
    }
}
