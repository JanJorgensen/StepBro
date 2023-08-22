using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core;
using StepBro.Core.Logging;
using StepBro.Core.Parser;
using StepBro.Core.ScriptData;
using StepBroCoreTest.Data;
using StepBroCoreTest.Utils;
using System;
using System.Linq;
using System.Text;

namespace StepBroCoreTest.Execution
{
    [TestClass]
    public class FormalTestSetupExecution
    {
        [TestMethod]
        public void TestPartnerOnProcedureUsingThisModifier()
        {
            var f = new StringBuilder();
            f.AppendLine("namespace MyFile;");
            f.AppendLine("procedure MyProcedure() :");
            f.AppendLine("    partner MyHelper : Helper {}");
            f.AppendLine("procedure void Helper(this MyProcedure proc)");   // Note the 'this' keyword.
            f.AppendLine("{ log (\"Doing partner for \" + proc.Name); }");
            f.AppendLine("procedure void ExecuteIt()");
            f.AppendLine("{");
            f.AppendLine("   log (\"Before\");");
            f.AppendLine("   MyProcedure.MyHelper();");                     // Not setting the 'proc' parameter; it is implicit via the 'this' modifier.
            f.AppendLine("   log (\"After\");");
            f.AppendLine("}");

            var file = FileBuilder.ParseFiles((ILogger)null, new Tuple<string, string>("myfile." + Main.StepBroFileExtension, f.ToString()))[0];

            Assert.AreEqual(3, file.ListElements().Where(e => e.ElementType == FileElementType.ProcedureDeclaration).Count());
            var procedure = file.ListElements().First(p => p.Name == "ExecuteIt") as IFileProcedure;

            Exception exeException = null;
            var taskContext = ExecutionHelper.ExeContext();
            try
            {
                taskContext.CallProcedure(procedure);
            }
            catch (Exception ex)
            {
                exeException = ex;
            }
            var log = new LogInspector(taskContext.Logger);
            log.DebugDump();
            if (exeException != null) throw exeException;

            log.ExpectNext("0 - Pre - TestRun - Starting");
            log.ExpectNext("1 - Pre - MyFile.ExecuteIt - <no arguments>");
            log.ExpectNext("2 - Normal - 8 Log - Before");
            log.ExpectNext("2 - Pre - MyFile.Helper - <no arguments>");
            log.ExpectNext("3 - Normal - 5 Log - Doing partner for MyProcedure");
            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - Normal - 10 Log - After");
            log.ExpectNext("2 - Post");
            log.ExpectEnd();
        }

        [TestMethod]
        public void TestSetupBasic()
        {
            var f = new StringBuilder();
            f.AppendLine("namespace MyFile;");
            f.AppendLine("procedure void TestCase() :");
            f.AppendLine("    FreeParameters,");
            f.AppendLine("    partner Setup : TestCaseBaseSetup,");
            f.AppendLine("    partner Cleanup : TestCaseBaseCleanup;");

            f.AppendLine("procedure void TestCaseBaseSetup(this TestCase testcase){ log (\"Doing setup for \" + testcase.Name); }");
            f.AppendLine("procedure void TestCaseBaseCleanup(this TestCase testcase){ log (\"Doing cleanup for \" + testcase.Name); }");

            f.AppendLine("procedure void FirstTestCase() : TestCase {}");
            f.AppendLine("procedure void SecondTestCase() : TestCase {}");
            f.AppendLine("procedure void ThirdTestCase() : TestCase {}");

            f.AppendLine("testlist AllTests");
            f.AppendLine("{");
            f.AppendLine("   * FirstTestCase");
            f.AppendLine("   * SecondTestCase");
            f.AppendLine("   * ThirdTestCase");
            f.AppendLine("}");

            f.AppendLine("procedure void ExecuteAllTests()");
            f.AppendLine("{");
            f.AppendLine("   var iterator = AllTests.GetProcedureIterator();");
            f.AppendLine("   while (iterator.GetNext())");
            f.AppendLine("   {");
            f.AppendLine("      log (\"Starting Test: \" + iterator.Procedure.Name);");
            f.AppendLine("      TestCase testcase = iterator.Procedure as TestCase;");
            f.AppendLine("      testcase.Setup();");
            f.AppendLine("      iterator.Procedure( iterator.Arguments );");
            f.AppendLine("      testcase.Cleanup();");
            f.AppendLine("   }");
            f.AppendLine("}");

            var file = FileBuilder.ParseFiles((ILogger)null, new Tuple<string, string>("myfile." + Main.StepBroFileExtension, f.ToString()))[0];

            Assert.AreEqual(1, file.ListElements().Where(e => e.ElementType == FileElementType.TestList).Count());
            var list = file["AllTests"] as ITestList;
            Assert.AreEqual("AllTests", list.Name);
            Assert.AreEqual(3, list.EntryCount);
            Assert.AreEqual("FirstTestCase", list[0].ReferenceName);
            Assert.AreEqual("SecondTestCase", list[1].ReferenceName);
            Assert.AreEqual("ThirdTestCase", list[2].ReferenceName);
            Assert.AreSame(file["FirstTestCase"], list[0].Reference);
            Assert.AreSame(file["SecondTestCase"], list[1].Reference);
            Assert.AreSame(file["ThirdTestCase"], list[2].Reference);
            Assert.AreEqual("FirstTestCase", list[0].Reference.Name);
            Assert.AreEqual("SecondTestCase", list[1].Reference.Name);
            Assert.AreEqual("ThirdTestCase", list[2].Reference.Name);
            Assert.AreSame(list, list[0].Home);
            Assert.AreSame(list, list[1].Home);
            Assert.AreSame(list, list[2].Home);

            var procedure = file.ListElements().First(p => p.Name == "ExecuteAllTests") as IFileProcedure;

            var taskContext = ExecutionHelper.ExeContext();
            taskContext.CallProcedure(procedure);
            var log = new LogInspector(taskContext.Logger);
            log.DebugDump();
            log.ExpectNext("0 - Pre - TestRun - Starting");
            log.ExpectNext("1 - Pre - MyFile.ExecuteAllTests - <no arguments>");
            log.ExpectNext("2 - Normal - 22 Log - Starting Test: FirstTestCase");
            log.ExpectNext("2 - Pre - MyFile.TestCaseBaseSetup - <no arguments>");
            log.ExpectNext("3 - Normal - 6 Log - Doing setup for FirstTestCase");
            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - Pre - <DYNAMIC CALL> MyFile.FirstTestCase - ( (<empty>) )");
            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - Pre - MyFile.TestCaseBaseCleanup - <no arguments>");
            log.ExpectNext("3 - Normal - 7 Log - Doing cleanup for FirstTestCase");
            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - Normal - 22 Log - Starting Test: SecondTestCase");
            log.ExpectNext("2 - Pre - MyFile.TestCaseBaseSetup - <no arguments>");
            log.ExpectNext("3 - Normal - 6 Log - Doing setup for SecondTestCase");
            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - Pre - <DYNAMIC CALL> MyFile.SecondTestCase - ( (<empty>) )");
            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - Pre - MyFile.TestCaseBaseCleanup - <no arguments>");
            log.ExpectNext("3 - Normal - 7 Log - Doing cleanup for SecondTestCase");
            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - Normal - 22 Log - Starting Test: ThirdTestCase");
            log.ExpectNext("2 - Pre - MyFile.TestCaseBaseSetup - <no arguments>");
            log.ExpectNext("3 - Normal - 6 Log - Doing setup for ThirdTestCase");
            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - Pre - <DYNAMIC CALL> MyFile.ThirdTestCase - ( (<empty>) )");
            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - Pre - MyFile.TestCaseBaseCleanup - <no arguments>");
            log.ExpectNext("3 - Normal - 7 Log - Doing cleanup for ThirdTestCase");
            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - Post");
            log.ExpectEnd();
        }

