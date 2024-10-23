using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevOpsInterface
{
    public interface ISourceCodePendingChange
    {
        OperationStatus Status { get; set; }
        ISourceCodeItem OriginalItem { get; set; }
        string NewItemPath { get; set; }
        SourceCodeChangeType ChangeType { get; set; }
    }
}
