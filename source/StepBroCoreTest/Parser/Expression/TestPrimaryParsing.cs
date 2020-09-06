using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core.Parser;

namespace StepBroCoreTest.Parser
{
    [TestClass]
    public class TestPrimaryParsing
    {
        [TestMethod]
        public void TestPrimaryQualifiedName()
        {
            var result = FileBuilder.ParsePrimary("Mor");
            Assert.AreEqual(SBExpressionType.UnknownIdentifier, result.ReferencedType);
            Assert.IsTrue(result.Value is string);
            Assert.AreEqual("Mor", (string)result.Value);

            result = FileBuilder.ParsePrimary("K9236326");
            Assert.AreEqual(SBExpressionType.UnknownIdentifier, result.ReferencedType);
            Assert.IsTrue(result.Value is string);
            Assert.AreEqual("K9236326", (string)result.Value);

            result = FileBuilder.ParsePrimary("lireighreghregh43lh54334h54l54l3trnl4urtl3tr43lnf4lu3nlrn43l4uht43t4uh5r4");
            Assert.AreEqual(SBExpressionType.UnknownIdentifier, result.ReferencedType);
            Assert.IsTrue(result.Value is string);
            Assert.AreEqual("lireighreghregh43lh54334h54l54l3trnl4urtl3tr43lnf4lu3nlrn43l4uht43t4uh5r4", (string)result.Value);
        }
    }
}
