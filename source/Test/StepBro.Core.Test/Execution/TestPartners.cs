using System;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core.Logging;
using StepBro.Core.Parser;
using StepBro.Core.ScriptData;
using StepBroCoreTest.Utils;

namespace StepBroCoreTest.Execution
{
    [TestClass]
    public class TestPartners
    {
        [TestMethod]
        public void TestProcedurePartnerDirectCall()
        {
            var f = new StringBuilder();
            f.AppendLine("int ParkProc() { return 1582; }");
            f.AppendLine("function int Anton() :");
            f.AppendLine("    partner Park : ParkProc;");
            f.AppendLine("int Moskus() {");
            f.AppendLine("   int n = 0;");
            f.AppendLine("   n = Anton.Park();");
            f.AppendLine("   return n;");
            f.AppendLine("}");
            var file = FileBuilder.ParseFile(null, f.ToString());
            var procedure = file["Moskus"] as IFileProcedure;

            var taskContext = ExecutionHelper.ExeContext();
            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(1582, (long)result);
        }

        [TestMethod]
        public void TestTestListPartnerDirectCall()
        {
            var f = new StringBuilder();
            f.AppendLine("int ParkProc() { return 752; }");
            f.AppendLine("testlist Anton :");
            f.AppendLine("    partner Park : ParkProc;");
            f.AppendLine("int Moskus() {");
            f.AppendLine("   int n = 0;");
            f.AppendLine("   n = Anton.Park();");
            f.AppendLine("   return n;");
            f.AppendLine("}");
            var file = FileBuilder.ParseFile(null, f.ToString());
            var procedure = file["Moskus"] as IFileProcedure;

            var taskContext = ExecutionHelper.ExeContext();
            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(752, (long)result);
        }

        [TestMethod]
        public void TestProcedurePartnerOverrideCall()
        {
            var f = new StringBuilder();
            f.AppendLine("int ParkProc1() { return 7722; }");
            f.AppendLine("int ParkProc2() { return 6633; }");
            f.AppendLine("function int Anton() :");
            f.AppendLine("    partner Park : ParkProc1;");
            f.AppendLine("function int Benny() : Anton,");
            f.AppendLine("    partner override Park : ParkProc2;");
            f.AppendLine("int Moskus() {");
            f.AppendLine("   int n = 0;");
            f.AppendLine("   n = Benny.Park();");
            f.AppendLine("   return n;");
            f.AppendLine("}");
            var file = FileBuilder.ParseFile(null, f.ToString());
            var procedure = file["Moskus"] as IFileProcedure;

            var taskContext = ExecutionHelper.ExeContext();
            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(6633, (long)result);
        }

        [TestMethod]
        public void TestTestListPartnerOverrideCall()
        {
            var f = new StringBuilder();
            f.AppendLine("int ParkProc1() { return 543; }");
            f.AppendLine("int ParkProc2() { return 432; }");
            f.AppendLine("testlist Anton :");
            f.AppendLine("    partner Park : ParkProc1;");
            f.AppendLine("testlist Benny : Anton,");
            f.AppendLine("    partner override Park : ParkProc2;");
            f.AppendLine("int Moskus() {");
            f.AppendLine("   int n = 0;");
            f.AppendLine("   n = Benny.Park();");
            f.AppendLine("   return n;");
            f.AppendLine("}");
            var file = FileBuilder.ParseFile(null, f.ToString());
            var procedure = file["Moskus"] as IFileProcedure;

            var taskContext = ExecutionHelper.ExeContext();
            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(432, (long)result);
        }

        [TestMethod]
        public void TestProcedureReferencePartnerOverrideCall()
        {
            var f = new StringBuilder();
            f.AppendLine("int ParkProc1() { return 7722; }");
            f.AppendLine("int ParkProc2() { return 6633; }");
            f.AppendLine("function int Anton() :");
            f.AppendLine("    partner Park : ParkProc1;");
            f.AppendLine("function int Benny() : Anton,");
            f.AppendLine("    partner override Park : ParkProc2;");
            f.AppendLine("int Moskus() {");
            f.AppendLine("   Anton proc = Benny;");
            f.AppendLine("   int n = 0;");
            f.AppendLine("   n = proc.Park();");
            f.AppendLine("   return n;");
            f.AppendLine("}");
            var file = FileBuilder.ParseFile(null, f.ToString());
            var procedure = file["Moskus"] as IFileProcedure;

            var taskContext = ExecutionHelper.ExeContext();
            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(6633, (long)result);
        }

