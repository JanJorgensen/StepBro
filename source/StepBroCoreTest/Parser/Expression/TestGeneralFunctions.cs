using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    }
}