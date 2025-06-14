using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevOpsMatrix.Interface
{
    public interface IDevOpsWorkspace
    {
        string LocalWorkspaceRoot { get; }
        string ServerPathRoot { get; }
        string LocalBranchRoot { get; }
        IDevOpsServer DevOpsServer { get; }

        WorkspaceCreator GetCreator();
        bool IsValidWorkspace();

        void Checkout(string itemPath);
        void UndoChanges(string itemPath);
        void AddItem(string itemPath);
        void DeleteItem(string itemPath);
        void RenameItem(string itemPath, string newName);
        
        List<ISourceCodePendingChange> GetPendingChanges(string itemPath);
        void CommitChanges(string itemPath, string comment, bool recursive);

        string GetServerPath(string localPath);
        string GetLocalPath(string serverPath);
    }
}
