
namespace DevOpsInterface
{
    public interface IDevOpsBuildSystem : IDevOpsService
    {
        IDevOpsPipeline GetPipeline(string Name);
    }
}