        [TestMethod]
        public void TestPartnerOnTestList()
        {
            var f = new StringBuilder();
            f.AppendLine("namespace MyFile;");
            f.AppendLine("testlist MyTestSuite :");
            f.AppendLine("    partner MyHelper : Helper;");
            f.AppendLine("procedure void Helper(this MyTestSuite suite)");
            f.AppendLine("{ log (\"Inside Helper for \" + suite.Name); }");
            f.AppendLine("procedure void ExecuteIt()");
            f.AppendLine("{");
            f.AppendLine("   log (\"Before\");");
            f.AppendLine("   MyTestSuite.MyHelper();");
            f.AppendLine("   log (\"After\");");
            f.AppendLine("}");

            var file = FileBuilder.ParseFiles((ILogger)null, new Tuple<string, string>("myfile.tss", f.ToString()))[0];

            Assert.AreEqual(1, file.ListElements().Where(e => e.ElementType == FileElementType.TestList).Count());
            var procedure = file.ListElements().First(p => p.Name == "ExecuteIt") as IFileProcedure;

            Exception exeException = null;
            var taskContext = ExecutionHelper.ExeContext();
            try
            {
                taskContext.CallProcedure(procedure);
            }
            catch (Exception ex)
            {
                exeException = ex;
            }
            var log = new LogInspector(taskContext.Logger);
            log.DebugDump();
            if (exeException != null) throw exeException;

            log.ExpectNext("0 - Pre - TestRun - Starting");
            log.ExpectNext("1 - Pre - MyFile.ExecuteIt - <no arguments>");
            log.ExpectNext("2 - Normal - 8 Log - Before");
            log.ExpectNext("2 - Pre - MyFile.Helper - <no arguments>");
            log.ExpectNext("3 - Normal - 5 Log - Inside Helper for MyTestSuite");
            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - Normal - 10 Log - After");
            log.ExpectNext("2 - Post");
            log.ExpectEnd();
        }

