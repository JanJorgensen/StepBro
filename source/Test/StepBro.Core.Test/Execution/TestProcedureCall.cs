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
    public class TestProcedureCall
    {
        [TestMethod]
        public void TestFunctionCallInReturnStatement()
        {
            var f = new StringBuilder();
            f.AppendLine("namespace MyFile;");
            f.AppendLine("function int MySub() {");
            f.AppendLine("   return 323;");
            f.AppendLine("}");
            f.AppendLine("int MyProcedure() {");
            f.AppendLine("   return MySub();");
            f.AppendLine("}");
            var file = FileBuilder.ParseFile(null, f.ToString());
            var procedure = file["MyProcedure"] as IFileProcedure;

            var taskContext = ExecutionHelper.ExeContext();
            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(323, (long)result);
        }

        [TestMethod]
        public void TestCallWithReturnValue()
        {
            var f = new StringBuilder();
            f.AppendLine("namespace MyFile;");
            f.AppendLine("int MySub() {");
            f.AppendLine("   return 772;");
            f.AppendLine("}");
            f.AppendLine("int MyProcedure() {");
            f.AppendLine("   int i = 278;");
            f.AppendLine("   i = 631;");
            f.AppendLine("   i = MySub();");
            f.AppendLine("   return i;");
            f.AppendLine("}");
            var file = FileBuilder.ParseFile(null, f.ToString());
            var procedure = file["MyProcedure"] as IFileProcedure;

            var taskContext = ExecutionHelper.ExeContext();
            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(772, (long)result);
        }

        [TestMethod]
        public void TestCallSimpleProcedureReference()
        {
            var f = new StringBuilder();
            f.AppendLine("namespace MyFile;");
            f.AppendLine("function int MySub() {");
            f.AppendLine("   return 104;");
            f.AppendLine("}");
            f.AppendLine("int MyProcedure() {");
            f.AppendLine("   var proc = MySub;");
            f.AppendLine("   var result = proc();");
            f.AppendLine("   return result;");
            f.AppendLine("}");
            var file = FileBuilder.ParseFile(null, f.ToString());
            var procedure = file["MyProcedure"] as IFileProcedure;

            var taskContext = ExecutionHelper.ExeContext();
            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(104, (long)result);
        }

        [TestMethod]
        public void ProcedureCallSimpleNoParamsNoReturn2Levels()
        {
            var taskContext = ExecutionHelper.ExeContext();

            var procedure = this.CreateTestFileNoParams("Bent", "AB");
            taskContext.CallProcedure(procedure);
            var log = new LogInspector(taskContext.Logger);
            log.ExpectNext("0 - Pre - TestRun - Starting");
            log.ExpectNext("1 - Pre - MyFile.Bent - <no arguments>");
            log.ExpectNext("2 - Normal - 6 Log - This is Bent");
            log.ExpectNext("2 - Pre - MyFile.Anders - <no arguments>");
            log.ExpectNext("3 - Normal - 3 Log - This is Anders");
            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - Post");
            log.ExpectEnd();
        }

        [TestMethod]
        public void ProcedureCallSimpleNoParamsNoReturn3Levels()
        {
            var taskContext = ExecutionHelper.ExeContext();

            var procedure = this.CreateTestFileNoParams("Christian", "ABC");
            taskContext.CallProcedure(procedure);
            var log = new LogInspector(taskContext.Logger);
            log.ExpectNext("0 - Pre - TestRun - Starting");
            log.ExpectNext("1 - Pre - MyFile.Christian - <no arguments>");
            log.ExpectNext("2 - Normal - 10 Log - This is Christian");
            log.ExpectNext("2 - Pre - MyFile.Anders - <no arguments>");
            log.ExpectNext("3 - Normal - 3 Log - This is Anders");
            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - Pre - MyFile.Bent - <no arguments>");
            log.ExpectNext("3 - Normal - 6 Log - This is Bent");
            log.ExpectNext("3 - Pre - MyFile.Anders - <no arguments>");
            log.ExpectNext("4 - Normal - 3 Log - This is Anders");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - Post");
            log.ExpectEnd();
        }

        [TestMethod]
        public void ProcedureCallReferenceNoParamsNoReturn()
        {
            var taskContext = ExecutionHelper.ExeContext();

            var procedure = this.CreateTestFileNoParams("Dennis", "AD");
            taskContext.CallProcedure(procedure);
            var log = new LogInspector(taskContext.Logger);
            log.ExpectNext("0 - Pre - TestRun - Starting");
            log.ExpectNext("1 - Pre - MyFile.Dennis - <no arguments>");
            log.ExpectNext("2 - Normal - 6 Log - This is Dennis");
            log.ExpectNext("2 - Pre - MyFile.Anders - <no arguments>");
            log.ExpectNext("3 - Normal - 3 Log - This is Anders");
            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - Post");
            log.ExpectEnd();
        }

        internal IFileProcedure CreateTestFileNoParams(string procedureName, string enabled)
        {
            var f = new StringBuilder();
            f.AppendLine("namespace MyFile;");
            if (enabled.Contains('A'))
            {
                f.AppendLine("procedure void Anders() {");
                f.AppendLine("   log (\"This is Anders\");");
                f.AppendLine("}");
            }
            if (enabled.Contains('B'))
            {
                f.AppendLine("procedure void Bent() {");
                f.AppendLine("   log (\"This is Bent\");");
                f.AppendLine("   Anders();");
                f.AppendLine("}");
            }
            if (enabled.Contains('C'))
            {
                f.AppendLine("procedure void Christian() {");
                f.AppendLine("   log (\"This is Christian\");");
                f.AppendLine("   Anders();");
                f.AppendLine("   Bent();");
                f.AppendLine("}");
            }
            if (enabled.Contains('D'))
            {
                f.AppendLine("procedure void Dennis() {");
                f.AppendLine("   log (\"This is Dennis\");");
                f.AppendLine("   var proc = Anders;");
                f.AppendLine("   proc();");
                f.AppendLine("}");
            }
            var file = FileBuilder.ParseFile(null, f.ToString());
            Assert.AreEqual(enabled.Length, file.ListElements().Where(e => e.ElementType == FileElementType.ProcedureDeclaration).Count());
            var procedure = file.ListElements().First(p => p.Name == procedureName) as IFileProcedure;
            Assert.AreEqual(procedureName, procedure.Name);
            return procedure;
        }
    }
}