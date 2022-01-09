using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core.Logging;
using StepBro.Core.Parser;
using StepBro.Core.ScriptData;
using StepBroCoreTest.Utils;
using System;
using System.Linq;
using System.Text;

namespace StepBroCoreTest.Execution
{
    [TestClass]
    public class TestTestList
    {
        [TestMethod]
        public void TestFileWithSingleSimpleTestList()
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
    }
}
