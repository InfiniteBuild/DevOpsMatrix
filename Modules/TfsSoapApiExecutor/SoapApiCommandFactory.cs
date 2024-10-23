using DevOpsMatrix.Tfs.Soap.Interface;
using System.Collections.Generic;

namespace DevOpsMatrix.Tfs.Soap.ApiExecutor
{
    public delegate SoapApiCmdBase SoapCommandCreator();

    public class SoapApiCommandFactory
    {
        public static SoapApiCommandFactory Instance { get; } = new SoapApiCommandFactory();

        private Dictionary<string, SoapCommandCreator> m_factory = new Dictionary<string, SoapCommandCreator>();

        private SoapApiCommandFactory()
        {
            RegisterInternalCommands();
        }

        public void RegisterCommand(string commandName, SoapCommandCreator creator)
        {
            m_factory[commandName] = creator;
        }

        public void UnregisterCommand(string commandName)
        {
            if (m_factory.ContainsKey(commandName))
                m_factory.Remove(commandName);
        }

        public SoapApiCmdBase CreateCommand(string commandName)
        {
            if (m_factory.ContainsKey(commandName))
            {
                return m_factory[commandName]();
            }

            return null;
        }

        private void RegisterInternalCommands()
        {
            RegisterCommand(SoapCmdNames.DevOpsSettings, () => { return new SoapApiCmdDevOpsSettings(); });

            RegisterCommand(SoapCmdNames.Checkin, () => { return new SoapApiCmdCheckin(); });
            RegisterCommand(SoapCmdNames.Undo, () => { return new SoapApiCmdUndo(); });

            RegisterCommand(SoapCmdNames.AddFile, () => { return new SoapApiCmdAddFile(); });
            RegisterCommand(SoapCmdNames.DeleteFile, () => { return new SoapApiCmdDeleteFile(); });
            RegisterCommand(SoapCmdNames.UpdateFile, () => { return new SoapApiCmdUpdateFile(); });
            RegisterCommand(SoapCmdNames.RenameFile, () => { return new SoapApiCmdRenameFile(); });
            RegisterCommand(SoapCmdNames.UndeleteFile, () => { return new SoapApiCmdUndeleteFile(); });
        }
    }
}
