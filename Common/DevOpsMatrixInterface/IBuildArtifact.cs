using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevOpsMatrix.Interface
{
    public interface IBuildArtifact
    {
        string Name { get; }
        string ResourceType { get; }
        string Data { get; }
    }
}