        [TestMethod]
        public void TestTestListReferencePartnerOverrideCall()
        {
            var f = new StringBuilder();
            f.AppendLine("int ParkProc1() { return 9992; }");
            f.AppendLine("int ParkProc2() { return 2229; }");
            f.AppendLine("testlist Anton :");
            f.AppendLine("    partner Park : ParkProc1;");
            f.AppendLine("testlist Benny : Anton,");
            f.AppendLine("    partner override Park : ParkProc2;");
            f.AppendLine("int Moskus() {");
            f.AppendLine("   Anton list = Benny;");
            f.AppendLine("   int n = 0;");
            f.AppendLine("   n = list.Park();");
            f.AppendLine("   return n;");
            f.AppendLine("}");
            var file = FileBuilder.ParseFile(null, f.ToString());
            var procedure = file["Moskus"] as IFileProcedure;

            var taskContext = ExecutionHelper.ExeContext();
            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(2229, (long)result);
        }

        [TestMethod]
        public void TestProcedureOverridePartnerInBase()
        {
            var f1 = new StringBuilder();
            f1.AppendLine("using @\"basefile.sbs\";");
            f1.AppendLine("override TestCaseBase {");
            f1.AppendLine("    partner override Park : ParkProc }");
            f1.AppendLine("procedure void ParkProc()");
            f1.AppendLine("{");
            f1.AppendLine("   log(\"Park\");");
            f1.AppendLine("}");
            f1.AppendLine("public procedure void Run()");
            f1.AppendLine("{");
            f1.AppendLine("   CallParkOnTestCaseBase();");
            f1.AppendLine("}");

            var f2 = new StringBuilder();
            f2.AppendLine("procedure void TestCaseBase() :");
            f2.AppendLine("    partner Park : ParkProcEmpty;");
            f2.AppendLine("procedure void ParkProcEmpty()");
            f2.AppendLine("{");
            f2.AppendLine("}");
            f2.AppendLine("public procedure void CallParkOnTestCaseBase()");
            f2.AppendLine("{");
            f2.AppendLine("   TestCaseBase.Park();");
            f2.AppendLine("}");

            var files = FileBuilder.ParseFiles((ILogger)null,
                new Tuple<string, string>("topfile.sbs", f1.ToString()),
                new Tuple<string, string>("basefile.sbs", f2.ToString()));

            Assert.AreEqual(2, files.Length);
            var file1 = files[0];
            var file2 = files[1];
            Assert.AreEqual(0, file1.Errors.ErrorCount);
            Assert.AreEqual(0, file2.Errors.ErrorCount);
            Assert.AreEqual(2, file1.ListElements().Where(e => e.ElementType == FileElementType.ProcedureDeclaration).Count());
            Assert.AreEqual(3, file2.ListElements().Where(e => e.ElementType == FileElementType.ProcedureDeclaration).Count());

            var procTestCaseBase = file2.ListElements().First(p => p.Name == "TestCaseBase") as IFileProcedure;
            Assert.IsNotNull(procTestCaseBase);

            var procTestCaseBaseOverride = file1.ListElements().First(p => p.Name == "TestCaseBase") as IFileElement;
            Assert.IsNotNull(procTestCaseBaseOverride);
            Assert.IsTrue(procTestCaseBaseOverride.ElementType == FileElementType.Override);
            Assert.AreSame(procTestCaseBase, procTestCaseBaseOverride.BaseElement);

            var procParkProc = file1.ListElements().First(p => p.Name == "ParkProc") as IFileProcedure;
            Assert.IsNotNull(procParkProc);
            var procParkProcEmpty = file2.ListElements().First(p => p.Name == "ParkProcEmpty") as IFileProcedure;
            Assert.IsNotNull(procParkProcEmpty);

            var partnerOverridden = procTestCaseBase.ListPartners().First();
            Assert.IsNotNull(partnerOverridden);
            Assert.AreSame(procParkProc, partnerOverridden.ProcedureReference);

            var procRun = file1.ListElements().First(p => p.Name == "Run") as IFileProcedure;
            Assert.IsNotNull(procRun);

            Exception exeException = null;
            var taskContext = ExecutionHelper.ExeContext(services: FileBuilder.LastServiceManager.Manager);
            try
            {
                taskContext.CallProcedure(procRun);
            }
            catch (Exception ex)
            {
                exeException = ex;
            }
            var log = new LogInspector(taskContext.Logger);
            log.DebugDump();
            if (exeException != null) throw exeException;

            log.ExpectNext("0 - Pre - TestRun - Starting");
            log.ExpectNext("1 - Pre - topfile.Run - <no arguments>");
            log.ExpectNext("2 - Pre - basefile.CallParkOnTestCaseBase - <no arguments>");
            log.ExpectNext("3 - Pre - topfile.ParkProc - <no arguments>");
            log.ExpectNext("4 - Normal - 6 Log - Park");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - Post");
            log.ExpectEnd();
        }

