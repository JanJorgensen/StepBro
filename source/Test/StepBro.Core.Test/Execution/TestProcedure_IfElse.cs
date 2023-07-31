using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core.Data;
using StepBro.Core.Logging;
using StepBro.Core.Parser;
using StepBro.Core.ScriptData;
using StepBroCoreTest.Data;
using StepBroCoreTest.Utils;

namespace StepBroCoreTest.Parser
{
    [TestClass]
    public class TestProcedure_IfElse
    {
        [TestMethod]
        public void TestProcedureSimpleIfStatement()
        {
            var proc = FileBuilder.ParseProcedureExpectNoErrors(
                "int Func(bool input){ var result = 0; if (input) result = 715; return result; }");
            Assert.AreEqual(typeof(long), proc.ReturnType.Type);
            Assert.AreEqual(1, proc.Parameters.Length);

            object result = proc.Call(false);
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(long));
            Assert.AreEqual(0L, (long)result);

            result = proc.Call(true);
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(long));
            Assert.AreEqual(715L, (long)result);
        }

        [TestMethod]
        public void TestProcedureSimpleIfStatementWithBlock()
        {
            var proc = FileBuilder.ParseProcedureExpectNoErrors(
                "int Func(bool input){ var result = 0; if (input) { result = 223; } return result; }");
            Assert.AreEqual(typeof(long), proc.ReturnType.Type);
            Assert.AreEqual(1, proc.Parameters.Length);

            object result = proc.Call(false);
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(long));
            Assert.AreEqual(0L, (long)result);

            result = proc.Call(true);
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(long));
            Assert.AreEqual(223L, (long)result);
        }

        [TestMethod]
        public void TestProcedureSimpleIfElseStatement()
        {
            var proc = FileBuilder.ParseProcedureExpectNoErrors(
                "int Func(bool input){ var result = 0; if (input) result = 175; else result = 126; return result; }");
            Assert.AreEqual(typeof(long), proc.ReturnType.Type);
            Assert.AreEqual(1, proc.Parameters.Length);

            object result = proc.Call(false);
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(long));
            Assert.AreEqual(126L, (long)result);

            result = proc.Call(true);
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(long));
            Assert.AreEqual(175L, (long)result);
        }

        [TestMethod]
        public void TestProcedureSimpleIfElseStatementWithBlock()
        {
            var proc = FileBuilder.ParseProcedureExpectNoErrors(
                "int Func(bool input)",
                "{  var result = 0;",
                "   if (input) { result = 626; }",
                "   else { result = 262; }",
                "   return result; ",
                "}");
            Assert.AreEqual(typeof(long), proc.ReturnType.Type);
            Assert.AreEqual(1, proc.Parameters.Length);

            object result = proc.Call(false);
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(long));
            Assert.AreEqual(262L, (long)result);

            result = proc.Call(true);
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(long));
            Assert.AreEqual(626L, (long)result);
        }

        [TestMethod]
        public void TestProcedureIfElseWithAwaitObjectFunctionStatements()
        {
            var f = new StringBuilder();
            f.AppendLine("using " + typeof(DummyClass).Namespace + ";");
            f.AppendLine("public " + nameof(DummyClass) + " myObject = " + nameof(DummyClass) + "{}");
            f.AppendLine("procedure void Func(bool state)");
            f.AppendLine("{");
            f.AppendLine("   if (state) await myObject." + nameof(DummyClass.MethodAsyncObject) + "(\"Anders\");");
            f.AppendLine("   else await myObject." + nameof(DummyClass.MethodAsyncObject) + "(\"Bent\");");
            f.AppendLine("}");
            var files = FileBuilder.ParseFiles((ILogger)null, this.GetType().Assembly,
                new Tuple<string, string>("myfile.sbs", f.ToString()));

            var procedure = files[0].ListElements().First(p => p.Name == "Func") as IFileProcedure;
            Assert.AreEqual("Func", procedure.Name);
            var taskContext = ExecutionHelper.ExeContext();
            taskContext.CallProcedure(procedure, true);

            var log = new LogInspector(taskContext.Logger);
            log.DebugDump();

            log.ExpectNext("0 - Pre - TestRun - Starting");
            log.ExpectNext("1 - Pre - myfile.Func - ( True )");
            log.ExpectNext("2 - Normal - 5 myObject.MethodAsyncObject - Yup: Anders");
            log.ExpectNext("2 - Post");
            log.ExpectEnd();
        }

        [TestMethod]
        public void TestProcedureIfWithErrorInCondition()
        {
            var proc = FileBuilder.ParseProcedure(
                "int Func()",
                "{  var result = 22;",
                "   if (undefinedVariable == 42) { result = 626; }",
                "   else { result = 262; }",
                "   return result; ",
                "}");
            Assert.AreEqual(typeof(long), proc.ReturnType.Type);
            Assert.AreEqual(1, FileBuilder.LastInstance.Errors.ErrorCount);
        }
    }
}
