using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core;
using StepBro.Core.Data;
using StepBro.Core.Execution;
using StepBro.Core.Logging;
using StepBro.Core.Parser;
using StepBro.Core.ScriptData;
using StepBroCoreTest.Data;
using StepBroCoreTest.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
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
            log.ExpectNext("1 - Pre - MyProcedure - <no arguments>");
            log.ExpectNext("2 - Normal - 3 - EXPECT: 4 < 10; Value: 4, Verdict: Pass");
            log.ExpectNext("2 - Normal - 4 - EXPECT: 7 < 10; Value: 7, Verdict: Pass");
            log.ExpectNext("2 - Error - 5 - EXPECT: 10 < 10; Value: 10, Verdict: Fail");
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
            log.ExpectNext("1 - Pre - MyProcedure - <no arguments>");
            log.ExpectNext("2 - Normal - 3 - EXPECT: 4 < 10; Value: 4, Verdict: Pass");
            log.ExpectNext("2 - Normal - 4 - EXPECT: 7 < 10; Value: 7, Verdict: Pass");
            log.ExpectNext("2 - Error - 5 - EXPECT: 10 < 10; Value: 10, Verdict: Fail");
            log.ExpectNext("2 - Normal - 6 - EXPECT: 9 < 10; Value: 9, Verdict: Pass");
            log.ExpectNext("2 - Post");
        }

        [TestMethod]
        public void ErrorHandling_SubProcedure_Pass()
        {
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
            log.ExpectNext("1 - Pre - MyProcedure - <no arguments>");
            log.ExpectNext("2 - Normal - 15 Log - ResultA1: Unset");
            log.ExpectNext("2 - Normal - 16 Log - ResultA1.HasFails: False");
            log.ExpectNext("2 - Normal - 17 Log - ResultA1.HasErrors: False");
            log.ExpectNext("2 - Pre - 18 MySub - <no arguments>");
            log.ExpectNext("3 - Normal - 3 Log - ResultB1: Unset");
            log.ExpectNext("3 - Normal - 4 Log - ResultB1.HasFails: False");
            log.ExpectNext("3 - Normal - 5 Log - ResultB1.HasErrors: False");
            log.ExpectNext("3 - Normal - 6 - EXPECT: 4 < 10; Value: 4, Verdict: Pass");
            log.ExpectNext("3 - Normal - 7 Log - ResultB2: Pass");
            log.ExpectNext("3 - Normal - 8 Log - ResultB2.HasFails: False");
            log.ExpectNext("3 - Normal - 9 Log - ResultB2.HasErrors: False");
            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - Normal - 19 Log - ResultA2: Pass");
            log.ExpectNext("2 - Normal - 20 Log - ResultA2.HasFails: False");
            log.ExpectNext("2 - Normal - 21 Log - ResultA2.HasErrors: False");
            log.ExpectNext("2 - Post");
        }

        [TestMethod]
        public void ErrorHandling_ReferenceSubProcedure_Pass()
        {
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
            f.AppendLine("   var proc = MySub;");
            f.AppendLine("   result = proc();");
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
            log.ExpectNext("1 - Pre - MyProcedure - <no arguments>");
            log.ExpectNext("2 - Normal - 15 Log - ResultA1: Unset");
            log.ExpectNext("2 - Normal - 16 Log - ResultA1.HasFails: False");
            log.ExpectNext("2 - Normal - 17 Log - ResultA1.HasErrors: False");
            log.ExpectNext("2 - Pre - 19 MySub - <no arguments>");
            log.ExpectNext("3 - Normal - 3 Log - ResultB1: Unset");
            log.ExpectNext("3 - Normal - 4 Log - ResultB1.HasFails: False");
            log.ExpectNext("3 - Normal - 5 Log - ResultB1.HasErrors: False");
            log.ExpectNext("3 - Normal - 6 - EXPECT: 4 < 10; Value: 4, Verdict: Pass");
            log.ExpectNext("3 - Normal - 7 Log - ResultB2: Pass");
            log.ExpectNext("3 - Normal - 8 Log - ResultB2.HasFails: False");
            log.ExpectNext("3 - Normal - 9 Log - ResultB2.HasErrors: False");
            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - Normal - 20 Log - ResultA2: Pass");
            log.ExpectNext("2 - Normal - 21 Log - ResultA2.HasFails: False");
            log.ExpectNext("2 - Normal - 22 Log - ResultA2.HasErrors: False");
            log.ExpectNext("2 - Post");
        }

        [TestMethod]
        public void ErrorHandling_SubProcedure_Fail()
        {
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
            log.ExpectNext("1 - Pre - MyProcedure - <no arguments>");
            log.ExpectNext("2 - Normal - 15 Log - ResultA1: Unset");
            log.ExpectNext("2 - Normal - 16 Log - ResultA1.HasFails: False");
            log.ExpectNext("2 - Normal - 17 Log - ResultA1.HasErrors: False");
            log.ExpectNext("2 - Pre - 18 MySub - <no arguments>");
            log.ExpectNext("3 - Normal - 3 Log - ResultB1: Unset");
            log.ExpectNext("3 - Normal - 4 Log - ResultB1.HasFails: False");
            log.ExpectNext("3 - Normal - 5 Log - ResultB1.HasErrors: False");
            log.ExpectNext("3 - Error - 6 - EXPECT: 11 < 10; Value: 11, Verdict: Fail");
            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - Post");
        }

        [TestMethod]
        public void ErrorHandling_ReferenceSubProcedure_Fail()
        {
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
            f.AppendLine("   var proc = MySub;");
            f.AppendLine("   result = proc();");
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
            log.ExpectNext("1 - Pre - MyProcedure - <no arguments>");
            log.ExpectNext("2 - Normal - 15 Log - ResultA1: Unset");
            log.ExpectNext("2 - Normal - 16 Log - ResultA1.HasFails: False");
            log.ExpectNext("2 - Normal - 17 Log - ResultA1.HasErrors: False");
            log.ExpectNext("2 - Pre - 19 MySub - <no arguments>");
            log.ExpectNext("3 - Normal - 3 Log - ResultB1: Unset");
            log.ExpectNext("3 - Normal - 4 Log - ResultB1.HasFails: False");
            log.ExpectNext("3 - Normal - 5 Log - ResultB1.HasErrors: False");
            log.ExpectNext("3 - Error - 6 - EXPECT: 11 < 10; Value: 11, Verdict: Fail");
            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - Post");
        }

        [TestMethod]
        public void ErrorHandling_SubProcedure_Fail_NoSubResultInheritance()
        {
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
            f.AppendLine("bool MyProcedure() : NoSubResultInheritance");    // Note this.
            f.AppendLine("{");
            f.AppendLine("   bool result;");
            f.AppendLine("   log (\"ResultA1: \" + this.Result.Verdict);");
            f.AppendLine("   log (\"ResultA1.HasFails: \" + this.HasFails);");
            f.AppendLine("   log (\"ResultA1.HasErrors: \" + this.HasErrors);");
            f.AppendLine("   result = MySub();");
            f.AppendLine("   log (\"ResultA2: \" + this.Result.Verdict);");
            f.AppendLine("   log (\"ResultA2.HasFails: \" + this.HasFails);");
            f.AppendLine("   log (\"ResultA2.HasErrors: \" + this.HasErrors);");
            f.AppendLine("   log (\"ResultS1: \" + this.LastCallResult.Verdict);");
            f.AppendLine("   log (\"ResultS1: \" + this.LastCallResult.Reference);");
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
            log.ExpectNext("1 - Pre - MyProcedure - <no arguments>");
            log.ExpectNext("2 - Normal - 15 Log - ResultA1: Unset");
            log.ExpectNext("2 - Normal - 16 Log - ResultA1.HasFails: False");
            log.ExpectNext("2 - Normal - 17 Log - ResultA1.HasErrors: False");
            log.ExpectNext("2 - Pre - 18 MySub - <no arguments>");
            log.ExpectNext("3 - Normal - 3 Log - ResultB1: Unset");
            log.ExpectNext("3 - Normal - 4 Log - ResultB1.HasFails: False");
            log.ExpectNext("3 - Normal - 5 Log - ResultB1.HasErrors: False");
            log.ExpectNext("3 - Error - 6 - EXPECT: 11 < 10; Value: 11, Verdict: Fail");
            log.ExpectNext("3 - Post");
            // Now execution continues in MyProcedure, because its result is not influenced by the the fail in MySub. 
            log.ExpectNext("2 - Normal - 19 Log - ResultA2: Unset");
            log.ExpectNext("2 - Normal - 20 Log - ResultA2.HasFails: False");
            log.ExpectNext("2 - Normal - 21 Log - ResultA2.HasErrors: False");
            log.ExpectNext("2 - Normal - 22 Log - ResultS1: Fail");
            log.ExpectNext("2 - Normal - 23 Log - ResultS1: MySub");
            log.ExpectNext("2 - Post");
        }

        [TestMethod]
        public void ErrorHandling_ReferenceSubProcedure_Fail_NoSubResultInheritance()
        {
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
            f.AppendLine("bool MyProcedure() : NoSubResultInheritance");    // Note this.
            f.AppendLine("{");
            f.AppendLine("   bool result;");
            f.AppendLine("   log (\"ResultA1: \" + this.Result.Verdict);");
            f.AppendLine("   log (\"ResultA1.HasFails: \" + this.HasFails);");
            f.AppendLine("   log (\"ResultA1.HasErrors: \" + this.HasErrors);");
            f.AppendLine("   var proc = MySub;");
            f.AppendLine("   result = proc();");
            f.AppendLine("   log (\"ResultA2: \" + this.Result.Verdict);");
            f.AppendLine("   log (\"ResultA2.HasFails: \" + this.HasFails);");
            f.AppendLine("   log (\"ResultA2.HasErrors: \" + this.HasErrors);");
            f.AppendLine("   log (\"ResultS1: \" + this.LastCallResult.Verdict);");
            f.AppendLine("   log (\"ResultS1: \" + this.LastCallResult.Reference);");
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
            log.ExpectNext("1 - Pre - MyProcedure - <no arguments>");
            log.ExpectNext("2 - Normal - 15 Log - ResultA1: Unset");
            log.ExpectNext("2 - Normal - 16 Log - ResultA1.HasFails: False");
            log.ExpectNext("2 - Normal - 17 Log - ResultA1.HasErrors: False");
            log.ExpectNext("2 - Pre - 19 MySub - <no arguments>");
            log.ExpectNext("3 - Normal - 3 Log - ResultB1: Unset");
            log.ExpectNext("3 - Normal - 4 Log - ResultB1.HasFails: False");
            log.ExpectNext("3 - Normal - 5 Log - ResultB1.HasErrors: False");
            log.ExpectNext("3 - Error - 6 - EXPECT: 11 < 10; Value: 11, Verdict: Fail");
            log.ExpectNext("3 - Post");
            // Now execution continues in MyProcedure, because its result is not influenced by the the fail in MySub. 
            log.ExpectNext("2 - Normal - 20 Log - ResultA2: Unset");
            log.ExpectNext("2 - Normal - 21 Log - ResultA2.HasFails: False");
            log.ExpectNext("2 - Normal - 22 Log - ResultA2.HasErrors: False");
            log.ExpectNext("2 - Normal - 23 Log - ResultS1: Fail");
            log.ExpectNext("2 - Normal - 24 Log - ResultS1: MySub");
            log.ExpectNext("2 - Post");
        }

        [TestMethod]
        public void ErrorHandling_ObjectMethod_Fail()
        {
            var f = new StringBuilder();
            f.AppendLine("using " + typeof(DummyClass).Namespace + ";");
            f.AppendLine("int MyProc()");
            f.AppendLine("{");
            f.AppendLine("   " + typeof(DummyClass).FullName + "." + nameof(DummyClass.MethodReportingFail) + "(); ");
            f.AppendLine("   log(\"After the failure.\");");  // This should not be executed.
            f.AppendLine("   return 9;");                     // This should not be used; default (0) should be returned
            f.AppendLine("}");

            var file = FileBuilder.ParseFiles((ILogger)null, this.GetType().Assembly, new Tuple<string, string>("myfile." + Main.StepBroFileExtension, f.ToString()))[0];
            var procedure = file.ListElements().First(p => p.Name == "MyProc") as IFileProcedure;

            var taskContext = ExecutionHelper.ExeContext();
            object result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(0L, (long)result);  // 
            var log = new LogInspector(taskContext.Logger);
            log.DebugDump();
            log.ExpectNext("0 - Pre - TestRun - Starting");
            log.ExpectNext("1 - Pre - myfile.MyProc - <no arguments>");
            log.ExpectNext("2 - Error - 4 - <the failure description>");
            log.ExpectNext("2 - Post");
            log.ExpectEnd();
        }

        [TestMethod]
        public void ErrorHandling_ObjectMethod_Error()
        {
            var f = new StringBuilder();
            f.AppendLine("using " + typeof(DummyClass).Namespace + ";");
            f.AppendLine("int MyProc()");
            f.AppendLine("{");
            f.AppendLine("   " + typeof(DummyClass).FullName + "." + nameof(DummyClass.MethodReportingError) + "(); ");
            f.AppendLine("   log(\"After the error.\");");  // This should not be executed.
            f.AppendLine("   return 11;");                  // This should not be used; default (0) should be returned
            f.AppendLine("}");

            var file = FileBuilder.ParseFiles((ILogger)null, this.GetType().Assembly, new Tuple<string, string>("myfile." + Main.StepBroFileExtension, f.ToString()))[0];
            var procedure = file.ListElements().First(p => p.Name == "MyProc") as IFileProcedure;

            var taskContext = ExecutionHelper.ExeContext();
            object result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(0L, (long)result);  // 
            var log = new LogInspector(taskContext.Logger);
            log.DebugDump();
            log.ExpectNext("0 - Pre - TestRun - Starting");
            log.ExpectNext("1 - Pre - myfile.MyProc - <no arguments>");
            log.ExpectNext("2 - Error - 4 - <the error description>");
            log.ExpectNext("2 - Post");
            log.ExpectEnd();
        }


        [TestMethod]
        public void ErrorHandling_ObjectMethod_Exception()
        {
            string fileContent = $$"""
                using {{typeof(DummyClass).Namespace}};
                int MyProc()
                {
                    log("Entered!");
                    var testReport = StartReport("TestReport", "UnitTest");
                    testReport.StartGroup("The Test", "");
                    {{typeof(DummyClass).FullName}}.{{nameof(DummyClass.MethodThrowingException)}}();
                    log("These lines should not be executed.");
                    testReport.Close();
                    return 11;
                }
                """;

            var file = FileBuilder.ParseFiles((ILogger)null, this.GetType().Assembly, new Tuple<string, string>("myfile." + Main.StepBroFileExtension, fileContent))[0];
            var procedure = file.ListElements().First(p => p.Name == "MyProc") as IFileProcedure;

            var taskContext = ExecutionHelper.ExeContext();
            object result = taskContext.CallProcedure(procedure);
            Assert.IsNull(result);  // No result value
            Assert.AreEqual(Verdict.Error, taskContext.Result.Verdict);
            Assert.IsNotNull(taskContext.ExecutionExeception);
            Assert.IsInstanceOfType(taskContext.ExecutionExeception, typeof(UnhandledExceptionInScriptException));
            Assert.IsNotNull(taskContext.ExecutionExeception.InnerException);
            Assert.IsInstanceOfType(taskContext.ExecutionExeception.InnerException, typeof(KeyNotFoundException));
            Assert.IsNotNull(taskContext.Report);
            var reportGroup = taskContext.Report.ListGroups().Last();
            Assert.IsTrue(reportGroup.IsLocked);
            var lastReported = reportGroup.ListData().Last();
            var log = new LogInspector(taskContext.Logger);
            log.DebugDump();
            log.ExpectNext("0 - Pre - TestRun - Starting");
            log.ExpectNext("1 - Pre - myfile.MyProc - <no arguments>");
            log.ExpectNext("2 - Normal - 4 Log - Entered!");
            log.ExpectNext("2 - Normal - 6 DataReport.StartGroup - Starting report group \"The Test\".");
            log.ExpectNext("2 - Error - 7 - Unhandled Exception. KeyNotFoundException - The given key was not present in the dictionary.");
            log.ExpectNext("2 - Post");
            log.ExpectEnd();
        }
    }
}