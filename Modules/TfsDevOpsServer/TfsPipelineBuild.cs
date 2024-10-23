using DevOpsMatrix.Interface;
using Microsoft.TeamFoundation.Build.WebApi;

namespace DevOpsMatrix.Tfs.Server
{
    public class TfsPipelineBuild : IDevOpsPipelineBuild
    {
        private Build? m_tfsBuild = null;

        public int Id
        {
            get
            {
                if (m_tfsBuild != null)
                    return m_tfsBuild.Id;
                return -1;
            }
        } 

        public string BuildNumber
        {
            get
            {
                if (m_tfsBuild != null)
                    return m_tfsBuild.BuildNumber;
                return string.Empty;
            }
        }

        public DevOpsBuildStatus Status
        {
            get
            {
                DevOpsBuildStatus retval = DevOpsBuildStatus.unknown;
                if (m_tfsBuild != null)
                {
                    if (m_tfsBuild.Status == BuildStatus.None)
                        retval = DevOpsBuildStatus.unknown;

                    if ((m_tfsBuild.Status == BuildStatus.InProgress) ||
                        (m_tfsBuild.Status == BuildStatus.NotStarted))
                    {
                        retval = DevOpsBuildStatus.running;
                    }

                    if (m_tfsBuild.Status == BuildStatus.Completed)
                    {
                        switch (m_tfsBuild.Result)
                        {
                            case BuildResult.None:
                                retval = DevOpsBuildStatus.unknown;
                                break;
                            case BuildResult.Succeeded:
                                retval = DevOpsBuildStatus.success;
                                break;
                            case BuildResult.PartiallySucceeded:
                                retval = DevOpsBuildStatus.partialsuccess;
                                break;
                            case BuildResult.Failed:
                                retval = DevOpsBuildStatus.failure;
                                break;
                            case BuildResult.Canceled:
                                retval = DevOpsBuildStatus.aborted;
                                break;
                        }
                    }
                }
                    
                return retval;
            }
        }

        public string BuildChangeId
        {
            get
            {
                if (m_tfsBuild != null)
                    return m_tfsBuild.SourceVersion;
                return string.Empty;
            }
        }

        public DateTime? StartTime
        {
            get
            {
                if (m_tfsBuild != null)
                    return m_tfsBuild.StartTime;
                return null;
            }
        }

        public DateTime? FinishTime
        {
            get
            {
                if (m_tfsBuild != null)
                    return m_tfsBuild.FinishTime;
                return null;
            }
        }

        public Dictionary<string, IBuildArtifact> ArtifactList { get; } = new Dictionary<string, IBuildArtifact>();
        public List<IBuildStep> BuildSteps { get; set; } = new List<IBuildStep>();

        protected TfsPipelineBuild() 
        {
        }

        public TfsPipelineBuild(Build tfsbuild) : this()
        {
            m_tfsBuild = tfsbuild;
        }
    }
}
