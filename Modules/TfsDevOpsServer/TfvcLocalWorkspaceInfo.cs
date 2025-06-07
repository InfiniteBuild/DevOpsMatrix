using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevOpsMatrix.Tfs.Server
{
    internal class TfvcLocalWorkspaceInfo
    {
        public string WorkspaceName { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public DateTime LastModified { get; set; }
        public string CollectionUrl { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;

        public Dictionary<string, string> ServerToLocalPathMap { get; set; } = new Dictionary<string, string>();

        public TfvcLocalWorkspaceInfo()
        {

        }
    }
}
