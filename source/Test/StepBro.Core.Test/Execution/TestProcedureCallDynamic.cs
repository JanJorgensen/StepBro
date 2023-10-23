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
    public class TestProcedureCallDynamic
    {
        [TestMethod]
        public void TestSimpleCallsNoParamsNoReturn()
        {
            var taskContext = ExecutionHelper.ExeContext();

            var procedure = this.CreateTestFile("Bent");
            taskContext.CallProcedure(procedure);
            var log = new LogInspector(taskContext.Logger);
            log.DebugDump();
            log.ExpectNext("0 - Pre - TestRun - Starting");
            log.ExpectNext("1 - Pre - MyFile.Bent - <no arguments>");
            log.ExpectNext("2 - Normal - 6 Log - This is Bent");
            log.ExpectNext("2 - Pre - 8 <DYNAMIC CALL> MyFile.Anders - <no arguments>");
            log.ExpectNext("3 - Normal - 3 Log - This is Anders");
            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - Post");
            log.ExpectEnd();
        }


        internal IFileProcedure CreateTestFile(string procedureName)
        {
            var f = new StringBuilder();
            f.AppendLine("namespace MyFile;");
            f.AppendLine("procedure void Anders() {");
            f.AppendLine("   log (\"This is Anders\");");
            f.AppendLine("}");
            f.AppendLine("procedure void Bent() {");
            f.AppendLine("   log (\"This is Bent\");");
            f.AppendLine("   procedure proc = Anders;");
            f.AppendLine("   proc();");
            f.AppendLine("}");
            var file = FileBuilder.ParseFile(null, f.ToString());
            Assert.AreEqual(2, file.ListElements().Where(e => e.ElementType == FileElementType.ProcedureDeclaration).Count());
            var procedure = file.ListElements().First(p => p.Name == procedureName) as IFileProcedure;
            Assert.AreEqual(procedureName, procedure.Name);
            return procedure;
        }
    }
}