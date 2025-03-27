
using DevOpsMatrix.Interface;

namespace DevOpsMatrix.Tfs.Server
{
    public class TfsVcMergeInfo : IMergeItemInfo
    {
        public ISourceCodeHistoryItem Source { get; set; }
        public string SourceItem { get; set; } = string.Empty;
        public int FromVersion { get; set; }
        public int ToVersion { get; set; }

        public TfsVcMergeInfo()
        {
        }

        public TfsVcMergeInfo(ISourceCodeHistoryItem source, string itempath, int fromver, int tover) : this()
        {
            Source = source;
            SourceItem = itempath;
            FromVersion = fromver;
            ToVersion = tover;
        }
    }
}
