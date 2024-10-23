using System.Text;
using System.Reflection;
using DevOpsMatrix.Interface;
using DevOpsMatrix.Core;
using DevOpsMatrix.Tfs.Server;

namespace TfsTestCases
{
    public class SourceControlTests
    {
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
        }

        [Test]
        public void GetItemHistory()
        {
            string branch = "$/TestProject/Dev/Dev1";

            ISourceCodeItem branchItem = m_sourceControl.GetItem(branch);
            List<ISourceCodeHistory> historyList = m_sourceControl.GetHistory(branchItem);
            Assert.That(historyList.Count > 0, "Failed to retrieve history for branch: " + branch);
        }

        [Test]
        public void GetItemHistoryWithRename()
        {
            string branch = "$/TestProject/Dev/Bugfix";

            ISourceCodeItem branchItem = m_sourceControl.GetItem(branch);
            List<ISourceCodeHistory> historyList = m_sourceControl.GetHistory(branchItem);
            Assert.That(historyList.Count > 0, "Failed to retrieve history for branch: " + branch);
        }

        [Test]
        public void GetItemWithTextContent()
        {
            string item = "$/TestProject/Dev/Dev1/buildtools/7-Zip/readme.txt";

            ISourceCodeItem branchItem = m_sourceControl.GetItem(item, true);
            Assert.That(branchItem.IsBinary == false, "Should not be binary");
            if (branchItem.ContentType.Contains("text"))
            {
                string content = Encoding.GetEncoding(branchItem.Encoding).GetString(branchItem.Content);
            }
        }

        [Test]
        public void GetItemWithBinaryContent()
        {
            string item = "$/TestProject/Dev/Dev1/buildtools/7-Zip/7z.exe";

            string asmPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string filePath = Path.Combine(asmPath, "TestData", "7z.exe");

            ISourceCodeItem branchItem = m_sourceControl.GetItem(item, true);
            Assert.That(branchItem.IsBinary == true, "7z.exe should be binary");
            if (branchItem.ContentType.Contains("application"))
            {
                byte[] content = branchItem.Content;
                byte[] controlContent = File.ReadAllBytes(filePath);

                Assert.That(content.SequenceEqual(controlContent), "Content of 7z.exe is not as expected");
            }

            branchItem = m_sourceControl.GetItem("$/TestProject/Dev/Dev1/buildtools/7-Zip/7-zip.dll", true);
            Assert.That(branchItem.IsBinary == true, "7-zip.dll should be binary");

            branchItem = m_sourceControl.GetItem("$/TestProject/Dev/Dev1/buildtools/7-Zip/7-zip.chm", true);
            Assert.That(branchItem.IsBinary == true, "7-zip.chm should be binary");
        }

        [Test]
        public void GetInvalidItem()
        {
            string item = "$/TestProject/Dev/Dev1/buildtools/7-Zip/badFile.txt";

            ISourceCodeItem branchItem = m_sourceControl.GetItem(item, false);
            Assert.That(branchItem == null, "The retrieved item should be null but wasn't");
        }

        [Test]
        public void GetParentBranch_Valid()
        {
            string branch = "$/TestProject/Dev/Dev1";
            ISourceCodeItem item = m_sourceControl.GetItem(branch);
            ISourceCodeItem parent = m_sourceControl.GetParentBranch(item);

            if (parent == null)
                Assert.Fail("Failed to get parent when parent is available");

            if (parent.ItemPath != "$/TestProject/Main")
                Assert.Fail("GetParentBranch retrieved the wrong parent");

            Assert.Pass("Worked as expected");
        }

        [Test]
        public void GetParentBranch_ChildItemOfBranch()
        {
            string sourceItem = "$/TestProject/Dev/Feature1/Feature_StaticTestFile.txt";
            ISourceCodeItem item = m_sourceControl.GetItem(sourceItem);
            ISourceCodeItem parent = m_sourceControl.GetParentBranch(item);

            if (parent == null)
                Assert.Fail("Failed to get parent when parent is available");

            if (parent.ItemPath != "$/TestProject/Dev/Dev1")
                Assert.Fail("GetParentBranch retrieved the wrong parent");

            Assert.Pass("Worked as expected");
        }

        [Test]
        public void GetParentBranch_NoParent()
        {
            string sourceItem = "$/TestProject/Main";
            ISourceCodeItem item = m_sourceControl.GetItem(sourceItem);
            ISourceCodeItem parent = m_sourceControl.GetParentBranch(item);

            if (parent != null)
                Assert.Fail("Parent should have been null was not");

            Assert.Pass("Worked as expected");
        }

