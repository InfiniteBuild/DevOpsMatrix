
namespace DevOpsMatrix.Interface
{
    public interface IDevOpsBuildSystem : IDevOpsService
    {
        IDevOpsPipeline GetPipeline(string Name);
        List<IDevOpsPipeline> GetPipelineList(string Name);

        IDevOpsPipelineBuild GetPipelineBuild(int buildId);
        Dictionary<string, IBuildArtifact> GetPipelineBuildArtifacts(IDevOpsPipelineBuild build);
        List<IBuildStep> GetPipelineBuildLogs(IDevOpsPipelineBuild build);

        IDevOpsPipelineBuild LaunchPipelineBuild(IDevOpsPipeline pipeline);
        IDevOpsPipelineBuild LaunchPipelineBuild(IDevOpsPipeline pipeline, Dictionary<string, string>? variableList = null, Dictionary<string, string>? demandList = null);
    }
}
