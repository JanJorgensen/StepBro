using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core.Data;
using StepBro.Core.Parser;
using static StepBroCoreTest.Parser.ExpressionParser;

namespace StepBroCoreTest.Parser
{
    [TestClass]
    public class TestBooleanExpression
    {
        [TestMethod]
        public void TestBooleanSimpleConstantOperations()
        {
            Assert.AreEqual(true, ParseAndRun<bool>("true"));
            Assert.AreEqual(false, ParseAndRun<bool>("false"));
        }

        [TestMethod]
        public void TestBooleanSimpleVariableOperations()
        {
            Assert.AreEqual(true, ParseAndRun<bool>("varBoolA"));
            Assert.AreEqual(false, ParseAndRun<bool>("varBoolB"));

            Assert.AreEqual(false, ParseAndRun<bool>("!varBoolA"));
            Assert.AreEqual(true, ParseAndRun<bool>("!varBoolB"));

            Assert.AreEqual(false, ParseAndRun<bool>("not varBoolA"));
            Assert.AreEqual(true, ParseAndRun<bool>("not varBoolB"));
        }
    }
}
