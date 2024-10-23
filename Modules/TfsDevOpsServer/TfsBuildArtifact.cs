using DevOpsMatrix.Interface;

namespace DevOpsMatrix.Tfs.Server
{
    public class TfsBuildArtifact : IBuildArtifact
    {
        public string Name { get; set; } = string.Empty;

        public string ResourceType { get; set; } = string.Empty;

        public string Data { get; set; } = string.Empty;
    }
}
