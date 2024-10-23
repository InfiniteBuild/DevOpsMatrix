using DevOpsMatrix.Interface;

namespace DevOpsMatrix.Tfs.Server
{
    public class TfsBuildStep : IBuildStep
    {
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; 
        public string AgentName { get; set; } = string.Empty;  
        public string Log { get; set; } = string.Empty;
    }
}
