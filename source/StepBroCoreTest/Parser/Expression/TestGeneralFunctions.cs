using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core.Parser;
using System;
using static StepBroCoreTest.Parser.ExpressionParser;

namespace StepBroCoreTest.Parser
{
    [TestClass]
    public class TestGeneralFunctions
    {
        [TestMethod]
        public void ExecuteDelay()
        {
            ParseAndRun<long>(statements: "delay(200ms); ", varGeneration: false);
            Assert.IsTrue(LastExecutionTime >= TimeSpan.FromTicks(TimeSpan.TicksPerMillisecond * 200));
            Assert.IsTrue(LastExecutionTime < TimeSpan.FromTicks(TimeSpan.TicksPerMillisecond * 220));
        }
        [TestMethod]
        public void ExecuteDelay2()
        {
            var proc = FileBuilder.ParseProcedure(
                "void Func(){ delay(200ms); }");

            Assert.AreEqual(typeof(void), proc.ReturnType.Type);
            Assert.AreEqual(0, proc.Parameters.Length);

            object result = proc.Call();
            Assert.IsNull(result);
        }
    }
}