        [TestMethod]
        public void TestPartnerOnTestList_WithReturn()
        {
            var f = new StringBuilder();
            f.AppendLine("namespace MyFile;");
            f.AppendLine("testlist MyTestSuite :");
            f.AppendLine("    partner MyHelper : Helper;");
            f.AppendLine("procedure string Helper(this MyTestSuite suite)");
            f.AppendLine("{ return \"Was here!\"; }");
            f.AppendLine("procedure void ExecuteIt()");
            f.AppendLine("{");
            f.AppendLine("   log (\"Before\");");
            f.AppendLine("   string res = \"\";");
            f.AppendLine("   res = MyTestSuite.MyHelper();");
            f.AppendLine("   log (\"res: \" + res);");
            f.AppendLine("}");

            var file = FileBuilder.ParseFiles((ILogger)null, new Tuple<string, string>("myfile.tss", f.ToString()))[0];

            Assert.AreEqual(1, file.ListElements().Where(e => e.ElementType == FileElementType.TestList).Count());
            var procedure = file.ListElements().First(p => p.Name == "ExecuteIt") as IFileProcedure;

            Exception exeException = null;
            var taskContext = ExecutionHelper.ExeContext();
            try
            {
                taskContext.CallProcedure(procedure);
            }
            catch (Exception ex)
            {
                exeException = ex;
            }
            var log = new LogInspector(taskContext.Logger);
            log.DebugDump();
            if (exeException != null) throw exeException;

            log.ExpectNext("0 - Pre - TestRun - Starting");
            log.ExpectNext("1 - Pre - MyFile.ExecuteIt - <no arguments>");
            log.ExpectNext("2 - Normal - 8 Log - Before");
            log.ExpectNext("2 - Pre - MyFile.Helper - <no arguments>");
            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - Normal - 11 Log - res: Was here!");
            log.ExpectNext("2 - Post");
            log.ExpectEnd();
        }

        [TestMethod]
        public void TestSetupWithBaseTypes()
        {
            var f = new StringBuilder();
            f.AppendLine("procedure void TestCaseBase() :");
            f.AppendLine("    FreeParameters,");
            f.AppendLine("    partner Setup : TestCaseBaseSetup,");
            f.AppendLine("    partner Cleanup : TestCaseBaseCleanup;");
            f.AppendLine("procedure void SpecialTestCaseBase() : TestCaseBase,");
            f.AppendLine("    partner override Setup : SpecialSetup;");

            f.AppendLine("procedure void TestCaseBaseSetup(this TestCaseBase testcase){ log (\"Doing setup for \" + testcase.Name); }");
            f.AppendLine("procedure void TestCaseBaseCleanup(this TestCaseBase testcase){ log (\"Doing cleanup for \" + testcase.Name); }");
            f.AppendLine("procedure void SpecialSetup(this TestCaseBase testcase) : TestCaseBaseSetup { log (\"Doing special setup for \" + testcase.Name); }");

            f.AppendLine("procedure void FirstTestCase() : TestCaseBase { expect (5 < 9); }");
            f.AppendLine("procedure void SecondTestCase() : TestCaseBase { expect (4 > 7); }");
            f.AppendLine("procedure void ThirdTestCase() : SpecialTestCaseBase { expect (7 <= 8); }");

            f.AppendLine("testlist TestSuiteBase :");
            f.AppendLine("    partner FormalTest : TestSuiteFormalTestExecution;");
            f.AppendLine("testlist AllTests : TestSuiteBase");
            f.AppendLine("{");
            f.AppendLine("   * FirstTestCase");
            f.AppendLine("   * SecondTestCase");
            f.AppendLine("   * ThirdTestCase");
            f.AppendLine("}");

            f.AppendLine("procedure void TestSuiteFormalTestExecution(this TestSuiteBase suite) : NoSubResultInheritance");
            f.AppendLine("{");
            f.AppendLine("   var iterator = suite.GetProcedureIterator();");
            f.AppendLine("   var summary = CreateTestSummary();");
            f.AppendLine("   while (iterator.GetNext())");
            f.AppendLine("   {");
            f.AppendLine("      log (\"Starting Test: \" + iterator.Procedure.Name);");
            f.AppendLine("      TestCaseBase testcase = iterator.Procedure as TestCaseBase;");
            f.AppendLine("      testcase.Setup();");
            f.AppendLine("      iterator.Procedure( iterator.Arguments );");
            f.AppendLine("      summary.AddResult(testcase.Name, this.LastCallResult);");
            f.AppendLine("      testcase.Cleanup();");
            f.AppendLine("   }");
            f.AppendLine("   log (\"Fails: \" + summary.CountSubFails());");
            f.AppendLine("}");

            f.AppendLine("procedure void ExecuteAllTests()");
            f.AppendLine("{");
            f.AppendLine("   AllTests.FormalTest();");  // 
            f.AppendLine("}");

            var file = FileBuilder.ParseFiles((ILogger)null, new Tuple<string, string>("myfile.tss", f.ToString()))[0];

            Assert.AreEqual(2, file.ListElements().Where(e => e.ElementType == FileElementType.TestList).Count());
            var list = file["AllTests"] as ITestList;
            Assert.AreEqual("AllTests", list.Name);
            Assert.AreEqual(3, list.EntryCount);
            Assert.AreEqual("FirstTestCase", list[0].ReferenceName);
            Assert.AreEqual("SecondTestCase", list[1].ReferenceName);
            Assert.AreEqual("ThirdTestCase", list[2].ReferenceName);
            Assert.AreSame(file["FirstTestCase"], list[0].Reference);
            Assert.AreSame(file["SecondTestCase"], list[1].Reference);
            Assert.AreSame(file["ThirdTestCase"], list[2].Reference);
            Assert.AreEqual("FirstTestCase", list[0].Reference.Name);
            Assert.AreEqual("SecondTestCase", list[1].Reference.Name);
            Assert.AreEqual("ThirdTestCase", list[2].Reference.Name);
            Assert.AreSame(list, list[0].Home);
            Assert.AreSame(list, list[1].Home);
            Assert.AreSame(list, list[2].Home);

            var procedure = file.ListElements().First(p => p.Name == "ExecuteAllTests") as IFileProcedure;

            var taskContext = ExecutionHelper.ExeContext();
            taskContext.CallProcedure(procedure);
            var log = new LogInspector(taskContext.Logger);
            log.DebugDump();
            log.ExpectNext("0 - Pre - TestRun - Starting");
            log.ExpectNext("1 - Pre - myfile.ExecuteAllTests - <no arguments>");
            log.ExpectNext("2 - Pre - myfile.TestSuiteFormalTestExecution - <no arguments>");
            log.ExpectNext("3 - Normal - 27 Log - Starting Test: FirstTestCase");
            log.ExpectNext("3 - Pre - myfile.TestCaseBaseSetup - <no arguments>");
            log.ExpectNext("4 - Normal - 7 Log - Doing setup for FirstTestCase");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - <DYNAMIC CALL> myfile.FirstTestCase - ( (<empty>) )");
            log.ExpectNext("4 - Normal - 10 - EXPECT: 5<9; Actual: <TRUE>  =>  Pass");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - myfile.TestCaseBaseCleanup - <no arguments>");
            log.ExpectNext("4 - Normal - 8 Log - Doing cleanup for FirstTestCase");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Normal - 27 Log - Starting Test: SecondTestCase");
            log.ExpectNext("3 - Pre - myfile.TestCaseBaseSetup - <no arguments>");
            log.ExpectNext("4 - Normal - 7 Log - Doing setup for SecondTestCase");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - <DYNAMIC CALL> myfile.SecondTestCase - ( (<empty>) )");
            log.ExpectNext("4 - Error - 11 - EXPECT: 4>7; Actual: <FALSE>  =>  Fail");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - myfile.TestCaseBaseCleanup - <no arguments>");
            log.ExpectNext("4 - Normal - 8 Log - Doing cleanup for SecondTestCase");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Normal - 27 Log - Starting Test: ThirdTestCase");
            log.ExpectNext("3 - Pre - myfile.SpecialSetup - <no arguments>");
            log.ExpectNext("4 - Normal - 9 Log - Doing special setup for ThirdTestCase");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - <DYNAMIC CALL> myfile.ThirdTestCase - ( (<empty>) )");
            log.ExpectNext("4 - Normal - 12 - EXPECT: 7<=8; Actual: <TRUE>  =>  Pass");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - myfile.TestCaseBaseCleanup - <no arguments>");
            log.ExpectNext("4 - Normal - 8 Log - Doing cleanup for ThirdTestCase");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Normal - 34 Log - Fails: 1");
            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - Post");
            log.ExpectEnd();
        }

