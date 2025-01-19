using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using StepBro.Core;
using StepBro.Core.Api;
using StepBro.Core.General;
using StepBro.Core.Logging;
using StepBro.Core.Parser;
using StepBro.Core.ScriptData;
using StepBro.Core.Test.Mocks;
using StepBro.HostSupport.Models;
using StepBroMain = StepBro.Core.Main;

namespace StepBro.HostSupport.Test
{
    [TestClass]
    public class TestToolsInteractionModel
    {
        HostAppModel m_appModel = null;
        TextFileSystemMock m_textFileSystem = null;
        HostAccess m_host = null;
        StepBroCoreTest.Data.DummyClass m_myScriptTool = null;

        [TestInitialize]
        public void Setup()
        {
            IService testFileSystemService = null;
            m_textFileSystem = new TextFileSystemMock(out testFileSystemService);
            m_appModel = new HostAppModel();
            IService hostAccessService = null;
            m_host = new HostAccess(m_appModel, out hostAccessService);

            m_appModel.Initialize(logViewerModel: null, testFileSystemService, hostAccessService);

            var f = new StringBuilder();
            f.AppendLine("using StepBroCoreTest.Data;");
            f.AppendLine("DummyToolX myScriptTool = DummyToolX();");

            f.AppendLine("procedure void Main()");
            f.AppendLine("{");
            f.AppendLine("    myTestDummy.PropBool = true;");
            f.AppendLine("}");
            f.AppendLine("procedure void CommandProcA(this StepBroCoreTest.Data.DummyClass obj)");
            f.AppendLine("{");
            f.AppendLine("    obj.PropInt = 43;");
            f.AppendLine("}");
            f.AppendLine("procedure void CommandProcB(this StepBroCoreTest.Data.DummyToolX obj)");
            f.AppendLine("{");
            f.AppendLine("    obj.IntProp = 124;");
            f.AppendLine("}");
            f.AppendLine("procedure void CommandProcC(this StepBroCoreTest.Data.DummyToolX obj)");
            f.AppendLine("{");
            f.AppendLine("    obj.IntProp = 441;");
            f.AppendLine("}");
            var fileName = m_textFileSystem.AddFile("c:/stepbrotest/myfile.sbs", f.ToString());

            var toolsInteraction = m_appModel.ToolsInteraction;
            toolsInteraction.Synchronize();
            Assert.AreEqual(2, toolsInteraction.SelectableTools.Count);     // Two tools are from Host, and does not require file parsing.
            Assert.AreEqual("myTestDummy", toolsInteraction.SelectableTools.ElementAt(0).PresentationFullName);
            Assert.AreEqual("myTestDummy", toolsInteraction.SelectableTools.ElementAt(0).PresentationName);
            Assert.AreEqual("myTestTool", toolsInteraction.SelectableTools.ElementAt(1).PresentationFullName);
            Assert.AreEqual("myTestTool", toolsInteraction.SelectableTools.ElementAt(1).PresentationName);
        }

        [TestCleanup]
        public void Cleanup()
        {
            StepBroMain.DeinitializeInternal(true);
        }

        private void ParseFiles(int expectedErrorsCount = 0)
        {
            var file = m_appModel.LoadScriptFile("c:/stepbrotest/myfile.sbs");
            Assert.IsTrue(Object.ReferenceEquals(file, m_appModel.LoadedFiles.ListFiles<IScriptFile>().FirstOrDefault()));

            m_appModel.ParseAllFilesCommand.Execute(null);
            var parsingTask = m_appModel.FileParsingTask;
            if (parsingTask != null)
            {
                parsingTask.Wait(TimeSpan.FromSeconds(100));
                System.Threading.Thread.Sleep(10);
            }
            Assert.IsNull(m_appModel.FileParsingTask);
            Assert.AreEqual(expectedErrorsCount, file.Errors.ErrorCount);
        }

        [TestMethod]
        public void TestInitialization()
        {
            var toolsInteraction = m_appModel.ToolsInteraction;

            var selected = toolsInteraction.SelectedTool;
            Assert.IsNotNull(selected);
            Assert.IsNotNull(selected.ToolContainer.Object);
            Assert.AreSame(selected.ToolContainer.Object, m_host.m_myTestDummy);
            Assert.AreEqual("myTestDummy", selected.PresentationFullName);
            Assert.AreEqual("myTestDummy", selected.PresentationName);
            Assert.IsTrue(selected.HasTextCommandInput);
            m_host.m_myTestDummy.PropBool = false;
            Assert.IsFalse(selected.IsReadyForTextCommand);
            m_host.m_myTestDummy.PropBool = true;
            Assert.IsTrue(selected.IsReadyForTextCommand);
            Assert.AreEqual(0, toolsInteraction.ListActivatableToolProcedures(selected).Count()); // Reason: file has not been parsed.
        }

        [TestMethod]
        public void TestSelectableTools()
        {
            this.ParseFiles();

            var toolsInteraction = m_appModel.ToolsInteraction;
            toolsInteraction.Synchronize();
            Assert.AreEqual(3, toolsInteraction.SelectableTools.Count);
            Assert.AreEqual("myTestDummy", toolsInteraction.SelectableTools.ElementAt(0).PresentationFullName);
            Assert.AreEqual("myTestDummy", toolsInteraction.SelectableTools.ElementAt(0).PresentationName);
            Assert.AreEqual("myTestTool", toolsInteraction.SelectableTools.ElementAt(1).PresentationFullName);
            Assert.AreEqual("myTestTool", toolsInteraction.SelectableTools.ElementAt(1).PresentationName);
            Assert.AreEqual("myfile.myScriptTool", toolsInteraction.SelectableTools.ElementAt(2).PresentationFullName);
            Assert.AreEqual("myScriptTool", toolsInteraction.SelectableTools.ElementAt(2).PresentationName);
        }

        [TestMethod]
        public void TestActivatableProcedures()
        {
            this.ParseFiles();  // Parse files to make procedures available.
            var toolsInteraction = m_appModel.ToolsInteraction;
            toolsInteraction.Synchronize();
            Assert.AreEqual(3, toolsInteraction.SelectableTools.Count);

            var tool = toolsInteraction.SelectableTools.ElementAt(0);
            var procedures = toolsInteraction.ListActivatableToolProcedures(tool).ToArray();
            Assert.AreEqual(1, procedures.Length);
            Assert.AreEqual("CommandProcA", procedures[0].Name);

            tool = toolsInteraction.SelectableTools.ElementAt(1);
            procedures = toolsInteraction.ListActivatableToolProcedures(tool).ToArray();
            Assert.AreEqual(2, procedures.Length);
            Assert.AreEqual("CommandProcB", procedures[0].Name);
            Assert.AreEqual("CommandProcC", procedures[1].Name);


        }
    }
}
