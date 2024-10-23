using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevOpsInterface
{
    public enum DevOpsBuildStatus
    {
        unknown = 0,
        running = 1,
        success = 2,
        partialsuccess = 4,
        failure = 8,
        aborted = 16,
    }

    public interface IDevOpsPipelineBuild
    {
        int Id { get; }
        string BuildNumber { get; }
        DevOpsBuildStatus Status { get; }
        string BuildChangeId { get; }
        DateTime? StartTime { get; }
        DateTime? FinishTime { get; }

        Dictionary<string, IBuildArtifact> ArtifactList { get; }
        List<IBuildStep> BuildSteps { get; set; }
    }
}