        [TestMethod]
        public void TestSetupCleanWithFrameworkFile()
        {
            var f = new StringBuilder();

            f.AppendLine("using TestFramework;");

            f.AppendLine("testlist AllTests : TestSuite");
            f.AppendLine("{");
            f.AppendLine("    * FirstTestCase");
            f.AppendLine("    * SecondTestCase");
            f.AppendLine("    * ThirdTestCase");
            f.AppendLine("}");

            f.AppendLine("procedure void FirstTestCase() : TestCase");
            f.AppendLine("{ log(\"Inside FirstTestCase\"); }");

            f.AppendLine("procedure void SecondTestCase() : TestCase");
            f.AppendLine("{ log(\"Inside SecondTestCase\"); }");

            f.AppendLine("procedure void ThirdTestCase() : TestCase");
            f.AppendLine("{ log(\"Inside ThirdTestCase\"); }");

            f.AppendLine("procedure void ExecuteAllTests()");
            f.AppendLine("{");
            f.AppendLine("   AllTests.FormalTest();");
            f.AppendLine("}");

            var files = FileBuilder.ParseFiles((ILogger)null,
                new Tuple<string, string>("myfile." + Main.StepBroFileExtension, f.ToString()),
                new Tuple<string, string>("TestFramework." + Main.StepBroFileExtension, CreateTestFrameworkFile()));
            var myfile = files.First(file => file.FileName == "myfile." + Main.StepBroFileExtension);
            var framework = files.First(file => file.FileName == "TestFramework." + Main.StepBroFileExtension);

            Assert.AreEqual(4, myfile.ListElements().Where(e => e.ElementType == FileElementType.ProcedureDeclaration).Count());
            Assert.AreEqual(1, myfile.ListElements().Where(e => e.ElementType == FileElementType.TestList).Count());
            Assert.AreEqual(7, framework.ListElements().Where(e => e.ElementType == FileElementType.ProcedureDeclaration).Count());
            Assert.AreEqual(1, framework.ListElements().Where(e => e.ElementType == FileElementType.TestList).Count());

            var list = myfile["AllTests"] as ITestList;
            Assert.AreEqual("AllTests", list.Name);
            Assert.AreEqual(3, list.EntryCount);
            Assert.AreEqual("FirstTestCase", list[0].ReferenceName);
            Assert.AreEqual("SecondTestCase", list[1].ReferenceName);
            Assert.AreEqual("ThirdTestCase", list[2].ReferenceName);

            var procedure = myfile.ListElements().First(p => p.Name == "ExecuteAllTests") as IFileProcedure;

            var taskContext = ExecutionHelper.ExeContext();
            taskContext.CallProcedure(procedure);
            var log = new LogInspector(taskContext.Logger);
            log.DebugDump();
            log.ExpectNext("0 - Pre - TestRun - Starting");
            log.ExpectNext("1 - Pre - myfile.ExecuteAllTests - <no arguments>");
            log.ExpectNext("2 - Pre - TestFramework.TestSuiteFormalTestExecution - <no arguments>");

            log.ExpectNext("3 - Pre - TestFramework.TestSuiteEmptyPreTest - <no arguments>");
            log.ExpectNext("4 - Post");

            log.ExpectNext("3 - Normal - 34 Log - Starting Test: FirstTestCase");
            log.ExpectNext("3 - Pre - TestFramework.TestCaseEmptySetup - <no arguments>");
            log.ExpectNext("4 - Normal - 7 Log - TestCaseEmptySetup FirstTestCase");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - <DYNAMIC CALL> myfile.FirstTestCase - ( (<empty>) )");
            log.ExpectNext("4 - Normal - 9 Log - Inside FirstTestCase");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - TestFramework.TestCaseEmptyCleanup - <no arguments>");
            log.ExpectNext("4 - Normal - 8 Log - TestCaseEmptyCleanup FirstTestCase");
            log.ExpectNext("4 - Post");

            log.ExpectNext("3 - Normal - 34 Log - Starting Test: SecondTestCase");
            log.ExpectNext("3 - Pre - TestFramework.TestCaseEmptySetup - <no arguments>");
            log.ExpectNext("4 - Normal - 7 Log - TestCaseEmptySetup SecondTestCase");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - <DYNAMIC CALL> myfile.SecondTestCase - ( (<empty>) )");
            log.ExpectNext("4 - Normal - 11 Log - Inside SecondTestCase");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - TestFramework.TestCaseEmptyCleanup - <no arguments>");
            log.ExpectNext("4 - Normal - 8 Log - TestCaseEmptyCleanup SecondTestCase");
            log.ExpectNext("4 - Post");

            log.ExpectNext("3 - Normal - 34 Log - Starting Test: ThirdTestCase");
            log.ExpectNext("3 - Pre - TestFramework.TestCaseEmptySetup - <no arguments>");
            log.ExpectNext("4 - Normal - 7 Log - TestCaseEmptySetup ThirdTestCase");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - <DYNAMIC CALL> myfile.ThirdTestCase - ( (<empty>) )");
            log.ExpectNext("4 - Normal - 13 Log - Inside ThirdTestCase");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - TestFramework.TestCaseEmptyCleanup - <no arguments>");
            log.ExpectNext("4 - Normal - 8 Log - TestCaseEmptyCleanup ThirdTestCase");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - Post");
            log.ExpectEnd();
        }

