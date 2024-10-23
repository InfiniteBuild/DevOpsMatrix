
using DevOpsMatrix.Interface;

namespace DevOpsMatrix.Tfs.Server
{
    public class TfsVcMergeInfo : IMergeItemInfo
    {
        public string SourceItem { get; set; } = string.Empty;
        public int FromVersion { get; set; }
        public int ToVersion { get; set; }

        public TfsVcMergeInfo()
        {
        }

        public TfsVcMergeInfo(string itempath, int fromver, int tover) : this()
        {
            SourceItem = itempath;
            FromVersion = fromver;
            ToVersion = tover;
        }
    }
}
