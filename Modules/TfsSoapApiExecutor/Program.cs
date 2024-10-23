using LoggingLibInterface;
using System;
using System.Threading;

namespace TfsSoapApiExecutor
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Feedback.CreateConsoleLog();
            Feedback.CreateLogFile("TfsSoapApiExecutor.log");

            IPCServer server = new IPCServer();
            server.Start();
            Console.WriteLine("Press Ctrl+Q to stop the server.");

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Feedback.LogError("Unhandled exception: " + e.ExceptionObject.ToString());
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

            Feedback.Close();

            return;
        }
    }
}
