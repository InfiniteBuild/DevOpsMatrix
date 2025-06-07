
namespace DevOpsMatrix.Interface
{
    public interface ITfvcSourceControl : ISourceCodeControl
    {
        ISourceCodeHistory GetShelvesetInfo(string shelveset, string owner);
    }
}
