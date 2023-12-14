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
using StepBro.Core;

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
            log.ExpectNext("2 - Pre - 7 MyFile.Anders - <no arguments>");
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
            log.ExpectNext("2 - Pre - 11 MyFile.Anders - <no arguments>");
            log.ExpectNext("3 - Normal - 3 Log - This is Anders");
            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - Pre - 12 MyFile.Bent - <no arguments>");
            log.ExpectNext("3 - Normal - 6 Log - This is Bent");
            log.ExpectNext("3 - Pre - 7 MyFile.Anders - <no arguments>");
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
            log.ExpectNext("2 - Pre - 8 MyFile.Anders - <no arguments>");
            log.ExpectNext("3 - Normal - 3 Log - This is Anders");
            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - Post");
            log.ExpectEnd();
        }

        [TestMethod]
        public void ProcedureCallSimpleNoParamsNoReturn3Levels_WithHighLevel()
        {
            var taskContext = ExecutionHelper.ExeContext();

            var procedure = this.CreateTestFileNoParams("Christian", "ABC", true);
            taskContext.CallProcedure(procedure);
            var log = new LogInspector(taskContext.Logger);
            log.ExpectNext("0 - Pre - TestRun - Starting");
            log.ExpectNext("1 - Pre - MyFile.Christian - <no arguments>");
            log.ExpectNext("2 - Normal - 10 Log - This is Christian");
            log.ExpectNext("2 - PreHighLevel - 12 MyFile.Anders - TEST - <no arguments>");
            log.ExpectNext("3 - Normal - 3 Log - This is Anders");
            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - PreHighLevel - 14 MyFile.Bent - TEST - <no arguments>");
            log.ExpectNext("3 - Normal - 6 Log - This is Bent");
            log.ExpectNext("3 - Pre - 7 MyFile.Anders - <no arguments>");
            log.ExpectNext("4 - Normal - 3 Log - This is Anders");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - Post");
            log.ExpectEnd();
        }

        [TestMethod]
        public void ProcedureCallReferenceNoParamsNoReturn_WithHighLevel()
        {
            var taskContext = ExecutionHelper.ExeContext();

            var procedure = this.CreateTestFileNoParams("Dennis", "AD", true);
            taskContext.CallProcedure(procedure);
            var log = new LogInspector(taskContext.Logger);
            log.ExpectNext("0 - Pre - TestRun - Starting");
            log.ExpectNext("1 - Pre - MyFile.Dennis - <no arguments>");
            log.ExpectNext("2 - Normal - 6 Log - This is Dennis");
            log.ExpectNext("2 - PreHighLevel - 9 MyFile.Anders - TEST - <no arguments>");
            log.ExpectNext("3 - Normal - 3 Log - This is Anders");
            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - Post");
            log.ExpectEnd();
        }

        internal IFileProcedure CreateTestFileNoParams(string procedureName, string enabled, bool setHighLevelCall = false)
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
                if (setHighLevelCall)
                    f.AppendLine("   NextProcedureIsHighLevel(\"TEST\");");
                f.AppendLine("   Anders();");
                if (setHighLevelCall)
                    f.AppendLine("   NextProcedureIsHighLevel(\"TEST\");");
                f.AppendLine("   Bent();");
                f.AppendLine("}");
            }
            if (enabled.Contains('D'))
            {
                f.AppendLine("procedure void Dennis() {");
                f.AppendLine("   log (\"This is Dennis\");");
                f.AppendLine("   var proc = Anders;");
                if (setHighLevelCall)
                    f.AppendLine("   NextProcedureIsHighLevel(\"TEST\");");
                f.AppendLine("   proc();");
                f.AppendLine("}");
            }
            var file = FileBuilder.ParseFile(null, f.ToString());
            Assert.AreEqual(enabled.Length, file.ListElements().Where(e => e.ElementType == FileElementType.ProcedureDeclaration).Count());
            var procedure = file.ListElements().First(p => p.Name == procedureName) as IFileProcedure;
            Assert.AreEqual(procedureName, procedure.Name);
            return procedure;
        }

        [TestMethod]
        public void ProcedureCallWithDefaultArguments()
        {
            var taskContext = ExecutionHelper.ExeContext();

            var f = new StringBuilder();
            f.AppendLine("namespace MyFile;");
            f.AppendLine("procedure void Bent(int a, string b = \"Wow\", bool c = true, int d = 126)");
            f.AppendLine("{");
            f.AppendLine("   log (\"a: \" + a + \", b: \" + b + \", c: \" + c + \", d: \" + d);");
            f.AppendLine("}");
            f.AppendLine("procedure void Anders()");
            f.AppendLine("{");
            f.AppendLine("   Bent(5, \"Ups\", false, 230);");
            f.AppendLine("   Bent(28);");
            f.AppendLine("   Bent(94, c: false);");
            f.AppendLine("   Bent(113, b: \"Musk\");");
            f.AppendLine("}");

            var file = FileBuilder.ParseFile(null, f.ToString());
            Assert.AreEqual(2, file.ListElements().Count());
            var procedure = file.ListElements().First(p => p.Name == "Anders") as IFileProcedure;
            Assert.IsNotNull(procedure);
            taskContext.CallProcedure(procedure);
            var log = new LogInspector(taskContext.Logger);
            log.DebugDump();

            log.ExpectNext("0 - Pre - TestRun - Starting");
            log.ExpectNext("1 - Pre - MyFile.Anders - <no arguments>");

            log.ExpectNext("2 - Pre - 8 MyFile.Bent - <no arguments>");
            log.ExpectNext("3 - Normal - 4 Log - a: 5, b: Ups, c: False, d: 230");
            log.ExpectNext("3 - Post");
            
            log.ExpectNext("2 - Pre - 9 MyFile.Bent - <no arguments>");
            log.ExpectNext("3 - Normal - 4 Log - a: 28, b: Wow, c: True, d: 126");
            log.ExpectNext("3 - Post");

            log.ExpectNext("2 - Pre - 10 MyFile.Bent - <no arguments>");
            log.ExpectNext("3 - Normal - 4 Log - a: 94, b: Wow, c: False, d: 126");
            log.ExpectNext("3 - Post");

            log.ExpectNext("2 - Pre - 11 MyFile.Bent - <no arguments>");
            log.ExpectNext("3 - Normal - 4 Log - a: 113, b: Musk, c: True, d: 126");
            log.ExpectNext("3 - Post");

            log.ExpectNext("2 - Post");
            log.ExpectEnd();
        }

        [TestMethod]
        public void ProcedureCallThroughPartnerWithDefaultArguments()
        {
            var taskContext = ExecutionHelper.ExeContext();

            var content =
                """
                namespace MyFile;
                procedure void Action() :
                    partner Loop : RunActionInLoop;
                procedure void RunActionInLoop(this Action action, int iterations = 4)
                {
                    int i = 0;
                    while (i < iterations)
                    {
                        action();
                        i++;
                    }
                }
                void Do() : Action
                {
                    log("Blip");
                }
                """;

            var file = FileBuilder.ParseFiles((ILogger)null, new Tuple<string, string>("myfile." + Main.StepBroFileExtension, content))[0];


            Assert.AreEqual(3, file.ListElements().Count());
            var doProc = file.ListElements().First(p => p.Name == "Do") as IFileProcedure;
            Assert.IsNotNull(doProc);

            var partner = doProc.ListPartners().First(p => String.Equals("Loop", p.Name, StringComparison.InvariantCultureIgnoreCase));
            Assert.IsNotNull(partner);
            var procedure = partner.ProcedureReference;

            taskContext.CallProcedure(procedure, doProc.ProcedureReference);
            var log = new LogInspector(taskContext.Logger);
            log.DebugDump();

            log.ExpectNext("0 - Pre - TestRun - Starting");
            log.ExpectNext("1 - Pre - MyFile.RunActionInLoop - ( StepBro.Core.ScriptData.FileProcedure+Reference`1[ret_System_Void] )");
            log.ExpectNext("2 - Pre - 9 MyFile.Do - <no arguments>");
            log.ExpectNext("3 - Normal - 15 Log - Blip");
            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - Pre - 9 MyFile.Do - <no arguments>");
            log.ExpectNext("3 - Normal - 15 Log - Blip");
            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - Pre - 9 MyFile.Do - <no arguments>");
            log.ExpectNext("3 - Normal - 15 Log - Blip");
            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - Pre - 9 MyFile.Do - <no arguments>");
            log.ExpectNext("3 - Normal - 15 Log - Blip");
            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - Post");
            log.ExpectEnd();
        }
    }
}