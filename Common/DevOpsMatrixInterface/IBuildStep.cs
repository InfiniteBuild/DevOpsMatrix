using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevOpsInterface
{
    public interface IBuildStep
    {
        string Name { get; set; }
        string Status { get; set; } 
        string AgentName { get; set; }
        string Log { get; set; } 
    }
}
