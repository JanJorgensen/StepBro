using StepBro.Core.General;
using StepBro.HostSupport.Models;
using System.Linq;
using StepBroMain = StepBro.Core.Main;

namespace StepBro.HostSupport.Test
{
    [TestClass]
    public class TestFileLoading
    {
        [TestCleanup]
        public void Cleanup()
        {
            StepBroMain.DeinitializeInternal(true);
        }

        [TestMethod]
        public void TestInitialization()
        {
            var hostAppModel = new HostAppModel();
            hostAppModel.Initialize();
            Assert.AreEqual(0, hostAppModel.Views.Count);
        }

        //[TestMethod]
        //public void LoadOneScriptfile()
        //{
        //    var hostAppModel = new HostAppModel();
        //    hostAppModel.Initialize();
        //    Assert.AreEqual(0, hostAppModel.Views.Count);

        //    var file = hostAppModel.LoadScriptFile(@"C:\SW_development\Private\StepBro\examples\scripts\Demo Procedure.sbs");
        //    Assert.AreEqual(1, hostAppModel.Views.Count);
        //    Assert.IsTrue(hostAppModel.Views[0].IsOpen);
        //}

        //[TestMethod]
        //public void LoadOneScriptfileBehind()
        //{
        //    var hostAppModel = new HostAppModel();
        //    hostAppModel.Initialize();
        //    Assert.AreEqual(0, hostAppModel.Views.Count);

        //    var file = StepBroMain.LoadScriptFile(new object(), @"C:\SW_development\Private\StepBro\examples\scripts\Demo Procedure.sbs");
        //    Assert.AreEqual(1, hostAppModel.Views.Count);
        //    Assert.IsFalse(hostAppModel.Views[0].IsOpen);
        //}

        //[TestMethod]
        //public void LoadOneScriptfileDeleteDocItem()
        //{
        //    var hostAppModel = new HostAppModel();
        //    hostAppModel.Initialize();
        //    Assert.AreEqual(0, hostAppModel.Views.Count);

        //    var file = hostAppModel.LoadScriptFile(@"C:\SW_development\Private\StepBro\examples\scripts\Demo Procedure.sbs");
        //    Assert.AreEqual(1, hostAppModel.Views.Count);

        //    hostAppModel.Views.RemoveAt(0);    // What happens when user closes the document view.
        //    Assert.AreEqual(0, hostAppModel.Views.Count);
        //    Assert.AreEqual(0, StepBroMain.GetLoadedFilesManager().ListFiles<ILoadedFile>().ToList().Count());
        //}
    }
}
