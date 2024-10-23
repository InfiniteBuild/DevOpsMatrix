using Lumberjack.Interface;
using System;
using System.Threading;

namespace DevOpsMatrix.Tfs.Soap.ApiExecutor
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Logging48.CreateConsoleLog();
            Logging48.CreateLogFile("TfsSoapApiExecutor.log");

            IPCServer server = new IPCServer();
            server.Start();
            Console.WriteLine("Press Ctrl+Q to stop the server.");

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Logging48.LogError("Unhandled exception: " + e.ExceptionObject.ToString());
            };

            while (server.IsRunning)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    if (key.Modifiers == ConsoleModifiers.Control && key.Key == ConsoleKey.Q)
                    {
                        server.Stop();
                        break;
                    }
                }
                Thread.Sleep(500);
            }

            Logging48.Close();

            return;
        }
    }
}
