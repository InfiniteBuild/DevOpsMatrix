using DevOpsInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TfsDevOpsServer
{
    public class TfsBuildStep : IBuildStep
    {
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; 
        public string AgentName { get; set; } = string.Empty;  
        public string Log { get; set; } = string.Empty;
    }
}
