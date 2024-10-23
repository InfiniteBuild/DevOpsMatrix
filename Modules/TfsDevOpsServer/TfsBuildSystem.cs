using DevOpsInterface;
using Microsoft.Azure.Pipelines.WebApi;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.VisualStudio.Services.WebApi;

namespace TfsDevOpsServer
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

            TfsPipeline pipeline = new TfsPipeline(buildDef);

            List<Build> buildList = buildClient.GetBuildsAsync(m_settings.ProjectName, new List<int> { buildDef.Id }, queryOrder: BuildQueryOrder.QueueTimeDescending).Result;
            foreach(Build buildItem in buildList)
            {
                TfsPipelineBuild build = new TfsPipelineBuild(buildItem);
                pipeline.BuildList[buildItem.Id] = build;

                Timeline buildtimeline = buildClient.GetBuildTimelineAsync(m_settings.ProjectName, buildItem.Id).Result;
                foreach(TimelineRecord record in buildtimeline.Records)
                {
                    TfsBuildStep step = new TfsBuildStep();
                    step.Name = record.Name;
                    step.Status = record.State.ToString();
                    if (record.State == TimelineRecordState.Completed)
                        step.Status = record.Result.ToString();
                    step.AgentName = record.WorkerName;
                    
                    if (record.Log != null)
                    {
                        Stream logstream = buildClient.GetBuildLogAsync(m_settings.ProjectName, buildItem.Id, record.Log.Id).Result;
                        step.Log = new StreamReader(logstream).ReadToEnd();
                    }
                    build.BuildSteps.Add(step);
                }
                
                // Get the build artifacts
                List<BuildArtifact> artifactList = buildClient.GetArtifactsAsync(m_settings.ProjectName, buildItem.Id).Result;
                foreach(BuildArtifact artifact in artifactList)
                {
                    TfsBuildArtifact buildArt = new TfsBuildArtifact();
                    buildArt.Name = artifact.Name;
                    buildArt.ResourceType = artifact.Resource.Type;
                    buildArt.Data = artifact.Resource.Data;

                    build.ArtifactList[buildArt.Name] = buildArt;
                }
            }

            return pipeline;
        }
    }
}
