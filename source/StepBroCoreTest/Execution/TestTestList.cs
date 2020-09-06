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
            log.ExpectNext("1 - Pre - MyFile.ExecuteAllTests - <arguments>");
            log.ExpectNext("2 - Pre - <DYNAMIC CALL> MyFile.FirstTestCase - <arguments>");
            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - Pre - <DYNAMIC CALL> MyFile.SecondTestCase - <arguments>");
            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - Pre - <DYNAMIC CALL> MyFile.ThirdTestCase - <arguments>");
            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - Post");
            log.ExpectEnd();

        }
    }
}
