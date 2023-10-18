using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core.Data;
using StepBro.Core.Parser;
using StepBro.Core.ScriptData;

namespace StepBroCoreTest
{
    [TestClass]
    public class TestExecutionSupportMethods
    {
        [TestMethod]
        public void TestThisReference()
        {
            var f = new StringBuilder();
            f.AppendLine("bool MyProcedure() {");
            f.AppendLine("   return this.HasFails;");
            f.AppendLine("}");
            var file = FileBuilder.ParseFile(null, f.ToString());
            var procedure = file.GetFileElement<IFileProcedure>("MyProcedure");

            var taskContext = ExecutionHelper.ExeContext();
            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(false, (bool)result);
        }

        [TestMethod]
        public void TestThisReferenceProcedureName()
        {
            var f = new StringBuilder();
            f.AppendLine("string MyProcedure() {");
            f.AppendLine("   return this.Name;");
            f.AppendLine("}");
            var file = FileBuilder.ParseFile(null, f.ToString());
            var procedure = file.GetFileElement<IFileProcedure>("MyProcedure"); ;

            var taskContext = ExecutionHelper.ExeContext();
            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual("MyProcedure", (string)result);
        }


        [TestMethod]
        public void TestSetResult()
        {
            var f = new StringBuilder();
            f.AppendLine("void MyProc() {");
            f.AppendLine("   this.SetResult(fail, \"Something very wrong!!\");");
            f.AppendLine("}");
            var file = FileBuilder.ParseFile(null, f.ToString());
            var procedure = file.GetFileElement<IFileProcedure>("MyProc");

            var taskContext = ExecutionHelper.ExeContext();
            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(Verdict.Fail, taskContext.Result.Verdict);
            Assert.AreEqual("Something very wrong!!", taskContext.Result.Description);
        }
    }
}