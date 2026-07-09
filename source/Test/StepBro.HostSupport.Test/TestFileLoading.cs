using StepBro.Core;
using StepBro.Core.General;
using StepBro.HostSupport.Models;
using System.Linq;
using StepBroMain = StepBro.Core.Main;

namespace StepBro.HostSupport.Test
{
    [TestClass]
    public class TestFileLoading
    {
        HostAppModel appModel;
        const int InitialViewCount = 2;

        [TestInitialize]
        public void Setup()
        {
            var logViewerModel = new LogViewerModel(new LogViewTestEntryFactory());
            IService testFileSystemService = null;
            var mockFileSystem = new StepBro.Core.Test.Mocks.TextFileSystemMock(out testFileSystemService);
            mockFileSystem.AddSomeScriptFiles();
            appModel = new HostAppModel();
            IService hostAccessService = null;
            var host = new HostAccess(appModel, out hostAccessService);
            appModel.Initialize(logViewerModel, testFileSystemService, hostAccessService);
            logViewerModel.Setup();
        }

        [TestCleanup]
        public void Cleanup()
        {
            StepBroMain.DeinitializeInternal(true);
        }

        [TestMethod]
        public void TestInitialization()
        {
            Assert.AreEqual(0, appModel.Views.Where(v => v.IsShownInDocuments()).Count());
        }

        [TestMethod]
        public void LoadOneScriptfile()
        {
            Assert.AreEqual(InitialViewCount, appModel.Views.Count);   // Log view and Problems view.

            var file = appModel.LoadScriptFile("c:/examples/scripts/Demo Procedure.sbs");
            Assert.IsNotNull(file);
            Assert.AreEqual(InitialViewCount + 1, appModel.Views.Count);
            Assert.IsTrue(appModel.Views[0].IsOpen);
        }

        [TestMethod]
        public void LoadOneScriptfileBehind()
        {
            //var hostAppModel = new HostAppModel();
            //hostAppModel.Initialize();
            //Assert.AreEqual(0, hostAppModel.Views.Count);

            var file = StepBroMain.LoadScriptFile(new object(), "c:/examples/scripts/Demo Procedure.sbs");
            Assert.IsNotNull(file);
            Assert.AreEqual(InitialViewCount + 1, appModel.Views.Count);
            Assert.IsFalse(appModel.Views[0].IsOpen);
        }

        [TestMethod]
        public void LoadOneScriptfileDeleteDocItem()
        {
            //var hostAppModel = new HostAppModel();
            //hostAppModel.Initialize();
            //Assert.AreEqual(0, hostAppModel.Views.Count);

            var file = appModel.LoadScriptFile("c:/examples/scripts/Demo Procedure.sbs");
            Assert.IsNotNull(file);
            Assert.AreEqual(InitialViewCount + 1, appModel.Views.Count);

            appModel.Views.Remove(appModel.Views.First(v => v.Title == "Demo Procedure.sbs"));    // What happens when user closes the document view.
            Assert.AreEqual(0, appModel.Views.Count);
            Assert.AreEqual(0, StepBroMain.GetLoadedFilesManager().ListFiles<ILoadedFile>().ToList().Count());
        }
    }
}
