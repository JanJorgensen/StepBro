using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core.Data;
using StepBro.Core.Parser;
using StepBro.Core.ScriptData;
using StepBro.Core.Execution;
using StepBro.Core.Logging;
using StepBroCoreTest.Utils;

namespace StepBroCoreTest
{
    [TestClass]
    public class TestTestStep
    {
        [TestMethod]
        public void TestStepsIndexNoTitle()
        {
            var taskContext = ExecutionHelper.ExeContext();
            var procedure = this.CreateTestFile("Anders");
            taskContext.CallProcedure(procedure);
            var log = new LogInspector(taskContext.Logger);
            log.ExpectNext("0 - Pre - TestRun - Starting");
            log.ExpectNext("1 - Pre - MyFile.Anders - <no arguments>");
            log.ExpectNext("2 - Normal - 4 - STEP #1");
            log.ExpectNext("2 - Normal - 5 - STEP #2");
            log.ExpectNext("2 - Normal - 6 - STEP #3");
            log.ExpectNext("2 - Post");
            log.ExpectEnd();
        }

        [TestMethod]
        public void TestStepsTitleNoIndex()
        {
            var taskContext = ExecutionHelper.ExeContext();
            var procedure = this.CreateTestFile("Bent");
            taskContext.CallProcedure(procedure);
            var log = new LogInspector(taskContext.Logger);
            log.ExpectNext("0 - Pre - TestRun - Starting");
            log.ExpectNext("1 - Pre - MyFile.Bent - <no arguments>");
            log.ExpectNext("2 - Normal - 10 - STEP #1 : First");
            log.ExpectNext("2 - Normal - 11 - STEP #2 : Second");
            log.ExpectNext("2 - Normal - 12 - STEP #3 : Third");
            log.ExpectNext("2 - Post");
            log.ExpectEnd();
        }

        [TestMethod]
        public void TestStepsIndexAndTitle()
        {
            var taskContext = ExecutionHelper.ExeContext();
            var procedure = this.CreateTestFile("Christian");
            taskContext.CallProcedure(procedure);
            var log = new LogInspector(taskContext.Logger);
            log.ExpectNext("0 - Pre - TestRun - Starting");
            log.ExpectNext("1 - Pre - MyFile.Christian - <no arguments>");
            log.ExpectNext("2 - Normal - 16 - STEP #1 : First");
            log.ExpectNext("2 - Normal - 17 - STEP #2 : Second");
            log.ExpectNext("2 - Normal - 18 - STEP #3 : Third");
            log.ExpectNext("2 - Post");
            log.ExpectEnd();
        }

        internal IFileProcedure CreateTestFile(string procedureName)
        {
            var f = new StringBuilder();
            f.AppendLine("namespace MyFile;");
            f.AppendLine("procedure void Anders()");
            f.AppendLine("{");
            f.AppendLine("   step 1;");
            f.AppendLine("   step 2;");
            f.AppendLine("   step 3;");
            f.AppendLine("}");
            f.AppendLine("procedure void Bent()");
            f.AppendLine("{");
            f.AppendLine("   step \"First\";");
            f.AppendLine("   step \"Second\";");
            f.AppendLine("   step \"Third\";");
            f.AppendLine("}");
            f.AppendLine("procedure void Christian()");
            f.AppendLine("{");
            f.AppendLine("   step 1, \"First\";");
            f.AppendLine("   step 2, \"Second\";");
            f.AppendLine("   step 3, \"Third\";");
            f.AppendLine("}");
            var file = FileBuilder.ParseFile(null, f.ToString());
            Assert.AreEqual(3, file.ListElements().Where(e => e.ElementType == FileElementType.ProcedureDeclaration).Count());
            var procedure = file.ListElements().First(p => p.Name == procedureName) as IFileProcedure;
            Assert.AreEqual(procedureName, procedure.Name);
            return procedure;
        }
    }
}