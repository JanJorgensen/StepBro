using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBroCoreTest.Parser
{
    [TestClass]
    public class TestProcedure_For
    {
        [TestMethod]
        public void TestProcedureForStatementWithExpression01()
        {
            var proc = FileBuilder.ParseProcedureExpectNoErrors(
                """
                int Func()
                {
                    var output = 200;
                    for (output = 0; output < 100; output += 12)
                    {
                        output++;
                    }
                    return output;
                }
                """);
            Assert.AreEqual(typeof(long), proc.ReturnType.Type);
            Assert.AreEqual(0, proc.Parameters.Length);
            object result = proc.Call();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(long));
            Assert.AreEqual(104L, (long)result);
        }

        [TestMethod]
        public void TestProcedureForStatementWithExpression02()
        {
            var proc = FileBuilder.ParseProcedureExpectNoErrors(
                """
                int Func()
                {
                    var output = 0;
                    for (var i = 0; i < 100; i += 12)
                    {
                        output += 12;
                    }
                    return output;
                }
                """);
            Assert.AreEqual(typeof(long), proc.ReturnType.Type);
            Assert.AreEqual(0, proc.Parameters.Length);
            object result = proc.Call();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(long));
            Assert.AreEqual(108L, (long)result);
        }

        [TestMethod]
        public void TestProcedureForStatementWithExpression03()
        {
            var proc = FileBuilder.ParseProcedureExpectNoErrors(
                """
                int Func()
                {
                    var output = 0;
                    for (var i = 0, var j = 0; i < 100; i += 12)
                    {
                        output += 12;
                    }
                    return output;
                }
                """);
            Assert.AreEqual(typeof(long), proc.ReturnType.Type);
            Assert.AreEqual(0, proc.Parameters.Length);
            object result = proc.Call();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(long));
            Assert.AreEqual(108L, (long)result);
        }

        [TestMethod]
        public void TestProcedureForStatementWithExpression04()
        {
            var proc = FileBuilder.ParseProcedureExpectNoErrors(
                """
                int Func()
                {
                    var output = 0;
                    var i = 0;
                    for (; i < 100; i += 12)
                    {
                        output += 12;
                    }
                    return output;
                }
                """);
            Assert.AreEqual(typeof(long), proc.ReturnType.Type);
            Assert.AreEqual(0, proc.Parameters.Length);
            object result = proc.Call();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(long));
            Assert.AreEqual(108L, (long)result);
        }

        [TestMethod]
        public void TestProcedureForStatementWithExpression05()
        {
            var proc = FileBuilder.ParseProcedureExpectNoErrors(
                """
                int Func()
                {
                    var output = 0;
                    for (var i = 0; i < 100; i += 12)
                    {
                        var a = 5;

                        output += 12;
                    }
                    return output;
                }
                """);
            Assert.AreEqual(typeof(long), proc.ReturnType.Type);
            Assert.AreEqual(0, proc.Parameters.Length);
            object result = proc.Call();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(long));
            Assert.AreEqual(108L, (long)result);
        }

        [TestMethod]
        public void TestProcedureForStatementWithExpression06()
        {
            var proc = FileBuilder.ParseProcedureExpectNoErrors(
                """
                int Func()
                {
                    var output = 0;
                    for (var i = 0; i < 100; i += 35)
                    {
                        output += 12;
                    }
                    return output;
                }
                """);
            Assert.AreEqual(typeof(long), proc.ReturnType.Type);
            Assert.AreEqual(0, proc.Parameters.Length);
            object result = proc.Call();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(long));
            Assert.AreEqual(36L, (long)result);
        }

        [TestMethod]
        public void TestProcedureForStatementWithExpression07()
        {
            var proc = FileBuilder.ParseProcedureExpectNoErrors(
                """
                int Func()
                {
                    var output = 0;
                    for (var i = 0; i < 100; i += 35)
                    {
                        if (i == 35)
                        {
                            i = 105;
                        }
                        else
                        {
                            output += 12;
                        }
                    }
                    return output;
                }
                """);
            Assert.AreEqual(typeof(long), proc.ReturnType.Type);
            Assert.AreEqual(0, proc.Parameters.Length);
            object result = proc.Call();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(long));
            Assert.AreEqual(12L, (long)result);
        }

        [TestMethod]
        public void TestProcedureForStatementWithExpression08()
        {
            var proc = FileBuilder.ParseProcedureExpectNoErrors(
                """
                int Func()
                {
                    var output = 0;
                    for (var i = 0; i < 100; i += 35)
                        output += i;
                    return output;
                }
                """);
            Assert.AreEqual(typeof(long), proc.ReturnType.Type);
            Assert.AreEqual(0, proc.Parameters.Length);
            object result = proc.Call();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(long));
            Assert.AreEqual(105L, (long)result);
        }

        [TestMethod]
        public void TestProcedureForStatementWithEmptyBody01()
        {
            var proc = FileBuilder.ParseProcedureExpectNoErrors(
                """
                int Func()
                {
                    var output = 0;
                    var i = 0;
                    for (; i < 100; i += 12, output += 12)
                    {
                    }
                    return output;
                }
                """);
            Assert.AreEqual(typeof(long), proc.ReturnType.Type);
            Assert.AreEqual(0, proc.Parameters.Length);
            object result = proc.Call();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(long));
            Assert.AreEqual(108L, (long)result);
        }

        [TestMethod]
        public void TestProcedureForStatementWithOnlyCondition01()
        {
            var proc = FileBuilder.ParseProcedureExpectNoErrors(
                """
                int Func()
                {
                    var output = 0;
                    var i = 0;
                    for (; i < 100;)
                    {
                        i += 12;
                        output += 12;
                    }
                    return output;
                }
                """);
            Assert.AreEqual(typeof(long), proc.ReturnType.Type);
            Assert.AreEqual(0, proc.Parameters.Length);
            object result = proc.Call();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(long));
            Assert.AreEqual(108L, (long)result);
        }

        [TestMethod]
        public void TestProcedureForStatementWithOnlyCondition02()
        {
            var proc = FileBuilder.ParseProcedureExpectNoErrors(
                """
                int Func()
                {
                    var output = 0;
                    var i = 0;
                    for (; i < 100;)
                    {
                        var a = 5;

                        i += 12;
                        output += 12;
                    }
                    return output;
                }
                """);
            Assert.AreEqual(typeof(long), proc.ReturnType.Type);
            Assert.AreEqual(0, proc.Parameters.Length);
            object result = proc.Call();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(long));
            Assert.AreEqual(108L, (long)result);
        }

        [TestMethod]
        public void TestProcedureForStatementWithInitAndCondition01()
        {
            var proc = FileBuilder.ParseProcedureExpectNoErrors(
                """
                int Func()
                {
                    var output = 0;
                    
                    for (var i = 0; i < 100;)
                    {
                        i += 12;
                        output += 12;
                    }

                    return output;
                }
                """);
            Assert.AreEqual(typeof(long), proc.ReturnType.Type);
            Assert.AreEqual(0, proc.Parameters.Length);
            object result = proc.Call();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(long));
            Assert.AreEqual(108L, (long)result);
        }

        [TestMethod]
        public void TestProcedureForStatementWithContinue01()
        {
            var proc = FileBuilder.ParseProcedureExpectNoErrors(
                """
                int Func()
                {
                    var output = 0;
                    
                    for (var i = 0; i < 100; i += 35)
                    {
                        output += 12;

                        if (output == 12)
                        {
                            output++;
                            continue;
                        }

                        output += 5;
                    }

                    return output;
                }
                """);
            Assert.AreEqual(typeof(long), proc.ReturnType.Type);
            Assert.AreEqual(0, proc.Parameters.Length);
            object result = proc.Call();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(long));
            Assert.AreEqual(47L, (long)result);
        }

        [TestMethod]
        public void TestProcedureForStatementWithinForStatement01()
        {
            var proc = FileBuilder.ParseProcedureExpectNoErrors(
                """
                int Func()
                {
                    var output = 0;
                    for (var i = 0; i < 100; i += 12)
                    {
                        output += 12;
                        for (var j = 0; j < 200; j += 4)
                        {
                            output += 1;
                        }
                    }
                    return output;
                }
                """);
            Assert.AreEqual(typeof(long), proc.ReturnType.Type);
            Assert.AreEqual(0, proc.Parameters.Length);
            object result = proc.Call();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(long));
            Assert.AreEqual(558L, (long)result);
        }
    }
}
