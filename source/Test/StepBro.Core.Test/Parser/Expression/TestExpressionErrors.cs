using Microsoft.VisualStudio.TestTools.UnitTesting;
using static StepBroCoreTest.Parser.ExpressionParser;

namespace StepBroCoreTest.Parser
{
    [TestClass]
    public class TestExpressionErrors
    {
        [TestMethod]
        public void ErrorReturnUnknownIdentifier()
        {
            var result = Parse<long>("varUnknownName", varGeneration: false);
            Assert.AreEqual(1, result.Errors.ErrorCount);
            Assert.AreEqual(3, result.Errors[0].Line);
            Assert.AreEqual("Unresolved identifier: \"varUnknownName\".", result.Errors[0].Message);
        }

        [TestMethod]
        public void ErrorAssignToUnknownVariable()
        {
            var result = Parse<long>("0", "varUnknownName = 13;", varGeneration: false);
            Assert.AreEqual(1, result.Errors.ErrorCount);
            Assert.AreEqual(2, result.Errors[0].Line);
            Assert.AreEqual("Unresolved identifier: \"varUnknownName\".", result.Errors[0].Message);
        }

        [TestMethod]
        public void ErrorInitializeWithUnknownIdentifier()
        {
            var result = Parse<long>("0", "int per = spidskommen;", varGeneration: false);
            Assert.AreEqual(1, result.Errors.ErrorCount);
            Assert.AreEqual(2, result.Errors[0].Line);
            Assert.AreEqual("Unresolved identifier: \"spidskommen\".", result.Errors[0].Message);
        }

        [TestMethod]
        public void ErrorAssignWithUnknownIdentifier()
        {
            var result = Parse<long>("0", "int per = 0;\r\n per = spidskommen;", varGeneration: false);
            Assert.AreEqual(1, result.Errors.ErrorCount);
            Assert.AreEqual(3, result.Errors[0].Line);
            Assert.AreEqual("Unresolved identifier: \"spidskommen\".", result.Errors[0].Message);
        }
    }
}