        [TestMethod]
        public void TestProcedureOverridePartnerInBase02()
        {
            var f1 = new StringBuilder();
            f1.AppendLine("using @\"basefile.sbs\";");
            f1.AppendLine("override TestCaseBase");
            f1.AppendLine("{");
            f1.AppendLine("    partner override Park : ParkProc");
            f1.AppendLine("}");
            f1.AppendLine("procedure void ParkProc(this TestCaseBase caseBase)");
            f1.AppendLine("{");
            f1.AppendLine("   log(\"Park\");");
            f1.AppendLine("}");
            f1.AppendLine("public procedure void Run()");
            f1.AppendLine("{");
            f1.AppendLine("   CallParkOnTestCaseBase();");
            f1.AppendLine("}");

            var f2 = new StringBuilder();
            f2.AppendLine("procedure void TestCaseBase() :");
            f2.AppendLine("    partner Park : ParkProcEmpty;");
            f2.AppendLine("procedure void ParkProcEmpty(this TestCaseBase caseBase)");
            f2.AppendLine("{");
            f2.AppendLine("}");
            f2.AppendLine("public procedure void CallParkOnTestCaseBase()");
            f2.AppendLine("{");
            f2.AppendLine("   TestCaseBase.Park();");
            f2.AppendLine("}");

            var files = FileBuilder.ParseFiles((ILogger)null,
                new Tuple<string, string>("topfile.sbs", f1.ToString()),
                new Tuple<string, string>("basefile.sbs", f2.ToString()));

            Assert.AreEqual(2, files.Length);
            var file1 = files[0];
            var file2 = files[1];
            Assert.AreEqual(0, file1.Errors.ErrorCount);
            Assert.AreEqual(0, file2.Errors.ErrorCount);
            Assert.AreEqual(2, file1.ListElements().Where(e => e.ElementType == FileElementType.ProcedureDeclaration).Count());
            Assert.AreEqual(3, file2.ListElements().Where(e => e.ElementType == FileElementType.ProcedureDeclaration).Count());

            var procTestCaseBase = file2.ListElements().First(p => p.Name == "TestCaseBase") as IFileProcedure;
            Assert.IsNotNull(procTestCaseBase);

            var procTestCaseBaseOverride = file1.ListElements().First(p => p.Name == "TestCaseBase") as IFileElement;
            Assert.IsNotNull(procTestCaseBaseOverride);
            Assert.IsTrue(procTestCaseBaseOverride.ElementType == FileElementType.Override);
            Assert.AreSame(procTestCaseBase, procTestCaseBaseOverride.BaseElement);

            var procParkProc = file1.ListElements().First(p => p.Name == "ParkProc") as IFileProcedure;
            Assert.IsNotNull(procParkProc);
            var procParkProcEmpty = file2.ListElements().First(p => p.Name == "ParkProcEmpty") as IFileProcedure;
            Assert.IsNotNull(procParkProcEmpty);

            var partnerOverridden = procTestCaseBase.ListPartners().First();
            Assert.IsNotNull(partnerOverridden);
            Assert.AreSame(procParkProc, partnerOverridden.ProcedureReference);

            var procRun = file1.ListElements().First(p => p.Name == "Run") as IFileProcedure;
            Assert.IsNotNull(procRun);

            Exception exeException = null;
            var taskContext = ExecutionHelper.ExeContext(services: FileBuilder.LastServiceManager.Manager);
            try
            {
                taskContext.CallProcedure(procRun);
            }
            catch (Exception ex)
            {
                exeException = ex;
            }
            var log = new LogInspector(taskContext.Logger);
            log.DebugDump();
            if (exeException != null) throw exeException;

            log.ExpectNext("0 - Pre - TestRun - Starting");
            log.ExpectNext("1 - Pre - topfile.Run - <no arguments>");
            log.ExpectNext("2 - Pre - basefile.CallParkOnTestCaseBase - <no arguments>");
            log.ExpectNext("3 - Pre - topfile.ParkProc - <no arguments>");
            log.ExpectNext("4 - Normal - 8 Log - Park");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - Post");
            log.ExpectEnd();
        }
    }
}