        [TestMethod]
        public void TestSetupWithFrameworkFile()
        {
            var f = new StringBuilder();

            f.AppendLine("using TestFramework;");
            f.AppendLine("procedure MyTestCase() : TestCase,");
            f.AppendLine("   FreeParameters,");
            f.AppendLine("   partner override Setup: TestCaseSetup,");
            f.AppendLine("   partner override Cleanup:	TestCaseCleanup;");

            f.AppendLine("procedure void TestCaseSetup(this TestCase testcase)");
            f.AppendLine("{ log (\"Doing default setup for \" + testcase.Name); }");
            f.AppendLine("procedure void TestCaseCleanup(this TestCase testcase)");
            f.AppendLine("{ log (\"Doing default cleanup for \" + testcase.Name); }");

            f.AppendLine("procedure void TestCaseSpecialSetup(this TestCase testcase)");
            f.AppendLine("{ log (\"Doing special setup for \" + testcase.Name); }");
            f.AppendLine("procedure void TestCaseSpecialCleanup(this TestCase testcase)");
            f.AppendLine("{ log (\"Doing special cleanup for \" + testcase.Name); }");

            f.AppendLine("testlist AllTests : TestSuite");
            f.AppendLine("{");
            f.AppendLine("    * FirstTestCase");
            f.AppendLine("    * SecondTestCase");
            f.AppendLine("    * ThirdTestCase");
            f.AppendLine("}");

            f.AppendLine("procedure void FirstTestCase() : MyTestCase,");
            f.AppendLine(" partner override Setup: TestCaseSpecialSetup");
            f.AppendLine("{ log(\"Inside FirstTestCase\"); }");

            f.AppendLine("procedure void SecondTestCase() : MyTestCase,");
            f.AppendLine("partner override Cleanup:	TestCaseSpecialCleanup");
            f.AppendLine("{ log(\"Inside SecondTestCase\"); }");

            f.AppendLine("procedure void ThirdTestCase() : MyTestCase");
            f.AppendLine("{ log(\"Inside ThirdTestCase\"); }");

            f.AppendLine("procedure void ExecuteAllTests()");
            f.AppendLine("{");
            f.AppendLine("   AllTests.FormalTest();");
            f.AppendLine("}");

            var files = FileBuilder.ParseFiles((ILogger)null,
                new Tuple<string, string>("myfile." + Main.StepBroFileExtension, f.ToString()),
                new Tuple<string, string>("TestFramework." + Main.StepBroFileExtension, CreateTestFrameworkFile()));
            var myfile = files.First(file => file.FileName == "myfile." + Main.StepBroFileExtension);
            var framework = files.First(file => file.FileName == "TestFramework." + Main.StepBroFileExtension);

            Assert.AreEqual(9, myfile.ListElements().Where(e => e.ElementType == FileElementType.ProcedureDeclaration).Count());
            Assert.AreEqual(1, myfile.ListElements().Where(e => e.ElementType == FileElementType.TestList).Count());
            Assert.AreEqual(7, framework.ListElements().Where(e => e.ElementType == FileElementType.ProcedureDeclaration).Count());
            Assert.AreEqual(1, framework.ListElements().Where(e => e.ElementType == FileElementType.TestList).Count());

            var list = myfile["AllTests"] as ITestList;
            Assert.AreEqual("AllTests", list.Name);
            Assert.AreEqual(3, list.EntryCount);
            Assert.AreEqual("FirstTestCase", list[0].ReferenceName);
            Assert.AreEqual("SecondTestCase", list[1].ReferenceName);
            Assert.AreEqual("ThirdTestCase", list[2].ReferenceName);

            var procedure = myfile.ListElements().First(p => p.Name == "ExecuteAllTests") as IFileProcedure;

            var taskContext = ExecutionHelper.ExeContext();
            taskContext.CallProcedure(procedure);
            var log = new LogInspector(taskContext.Logger);
            log.DebugDump();
            log.ExpectNext("0 - Pre - TestRun - Starting");
            log.ExpectNext("1 - Pre - myfile.ExecuteAllTests - <no arguments>");
            log.ExpectNext("2 - Pre - TestFramework.TestSuiteFormalTestExecution - <no arguments>");

            log.ExpectNext("3 - Pre - TestFramework.TestSuiteEmptyPreTest - <no arguments>");
            log.ExpectNext("4 - Post");

            log.ExpectNext("3 - Normal - 34 Log - Starting Test: FirstTestCase");
            log.ExpectNext("3 - Pre - myfile.TestCaseSpecialSetup - <no arguments>");
            log.ExpectNext("4 - Normal - 11 Log - Doing special setup for FirstTestCase");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - <DYNAMIC CALL> myfile.FirstTestCase - ( (<empty>) )");
            log.ExpectNext("4 - Normal - 22 Log - Inside FirstTestCase");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - myfile.TestCaseCleanup - <no arguments>");
            log.ExpectNext("4 - Normal - 9 Log - Doing default cleanup for FirstTestCase");
            log.ExpectNext("4 - Post");

            log.ExpectNext("3 - Normal - 34 Log - Starting Test: SecondTestCase");
            log.ExpectNext("3 - Pre - myfile.TestCaseSetup - <no arguments>");
            log.ExpectNext("4 - Normal - 7 Log - Doing default setup for SecondTestCase");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - <DYNAMIC CALL> myfile.SecondTestCase - ( (<empty>) )");
            log.ExpectNext("4 - Normal - 25 Log - Inside SecondTestCase");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - myfile.TestCaseSpecialCleanup - <no arguments>");
            log.ExpectNext("4 - Normal - 13 Log - Doing special cleanup for SecondTestCase");
            log.ExpectNext("4 - Post");

            log.ExpectNext("3 - Normal - 34 Log - Starting Test: ThirdTestCase");
            log.ExpectNext("3 - Pre - myfile.TestCaseSetup - <no arguments>");
            log.ExpectNext("4 - Normal - 7 Log - Doing default setup for ThirdTestCase");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - <DYNAMIC CALL> myfile.ThirdTestCase - ( (<empty>) )");
            log.ExpectNext("4 - Normal - 27 Log - Inside ThirdTestCase");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - myfile.TestCaseCleanup - <no arguments>");
            log.ExpectNext("4 - Normal - 9 Log - Doing default cleanup for ThirdTestCase");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - Post");
            log.ExpectEnd();
        }

