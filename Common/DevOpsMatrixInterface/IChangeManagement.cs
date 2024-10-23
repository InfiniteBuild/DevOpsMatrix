
namespace DevOpsMatrix.Interface
{
    public interface IChangeManagement : IDevOpsService
    {
        IChangeItemFactory ItemFactory { get; }
        IDevOpsChangeRequest GetChangeRequest(int id);
        void UpdateChangeRequest(IDevOpsChangeRequest request);
        IDevOpsChangeRequest CreateChangeRequest(IDevOpsChangeRequest request, string itemType);

        object? GetServerProperty(string itemId, Dictionary<string, object>? arguments);

        Dictionary<string, List<IDevOpsChangeRequest>> GetLinkedItems(IDevOpsChangeRequest item);
        void AddLinkedItem(string linktype, IDevOpsChangeRequest sourceItem, IDevOpsChangeRequest targetItem);
        void RemoveLinkedItem(string linktype, IDevOpsChangeRequest sourceItem, IDevOpsChangeRequest targetItem);
    }
}
