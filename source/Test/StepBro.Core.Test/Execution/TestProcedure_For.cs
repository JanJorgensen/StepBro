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
    }
}
