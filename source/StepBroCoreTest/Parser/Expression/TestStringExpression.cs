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
    public class TestStringExpression
    {
        [TestMethod]
        public void TestStringSimpleConstantOperations()
        {
            Assert.AreEqual("Hej12", ParseAndRun<string>("\"Hej\" + 12"));
        }

        [TestMethod]
        public void TestStringSimpleVariableOperations()
        {
            Assert.AreEqual("Vaffel", ParseAndRun<string>("varStringA"));
            Assert.AreEqual("Jan", ParseAndRun<string>("varStringB"));
        }

        [TestMethod]
        public void TestStringPropertyAdd()
        {
            Assert.AreEqual("Prop is: False", ParseAndRun<string>("\"Prop is: \" + varDummyA.PropBool", varDummyClass: true));
        }
    }
}
