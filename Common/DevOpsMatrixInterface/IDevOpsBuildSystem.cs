
namespace DevOpsMatrix.Interface
{
    public interface IDevOpsBuildSystem : IDevOpsService
    {
        IDevOpsPipeline GetPipeline(string Name);
    }
}
