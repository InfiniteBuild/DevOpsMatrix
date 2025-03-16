using DevOpsMatrix.Core;
using DevOpsMatrix.Interface;
using TestConfig;

namespace TfsTestCases
{
    public class BuildSystemTests
    {
        private IDevOpsSettings? m_settings;
        private IDevOpsBuildSystem? m_buildSystem;

        [SetUp]
        public void Setup()
        {
            IDevOpsServer server = DevOpsFactory.Instance.CreateServer(m_settings);
            m_buildSystem = server.GetDevOpsService<IDevOpsBuildSystem>();
        }

        [TearDown]
        public void TearDown()
        {
            m_buildSystem = null;
        }

        [OneTimeSetUp] 
        public void SetupFixture()
        {
            if (TestSettings.Instance.TestServers.ContainsKey("tfssvr-PNB"))
                m_settings = TestSettings.Instance.TestServers["tfssvr-PNB"];
            else
                throw new InvalidDataException("Test Settings does not contain an entry for tfssvr-PNB");
        }

        [Test]
        public void GetBuildDefinition()
        {
            IDevOpsPipeline pipe = m_buildSystem.GetPipeline("PNB_Dev1");
            if (pipe == null)
                Assert.Fail("Failed to retrieve Pipeline PNB_Dev1");

            Assert.Pass("Everything worked as expected");
        }

        [Test]
        public void GetLatestBuild()
        {
            IDevOpsPipeline pipe = m_buildSystem.GetPipeline("PNB_Dev1");
            if (pipe == null)
                Assert.Fail("Failed to retrieve Pipeline PNB_Dev1");

            IDevOpsPipelineBuild build = pipe.GetLatestBuild();
            Assert.That(build != null);

            Assert.Pass("Everything worked as expected");
        }

        [Test]
        public void GetBuildLogs()
        {
            IDevOpsPipeline pipe = m_buildSystem.GetPipeline("PNB_Dev1");
            if (pipe == null)
                Assert.Fail("Failed to retrieve Pipeline PNB_Dev1");

            IDevOpsPipelineBuild build = pipe.GetLatestBuild();
            Assert.That(build != null);

            Assert.That(build.BuildSteps.Count > 0);

            Assert.Pass("Everything worked as expected");
        }

        [Test]
        public void LaunchBuildWithParams()
        {
            IDevOpsPipeline pipe = m_buildSystem.GetPipeline("PNB_Dev1");
            if (pipe == null)
                Assert.Fail("Failed to retrieve Pipeline PNB_Dev1");

            Dictionary<string, string> buildvars = new Dictionary<string, string>();
            buildvars.Add("ChangeId", "12");

            IDevOpsPipelineBuild build = m_buildSystem.LaunchPipelineBuild(pipe, buildvars);

            Assert.That(build != null);

            Assert.That(build.BuildSteps.Count > 0);

            Assert.Pass("Everything worked as expected");
        }
    }
}