        [Test]
        public void AddAndDeleteItemInBranch()
        {
            ISourceCodeItem branch = m_sourceControl.GetItem("$/TestProject/Dev/Dev1");
            Assert.That(branch != null, "Failed to retrieve branch: $/TestProject/Dev/Dev1");

            ISourceCodePendingChangeSet pendingChanges = m_sourceControl.CreatePendingChangeset();
            pendingChanges.Comment = "Create a test changeset";
            pendingChanges.AddItem(branch.ItemPath, "TestFile.txt", Encoding.UTF8.GetBytes("This is a test file"), "text/plain", false, Encoding.UTF8.CodePage);

            m_sourceControl.CommitChanges(pendingChanges);
            ISourceCodeItem testItem = m_sourceControl.GetItem("$/TestProject/Dev/Dev1/TestFile.txt", true);
            Assert.That(testItem != null, "Failed to retrieve test item: $/TestProject/Dev/Dev1/TestFile.txt");

            string content = Encoding.UTF8.GetString(testItem.Content);
            Assert.That(content == "This is a test file", "Content of test item is not as expected: " + content);

            ISourceCodePendingChangeSet deleteChanges = m_sourceControl.CreatePendingChangeset();
            deleteChanges.Comment = "Delete the test file";
            deleteChanges.DeleteItem(testItem);
            m_sourceControl.CommitChanges(deleteChanges);

            testItem = m_sourceControl.GetItem("$/TestProject/Dev/Dev1/TestFile.txt");
            Assert.That(testItem == null, "Failed to delete test item: $/TestProject/Dev/Dev1/TestFile.txt");
        }

        [Test]
        public void AddAndDeleteBinaryItemInBranch()
        {
            ISourceCodeItem branch = m_sourceControl.GetItem("$/TestProject/Dev/Dev1");
            Assert.That(branch != null, "Failed to retrieve branch: $/TestProject/Dev/Dev1");

            ISourceCodeItem existingBin = m_sourceControl.GetItem("$/TestProject/Dev/Dev1/buildtools/7-Zip/7-zip.dll", true);


            ISourceCodePendingChangeSet pendingChanges = m_sourceControl.CreatePendingChangeset();
            pendingChanges.Comment = "Create a test changeset";
            pendingChanges.AddItem(branch.ItemPath, "Second_7-zip.dll", existingBin.Content, existingBin.ContentType, existingBin.IsBinary, existingBin.Encoding);

            m_sourceControl.CommitChanges(pendingChanges);
            ISourceCodeItem testItem = m_sourceControl.GetItem("$/TestProject/Dev/Dev1/Second_7-zip.dll", true);
            Assert.That(testItem != null, "Failed to retrieve test item: $/TestProject/Dev/Dev1/Second_7-zip.dll");

            ISourceCodePendingChangeSet deleteChanges = m_sourceControl.CreatePendingChangeset();
            deleteChanges.Comment = "Delete the test file";
            deleteChanges.DeleteItem(testItem);
            m_sourceControl.CommitChanges(deleteChanges);

            testItem = m_sourceControl.GetItem("$/TestProject/Dev/Dev1/Second_7-zip.dll");
            Assert.That(testItem == null, "Failed to delete test item: $/TestProject/Dev/Dev1/Second_7-zip.dll");
        }

        [Test]
        public void CreateRenameDeleteItemInBranch()
        {
            ISourceCodeItem branch = m_sourceControl.GetItem("$/TestProject/Dev/Dev1");
            Assert.That(branch != null, "Failed to retrieve branch: $/TestProject/Dev/Dev1");

            ISourceCodePendingChangeSet pendingChanges = m_sourceControl.CreatePendingChangeset();
            pendingChanges.Comment = "Create a test changeset";
            pendingChanges.AddItem(branch.ItemPath, "FileToBeRenamed.txt", Encoding.UTF8.GetBytes("This is a test file"), "text/plain", false, Encoding.UTF8.CodePage);

            m_sourceControl.CommitChanges(pendingChanges);
            ISourceCodeItem testItem = m_sourceControl.GetItem(branch.ItemPath + "/" + "FileToBeRenamed.txt");
            Assert.That(testItem != null, "Failed to retrieve test item: " + branch.ItemPath + "/" + "FileToBeRenamed.txt");

            string renamedPath = Path.GetDirectoryName(testItem.ItemPath) + "/" + "RenamedFile.txt";
            ISourceCodePendingChangeSet renameChange = m_sourceControl.CreatePendingChangeset();
            renameChange.Comment = "Rename the test file";
            renameChange.RenameItem(testItem, renamedPath);
            m_sourceControl.CommitChanges(renameChange);

            testItem = m_sourceControl.GetItem(renamedPath);
            Assert.That(testItem != null, "Failed to retrieve renamed item: " + renamedPath);

            ISourceCodePendingChangeSet deleteChanges = m_sourceControl.CreatePendingChangeset();
            deleteChanges.Comment = "Delete the test file";
            deleteChanges.DeleteItem(testItem);
            m_sourceControl.CommitChanges(deleteChanges);
        }

