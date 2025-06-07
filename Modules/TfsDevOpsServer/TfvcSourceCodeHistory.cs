using DevOpsMatrix.Interface;
using Microsoft.TeamFoundation.Build.WebApi;

namespace DevOpsMatrix.Tfs.Server
{
    public class TfvcSourceCodeHistory : ISourceCodeHistory
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public string Comment { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; }

        public string Owner { get; set; } = string.Empty;

        public List<ISourceCodeHistoryItem> Changes { get; } = new List<ISourceCodeHistoryItem>();

        public List<IMergeItemInfo> MergeInfo { get; } = new List<IMergeItemInfo>();
    }
}
