using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.VersionControl.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;

namespace DevOpsMatrix.Tfs.Soap.ApiExecutor
{
    public class WorkContext
    {
        public static WorkContext Instance { get; private set; }

        private string m_tfsUrl = string.Empty;
        private string m_accessToken = string.Empty;    

        private List<Workspace> m_workspaceCache = new List<Workspace>();

        protected WorkContext(string tfsurl, string tfsproject, string accesstoken)
        {
            m_tfsUrl = tfsurl;
            m_accessToken = accesstoken;

            VersionControlServer vcsServer = GetTfsVersionControlServer();
            Workstation.Current.EnsureUpdateWorkspaceInfoCache(vcsServer, vcsServer.AuthorizedUser);
        }

        private TfsTeamProjectCollection GetTfsProjectCollection()
        {
            // TODO: Set authentication - use access token if available
            TfsTeamProjectCollection tfs = new TfsTeamProjectCollection(new Uri(m_tfsUrl));
            tfs.EnsureAuthenticated();
            return tfs;
        }

        private VersionControlServer GetTfsVersionControlServer()
        {
            TfsTeamProjectCollection tfs = GetTfsProjectCollection();
            VersionControlServer vcsServer = tfs.GetService<VersionControlServer>();
            return vcsServer;
        }

        public Workspace GetWorkspace(string serverPath)
        {
            Workspace found = null;

            foreach (Workspace wkspace in m_workspaceCache)
            {
                try
                {
                    if (wkspace.IsServerPathMapped(serverPath))
                    {
                        found = wkspace;
                        break;
                    }
                }
                catch 
                {
                }
            }

            if (found == null)
            {
                try
                {
                    string branchPath = GetBranchPath(serverPath);
                    found = SearchForLocalWorkspace(branchPath);
                    if (found != null)
                        m_workspaceCache.Add(found);
                }
                catch 
                {
                }
            }

            return found;
        }

        public int CommitChanges(string comment)
        {
            int result = 0;

            foreach (Workspace ws in m_workspaceCache)
            {
                PendingChange[] changes = ws.GetPendingChanges();
                if (changes.Length > 0)
                {
                    WorkspaceCheckInParameters wip = new WorkspaceCheckInParameters(ws.GetPendingChanges(), comment)
                    {
                        // Enable the override of gated check-in
                        OverrideGatedCheckIn = ((CheckInOptions2)ws.VersionControlServer.SupportedFeatures & CheckInOptions2.OverrideGatedCheckIn) == CheckInOptions2.OverrideGatedCheckIn,
                        PolicyOverride = new PolicyOverrideInfo("Automation tool checkin", null)
                    };
                    int changeset = ws.CheckIn(wip);
                    result = changeset;
                }
            }

            m_workspaceCache.Clear();
            return result;
        }

        public void UndoChanges()
        {
            foreach (Workspace ws in m_workspaceCache)
            {
                PendingChange[] changes = ws.GetPendingChanges();
                if (changes.Length > 0)
                {
                    ws.Undo(changes);
                }
            }

            m_workspaceCache.Clear();
        }

        private string GetBranchPath(string serverpath)
        {
            VersionControlServer vcsServer = GetTfsVersionControlServer();
            foreach(BranchObject branch in vcsServer.QueryRootBranchObjects(RecursionType.Full))
            {
                string testbranchpath = branch.Properties.RootItem.Item.ToLower();

                if (serverpath.ToLower() == testbranchpath)
                    return branch.Properties.RootItem.Item;

                if (serverpath.ToLower().Contains(testbranchpath + "/"))
                {
                    return branch.Properties.RootItem.Item;
                }
            }

            return string.Empty;
        }

        private Workspace SearchForLocalWorkspace(string branchPath)
        {
            TfsTeamProjectCollection tfs = GetTfsProjectCollection();
            foreach(WorkspaceInfo wsInfo in Workstation.Current.GetAllLocalWorkspaceInfo())
            {
                try
                {
                    Workspace wkspace = null;
                    if (wsInfo.ServerUri == tfs.Uri)
                    {
                        try { wkspace = wsInfo.GetWorkspace(tfs); }
                        catch { wkspace = null; }
                    }
                    if (wkspace == null)
                        continue;

                    if (wkspace.IsServerPathMapped(branchPath))
                        return wkspace;
                }
                catch
                {
                }
            }

            return null;
        }

        public static WorkContext CreateContext(string tfsurl, string tfsproject, string accesstoken)
        {
            if (Instance != null)
            {
                throw new InvalidOperationException("WorkContext already exists.");
            }

            if (Instance == null)
            {
                Instance = new WorkContext(tfsurl, tfsproject, accesstoken);
            }

            return Instance;
        }
    }
}
