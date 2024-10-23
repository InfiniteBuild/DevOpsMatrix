using DevOpsInterface;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.WebApi;

namespace TfsDevOpsServer
{
    public class TfsDevOpsServer : DevOpsServer
    {
        public override DevOpsServerType ServerType { get { return DevOpsServerType.Tfs; } }

        public TfsDevOpsServer()
        {

        }

        public TfsDevOpsServer(IDevOpsSettings settings) : base(settings)
        {

        }

        public override T GetDevOpsService<T>()
        {
            IDevOpsService? retval = null;

            if (typeof(T) == typeof(IChangeManagement))
                retval = new TfsChangeManagement(Settings);

            if (typeof(T) == typeof(IDevOpsBuildSystem))
                retval = new TfsBuildSystem(Settings);

            if (typeof(T) == typeof(ITfvcSourceControl))
                retval = new TfvcSourceControl(Settings);

            if (typeof(T) == typeof(IGitSourceControl))
                retval = new GitSourceControl(Settings);

            return (T)retval;
        }

        public override ServerCreator GetCreator()
        {
            return (settings) => { return new TfsDevOpsServer(settings); };
        }

        public List<string> GetProjects()
        {
            List<string> projectList = new List<string>();

            VssConnection connection = TfsServiceTools.CreateConnection(Settings);
            ProjectHttpClient projectClient = connection.GetClient<ProjectHttpClient>();

            foreach (TeamProjectReference project in projectClient.GetProjects().Result)
                projectList.Add(project.Name);

            return projectList;
        }
    }
}
