using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core;
using StepBro.Core.Parser;
using StepBro.Core.ScriptData;
using System.Text;

namespace StepBroCoreTest
{
    [TestClass]
    public class TestProcedure_Expect
    {
        [TestInitialize]
        public void Setup()
        {
            ServiceManager.Dispose();
        }

        [TestMethod]
        public void TestExpectWithVariableInput()
        {
            // With title and expect is passing
            var f = new StringBuilder();
            f.AppendLine("bool MyProcedure() {");
            f.AppendLine("   int x = 10;");
            f.AppendLine("   expect (x == 10);");
            f.AppendLine("   return this.HasFails;");
            f.AppendLine("}");
            var file = FileBuilder.ParseFile(null, f.ToString());
            var procedure = file.GetFileElement<IFileProcedure>("MyProcedure"); ;

            var taskContext = ExecutionHelper.ExeContext();
            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(false, (bool)result);
        }
        
        [TestMethod]
        public void TestExpectStatement()
        {
            // With title and expect is passing
            var f = new StringBuilder();
            f.AppendLine("bool MyProcedure() {");
            f.AppendLine("   expect \"My Expectations\": (4 < 10);");
            f.AppendLine("   return true;");
            f.AppendLine("}");
            var file = FileBuilder.ParseFile(null, f.ToString());
            var procedure = file.GetFileElement<IFileProcedure>("MyProcedure"); ;

            var taskContext = ExecutionHelper.ExeContext();
            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(true, (bool)result);

            // With title and expect is failing
            f = new StringBuilder();
            f.AppendLine("bool MyProcedure() {");
            f.AppendLine("   expect \"My Expectations\": (4 > 10);");
            f.AppendLine("   return true;");
            f.AppendLine("}");
            file = FileBuilder.ParseFile(null, f.ToString());
            procedure = file.GetFileElement<IFileProcedure>("MyProcedure"); ;

            taskContext = ExecutionHelper.ExeContext();
            result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(false, (bool)result);

            // Without title and expect is passing
            f = new StringBuilder();
            f.AppendLine("bool MyProcedure() {");
            f.AppendLine("   expect (4 < 10);");
            f.AppendLine("   return true;");
            f.AppendLine("}");
            file = FileBuilder.ParseFile(null, f.ToString());
            procedure = file.GetFileElement<IFileProcedure>("MyProcedure"); ;

            taskContext = ExecutionHelper.ExeContext();
            result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(true, (bool)result);

            // Without title and expect is failing
            f = new StringBuilder();
            f.AppendLine("bool MyProcedure() {");
            f.AppendLine("   expect (4 > 10);");
            f.AppendLine("   return true;");
            f.AppendLine("}");
            file = FileBuilder.ParseFile(null, f.ToString());
            procedure = file.GetFileElement<IFileProcedure>("MyProcedure"); ;

            taskContext = ExecutionHelper.ExeContext();
            result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(false, (bool)result);
        }

        [TestMethod]
        public void TestExpectWithEqualsStatement()
        {
            // With title and expect is passing
            var f = new StringBuilder();
            f.AppendLine("bool MyProcedure() {");
            f.AppendLine("   int x = 10;");
            f.AppendLine("   expect (x == 10);");
            f.AppendLine("   return this.HasFails;");
            f.AppendLine("}");
            var file = FileBuilder.ParseFile(null, f.ToString());
            var procedure = file.GetFileElement<IFileProcedure>("MyProcedure"); ;

            var taskContext = ExecutionHelper.ExeContext();
            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(false, (bool)result);
        }
    }
}