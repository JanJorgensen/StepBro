using Microsoft.VisualStudio.TestTools.UnitTesting;
using static StepBroCoreTest.Parser.ExpressionParser;

namespace StepBroCoreTest.Parser
{
    [TestClass]
    public class TestExpressionErrors
    {
        [TestMethod]
        public void ErrorAssignWithUnknownIdentifier()
        {
            var result = Parse<long>("varUnknownName", varGeneration: false);
            Assert.AreEqual(1, result.Errors.ErrorCount);
            Assert.AreEqual(3, result.Errors[0].Line);
        }
    }
}
