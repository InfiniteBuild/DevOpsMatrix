
namespace DevOpsMatrix.Interface
{
    public interface ISourceCodePendingChangeSet
    {
        Dictionary<string, ISourceCodePendingChange> Changes { get; }
        string Comment { get; set; }

        void AddItem(string itemPath, string itemName, byte[] itemContent, string contentType, bool isBinary, int encoding);
        void DeleteItem(ISourceCodeItem item);
        void UndeleteItem(string serverItemPath);
        void UndeleteItem(string serverItemPath, byte[] itemContent);
        void UpdateItem(ISourceCodeItem item);
        void UpdateItem(ISourceCodeItem item, int codePage);
        void RenameItem(ISourceCodeItem item, string newName);
    }
}
