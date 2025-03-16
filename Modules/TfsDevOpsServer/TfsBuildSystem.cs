using DevOpsMatrix.Interface;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using TfsDevOpsMatrix.Tfs.ServerDevOpsServer;

namespace DevOpsMatrix.Tfs.Server
{
    public class TfsBuildSystem : IDevOpsBuildSystem
    {
        private IDevOpsSettings m_settings;

        public string ServiceName { get { return "build"; } }

        public TfsBuildSystem(IDevOpsSettings settings)
        {
            m_settings = settings;
        }

        private BuildHttpClient GetBuildClient()
        {
            VssConnection connection = TfsServiceTools.CreateConnection(m_settings);
            BuildHttpClient client = connection.GetClient<BuildHttpClient>();
            return client;
        }

        public IDevOpsPipeline GetPipeline(string Name)
        {
            BuildHttpClient buildClient = GetBuildClient();

            List<BuildDefinitionReference> buildDefList = buildClient.GetDefinitionsAsync(m_settings.ProjectName, name: Name).Result;
            BuildDefinitionReference buildDefRef = buildDefList[0];
            BuildDefinition buildDef = buildClient.GetDefinitionAsync(m_settings.ProjectName, buildDefRef.Id).Result;

            TfsPipeline pipeline = new TfsPipeline(buildDef, this);

            List<Build> buildList = buildClient.GetBuildsAsync(m_settings.ProjectName, new List<int> { buildDef.Id }, queryOrder: BuildQueryOrder.QueueTimeDescending).Result;
            foreach(Build buildItem in buildList)
            {
                TfsPipelineBuild build = new TfsPipelineBuild(buildItem, this);
                pipeline.BuildList[buildItem.Id] = build;
            }

            return pipeline;
        }

        public List<IDevOpsPipeline> GetPipelineList(string Name)
        {
            BuildHttpClient buildClient = GetBuildClient();

            List<IDevOpsPipeline> retList = new List<IDevOpsPipeline>();

            List<BuildDefinitionReference> buildDefList = buildClient.GetDefinitionsAsync(m_settings.ProjectName, name: Name).Result;
            foreach(BuildDefinitionReference buildDefRef in buildDefList)
            {
                BuildDefinition buildDef = buildClient.GetDefinitionAsync(m_settings.ProjectName, buildDefRef.Id).Result;
                TfsPipeline pipeline = new TfsPipeline(buildDef, this);
                retList.Add(pipeline);
            }

            return retList;
        }

        public IDevOpsPipelineBuild LaunchPipelineBuild(IDevOpsPipeline pipeline)
        {
            BuildHttpClient buildClient = GetBuildClient();

            Build queuedBuild = buildClient.QueueBuildAsync(new Build() { Definition = new DefinitionReference() { Id = pipeline.Id } }, m_settings.ProjectName).Result;
            TfsPipelineBuild build = new TfsPipelineBuild(queuedBuild, this);

            return build;
        }

        public IDevOpsPipelineBuild LaunchPipelineBuild(IDevOpsPipeline pipeline, Dictionary<string,string>? variableList = null, Dictionary<string,string>? demandList = null)
        {
            BuildHttpClient buildClient = GetBuildClient();

            Build buildsettings = new Build()
            {
                Definition = new DefinitionReference() { Id = pipeline.Id }
            };

            if ((variableList != null) && (variableList.Count > 0))
                buildsettings.Parameters = Newtonsoft.Json.JsonConvert.SerializeObject(variableList);
            
            if ((demandList != null) && (demandList.Count > 0))
            {
                foreach (string key in demandList.Keys)
                {
                    buildsettings.Demands.Add(new DemandEquals(key, demandList[key]));
                }
            }


            Build queuedBuild = buildClient.QueueBuildAsync(buildsettings, m_settings.ProjectName).Result;
            TfsPipelineBuild build = new TfsPipelineBuild(queuedBuild, this);

            return build;
        }

        public IDevOpsPipelineBuild GetPipelineBuild(int buildId)
        {
            BuildHttpClient buildClient = GetBuildClient();

            Build buildItem = buildClient.GetBuildAsync(m_settings.ProjectName, buildId).Result;
            TfsPipelineBuild build = new TfsPipelineBuild(buildItem, this);

            return build;
        }

        public Dictionary<string, IBuildArtifact>  GetPipelineBuildArtifacts(IDevOpsPipelineBuild build)
        {
            BuildHttpClient buildClient = GetBuildClient();
            Dictionary<string, IBuildArtifact> retItem = new Dictionary<string, IBuildArtifact>();

            // Get the build artifacts
            List<BuildArtifact> artifactList = buildClient.GetArtifactsAsync(m_settings.ProjectName, build.Id).Result;
            foreach (BuildArtifact artifact in artifactList)
            {
                TfsBuildArtifact buildArt = new TfsBuildArtifact();
                buildArt.Name = artifact.Name;
                buildArt.ResourceType = artifact.Resource.Type;
                buildArt.Data = artifact.Resource.Data;

                retItem[buildArt.Name] = buildArt;
            }

            return retItem;
        }

        public List<IBuildStep> GetPipelineBuildLogs(IDevOpsPipelineBuild build)
        {
            BuildHttpClient buildClient = GetBuildClient();
            List<IBuildStep> buildSteps = new List<IBuildStep>();

            Timeline buildtimeline = buildClient.GetBuildTimelineAsync(m_settings.ProjectName, build.Id).Result;
            foreach (TimelineRecord record in buildtimeline.Records)
            {
                TfsBuildStep step = new TfsBuildStep();
                step.Name = record.Name;
                step.Status = record.State.ToString();
                if (record.State == TimelineRecordState.Completed)
                    step.Status = record.Result.ToString();
                step.AgentName = record.WorkerName;

                if (record.Log != null)
                {
                    Stream logstream = buildClient.GetBuildLogAsync(m_settings.ProjectName, build.Id, record.Log.Id).Result;
                    step.Log = new StreamReader(logstream).ReadToEnd();
                }
                buildSteps.Add(step);
            }

            return buildSteps;
        }
    }
}
