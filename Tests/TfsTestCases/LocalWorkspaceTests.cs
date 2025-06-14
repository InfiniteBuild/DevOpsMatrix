
using DevOpsMatrix.Core;
using DevOpsMatrix.Interface;

namespace TfsTestCases
{
    public class LocalWorkspaceTests
    {
        [SetUp]
        public void Setup()
        {
            
        }

        [TearDown]
        public void TearDown()
        {
        }

        [OneTimeSetUp] 
        public void SetupFixture()
        {
            
        }

        [Test]
        public void GetValidTfVcWorkspaceFromBranchPath()
        {
            string localWorkspacePath = @"D:\TFS\TestProjects\Dev\Feature1";
            DevOpsFactory factory = DevOpsFactory.Instance;

            List<IDevOpsWorkspace> workspaceList = factory.GetWorkspace(localWorkspacePath);
            if (workspaceList.Count == 0)
            {
                Assert.Fail("No workspaces found for the specified path.");
            }

            if (workspaceList.Count > 1)
            {
                Assert.Fail("Multiple workspaces found for the specified path.");
            }

            IDevOpsWorkspace workspace = workspaceList[0];
            Assert.That(workspace != null, "Workspace should not be null.");
            Assert.That(workspace.IsValidWorkspace, Is.True, "Workspace is not valid.");
            Assert.That(workspace.LocalBranchRoot.Equals(@"D:\TFS\TestProjects\Dev\Feature1"), Is.True, "Workspace branch root does not match the expected path.");
        }

        [Test]
        public void GetValidTfVcWorkspaceFromBranchItemPath()
        {
            string localWorkspacePath = @"D:\TFS\TestProjects\Dev\Feature1\buildtools\nuget\nuget.exe";
            DevOpsFactory factory = DevOpsFactory.Instance;

            List<IDevOpsWorkspace> workspaceList = factory.GetWorkspace(localWorkspacePath);
            if (workspaceList.Count == 0)
            {
                Assert.Fail("No workspaces found for the specified path.");
            }

            if (workspaceList.Count > 1)
            {
                Assert.Fail("Multiple workspaces found for the specified path.");
            }

            IDevOpsWorkspace workspace = workspaceList[0];
            Assert.That(workspace != null, "Workspace should not be null.");
            Assert.That(workspace.IsValidWorkspace, Is.True, "Workspace is not valid.");
            Assert.That(workspace.LocalBranchRoot.Equals(@"D:\TFS\TestProjects\Dev\Feature1"), Is.True, "Workspace branch root does not match the expected path.");
        }

        [Test]
        public void GetValidTfVcWorkspaceFromBranchItemNotInSourceControl()
        {
            string localWorkspacePath = @"D:\TFS\TestProjects\Dev\Feature1\NotInSourceControl\TextFileNotInSourceControl.txt";
            DevOpsFactory factory = DevOpsFactory.Instance;

            List<IDevOpsWorkspace> workspaceList = factory.GetWorkspace(localWorkspacePath);
            if (workspaceList.Count == 0)
            {
                Assert.Fail("No workspaces found for the specified path.");
            }

            if (workspaceList.Count > 1)
            {
                Assert.Fail("Multiple workspaces found for the specified path.");
            }

            IDevOpsWorkspace workspace = workspaceList[0];
            Assert.That(workspace != null, "Workspace should not be null.");
            Assert.That(workspace.IsValidWorkspace, Is.True, "Workspace is not valid.");
            Assert.That(workspace.LocalBranchRoot.Equals(@"D:\TFS\TestProjects\Dev\Feature1"), Is.True, "Workspace branch root does not match the expected path.");
        }

        [Test]
        public void GetInvalidTfVcWorkspace()
        {
            string localWorkspacePath = @"D:\Temp";
            DevOpsFactory factory = DevOpsFactory.Instance;

            List<IDevOpsWorkspace> workspaceList = factory.GetWorkspace(localWorkspacePath);
            Assert.That(workspaceList.Count == 0, workspaceList.Count.ToString() + " workspaces found for the specified path, expected 0.");
        }

        [Test]
        public void Checkout_GetPending_Undo_Item()
        {
            string localWorkspacePath = @"D:\TFS\TestProjects\Dev\Feature1\buildtools\nuget\nuget.exe";
            DevOpsFactory factory = DevOpsFactory.Instance;

            List<IDevOpsWorkspace> workspaceList = factory.GetWorkspace(localWorkspacePath);
            if (workspaceList.Count == 0)
            {
                Assert.Fail("No workspaces found for the specified path.");
            }

            IDevOpsWorkspace workspace = workspaceList[0];
            workspace.Checkout(localWorkspacePath);

            List<ISourceCodePendingChange> pendingchanges = workspace.GetPendingChanges(workspace.LocalBranchRoot);
            Assert.That(pendingchanges.Count > 0, "No pending changes found after checkout.");

            workspace.UndoChanges(localWorkspacePath);
        }
    }
}