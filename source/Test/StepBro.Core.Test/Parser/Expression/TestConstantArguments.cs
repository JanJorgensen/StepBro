using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using StepBro.Core.Parser;
using StepBro.Core.Data;

namespace StepBroCoreTest.Parser
{
    [TestClass]
    public class TestConstantArguments
    {
        [TestMethod]
        public void ParseSingleArgument()
        {
            var result = FileBuilder.ParseSimpleArguments("()");
            Assert.AreEqual(0, result.Item2.ErrorCount);
            Assert.AreEqual(0, result.Item1.Count);

            result = FileBuilder.ParseSimpleArguments("(635)");
            Assert.AreEqual(0, result.Item2.ErrorCount);
            Assert.AreEqual(1, result.Item1.Count);
            Assert.AreEqual(SBExpressionType.Constant, result.Item1.Peek().ReferencedType);
            Assert.AreEqual(typeof(long), result.Item1.Peek().DataType.Type);
            Assert.AreEqual(635L, result.Item1.Peek().Value);

            result = FileBuilder.ParseSimpleArguments("(true)");
            Assert.AreEqual(0, result.Item2.ErrorCount);
            Assert.AreEqual(1, result.Item1.Count);
            Assert.AreEqual(SBExpressionType.Constant, result.Item1.Peek().ReferencedType);
            Assert.AreEqual(typeof(bool), result.Item1.Peek().DataType.Type);
            Assert.AreEqual(true, result.Item1.Peek().Value);

            result = FileBuilder.ParseSimpleArguments("(125ms)");
            Assert.AreEqual(0, result.Item2.ErrorCount);
            Assert.AreEqual(1, result.Item1.Count);
            Assert.AreEqual(SBExpressionType.Constant, result.Item1.Peek().ReferencedType);
            Assert.AreEqual(typeof(TimeSpan), result.Item1.Peek().DataType.Type);
            Assert.AreEqual(TimeSpan.FromTicks(TimeSpan.TicksPerMillisecond * 125), result.Item1.Peek().Value);

            result = FileBuilder.ParseSimpleArguments("('absolut')");
            Assert.AreEqual(0, result.Item2.ErrorCount);
            Assert.AreEqual(1, result.Item1.Count);
            Assert.AreEqual(SBExpressionType.Constant, result.Item1.Peek().ReferencedType);
            Assert.AreEqual(typeof(Identifier), result.Item1.Peek().DataType.Type);
            Assert.AreEqual("absolut", ((Identifier)(result.Item1.Peek().Value)).Name);
        }

        [TestMethod]
        public void ParseArgumentsWithoutSeparator()
        {
            var result = FileBuilder.ParseSimpleArguments("(1 2)");
            Assert.AreEqual(2, result.Item2.ErrorCount);
        }
    }
}
