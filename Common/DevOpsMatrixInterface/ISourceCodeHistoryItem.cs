
namespace DevOpsInterface
{
    public interface ISourceCodeHistoryItem
    {
        string ItemType { get; set; } 
        string ItemPath { get; }
        string OriginalPath { get; }
        SourceCodeChangeType ChangeType { get; }
    }
}
