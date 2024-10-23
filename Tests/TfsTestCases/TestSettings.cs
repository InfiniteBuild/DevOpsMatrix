using DevOpsMatrix.Interface;
using System.Reflection;
using System.Xml;

namespace TfsTestCases
{
    public class TestSettings
    {
        private static TestSettings m_instance = null;

        public static TestSettings Instance
        {
            get
            {
                if (m_instance == null)
                    m_instance = new TestSettings();
                return m_instance;
            }
        }

        public Dictionary<string, IDevOpsSettings> TestServers { get; } = new Dictionary<string, IDevOpsSettings>();

        protected TestSettings() 
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            string asmPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string dataFile = Path.Combine(asmPath, "TestSettings.xml");

            if (!File.Exists(dataFile))
                return;

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(dataFile);

            Load(xmlDoc.DocumentElement);
        }

        private void Load(XmlNode xmlnode)
        {
            XmlNode node = null;

            node = xmlnode.SelectSingleNode("DevOpsServers");
            XmlNodeList devopsServers = node.SelectNodes("Server");
            foreach(XmlNode serverNode in devopsServers)
            {
                DevOpsSettings serversettings = new DevOpsSettings();
                XmlNode temp = serverNode.SelectSingleNode("ServerType");
                if (temp != null)
                {
                    DevOpsServerType svrType;
                    if (Enum.TryParse(temp.InnerText, true, out svrType))
                        serversettings.ServerType = svrType;
                }

                temp = serverNode.SelectSingleNode("Name");
                if (temp != null)
                    serversettings.Name = temp.InnerText;

                temp = serverNode.SelectSingleNode("Project");
                if (temp != null)
                    serversettings.ProjectName = temp.InnerText;

                temp = serverNode.SelectSingleNode("Url");
                if (temp != null)
                    serversettings.ServerUri = new Uri(temp.InnerText);

                temp = serverNode.SelectSingleNode("Token");
                if (temp != null)
                    serversettings.AccessToken = temp.InnerText;

                TestServers[serversettings.Name] = serversettings;
            }
        }
    }
}
