
namespace DevOpsMatrix.Interface
{
    public interface IDevOpsBuildSystem : IDevOpsService
    {
        IDevOpsPipeline GetPipeline(string Name, bool includeArtifacts = false, bool includeLogs = false);
        List<IDevOpsPipeline> GetPipelineList(string Name);

        IDevOpsPipelineBuild GetPipelineBuild(int buildId, bool includeArtifacts = false, bool includeLogs = false);
        void LoadPipelineBuildArtifacts(IDevOpsPipelineBuild build);
        void LoadPipelineBuildLogs(IDevOpsPipelineBuild build);

        IDevOpsPipelineBuild LaunchPipelineBuild(IDevOpsPipeline pipeline);
    }
}
