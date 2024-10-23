
namespace DevOpsMatrix.Interface
{
    public interface IDevOpsSettings
    {
        public string Name { get; }
        public DevOpsServerType ServerType { get; }
        public Uri ServerUri { get; }
        public string ProjectName { get; }
        public string AccessToken { get; }
    }
}
