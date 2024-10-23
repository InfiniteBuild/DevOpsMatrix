
namespace DevOpsSoapInterface
{
    public abstract class SoapPayloadBase
    {
        public string ItemServerPath { get; set; }

        public SoapPayloadBase()
        {
        }

        public SoapPayloadBase(string itemserverpath)
        {
            ItemServerPath = itemserverpath;
        }
    }
}
