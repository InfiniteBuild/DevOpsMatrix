
namespace DevOpsMatrix.Interface
{
    public interface IDevOpsPipeline
    {
        string Name { get; }

        Dictionary<int, IDevOpsPipelineBuild> BuildList { get; }

        IDevOpsPipelineBuild? GetLatestBuild();
    }
}
