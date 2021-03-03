using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core.Parser;
using StepBro.Core.ScriptData;
using StepBroCoreTest.Utils;
using System.Text;

namespace StepBroCoreTest
{
    [TestClass]
    public class TestProcedure_ErrorHandling
    {
        [TestMethod]
        public void ErrorHandling_Normal()
        {
            // With title and expect is passing
            var f = new StringBuilder();
            f.AppendLine("bool MyProcedure()");
            f.AppendLine("{");
            f.AppendLine("   expect (4 < 10);");
            f.AppendLine("   expect (7 < 10);");
            f.AppendLine("   expect (10 < 10);");
            f.AppendLine("   expect (9 < 10);");
            f.AppendLine("   return true;");
            f.AppendLine("}");
            var file = FileBuilder.ParseFile(null, f.ToString());
            var procedure = file.GetFileElement<IFileProcedure>("MyProcedure"); ;

            var taskContext = ExecutionHelper.ExeContext();
            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(false, (bool)result);

            var log = new LogInspector(taskContext.Logger);
            log.DebugDump();

            log.ExpectNext("0 - Pre - TestRun - Starting");
            log.ExpectNext("1 - Pre - MyProcedure - <arguments>");
            log.ExpectNext("2 - Normal - 3 - EXPECT: 4<10; Actual: <TRUE>  =>  Pass");
            log.ExpectNext("2 - Normal - 4 - EXPECT: 7<10; Actual: <TRUE>  =>  Pass");
            log.ExpectNext("2 - Error - 5 - EXPECT: 10<10; Actual: <FALSE>  =>  Fail");
            log.ExpectNext("2 - Post");
        }

        [TestMethod]
        public void ErrorHandling_ContinueOnFail()
        {
            // With title and expect is passing
            var f = new StringBuilder();
            f.AppendLine("bool MyProcedure() : ContinueOnFail");
            f.AppendLine("{");
            f.AppendLine("   expect (4 < 10);");
            f.AppendLine("   expect (7 < 10);");
            f.AppendLine("   expect (10 < 10);");
            f.AppendLine("   expect (9 < 10);");
            f.AppendLine("   return true;");
            f.AppendLine("}");
            var file = FileBuilder.ParseFile(null, f.ToString());
            var procedure = file.GetFileElement<IFileProcedure>("MyProcedure"); ;

            var taskContext = ExecutionHelper.ExeContext();
            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(true, (bool)result);

            var log = new LogInspector(taskContext.Logger);
            log.DebugDump();

            log.ExpectNext("0 - Pre - TestRun - Starting");
            log.ExpectNext("1 - Pre - MyProcedure - <arguments>");
            log.ExpectNext("2 - Normal - 3 - EXPECT: 4<10; Actual: <TRUE>  =>  Pass");
            log.ExpectNext("2 - Normal - 4 - EXPECT: 7<10; Actual: <TRUE>  =>  Pass");
            log.ExpectNext("2 - Error - 5 - EXPECT: 10<10; Actual: <FALSE>  =>  Fail");
            log.ExpectNext("2 - Normal - 6 - EXPECT: 9<10; Actual: <TRUE>  =>  Pass");
            log.ExpectNext("2 - Post");
        }

