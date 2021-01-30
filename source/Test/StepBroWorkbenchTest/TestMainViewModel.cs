using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core.General;
using StepBro.Workbench;
using System.Linq;
using StepBroMain = StepBro.Core.Main;

namespace StepBroWorkbenchTest
{
    [TestClass]
    public class TestMainViewModel
    {
        [TestCleanup]
        public void Cleanup()
        {
            StepBroMain.DeinitializeInternal(true);
        }

        [TestMethod]
        public void TestInitialization()
        {
            var mainViewModel = new MainViewModel();
            Assert.AreEqual(0, mainViewModel.DocumentItems.Count);
        }

        [TestMethod]
        public void LoadOneScriptfile()
        {
            var mainViewModel = new MainViewModel();
            Assert.AreEqual(0, mainViewModel.DocumentItems.Count);

            var file = mainViewModel.LoadScriptFile(@"C:\SW_development\Private\StepBro\examples\scripts\Demo Procedure.sbs");
            Assert.AreEqual(1, mainViewModel.DocumentItems.Count);
        }

        [TestMethod]
        public void LoadOneScriptfileBehind()
        {
            var mainViewModel = new MainViewModel();
            Assert.AreEqual(0, mainViewModel.DocumentItems.Count);

            var file = StepBroMain.LoadScriptFile(new object(), @"C:\SW_development\Private\StepBro\examples\scripts\Demo Procedure.sbs");
            Assert.AreEqual(0, mainViewModel.DocumentItems.Count);
        }

        [TestMethod]
        public void LoadOneScriptfileDeleteDocItem()
        {
            var mainViewModel = new MainViewModel();
            Assert.AreEqual(0, mainViewModel.DocumentItems.Count);

            var file = mainViewModel.LoadScriptFile(@"C:\SW_development\Private\StepBro\examples\scripts\Demo Procedure.sbs");
            Assert.AreEqual(1, mainViewModel.DocumentItems.Count);

            mainViewModel.DocumentItems.RemoveAt(0);    // What happens when user closes the document view.
            Assert.AreEqual(0, mainViewModel.DocumentItems.Count);
            Assert.AreEqual(0, StepBroMain.GetLoadedFilesManager().ListFiles<ILoadedFile>().ToList().Count());
        }
    }
}
