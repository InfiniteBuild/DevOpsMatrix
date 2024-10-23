using DevOpsInterface;
using System.Text;
using TfsDevOpsServer;
using System.Reflection;

namespace TfsTestCases
{
    public class TfvcPendingChangeTest
    {
        private const string s_branchPath = "$/TestProject/Dev/Dev1";
        private IDevOpsSettings? m_settings;
        private ITfvcSourceControl? m_sourceControl;

        [SetUp]
        public void Setup()
        {
            IDevOpsServer server = DevOpsFactory.Instance.CreateServer(m_settings);
            m_sourceControl = server.GetDevOpsService<ITfvcSourceControl>();
        }

        [TearDown]
        public void TearDown()
        {
            m_sourceControl = null;
        }

        [OneTimeSetUp]
        public void SetupFixture()
        {
            if (TestSettings.Instance.TestServers.ContainsKey("Chief"))
                m_settings = TestSettings.Instance.TestServers["Chief"];
            else
                throw new InvalidDataException("Test Settings does not contain an entry for Chief");

            DevOpsFeatureToggle.Instance.EnableFeature(TfvcSourceControl.FeatureSoapCommit);
        }

        private void InitializeInitialStructure()
        {
            if (m_sourceControl == null)
                throw new InvalidOperationException("Source Control is not initialized");

            string baseDirPath = s_branchPath + "/PendChangeTests";
            ISourceCodeItem baseDirItem = m_sourceControl.GetItem(baseDirPath);
            Assert.That(baseDirItem != null);


        }

        [Test]
        public void GetItemHistoryWithRename()
        {
            
        }

    }
}