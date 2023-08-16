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
    public class TestProcedure_While
    {
        [TestMethod]
        public void TestProcedureWhileStatementWithExpression()
        {
            var proc = FileBuilder.ParseProcedureExpectNoErrors(
                "int Func(){ var output = 0; while ( output < 100) output += 12; return output; }");
            Assert.AreEqual(typeof(long), proc.ReturnType.Type);
            Assert.AreEqual(0, proc.Parameters.Length);
            object result = proc.Call();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(long));
            Assert.AreEqual(108L, (long)result);
        }

        [TestMethod]
        public void TestProcedureWhileStatementWithBlock()
        {
            var proc = FileBuilder.ParseProcedureExpectNoErrors(
                "int Func(){ var output = 0; var n = 0; while (n < 10) { output += 13; n++; } return output; }");
            Assert.AreEqual(typeof(long), proc.ReturnType.Type);
            Assert.AreEqual(0, proc.Parameters.Length);
            object result = proc.Call();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(long));
            Assert.AreEqual(130L, (long)result);
        }

        [TestMethod]
        public void TestProcedureWhileStatementWithBlockAndBreak()
        {
            var proc = FileBuilder.ParseProcedureExpectNoErrors(
                "int Func(){ var output = 3; var n = 0; while (n < 1000) { output += 13; break; } return output; }");
            Assert.AreEqual(typeof(long), proc.ReturnType.Type);
            Assert.AreEqual(0, proc.Parameters.Length);
            object result = proc.Call();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(long));
            Assert.AreEqual(16L, (long)result);
        }

        [TestMethod]
        public void TestProcedureWhileStatementWithBlockAndConditionalBreak()
        {
            var proc = FileBuilder.ParseProcedureExpectNoErrors(
                "int Func(){ var output = 5; var n = 0; while (n < 1000) { output += 13; n++; if (n >= 4) break; } return output; }");
            Assert.AreEqual(typeof(long), proc.ReturnType.Type);
            Assert.AreEqual(0, proc.Parameters.Length);
            object result = proc.Call();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(long));
            Assert.AreEqual(57L, (long)result);
        }

        [TestMethod]
        public void TestProcedureWhileStatementWithTimeout()
        {
            var proc = FileBuilder.ParseProcedureExpectNoErrors(
                "int Func(){ var n = 0; " +
                "while (true) :" +
                "    Timeout: 20ms" +
                "{ n++; if (n > 5000000) break; }" +        // ENSURE THIS TAKES MORE THAN 20ms !!
                "return n; }");
            Assert.AreEqual(typeof(long), proc.ReturnType.Type);
            Assert.AreEqual(0, proc.Parameters.Length);

            object result = proc.Call();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(long));
            Assert.IsTrue((long)result < 5000000);
            Assert.IsTrue((long)result > 1000);         // Just to check that the loop iterated several times
        }

        [TestMethod]
        public void TestProcedureWhileStatementWithTimeoutFromVariable()
        {
            var proc = FileBuilder.ParseProcedureExpectNoErrors(
                "int Func(){ var n = 0; " +
                "var t = 20ms; " +
                "while (true) :" +
                "    Timeout: t" +
                "{ n++; if (n > 5000000) break; }" +        // ENSURE THIS TAKES MORE THAN 20ms !!
                "return n; }");
            Assert.AreEqual(typeof(long), proc.ReturnType.Type);
            Assert.AreEqual(0, proc.Parameters.Length);

            object result = proc.Call();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(long));
            Assert.IsTrue((long)result < 5000000);
            Assert.IsTrue((long)result > 1000);         // Just to check that the loop iterated several times
        }

        // "[Title: \"Looping for a while\"] [Timeout: 2s] [Break: \"Found\", Break: \"Error\", CountVar: c]" +

        [TestMethod]
        [Ignore("Interactive interface not implemented")]
        public void TestProcedureWhileStatementWithSpecifiedIterationVariable()
        {
            var proc = FileBuilder.ParseProcedureExpectNoErrors(
                "int Func(){ var n = 9; var m = 0; var c = 0;" +
                "while (true) : index: c" +
                "{ n++; if (n >= 16) break; m = c; }" +
                "return c + m; }");
            Assert.AreEqual(typeof(long), proc.ReturnType.Type);
            Assert.AreEqual(0, proc.Parameters.Length);

            object result = proc.Call();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(long));
            Assert.AreEqual(5, (long)result);
        }

        [TestMethod]
        [Ignore("Interactive interface not implemented")]
        public void TestProcedureWhileStatementWithInteractiveBreak()
        {
            var proc = FileBuilder.ParseProcedureExpectNoErrors(
                "int Func(){ var n = 0;" +
                "[Break: \"Stop\"]" +
                "while (true) : Break: \"Stop\"" +
                "{ n++; if (n >= 100) break; }" +
                "return n; }");
            Assert.AreEqual(typeof(long), proc.ReturnType.Type);
            Assert.AreEqual(0, proc.Parameters.Length);

            object result = proc.Call();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(long));
            Assert.AreEqual(5, (long)result);
        }

        [TestMethod]
        public void TestProcedureWhileStatementDouble01()
        {
            var proc = FileBuilder.ParseProcedureExpectNoErrors(
                """
                int Func()
                {
                    int i = 0;
                    while (i < 1000)
                    {
                        i++;
                    }
                    int i = 0;
                    while (i < 1500)
                    {
                        i++;
                    }
                    return i;
                }
                """);
            Assert.AreEqual(typeof(long), proc.ReturnType.Type);

            object result = proc.Call();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(long));
            Assert.AreEqual(1500, (long)result);
        }

        [TestMethod]
        public void TestProcedureWhileStatementDouble02()
        {
            var proc = FileBuilder.ParseProcedureExpectNoErrors(
                """
                int Func()
                {
                    int i = 0;
                    while (i < 1000)
                    {
                        i++;
                    }
                    int j = 0;
                    while (j < 1500)
                    {
                        j++;
                    }
                    return j;
                }
                """);
            Assert.AreEqual(typeof(long), proc.ReturnType.Type);

            object result = proc.Call();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(long));
            Assert.AreEqual(1500, (long)result);
        }
    }
}
