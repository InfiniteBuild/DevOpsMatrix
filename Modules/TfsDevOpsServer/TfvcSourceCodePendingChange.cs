using DevOpsInterface;

namespace TfsDevOpsServer
{
    public class TfvcSourceCodePendingChange : ISourceCodePendingChange
    {
        public OperationStatus Status { get; set; } = OperationStatus.Pending;
        public ISourceCodeItem OriginalItem { get; set; }
        public string NewItemPath { get; set; }
        public SourceCodeChangeType ChangeType { get; set; }
        public Dictionary<SourceCodeChangeType, TfvcSourceCodePendingOperation> PendingOperations { get; set; } = new Dictionary<SourceCodeChangeType, TfvcSourceCodePendingOperation>();
    }
}
