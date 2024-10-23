using DevOpsInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TfsDevOpsServer
{
    class TfvcPendingChangeOpWrapper
    {
        public TfvcSourceCodePendingChange PendingChange { get; set; }
        public OperationStatus Status 
        {
            get
            {
                if (PendingChange == null)
                    return OperationStatus.Pending;

                return PendingChange.Status;
            }
            set
            {
                PendingChange.Status = value;
            } 
        }

        public List<TfvcSourceCodePendingChange> Dependencies { get; set; } = new List<TfvcSourceCodePendingChange>();

        public TfvcPendingChangeOpWrapper(TfvcSourceCodePendingChange pendingChange)
        {
            PendingChange = pendingChange;
        }
    }
}
