using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core;
using StepBro.Core.Data;
using StepBro.Core.Parser;

namespace StepBroCoreTest.Parser
{
    [TestClass]
    public class TestProcedure_While
    {
        [TestInitialize]
        public void Setup()
        {
            ServiceManager.Dispose();
        }

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
        public void TestProcedureWhileStatementWithTimeout02()
        {
            var proc = FileBuilder.ParseProcedureExpectNoErrors(
                "int Func(){ var n = 0; " +
                "while (n < 5000000) :" +
                "    Timeout: 20ms" +
                "{ n++; }" +        // ENSURE THIS TAKES MORE THAN 20ms !!
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

        [TestMethod]
        public void TestProcedureWhileStatementWithTimeoutFromVariable02()
        {
            var proc = FileBuilder.ParseProcedureExpectNoErrors(
                "int Func(){ var n = 0; " +
                "var t = 20ms; " +
                "while (n < 5000000) :" +
                "    Timeout: t" +
                "{ n++; }" +        // ENSURE THIS TAKES MORE THAN 20ms !!
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
        public void TestProcedureWhileStatementStoppable()
        {
            var proc = FileBuilder.ParseProcedureExpectNoErrors(
                "int Func(){ var n = 0;" +
                "while (true) : Stoppable " +
                "{ n++; if (n >= 100) break; }" +
                "return n; }");
            Assert.AreEqual(typeof(long), proc.ReturnType.Type);

            object result = proc.Call();
            Assert.IsInstanceOfType(result, typeof(long));
            Assert.IsTrue((long)result >= 100L);

            // TODO: Check that loop can be stopped.
        }

        [TestMethod]
        [Ignore("Nice way to set stop signal must be found")]
        public void TestProcedureWhileStatementStoppable_DoStop()
        {
            // TODO: Check that loop can be stopped. Just implement in the precious test case.
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
                    i = 0;
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

        [TestMethod]
        public void TestProcedureWhileStatementVariableInLoop01()
        {
            var proc = FileBuilder.ParseProcedureExpectNoErrors(
                """
                int Func()
                {
                    int i = 0;
                    while (i < 1000)
                    {
                        int a = 5;

                        i += a;
                    }
                    return i;
                }
                """);
            Assert.AreEqual(typeof(long), proc.ReturnType.Type);

            object result = proc.Call();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(long));
            Assert.AreEqual(1000, (long)result);
        }

        [TestMethod]
        public void TestProcedureWhileStatementContinue01()
        {
            var proc = FileBuilder.ParseProcedureExpectNoErrors(
                """
                int Func()
                {
                    int i = 0;
                    int output = 0;
                    while (i < 1000)
                    {
                        int a = 5;

                        if (i == 200)
                        {
                            output += 5;
                            i += 15;
                            continue;
                        }

                        output += a;
                        i += a;
                    }
                    return output;
                }
                """);
            Assert.AreEqual(typeof(long), proc.ReturnType.Type);

            object result = proc.Call();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(long));
            Assert.AreEqual(990, (long)result);
        }

        [TestMethod]
        public void TestProcedureWhileStatementSingleStatementInWhileLoop01()
        {
            var proc = FileBuilder.ParseProcedureExpectNoErrors(
                """
                int Func()
                {
                    int i = 0;
                    int j = 0;
                    int output = 0;

                    while (i < 250)
                        while (j < 500)
                        {
                            output += 2;
                            i++;
                            j++;
                        }
                        
                    return output;
                }
                """);
            Assert.AreEqual(typeof(long), proc.ReturnType.Type);

            object result = proc.Call();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(long));
            Assert.AreEqual(1000, (long)result);
        }

        [TestMethod]
        public void TestProcedureWhileStatementSingleStatementInWhileLoop02()
        {
            var proc = FileBuilder.ParseProcedureExpectNoErrors(
                """
                int Func()
                {
                    int i = 0;
                    int j = 0;
                    int output = 0;

                    while (i < 1000)
                    {
                        while (j < 500)
                        {
                            output += 2;
                            i++;
                            j++;
                        }
                        j = 0;
                    }
                        
                    return output;
                }
                """);
            Assert.AreEqual(typeof(long), proc.ReturnType.Type);

            object result = proc.Call();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(long));
            Assert.AreEqual(2000, (long)result);
        }
    }
}
