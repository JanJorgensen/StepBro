using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core.Parser;
using StepBro.Core.ScriptData;

namespace StepBroCoreTest
{
    [TestClass]
    public class TestProcedure_Expect
    {
        [TestMethod]
        public void TestExpectStatement()
        {
            // With title and expect is passing
            var f = new StringBuilder();
            f.AppendLine("bool MyProcedure() {");
            f.AppendLine("   expect \"My Expectations\": (4 < 10);");
            f.AppendLine("   return this.HasFails;");
            f.AppendLine("}");
            var file = FileBuilder.ParseFile(null, f.ToString());
            var procedure = file.GetFileElement<IFileProcedure>("MyProcedure"); ;

            var taskContext = ExecutionHelper.ExeContext();
            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(false, (bool)result);

            // With title and expect is failing
            f = new StringBuilder();
            f.AppendLine("bool MyProcedure() {");
            f.AppendLine("   expect \"My Expectations\": (4 > 10);");
            f.AppendLine("   return this.HasFails;");
            f.AppendLine("}");
            file = FileBuilder.ParseFile(null, f.ToString());
            procedure = file.GetFileElement<IFileProcedure>("MyProcedure"); ;

            taskContext = ExecutionHelper.ExeContext();
            result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(true, (bool)result);

            // Without title and expect is passing
            f = new StringBuilder();
            f.AppendLine("bool MyProcedure() {");
            f.AppendLine("   expect (4 < 10);");
            f.AppendLine("   return this.HasFails;");
            f.AppendLine("}");
            file = FileBuilder.ParseFile(null, f.ToString());
            procedure = file.GetFileElement<IFileProcedure>("MyProcedure"); ;

            taskContext = ExecutionHelper.ExeContext();
            result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(false, (bool)result);

            // Without title and expect is failing
            f = new StringBuilder();
            f.AppendLine("bool MyProcedure() {");
            f.AppendLine("   expect (4 > 10);");
            f.AppendLine("   return this.HasFails;");
            f.AppendLine("}");
            file = FileBuilder.ParseFile(null, f.ToString());
            procedure = file.GetFileElement<IFileProcedure>("MyProcedure"); ;

            taskContext = ExecutionHelper.ExeContext();
            result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(true, (bool)result);
        }
    }
}