
namespace DevOpsMatrix.Interface
{
    public interface ISourceCodeHistory
    {
        int Id { get; }
        string Name { get; }
        string Comment { get; }
        DateTime Timestamp { get; }
        string Owner { get; }

        List<ISourceCodeHistoryItem> Changes { get; }
        List<IMergeItemInfo> MergeInfo { get; }
    }
}
