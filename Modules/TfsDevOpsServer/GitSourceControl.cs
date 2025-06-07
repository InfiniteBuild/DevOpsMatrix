using DevOpsMatrix.Interface;

namespace DevOpsMatrix.Tfs.Server
{
    public class GitSourceControl : TfsSourceControl, IGitSourceControl
    {
        public GitSourceControl(IDevOpsSettings settings) : base(settings) 
        {
        }

        public override List<string> GetBranchList()
        {
            throw new NotImplementedException();
        }

        public override ISourceCodeItem? GetItemBranch(string svrPath)
        {
            throw new NotImplementedException();
        }

        public override ISourceCodeItem? GetParentBranch(ISourceCodeItem svritem)
        {
            throw new NotImplementedException();
        }

        public override ISourceCodeItem GetItem(string itemPath, bool includeContent = false)
        {
            return null;
        }

        public override ISourceCodeItem GetItem(string itemPath, int version, bool includeContent = false)
        { 
            return null;
        }

        public override byte[]? GetItemContent(ISourceCodeItem item)
        {
            return null;
        }

        public override List<int> CommitChanges(ISourceCodePendingChangeSet pendingchanges)
        {
            throw new NotImplementedException();
        }

        public override ISourceCodePendingChangeSet CreatePendingChangeset()
        {
            throw new NotImplementedException();
        }

        public override List<ISourceCodeHistory> GetHistory(ISourceCodeItem sourceItem)
        {
            return new List<ISourceCodeHistory>();
        }

        public override List<ISourceCodeHistory> GetHistory(ISourceCodeItem sourceItem, int count = -1, int fromId = -1, int toId = -1)
        {
            return new List<ISourceCodeHistory>();
        }

        public override List<ISourceCodeHistory> GetHistory(ISourceCodeItem sourceItem, string from = "", string until = "")
        {
            return new List<ISourceCodeHistory>();
        }
    }
}
