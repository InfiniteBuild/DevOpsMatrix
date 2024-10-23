using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevOpsInterface
{
    public interface IMergeItemInfo
    {
        string SourceItem { get; set; }
        int FromVersion { get; set; }
        int ToVersion { get; set; }
    }
}
