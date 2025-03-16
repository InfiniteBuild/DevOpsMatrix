using DevOpsMatrix.Interface;
using Microsoft.TeamFoundation.Build.WebApi;

namespace DevOpsMatrix.Tfs.Server
{
    public class TfsPipelineBuild : IDevOpsPipelineBuild
    {
        private Build? m_tfsBuild = null;
        private IDevOpsBuildSystem m_buildSystem = null;
        private Dictionary<string, IBuildArtifact> m_artifactList = new Dictionary<string, IBuildArtifact>();
        private List<IBuildStep> m_buildSteps = new List<IBuildStep>();

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

        public Dictionary<string, IBuildArtifact> ArtifactList
        {
            get
            {
                if (m_artifactList.Count > 0)
                    return m_artifactList;

                m_artifactList = m_buildSystem.GetPipelineBuildArtifacts(this);
                return m_artifactList;
            }
        }

        public List<IBuildStep> BuildSteps
        {
            get 
            {
                if (m_buildSteps.Count > 0)
                    return m_buildSteps;
                m_buildSteps = m_buildSystem.GetPipelineBuildLogs(this);
                return m_buildSteps;
            }
        }

        protected TfsPipelineBuild(IDevOpsBuildSystem buildsystem) 
        {
            m_buildSystem = buildsystem;
        }

        public TfsPipelineBuild(Build tfsbuild, IDevOpsBuildSystem buildsystem) : this(buildsystem)
        {
            m_tfsBuild = tfsbuild;
        }
    }
}