        [TestMethod]
        public void TestSetupWithSpecialSetupFile()
        {
            var f = new StringBuilder();

            f.AppendLine("using TestFramework;");
            f.AppendLine("using SpecialTest;");
            f.AppendLine("using StepBroCoreTest.Data;");
            f.AppendLine("using SomeTool;");

            f.AppendLine("DummyClass anders = DummyClass{ PropInt = 20 }");

            f.AppendLine("procedure void TestCaseLocalSetup(this TestCase testcase)");
            f.AppendLine("{ log (this.Name + \" \" + testcase.Name); }");
            f.AppendLine("procedure void TestCaseLocalCleanup(this TestCase testcase)");
            f.AppendLine("{ log (this.Name + \" \" + testcase.Name); }");

            f.AppendLine("testlist AllTests : TestSuite,");
            f.AppendLine("    partner override PreTest : MyPreTest");
            f.AppendLine("{");
            f.AppendLine("    * FirstTestCase");
            f.AppendLine("    * SecondTestCase");
            f.AppendLine("    * ThirdTestCase");
            f.AppendLine("}");

            f.AppendLine("procedure void FirstTestCase() : SpecialTestCase,");
            f.AppendLine("    partner override Setup: TestCaseLocalSetup");
            f.AppendLine("{ log(\"Inside \" + this.Name + \": \" + anders.PropInt); }");

            f.AppendLine("procedure void SecondTestCase() : SpecialTestCase,");
            f.AppendLine("    partner override Setup: TestCaseSpecialSetup");
            f.AppendLine("{ log(\"Inside \" + this.Name + \": \" + bent.PropInt); }");

            f.AppendLine("procedure void ThirdTestCase() : SpecialTestCase");
            f.AppendLine("{ log(\"Inside \" + this.Name + \": \" + DummyFunc()); }");

            f.AppendLine("procedure void ExecuteAllTests()");
            f.AppendLine("{");
            f.AppendLine("   AllTests.FormalTest();");
            f.AppendLine("}");

            f.AppendLine("procedure bool MyPreTest()");
            f.AppendLine("{");
            f.AppendLine("    log(this.Name + \":\" + anders.PropInt);");
            f.AppendLine("    return true;");
            f.AppendLine("}");

            var tf = new StringBuilder();
            tf.AppendLine("using StepBroCoreTest.Data;");
            tf.AppendLine("public DummyClass bent = DummyClass{ PropInt = 1986 }");
            tf.AppendLine("public function int DummyFunc(){ return 729; }");

            var files = FileBuilder.ParseFiles((ILogger)null, typeof(DummyClass).Assembly,
                new Tuple<string, string>("myfile." + Main.StepBroFileExtension, f.ToString()),
                new Tuple<string, string>("SomeTool." + Main.StepBroFileExtension, tf.ToString()),
                new Tuple<string, string>("SpecialTest." + Main.StepBroFileExtension, CreateSpecialTestFile()),
                new Tuple<string, string>("TestFramework." + Main.StepBroFileExtension, CreateTestFrameworkFile()));
            var myfile = files.First(file => file.FileName == "myfile." + Main.StepBroFileExtension);
            var special = files.First(file => file.FileName == "SpecialTest." + Main.StepBroFileExtension);
            var framework = files.First(file => file.FileName == "TestFramework." + Main.StepBroFileExtension);

            Assert.AreEqual(7, myfile.ListElements().Where(e => e.ElementType == FileElementType.ProcedureDeclaration).Count());
            Assert.AreEqual(1, myfile.ListElements().Where(e => e.ElementType == FileElementType.TestList).Count());
            Assert.AreEqual(6, special.ListElements().Where(e => e.ElementType == FileElementType.ProcedureDeclaration).Count());
            Assert.AreEqual(1, special.ListElements().Where(e => e.ElementType == FileElementType.TestList).Count());
            Assert.AreEqual(7, framework.ListElements().Where(e => e.ElementType == FileElementType.ProcedureDeclaration).Count());
            Assert.AreEqual(1, framework.ListElements().Where(e => e.ElementType == FileElementType.TestList).Count());

            var list = myfile["AllTests"] as ITestList;
            Assert.AreEqual("AllTests", list.Name);
            Assert.AreEqual(3, list.EntryCount);
            Assert.AreEqual("FirstTestCase", list[0].ReferenceName);
            Assert.AreEqual("SecondTestCase", list[1].ReferenceName);
            Assert.AreEqual("ThirdTestCase", list[2].ReferenceName);

            var procedure = myfile.ListElements().First(p => p.Name == "ExecuteAllTests") as IFileProcedure;
            //var testcase = special.ListElements().First(p => p.Name == "TestCase") as IFileProcedure;
            //var testcasebase = framework.ListElements().First(p => p.Name == "TestCaseBase") as IFileProcedure;

            var taskContext = ExecutionHelper.ExeContext(services: FileBuilder.LastServiceManager.Manager);
            taskContext.CallProcedure(procedure);
            var log = new LogInspector(taskContext.Logger);
            log.DebugDump();
            log.ExpectNext("0 - Pre - TestRun - Starting");
            log.ExpectNext("1 - Pre - myfile.ExecuteAllTests - <no arguments>");
            log.ExpectNext("2 - Pre - TestFramework.TestSuiteFormalTestExecution - <no arguments>");

            log.ExpectNext("3 - Pre - myfile.MyPreTest - <no arguments>");
            log.ExpectNext("4 - Normal - 31 Log - MyPreTest:20");
            log.ExpectNext("4 - Post");

            log.ExpectNext("3 - Normal - 34 Log - Starting Test: FirstTestCase");
            log.ExpectNext("3 - Pre - myfile.TestCaseLocalSetup - <no arguments>");
            log.ExpectNext("4 - Normal - 7 Log - TestCaseLocalSetup FirstTestCase");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - <DYNAMIC CALL> myfile.FirstTestCase - ( (<empty>) )");
            log.ExpectNext("4 - Normal - 19 Log - Inside FirstTestCase: 20");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - SpecialTest.TestCaseCleanup - <no arguments>");
            log.ExpectNext("4 - Normal - 8 Log - TestCaseCleanup FirstTestCase");
            log.ExpectNext("4 - Post");

            log.ExpectNext("3 - Normal - 34 Log - Starting Test: SecondTestCase");
            log.ExpectNext("3 - Pre - SpecialTest.TestCaseSpecialSetup - <no arguments>");
            log.ExpectNext("4 - Normal - 10 Log - TestCaseSpecialSetup SecondTestCase");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - <DYNAMIC CALL> myfile.SecondTestCase - ( (<empty>) )");
            log.ExpectNext("4 - Normal - 22 Log - Inside SecondTestCase: 1986");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - SpecialTest.TestCaseCleanup - <no arguments>");
            log.ExpectNext("4 - Normal - 8 Log - TestCaseCleanup SecondTestCase");
            log.ExpectNext("4 - Post");

            log.ExpectNext("3 - Normal - 34 Log - Starting Test: ThirdTestCase");
            log.ExpectNext("3 - Pre - SpecialTest.TestCaseSetup - <no arguments>");
            log.ExpectNext("4 - Normal - 6 Log - TestCaseSetup ThirdTestCase");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - <DYNAMIC CALL> myfile.ThirdTestCase - ( (<empty>) )");
            log.ExpectNext("4 - Normal - 24 Log - Inside ThirdTestCase: 729");
            log.ExpectNext("4 - Post");
            log.ExpectNext("3 - Pre - SpecialTest.TestCaseCleanup - <no arguments>");
            log.ExpectNext("4 - Normal - 8 Log - TestCaseCleanup ThirdTestCase");
            log.ExpectNext("4 - Post");

            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - Post");
            log.ExpectEnd();
        }

