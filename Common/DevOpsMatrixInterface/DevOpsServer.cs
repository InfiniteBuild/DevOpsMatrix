
namespace DevOpsInterface
{
    public abstract class DevOpsServer : IDevOpsServer
    {
        public abstract DevOpsServerType ServerType { get; }
        public IDevOpsSettings? Settings { get; set; } = null;

        public DevOpsServer()
        {

        }

        public DevOpsServer(IDevOpsSettings settings)
        {
            Settings = settings;
        }

        public abstract T GetDevOpsService<T>() where T : IDevOpsService;
        public abstract ServerCreator GetCreator();
    }
}
