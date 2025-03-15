
namespace DevOpsMatrix.Interface
{
    public interface IDevOpsPipeline
    {
        int Id { get; }
        string Name { get; }

        Dictionary<string, string> PipelineVariables { get; }

        Dictionary<int, IDevOpsPipelineBuild> BuildList { get; }

        IDevOpsPipelineBuild? GetLatestBuild();
    }
}
