using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevOpsMatrix.Interface
{
    public interface IMergeItemInfo
    {
        ISourceCodeHistoryItem Source { get; set; }
        string SourceItem { get; set; }
        int FromVersion { get; set; }
        int ToVersion { get; set; }
    }
}
