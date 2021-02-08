using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core.General;
using StepBro.Workbench;
using System;
using System.Collections.Generic;
using System.Linq;
using StepBroMain = StepBro.Core.Main;

namespace StepBroWorkbenchTest
{
    [TestClass]
    public class TestFileParsing
    {
        List<string> s_propertyChangedEvents = new List<string>();

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

            var file = mainViewModel.LoadScriptFile(@"C:\SW_development\Private\StepBro\examples\scripts\Demo Procedure.sbs");
            Assert.AreEqual(1, mainViewModel.DocumentItems.Count);
            Assert.IsTrue(mainViewModel.DocumentItems[0].IsOpen);

            Assert.IsTrue(mainViewModel.ParseAllFilesCommand.CanExecute(null));

            mainViewModel.PropertyChanged += MainViewModel_PropertyChanged;
            Assert.IsFalse(mainViewModel.FileParsingRunning);

            var timeBeforeParsing = DateTime.Now;
            mainViewModel.ParseAllFilesCommand.Execute(null);
            Assert.IsTrue(mainViewModel.FileParsingRunning);
            Assert.IsTrue(s_propertyChangedEvents.Contains("FileParsingRunning"));
            s_propertyChangedEvents.Clear();

            var timeoutExpiry = timeBeforeParsing + TimeSpan.FromSeconds(10);
            while (mainViewModel.FileParsingRunning && DateTime.Now < timeoutExpiry)
            {
                System.Threading.Thread.Sleep(20);
            }
            Assert.IsFalse(mainViewModel.FileParsingRunning);
            Assert.IsTrue(s_propertyChangedEvents.Contains("FileParsingRunning"));
            s_propertyChangedEvents.Clear();
        }

        private void MainViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            s_propertyChangedEvents.Add(e.PropertyName);
        }
    }
}
