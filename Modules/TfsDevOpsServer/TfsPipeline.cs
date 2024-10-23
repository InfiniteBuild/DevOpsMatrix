using DevOpsInterface;
using Microsoft.TeamFoundation.Build.WebApi;

namespace TfsDevOpsServer
{
    public class TfsPipeline : IDevOpsPipeline
    {
        private BuildDefinition? m_buildDef = null;

        public string Name 
        {
            get
            {
                if (m_buildDef != null) 
                    return m_buildDef.Name;
                return string.Empty;
            } 
        }

        public Dictionary<int, IDevOpsPipelineBuild> BuildList { get; } = new Dictionary<int, IDevOpsPipelineBuild>();

        protected TfsPipeline() 
        { 
        }

        public TfsPipeline(BuildDefinition buildDef) : this()
        {
            m_buildDef = buildDef;
        }

        public IDevOpsPipelineBuild? GetLatestBuild()
        {
            if (m_buildDef != null)
            {
                if (m_buildDef.LatestBuild != null)
                {
                    if (BuildList.ContainsKey(m_buildDef.LatestBuild.Id))
                        return BuildList[m_buildDef.LatestBuild.Id];
                }
            }

            if (BuildList.Count > 0)
                return BuildList[BuildList.Keys.FirstOrDefault()];

            return null;
        }
    }
}
