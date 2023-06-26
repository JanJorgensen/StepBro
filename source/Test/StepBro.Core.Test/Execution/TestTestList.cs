using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core.Logging;
using StepBro.Core.Parser;
using StepBro.Core.ScriptData;
using StepBroCoreTest.Utils;
using System;
using System.Linq;
using System.Text;
using static StepBro.Core.ScriptData.FileDatatable;

namespace StepBroCoreTest.Execution
{
    [TestClass]
    public class TestTestList
    {
        [TestMethod]
        public void TestSingleSimpleTestList()
        {
            var f = new StringBuilder();
            f.AppendLine("namespace MyFile;");
            f.AppendLine("procedure void FirstTestCase() {}");
            f.AppendLine("procedure void SecondTestCase() {}");
            f.AppendLine("procedure void ThirdTestCase() {}");
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
            f.AppendLine("      iterator.Procedure( iterator.Arguments );");
            f.AppendLine("   }");
            f.AppendLine("}");

            var file = FileBuilder.ParseFile(null, f.ToString());
            Assert.AreEqual(1, file.ListElements().Where(e => e.ElementType == FileElementType.TestList).Count());
            var list = file["AllTests"] as ITestList;
            Assert.AreEqual("AllTests", list.Name);
            Assert.AreEqual(3, list.EntryCount);
            Assert.AreSame(file["FirstTestCase"], list[0].Reference);
            Assert.AreSame(file["SecondTestCase"], list[1].Reference);
            Assert.AreSame(file["ThirdTestCase"], list[2].Reference);
            Assert.AreEqual("FirstTestCase", list[0].Reference.Name);
            Assert.AreEqual("SecondTestCase", list[1].Reference.Name);
            Assert.AreEqual("ThirdTestCase", list[2].Reference.Name);
            Assert.AreEqual("FirstTestCase", list[0].ReferenceName);
            Assert.AreEqual("SecondTestCase", list[1].ReferenceName);
            Assert.AreEqual("ThirdTestCase", list[2].ReferenceName);
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
            log.ExpectNext("2 - Pre - <DYNAMIC CALL> MyFile.FirstTestCase - ( (<empty>) )");
            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - Pre - <DYNAMIC CALL> MyFile.SecondTestCase - ( (<empty>) )");
            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - Pre - <DYNAMIC CALL> MyFile.ThirdTestCase - ( (<empty>) )");
            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - Post");
            log.ExpectEnd();

        }

        [TestMethod]
        public void TestSingleSimpleTestListWithParameters()
        {
            var f = new StringBuilder();
            f.AppendLine("namespace MyFile;");
            f.AppendLine("procedure void TestCaseWithParameters(int a = 20, string b = \"noffin\", verdict c = pass, timespan d = 5s)");
            f.AppendLine("{");
            f.AppendLine("}");
            f.AppendLine("testlist AllTests");
            f.AppendLine("{");
            f.AppendLine("   * TestCaseWithParameters(b: \"Brian\")");
            f.AppendLine("   * TestCaseWithParameters");
            f.AppendLine("   * TestCaseWithParameters");
            f.AppendLine("}");
            f.AppendLine("procedure void ExecuteAllTests()");
            f.AppendLine("{");
            f.AppendLine("   var iterator = AllTests.GetProcedureIterator();");
            f.AppendLine("   while (iterator.GetNext())");
            f.AppendLine("   {");
            f.AppendLine("      iterator.Procedure( iterator.Arguments );");
            f.AppendLine("   }");
            f.AppendLine("}");

            var file = FileBuilder.ParseFile(null, f.ToString());
            Assert.AreEqual(1, file.ListElements().Where(e => e.ElementType == FileElementType.TestList).Count());
            var list = file["AllTests"] as ITestList;
            Assert.AreEqual("AllTests", list.Name);
            Assert.AreEqual(3, list.EntryCount);
            Assert.AreSame(file["TestCaseWithParameters"], list[0].Reference);
            Assert.AreSame(file["TestCaseWithParameters"], list[1].Reference);
            Assert.AreSame(file["TestCaseWithParameters"], list[2].Reference);
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
            log.ExpectNext("2 - Pre - <DYNAMIC CALL> MyFile.TestCaseWithParameters - ( (b: \"Brian\") )");
            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - Pre - <DYNAMIC CALL> MyFile.TestCaseWithParameters - ( (<empty>) )");
            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - Pre - <DYNAMIC CALL> MyFile.TestCaseWithParameters - ( (<empty>) )");
            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - Post");
            log.ExpectEnd();
        }
        [TestMethod]
        public void TestlistUseWithOppositeOrder()
        {
            var f = new StringBuilder();
            f.AppendLine("procedure void ExecuteAllTests()");
            f.AppendLine("{");
            f.AppendLine("   var iterator = AllTests.GetProcedureIterator();");
            f.AppendLine("   while (iterator.GetNext())");
            f.AppendLine("   {");
            f.AppendLine("      iterator.Procedure( iterator.Arguments );");
            f.AppendLine("   }");
            f.AppendLine("}");
            f.AppendLine("testlist AllTests");
            f.AppendLine("{");
            f.AppendLine("   * FirstTestCase");
            f.AppendLine("}");
            f.AppendLine("procedure void FirstTestCase() {}");

            var file = FileBuilder.ParseFiles((ILogger)null, new Tuple<string, string>("myfile.tss", f.ToString()))[0];

            Assert.AreEqual(1, file.ListElements().Where(e => e.ElementType == FileElementType.TestList).Count());
            var procedure = file.ListElements().First(p => p.Name == "ExecuteAllTests") as IFileProcedure;

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
            log.ExpectNext("1 - Pre - myfile.ExecuteAllTests - <no arguments>");
            log.ExpectNext("2 - Pre - <DYNAMIC CALL> myfile.FirstTestCase - ( (<empty>) )");
            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - Post");
            log.ExpectEnd();
        }

