using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using StepBro.Core.Data;
using StepBro.Core.Parser;
using static StepBroCoreTest.Parser.ExpressionParser;

namespace StepBroCoreTest.Parser
{
    [TestClass]
    public class TestTypeExpression
    {
        [TestMethod]
        public void ExpressionBasicTypeCast()
        {
            Assert.AreEqual(14.0, ParseAndRun<double>("(decimal)varIntA"));
        }

        [TestMethod]
        public void ExpressionTypeReference()
        {
            this.AssertTypeExpression(typeof(long), "typeof(int)");
            this.AssertTypeExpression(typeof(void), "typeof(void)");
            this.AssertTypeExpression(typeof(string), "typeof(string)");
        }

        [TestMethod]
        public void ExpressionIsOperation()
        {
            Assert.IsTrue(ParseAndRun<bool>("varIntA is int"));
            Assert.IsFalse(ParseAndRun<bool>("varIntA is bool"));
            Assert.IsTrue(ParseAndRun<bool>("varDummyA is DummyClass", varDummyClass: true));
            Assert.IsFalse(ParseAndRun<bool>("varDummyA is string", varDummyClass: true));
        }

        [TestMethod]
        public void ExpressionIsNotOperation()
        {
            Assert.IsFalse(ParseAndRun<bool>("varIntA is not int"));
            Assert.IsTrue(ParseAndRun<bool>("varIntA is not bool"));
            Assert.IsFalse(ParseAndRun<bool>("varDummyA is not DummyClass", varDummyClass: true));
            Assert.IsTrue(ParseAndRun<bool>("varDummyA is not string", varDummyClass: true));
        }

        private void AssertTypeExpression(Type expected, string expression)
        {
            var builder = FileBuilder.ParseExpression(expression);
            var result = builder.Listener.GetExpressionResult();
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is TypeReference);
            Assert.AreEqual(expected, (((TypeReference)result.Value).Type));
        }
    }
}