        public static string CreateSpecialTestFile()
        {
            var f = new StringBuilder();
            f.AppendLine("public using \"TestFramework.sbs\";");
            f.AppendLine("public procedure SpecialTestCase() : TestCase, FreeParameters,");
            f.AppendLine("    partner override Setup:     TestCaseSetup,");
            f.AppendLine("    partner override Cleanup:   TestCaseCleanup;");

            f.AppendLine("public procedure void TestCaseSetup(this TestCase testcase)");
            f.AppendLine("{ log(this.Name + \" \" + testcase.Name); }") ;
            f.AppendLine("public procedure void TestCaseCleanup(this TestCase testcase)");
            f.AppendLine("{ log(this.Name + \" \" + testcase.Name); }") ;

            f.AppendLine("public procedure void TestCaseSpecialSetup(this TestCase testcase)");
            f.AppendLine("{ log(this.Name + \" \" + testcase.Name); }");
            f.AppendLine("public procedure void TestCaseSpecialCleanup(this TestCase testcase)");
            f.AppendLine("{ log(this.Name + \" \" + testcase.Name); }");

            f.AppendLine("public testlist MyTestSuite : TestSuite,");
            f.AppendLine("    partner override PreTest : SpecialPreTest;");

            f.AppendLine("procedure bool SpecialPreTest()");
            f.AppendLine("{");
            f.AppendLine("    log(this.Name);");
            f.AppendLine("    return true;");
            f.AppendLine("}");

            return f.ToString();
        }

