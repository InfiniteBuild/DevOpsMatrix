using DevOpsMatrix.Core;
using DevOpsMatrix.Interface;
using DevOpsMatrix.Tfs.Server;

namespace TfsTestCases
{
    public class ChangeManagementTests
    {
        private IDevOpsSettings? m_settings;
        private IChangeManagement? m_changeMgmt;

        [SetUp]
        public void Setup()
        {
            IDevOpsServer server = DevOpsFactory.Instance.CreateServer(m_settings);
            m_changeMgmt = server.GetDevOpsService<IChangeManagement>();
        }

        [TearDown]
        public void TearDown()
        {
            m_changeMgmt = null;
        }

        [OneTimeSetUp] 
        public void SetupFixture()
        {
            //if (TestSettings.Instance.TestServers.ContainsKey("Chief"))
            //    m_settings = TestSettings.Instance.TestServers["Chief"];
            //else
            //    throw new InvalidDataException("Test Settings does not contain an entry for Chief");
        }

        [Test]
        public void GetWorkItem()
        {
            IDevOpsChangeRequest request = m_changeMgmt.GetChangeRequest(1);
            if (request == null) 
                Assert.Fail("Failed to get change request");

            if (request.Id == 0)
                Assert.Fail("Workitem had invalid Id");

            Assert.Pass("Everything worked as expected");
        }

        [Test]
        public void CreateWorkItem_Feature()
        {
            TfsWorkItem workitem = new TfsWorkItem();
            workitem.Title = "Test Feature Workitem";
            workitem.Description = "A test workitem to make sure everything works<br/>Second line of text";
            workitem.AssignedTo = "shipley";

            IDevOpsChangeRequest item = m_changeMgmt.CreateChangeRequest(workitem, "Feature");

            if (item == null)
                Assert.Fail("Failed to create workitem");

            if (item.Id == 0)
                Assert.Fail("Workitem has invalid Id");

            item.State = "Removed";
            m_changeMgmt.UpdateChangeRequest(item);

            ((TfsChangeManagement)m_changeMgmt).PermenantlyDeleteChangeRequest(item.Id);

            Assert.Pass("Everything worked as expected");
        }

        [Test]
        public void CreateWorkItem_SetFeatureTags()
        {
            TfsWorkItem workitem = new TfsWorkItem();
            workitem.Title = "Test Feature Workitem with Tags";
            workitem.Description = "A test workitem to make sure everything works<br/>Second line of text";
            workitem.AssignedTo = "shipley";
            workitem.Tags = "TestTag1;TestTag2";

            IDevOpsChangeRequest item = m_changeMgmt.CreateChangeRequest(workitem, "Feature");

            if (item == null)
                Assert.Fail("Failed to create workitem");

            if (item.Id == 0)
                Assert.Fail("Workitem has invalid Id");

            Assert.That(item.Tags.Equals("TestTag1; TestTag2"), $"Tags were not set right");

            ((TfsChangeManagement)m_changeMgmt).PermenantlyDeleteChangeRequest(item.Id);

            Assert.Pass("Everything worked as expected");
        }

        [Test]
        public void CreateWorkItem_WithChild()
        {
            TfsWorkItem epiccreate = new TfsWorkItem();
            epiccreate.Title = "Test Epic Workitem";
            epiccreate.Description = "A test workitem to make sure everything works";

            IDevOpsChangeRequest epic = m_changeMgmt.CreateChangeRequest(epiccreate, "Epic");

            TfsWorkItem featurecreate = new TfsWorkItem();
            featurecreate.Title = "Test Feature Workitem";
            featurecreate.Description = "a test workitem to be a child";

            IDevOpsChangeRequest feature = m_changeMgmt.CreateChangeRequest(featurecreate, "Feature");

            m_changeMgmt.AddLinkedItem("Child", epic, feature);

            Dictionary<string, List<IDevOpsChangeRequest>> links = m_changeMgmt.GetLinkedItems(epic);
            if (!links.ContainsKey(TfsRelationNames.Child))
                Assert.Fail("Links does not contain a child link type");

            bool ischildinlist = false;
            foreach(IDevOpsChangeRequest child in links[TfsRelationNames.Child])
            {
                if (child.Id == feature.Id)
                    ischildinlist = true;
            }

            if (!ischildinlist)
                Assert.Fail("Feature not in the list of linked children");

            m_changeMgmt.RemoveLinkedItem(TfsRelationNames.Child, epic, feature);
            links = m_changeMgmt.GetLinkedItems(epic);
            if (links.ContainsKey(TfsRelationNames.Child))
                Assert.Fail("Child links still present - should not be set");

            Assert.Pass("Everything worked as expected");
        }

        [Test]
        public void GetCurrentIterationPath()
        {
            string currentIteration = m_changeMgmt.GetServerProperty("CurrentIterationPath", new Dictionary<string, object> { { "IterationPathRoot", m_settings.ProjectName } }).ToString();

            if (string.IsNullOrWhiteSpace(currentIteration))
                Assert.Fail("Failed to get current iteration");

            Assert.Pass("Everything worked as expected");
        }
    }
}