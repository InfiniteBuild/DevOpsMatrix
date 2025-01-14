using DevOpsMatrix.Tfs.Soap.Interface;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace TfsSoapTests
{
    [TestClass]
    public class SoapApiBasicTests
    {
        [TestMethod]
        public void StartAndShutdown()
        {
            using (var executor = new SoapExecutor())
            {
                executor.Startup();
                Assert.AreEqual("running", executor.Status);

                string response = executor.Ping();
                Assert.AreEqual("Pong!", response);

                executor.Shutdown();
                Thread.Sleep(500);

                Assert.AreEqual("stopped", executor.Status);
            }
        }
    }
}