        public static string CreateTestFrameworkFile()
        {
            var f = new StringBuilder();
            f.AppendLine("procedure void TestCase() :");
            f.AppendLine("    FreeParameters,");
            f.AppendLine("    partner Setup : TestCaseEmptySetup,");
            f.AppendLine("    partner Cleanup : TestCaseEmptyCleanup,");
            f.AppendLine("    partner FormalTest : TestCaseFormalTest,");
            f.AppendLine("    partner FormalTestUntilFail : TestCaseFormalTestUntilFail;");

            f.AppendLine("procedure void TestCaseEmptySetup(this TestCase testcase){ log (this.Name + \" \" + testcase.Name); }");
            f.AppendLine("procedure void TestCaseEmptyCleanup(this TestCase testcase){ log (this.Name + \" \" + testcase.Name); }");

            f.AppendLine("procedure void TestCaseFormalTest(this TestCase testcase)");
            f.AppendLine("{");
            f.AppendLine("    testcase.Setup();");
            f.AppendLine("    testcase();");
            f.AppendLine("    testcase.Cleanup();");
            f.AppendLine("}");

            f.AppendLine("procedure void TestCaseFormalTestUntilFail(this TestCase testcase)");
            f.AppendLine("{");
            f.AppendLine("}");

            f.AppendLine("testlist TestSuite :");
            f.AppendLine("    partner FormalTest : TestSuiteFormalTestExecution,");
            f.AppendLine("    partner PreTest    : TestSuiteEmptyPreTest;");

            f.AppendLine("procedure bool TestSuiteEmptyPreTest()");
            f.AppendLine("{");
            f.AppendLine("    return true;");
            f.AppendLine("}");

            f.AppendLine("procedure void TestSuiteFormalTestExecution(this TestSuite suite)");
            f.AppendLine("{");
            f.AppendLine("   var preTestSuccess = false;");
            f.AppendLine("   preTestSuccess = suite.PreTest();");
            f.AppendLine("   if (preTestSuccess)");
            f.AppendLine("   {");
            f.AppendLine("       var iterator = suite.GetProcedureIterator();");
            f.AppendLine("       while (iterator.GetNext())");
            f.AppendLine("       {");
            f.AppendLine("           log (\"Starting Test: \" + iterator.Procedure.Name);");
            f.AppendLine("           TestCase testcase = iterator.Procedure as TestCase;");
            f.AppendLine("           testcase.Setup();");
            f.AppendLine("           iterator.Procedure( iterator.Arguments );");
            f.AppendLine("           testcase.Cleanup();");
            f.AppendLine("       }");
            f.AppendLine("   }");
            f.AppendLine("}");

            return f.ToString();
        }
    }
}