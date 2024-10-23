using DevOpsMatrix.Interface;
using System.Reflection;

namespace DevOpsMatrix.Core
{
    public class DevOpsFactory
    {
        private static DevOpsFactory? m_instance;

        private Dictionary<DevOpsServerType, ServerCreator> m_factory = new Dictionary<DevOpsServerType, ServerCreator>();

        public static DevOpsFactory Instance
        {
            get
            {
                if (m_instance == null)
                    m_instance = new DevOpsFactory();
                return m_instance;
            }
        }

        protected DevOpsFactory() 
        {
            DiscoverModules();
        }

        private void DiscoverModules()
        {
            string asmPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string modulePath = Path.Combine(asmPath, "modules");
            List<string> scanDirectories = new List<string>();

            // Note: To support tests, have to load modules from root directory
            if (!Directory.Exists(modulePath))
            {
                modulePath = asmPath;
                scanDirectories.Add(modulePath);
            }
            else
            {
                foreach(string subdir in Directory.GetDirectories(modulePath))
                    scanDirectories.Add(subdir);
            }

            foreach (string scanDir in scanDirectories)
            {
                foreach (string dll in Directory.GetFiles(scanDir, "*.dll"))
                {
                    try
                    {
                        Assembly modAsm = Assembly.LoadFrom(dll);

                        foreach (TypeInfo asmType in modAsm.GetTypes())
                        {
                            if (asmType.IsAbstract)
                                continue;

                            if (asmType.GetInterface(typeof(IDevOpsServer).FullName) != null)
                            {
                                IDevOpsServer handler = (IDevOpsServer)modAsm.CreateInstance(asmType.FullName);
                                RegisterServer(handler.ServerType, handler.GetCreator());
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        public IDevOpsServer? CreateServer(IDevOpsSettings settings) 
        {
            IDevOpsServer? server = null;

            if (m_factory.ContainsKey(settings.ServerType))
                server = m_factory[settings.ServerType](settings);

            return server;
        }

        public void RegisterServer(DevOpsServerType serverType, ServerCreator creator)
        {
            m_factory.Add(serverType, creator);
        }

        public void UnregisterServer(DevOpsServerType serverType)
        {
            if (m_factory.ContainsKey(serverType))
                m_factory.Remove(serverType);
        }
    }
}
