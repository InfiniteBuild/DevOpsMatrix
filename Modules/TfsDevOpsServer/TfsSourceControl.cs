using DevOpsInterface;

namespace TfsDevOpsServer
{
    public abstract class TfsSourceControl : ISourceCodeControl
    {
        protected IDevOpsSettings m_settings;

        public string ServiceName { get { return "sourcecontrol"; } }

        public TfsSourceControl(IDevOpsSettings settings)
        {
            m_settings = settings;
        }

        public abstract List<string> GetBranchList();
        public abstract ISourceCodeItem? GetParentBranch(ISourceCodeItem svritem);

        public abstract ISourceCodeItem GetItem(string itemPath, bool includeContent = false);
        public abstract ISourceCodeItem GetItem(string itemPath, int version, bool includeContent = false);
        public abstract byte[]? GetItemContent(ISourceCodeItem item);
        public abstract List<int> CommitChanges(ISourceCodePendingChangeSet pendingchanges);
        public abstract ISourceCodePendingChangeSet CreatePendingChangeset();

        public abstract List<ISourceCodeHistory> GetHistory(ISourceCodeItem sourceItem);
        public abstract List<ISourceCodeHistory> GetHistory(ISourceCodeItem sourceItem, int count = -1, int fromId = -1, int toId = -1);
        public abstract List<ISourceCodeHistory> GetHistory(ISourceCodeItem sourceItem, string from = "", string until = "");
    }
}
