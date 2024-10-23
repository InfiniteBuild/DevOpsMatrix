
using System.Runtime.Serialization;

namespace DevOpsMatrix.Interface
{
    public delegate IDevOpsServer ServerCreator(IDevOpsSettings settings);

    public enum DevOpsServerType
    {
        None = 0,
        Tfs = 1,
        GitLab = 2,
        GitHub = 3,
    }

    [Flags]
    public enum SourceCodeChangeType
    {
        None = 0,
        Add = 1,
        Edit = 2,
        Rename = 4,
        SourceRename = 8,
        TargetRename = 16,
        Delete = 32,
        Undelete = 64,
        Branch = 128,
        Merge = 256,
        Undo = 512,
    }

    public enum OperationStatus
    {
        Pending = 0,
        InWork = 1,
        Success = 2,
        PartialSuccess = 4,
        Failure = 8,
    }
}
