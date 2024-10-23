
namespace DevOpsMatrix.Interface
{
    public class DevOpsSettings : IDevOpsSettings
    {
        public string Name { get; set; } = string.Empty;

        public DevOpsServerType ServerType { get; set; } = DevOpsServerType.None;

        public Uri? ServerUri { get; set; }

        public string ProjectName { get; set; } = string.Empty;

        public string AccessToken { get; set; } = string.Empty;

        public DevOpsSettings()
        {

        }

        public DevOpsSettings(string name, DevOpsServerType svrType, Uri uri, string project) : this()
        {
            Name = name;
            ServerType = svrType;
            ServerUri = uri;
            ProjectName = project;
        }

        public DevOpsSettings(string name, string svrType, string uri, string project) : this()
        {
            Name = name;
            ServerType = (DevOpsServerType)Enum.Parse(typeof(DevOpsServerType), svrType);
            ServerUri = new Uri(uri);
            ProjectName = project;
        }

        public DevOpsSettings(string name, DevOpsServerType svrType, Uri uri, string project, string accesstoken) : this(name, svrType, uri, project)
        {
            AccessToken = accesstoken;
        }

        public DevOpsSettings(string name, string svrType, string uri, string project, string accesstoken) : this(name, svrType, uri, project)
        {
            AccessToken = accesstoken;
        }
    }
}
