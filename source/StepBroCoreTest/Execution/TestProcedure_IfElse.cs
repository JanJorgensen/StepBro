using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core.Data;
using StepBro.Core.Parser;

namespace StepBroCoreTest.Parser
{
    [TestClass]
    public class TestProcedure_IfElse
    {
        [TestMethod]
        public void TestProcedureSimpleIfStatement()
        {
            var proc = FileBuilder.ParseProcedure(
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
            var proc = FileBuilder.ParseProcedure(
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
            var proc = FileBuilder.ParseProcedure(
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
            var proc = FileBuilder.ParseProcedure(
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
    }
}
