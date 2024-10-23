using DevOpsInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TfsDevOpsServer
{
    public class TfsBuildArtifact : IBuildArtifact
    {
        public string Name { get; set; } = string.Empty;

        public string ResourceType { get; set; } = string.Empty;

        public string Data { get; set; } = string.Empty;
    }
}
