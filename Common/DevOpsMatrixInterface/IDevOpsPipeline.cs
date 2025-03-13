
namespace DevOpsMatrix.Interface
{
    public interface IDevOpsPipeline
    {
        int Id { get; }
        string Name { get; }

        Dictionary<int, IDevOpsPipelineBuild> BuildList { get; }

        IDevOpsPipelineBuild? GetLatestBuild();
    }
}