        [TestMethod]
        public void ErrorHandling_SubProcedure_Pass()
        {
            // With title and expect is passing
            var f = new StringBuilder();
            f.AppendLine("bool MySub()");
            f.AppendLine("{");
            f.AppendLine("   log (\"ResultB1: \" + this.Result.Verdict);");
            f.AppendLine("   log (\"ResultB1.HasFails: \" + this.HasFails);");
            f.AppendLine("   log (\"ResultB1.HasErrors: \" + this.HasErrors);");
            f.AppendLine("   expect (4 < 10);");
            f.AppendLine("   log (\"ResultB2: \" + this.Result.Verdict);");
            f.AppendLine("   log (\"ResultB2.HasFails: \" + this.HasFails);");
            f.AppendLine("   log (\"ResultB2.HasErrors: \" + this.HasErrors);");
            f.AppendLine("   return true;");
            f.AppendLine("}");
            f.AppendLine("bool MyProcedure()");
            f.AppendLine("{");
            f.AppendLine("   bool result;");
            f.AppendLine("   log (\"ResultA1: \" + this.Result.Verdict);");
            f.AppendLine("   log (\"ResultA1.HasFails: \" + this.HasFails);");
            f.AppendLine("   log (\"ResultA1.HasErrors: \" + this.HasErrors);");
            f.AppendLine("   result = MySub();");
            f.AppendLine("   log (\"ResultA2: \" + this.Result.Verdict);");
            f.AppendLine("   log (\"ResultA2.HasFails: \" + this.HasFails);");
            f.AppendLine("   log (\"ResultA2.HasErrors: \" + this.HasErrors);");
            f.AppendLine("   return result;");
            f.AppendLine("}");
            var file = FileBuilder.ParseFile(null, f.ToString());
            var procedure = file.GetFileElement<IFileProcedure>("MyProcedure"); ;

            var taskContext = ExecutionHelper.ExeContext();
            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(true, (bool)result);

            var log = new LogInspector(taskContext.Logger);
            log.DebugDump();

            log.ExpectNext("0 - Pre - TestRun - Starting");
            log.ExpectNext("1 - Pre - MyProcedure - <arguments>");
            log.ExpectNext("2 - Normal - 15 - log: ResultA1: Unset");
            log.ExpectNext("2 - Normal - 16 - log: ResultA1.HasFails: False");
            log.ExpectNext("2 - Normal - 17 - log: ResultA1.HasErrors: False");
            log.ExpectNext("2 - Pre - MySub - <arguments>");
            log.ExpectNext("3 - Normal - 3 - log: ResultB1: Unset");
            log.ExpectNext("3 - Normal - 4 - log: ResultB1.HasFails: False");
            log.ExpectNext("3 - Normal - 5 - log: ResultB1.HasErrors: False");
            log.ExpectNext("3 - Normal - 6 - EXPECT: 4<10; Actual: <TRUE>  =>  Pass");
            log.ExpectNext("3 - Normal - 7 - log: ResultB2: Pass");
            log.ExpectNext("3 - Normal - 8 - log: ResultB2.HasFails: False");
            log.ExpectNext("3 - Normal - 9 - log: ResultB2.HasErrors: False");
            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - Normal - 19 - log: ResultA2: Pass");
            log.ExpectNext("2 - Normal - 20 - log: ResultA2.HasFails: False");
            log.ExpectNext("2 - Normal - 21 - log: ResultA2.HasErrors: False");
            log.ExpectNext("2 - Post");
        }

        [TestMethod]
        public void ErrorHandling_SubProcedure_Fail()
        {
            // With title and expect is passing
            var f = new StringBuilder();
            f.AppendLine("bool MySub()");
            f.AppendLine("{");
            f.AppendLine("   log (\"ResultB1: \" + this.Result.Verdict);");
            f.AppendLine("   log (\"ResultB1.HasFails: \" + this.HasFails);");
            f.AppendLine("   log (\"ResultB1.HasErrors: \" + this.HasErrors);");
            f.AppendLine("   expect (11 < 10);");
            f.AppendLine("   log (\"ResultB2: \" + this.Result.Verdict);");
            f.AppendLine("   log (\"ResultB2.HasFails: \" + this.HasFails);");
            f.AppendLine("   log (\"ResultB2.HasErrors: \" + this.HasErrors);");
            f.AppendLine("   return true;");
            f.AppendLine("}");
            f.AppendLine("bool MyProcedure()");
            f.AppendLine("{");
            f.AppendLine("   bool result;");
            f.AppendLine("   log (\"ResultA1: \" + this.Result.Verdict);");
            f.AppendLine("   log (\"ResultA1.HasFails: \" + this.HasFails);");
            f.AppendLine("   log (\"ResultA1.HasErrors: \" + this.HasErrors);");
            f.AppendLine("   result = MySub();");
            f.AppendLine("   log (\"ResultA2: \" + this.Result.Verdict);");
            f.AppendLine("   log (\"ResultA2.HasFails: \" + this.HasFails);");
            f.AppendLine("   log (\"ResultA2.HasErrors: \" + this.HasErrors);");
            f.AppendLine("   return result;");
            f.AppendLine("}");
            var file = FileBuilder.ParseFile(null, f.ToString());
            var procedure = file.GetFileElement<IFileProcedure>("MyProcedure"); ;

            var taskContext = ExecutionHelper.ExeContext();
            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(false, (bool)result);

            var log = new LogInspector(taskContext.Logger);
            log.DebugDump();

            log.ExpectNext("0 - Pre - TestRun - Starting");
            log.ExpectNext("1 - Pre - MyProcedure - <arguments>");
            log.ExpectNext("2 - Normal - 15 - log: ResultA1: Unset");
            log.ExpectNext("2 - Normal - 16 - log: ResultA1.HasFails: False");
            log.ExpectNext("2 - Normal - 17 - log: ResultA1.HasErrors: False");
            log.ExpectNext("2 - Pre - MySub - <arguments>");
            log.ExpectNext("3 - Normal - 3 - log: ResultB1: Unset");
            log.ExpectNext("3 - Normal - 4 - log: ResultB1.HasFails: False");
            log.ExpectNext("3 - Normal - 5 - log: ResultB1.HasErrors: False");
            log.ExpectNext("3 - Error - 6 - EXPECT: 11<10; Actual: <FALSE>  =>  Fail");
            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - Post");
        }
    }
}