
namespace DevOpsMatrix.Interface
{
    public interface IDevOpsServer
    {
        DevOpsServerType ServerType { get; }
        IDevOpsSettings Settings { get; }

        T GetDevOpsService<T>() where T : IDevOpsService;

        ServerCreator GetCreator();
    }
}
