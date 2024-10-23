using DevOpsMatrix.Interface;

namespace DevOpsMatrix.Tfs.Server
{
    public class TfvcSourceCodeHistoryItem : ISourceCodeHistoryItem
    {
        public string ItemType { get; set; } = string.Empty;
        public string ItemPath { get; set; } = string.Empty;
        public string OriginalPath { get; set; } = string.Empty;

        public SourceCodeChangeType ChangeType { get; set; } = SourceCodeChangeType.None;
    }
}
