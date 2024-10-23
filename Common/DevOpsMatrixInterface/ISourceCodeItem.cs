
namespace DevOpsMatrix.Interface
{
    public enum SourceCodeItemType
    {
        File,
        Folder,
        Branch
    }

    public interface ISourceCodeItem
    {
        string ItemPath { get; }
        SourceCodeItemType Itemtype { get; }
        int Version { get; }
        byte[] Content { get; set; }
        string ContentType { get; set; }
        bool IsBinary { get; set; }
        int Encoding { get; set; }
        bool ChangeEncoding { get; set; }
    }
}
