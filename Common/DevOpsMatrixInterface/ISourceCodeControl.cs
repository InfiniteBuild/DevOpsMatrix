
namespace DevOpsInterface
{
    public interface ISourceCodeControl : IDevOpsService
    {
        List<string> GetBranchList();
        ISourceCodeItem? GetParentBranch(ISourceCodeItem svritem);

        ISourceCodeItem GetItem(string itemPath, bool includeContent = false);
        ISourceCodeItem GetItem(string itemPath, int version, bool includeContent = false);
        byte[]? GetItemContent(ISourceCodeItem item);
        List<int> CommitChanges(ISourceCodePendingChangeSet pendingchanges);
        ISourceCodePendingChangeSet CreatePendingChangeset();

        List<ISourceCodeHistory> GetHistory(ISourceCodeItem sourceItem);
        List<ISourceCodeHistory> GetHistory(ISourceCodeItem sourceItem, int count = -1, int fromId = -1, int toId = -1);
        List<ISourceCodeHistory> GetHistory(ISourceCodeItem sourceItem, string from = "", string until = "");
    }
}
