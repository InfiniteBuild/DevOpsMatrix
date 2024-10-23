using DevOpsInterface;

namespace TfsDevOpsServer
{
    public class TfvcSourceCodePendingOperation
    {
        public OperationStatus Status { get; set; } = OperationStatus.Pending;
        
        public SourceCodeChangeType ChangeType { get; set; }
        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();

        public TfvcSourceCodePendingOperation(SourceCodeChangeType changetype)
        {
        }
    }
}