        [TestMethod]
        public void TestlistOverridePartnerInBase()
        {
            var f1 = new StringBuilder();
            f1.AppendLine("using @\"basefile.sbs\";");
            f1.AppendLine("override TestSuiteBase");
            f1.AppendLine("{");
            f1.AppendLine("    partner override Park : ParkProc");
            f1.AppendLine("}");
            f1.AppendLine("testlist mySuite : TestSuiteBase");
            f1.AppendLine("{");
            f1.AppendLine("   * FirstTestCase");
            f1.AppendLine("}");
            f1.AppendLine("procedure void FirstTestCase()");
            f1.AppendLine("{");
            f1.AppendLine("   log(\"Wuf\");");
            f1.AppendLine("}");
            f1.AppendLine("procedure void ParkProc()");
            f1.AppendLine("{");
            f1.AppendLine("   log(\"Park\");");
            f1.AppendLine("}");
            f1.AppendLine("procedure void CallParkOnMySuite()");
            f1.AppendLine("{");
            f1.AppendLine("   mySuite.Park();");
            f1.AppendLine("}");

            var f2 = new StringBuilder();
            f2.AppendLine("testlist TestSuiteBase :");
            f2.AppendLine("    partner Park : ParkProcEmpty;");
            f2.AppendLine("private procedure void ParkProcEmpty()");
            f2.AppendLine("{");
            f2.AppendLine("}");

            var files = FileBuilder.ParseFiles((ILogger)null, 
                new Tuple<string, string>("topfile.sbs", f1.ToString()),
                new Tuple<string, string>("basefile.sbs", f2.ToString()));

            Assert.AreEqual(2, files.Length);
            var file1 = files[0];
            Assert.AreEqual(0, file1.Errors.ErrorCount);
            var file2 = files[1];
            Assert.AreEqual(0, file2.Errors.ErrorCount);
            Assert.AreEqual(1, file1.ListElements().Where(e => e.ElementType == FileElementType.TestList).Count());
            Assert.AreEqual(3, file1.ListElements().Where(e => e.ElementType == FileElementType.ProcedureDeclaration).Count());
            Assert.AreEqual(1, file2.ListElements().Where(e => e.ElementType == FileElementType.TestList).Count());
            Assert.AreEqual(1, file2.ListElements().Where(e => e.ElementType == FileElementType.ProcedureDeclaration).Count());

            var listTestSuiteBase = file2.ListElements().First(p => p.Name == "TestSuiteBase") as ITestList;
            Assert.IsNotNull(listTestSuiteBase);

            var listTestSuiteBaseOverride = file1.ListElements().First(p => p.Name == "TestSuiteBase") as IFileElement;
            Assert.IsNotNull(listTestSuiteBaseOverride);
            Assert.IsTrue(listTestSuiteBaseOverride.ElementType == FileElementType.Override);
            Assert.AreSame(listTestSuiteBase, listTestSuiteBaseOverride.BaseElement);

            var listMySuite = file1.ListElements().First(p => p.Name == "mySuite") as ITestList;
            Assert.IsNotNull(listMySuite);
            Assert.AreSame(listTestSuiteBase, listMySuite.BaseElement);


            var procParkProc = file1.ListElements().First(p => p.Name == "ParkProc") as IFileProcedure;
            Assert.IsNotNull(procParkProc);
            var procParkProcEmpty = file2.ListElements().First(p => p.Name == "ParkProcEmpty") as IFileProcedure;
            Assert.IsNotNull(procParkProcEmpty);

            var partnerOverride = listMySuite.ListPartners().First();
            Assert.IsNotNull(partnerOverride);
            Assert.AreSame(procParkProc, partnerOverride.ProcedureReference);

            var partnerOverridden = listTestSuiteBase.ListPartners().First();
            Assert.IsNotNull(partnerOverridden);
            Assert.AreSame(procParkProc, partnerOverridden.ProcedureReference);

            var procCallParkOnMySuite = file1.ListElements().First(p => p.Name == "CallParkOnMySuite") as IFileProcedure;
            Assert.IsNotNull(procCallParkOnMySuite);

            Exception exeException = null;
            var taskContext = ExecutionHelper.ExeContext();
            try
            {
                taskContext.CallProcedure(procCallParkOnMySuite);
            }
            catch (Exception ex)
            {
                exeException = ex;
            }
            var log = new LogInspector(taskContext.Logger);
            log.DebugDump();
            if (exeException != null) throw exeException;

            log.ExpectNext("0 - Pre - TestRun - Starting");
            log.ExpectNext("1 - Pre - topfile.CallParkOnMySuite - <no arguments>");
            log.ExpectNext("2 - Pre - topfile.ParkProc - <no arguments>");
            log.ExpectNext("3 - Normal - 16 Log - Park");
            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - Post");
            log.ExpectEnd();
        }
    }
}