        [Test]
        public void AddEditDeleteItemInBranch()
        {
            ISourceCodeItem branch = m_sourceControl.GetItem("$/TestProject/Dev/Dev1");
            Assert.That(branch != null, "Failed to retrieve branch: $/TestProject/Dev/Dev1");

            ISourceCodePendingChangeSet pendingChanges = m_sourceControl.CreatePendingChangeset();
            pendingChanges.Comment = "Create a test changeset";
            pendingChanges.AddItem(branch.ItemPath, "TestFileForEdit.txt", Encoding.UTF8.GetBytes("This is a test file"), "text/plain", false, Encoding.UTF8.CodePage);
            m_sourceControl.CommitChanges(pendingChanges);

            ISourceCodeItem testItem = m_sourceControl.GetItem(branch.ItemPath + "/TestFileForEdit.txt", true);
            Assert.That(testItem != null, "Failed to retrieve test item: $/TestProject/Dev/Dev1/TestFile.txt");

            ISourceCodePendingChangeSet editFileChange = m_sourceControl.CreatePendingChangeset();
            string content = Encoding.UTF8.GetString(testItem.Content);
            content += Environment.NewLine + " with some additional content";
            testItem.Content = Encoding.UTF8.GetBytes(content);
            editFileChange.UpdateItem(testItem, -1);
            m_sourceControl.CommitChanges(editFileChange);

            testItem = m_sourceControl.GetItem(branch.ItemPath + "/TestFileForEdit.txt", true);
            string updatedContent = Encoding.UTF8.GetString(testItem.Content);
            Assert.That(updatedContent == content, "Failed to update content of test item: " + branch.ItemPath + "/TestFileForEdit.txt");

            ISourceCodePendingChangeSet deleteChanges = m_sourceControl.CreatePendingChangeset();
            deleteChanges.Comment = "Delete the test file";
            deleteChanges.DeleteItem(testItem);
            m_sourceControl.CommitChanges(deleteChanges);

            testItem = m_sourceControl.GetItem(branch.ItemPath + "/TestFileForEdit.txt");
            Assert.That(testItem == null, "Failed to delete test item: " + branch.ItemPath + "/TestFileForEdit.txt");
        }

        [Test]
        public void TestSoapUndelete()
        {
            DevOpsFeatureToggle.Instance.EnableFeature(TfvcSourceControl.FeatureSoapCommit);

            ISourceCodePendingChangeSet pendingChanges = m_sourceControl.CreatePendingChangeset();
            pendingChanges.Comment = "Test undelete";

            pendingChanges.UndeleteItem("$/TestProject/Dev/Dev1/TestData/WinCC-ES1_EU.lpu", null);

            List<int> csId = m_sourceControl.CommitChanges(pendingChanges);

            Assert.That(csId.Count > 0, "Failed to commit changes");

            ISourceCodeItem item = m_sourceControl.GetItem("$/TestProject/Dev/Dev1/TestData/WinCC-ES1_EU.lpu");
            ISourceCodePendingChangeSet deleteChange = m_sourceControl.CreatePendingChangeset();
            deleteChange.Comment = "Delete the large file";
            deleteChange.DeleteItem(item);
            m_sourceControl.CommitChanges(deleteChange);
        }

        [Test]
        public void TestSoapLargeFileUploadAndDelete()
        {
            DevOpsFeatureToggle.Instance.EnableFeature(TfvcSourceControl.FeatureSoapCommit);

            ISourceCodePendingChangeSet pendingChanges = m_sourceControl.CreatePendingChangeset();
            pendingChanges.Comment = "Test the SOAP API commit with a large file";

            string asmPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string filePath = Path.Combine(asmPath, "TestData", "WinCC-ES1_EU.lpu");

            pendingChanges.AddItem("$/TestProject/Dev/Dev1/TestData", "WinCC-ES1_EU.lpu", File.ReadAllBytes(filePath), "application/octet-stream", true, -1);

            List<int> csId = m_sourceControl.CommitChanges(pendingChanges);

            Assert.That(csId.Count > 0, "Failed to commit changes");

            ISourceCodeItem item = m_sourceControl.GetItem("$/TestProject/Dev/Dev1/TestData/WinCC-ES1_EU.lpu");
            ISourceCodePendingChangeSet deleteChange = m_sourceControl.CreatePendingChangeset();
            deleteChange.Comment = "Delete the large file";
            deleteChange.DeleteItem(item);
            m_sourceControl.CommitChanges(deleteChange);
        }

        [Test]
        public void TestRestApiLargeFileUpload()
        {
            DevOpsFeatureToggle.Instance.DisableFeature(TfvcSourceControl.FeatureSoapCommit);

            ISourceCodePendingChangeSet pendingChanges = m_sourceControl.CreatePendingChangeset();
            pendingChanges.Comment = "Test the Rest API commit with a large file";

            string asmPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string filePath = Path.Combine(asmPath, "TestData", "WinCC-ES1_EU.lpu");

            pendingChanges.AddItem("$/TestProject/Dev/Dev1/TestData", "WinCC-ES1_EU.lpu", File.ReadAllBytes(filePath), "application/octet-stream", true, -1);

            List<int> csId = m_sourceControl.CommitChanges(pendingChanges);

            Assert.That(csId.Count > 0, "Failed to commit changes");
        }
    }
}