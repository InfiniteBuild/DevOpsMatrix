
namespace DevOpsSoapInterface
{
    public class SoapPayloadCheckin : SoapPayloadBase
    {
        public static string CommandName = SoapCmdNames.Checkin;

        public string Comment { get; set; } = string.Empty;

        public SoapPayloadCheckin(string comment)
        {
            Comment = comment;
        }
    }
}
