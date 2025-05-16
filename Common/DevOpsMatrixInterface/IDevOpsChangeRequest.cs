
namespace DevOpsMatrix.Interface
{
    public interface IDevOpsChangeRequest
    {
        int Id { get; }
        string AssignedTo { get; set; }
        string State { get; set; }
        string Title { get; set; }
        string Description { get; set; }
        string ItemType { get; }
        string Tags { get; set; }

        T GetField<T>(string name);
        void SetField<T>(string name, T value);
        IDictionary<string, object> GetAllFields();
    }
}
