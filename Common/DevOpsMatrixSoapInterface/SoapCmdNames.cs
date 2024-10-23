using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevOpsSoapInterface
{
    public class SoapCmdNames
    {
        // System Commands
        public static string Quit = "quit";
        public static string Ping = "ping";

        // Soap Commands
        public static string DevOpsSettings = "DevOpsSettings";

        public static string Checkin = "Checkin";
        public static string Undo = "Undo";

        public static string AddFile = "AddFile";
        public static string DeleteFile = "DeleteFile";
        public static string RenameFile = "RenameFile";
        public static string UpdateFile = "UpdateFile";
        public static string UndeleteFile = "UndeleteFile"; 

    }
}
