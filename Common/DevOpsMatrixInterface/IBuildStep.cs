using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevOpsMatrix.Interface
{
    public interface IBuildStep
    {
        string Name { get; set; }
        string Status { get; set; } 
        string AgentName { get; set; }
        string Log { get; set; } 
    }
}
