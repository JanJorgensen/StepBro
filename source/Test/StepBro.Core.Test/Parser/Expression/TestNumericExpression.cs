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
    public class TestNumericExpression
    {
        [TestMethod]
        public void TestIntegerSimpleConstantMath()
        {
            Assert.AreEqual(27L, ParseAndRun<long>("15 + 12", varGeneration: false));
            Assert.AreEqual(7L, ParseAndRun<long>("15 - 8", varGeneration: false));
            Assert.AreEqual(19L, ParseAndRun<long>("15 - (-4)", varGeneration: false));
            Assert.AreEqual(-12L, ParseAndRun<long>("-9 - 3", varGeneration: false));
            Assert.AreEqual(18L, ParseAndRun<long>("3 * 6", varGeneration: false));
            Assert.AreEqual(5L, ParseAndRun<long>("20 / 4", varGeneration: false));
            Assert.AreEqual(3L, ParseAndRun<long>("13 % 5", varGeneration: false));
        }

        [TestMethod]
        public void TestDecimalSimpleConstantMath()
        {
            Assert.AreEqual(15.2 + 12.1, ParseAndRun<double>("15.2 + 12.1", varGeneration: false));
            Assert.AreEqual(15.2 - 8.6, ParseAndRun<double>("15.2 - 8.6", varGeneration: false));
            Assert.AreEqual(15.2 - (-4.8), ParseAndRun<double>("15.2 - (-4.8)", varGeneration: false));
            Assert.AreEqual(-9.2 - 3.7, ParseAndRun<double>("-9.2 - 3.7", varGeneration: false));
            Assert.AreEqual(3.9 * 6.4, ParseAndRun<double>("3.9 * 6.4", varGeneration: false));
            Assert.AreEqual(20.7 / 4.1, ParseAndRun<double>("20.7 / 4.1", varGeneration: false));
            Assert.AreEqual(13.3 % 5.5, ParseAndRun<double>("13.3 % 5.5", varGeneration: false));
        }

        [TestMethod]
        public void TestIntegerSimpleVariableMath()
        {
            Assert.AreEqual(14L, ParseAndRun<long>("varIntA"));
            Assert.AreEqual(-29L, ParseAndRun<long>("varIntB"));

            Assert.AreEqual(2014L, ParseAndRun<long>("varIntA + 2K"));
            Assert.AreEqual(8L, ParseAndRun<long>("varIntA - 6"));
            Assert.AreEqual(-30L, ParseAndRun<long>("varIntA + varIntA + varIntB + varIntB"));
            Assert.AreEqual(43L, ParseAndRun<long>("varIntA - varIntB"));

            Assert.AreEqual(3.753, ParseAndRun<double>("varDecA + varDecB"));
            Assert.AreEqual(3.647, ParseAndRun<double>("varDecA - varDecB"), 0.00000001);

            Assert.AreEqual(17.7, ParseAndRun<double>("varIntA + varDecA"), 0.00000001);
            Assert.AreEqual(-25.3, ParseAndRun<double>("varDecA + varIntB"), 0.00000001);

            Assert.AreEqual(10.3, ParseAndRun<double>("varIntA - varDecA"), 0.00000001);
            Assert.AreEqual(32.7, ParseAndRun<double>("varDecA - varIntB"), 0.00000001);

            Assert.AreEqual(3.7 * 0.053, ParseAndRun<double>("varDecA * varDecB"), 0.00000001);
            Assert.AreEqual(3.7 * -29.0, ParseAndRun<double>("varDecA * varIntB"), 0.00000001);
            Assert.AreEqual(14.0 * 3.7, ParseAndRun<double>("varIntA * varDecA"), 0.00000001);

            Assert.AreEqual(3.7 / 0.053, ParseAndRun<double>("varDecA / varDecB"), 0.00000001);
            Assert.AreEqual(3.7 / -29.0, ParseAndRun<double>("varDecA / varIntB"), 0.00000001);
            Assert.AreEqual(14.0 / 3.7, ParseAndRun<double>("varIntA / varDecA"), 0.00000001);
        }

        [TestMethod]
        public void TestIntegerUnaryOperations()
        {
            Assert.AreEqual(14L, ParseAndRun<long>("varIntA++"));
            Assert.AreEqual(15L, ParseAndRun<long>("++varIntA"));

            Assert.AreEqual(14L, ParseAndRun<long>("varIntA--"));
            Assert.AreEqual(13L, ParseAndRun<long>("--varIntA"));

            Assert.AreEqual(-14L, ParseAndRun<long>("-varIntA"));

            Assert.AreEqual(0x00000000FFFFFFFFL, ParseAndRun<long>("~varIntA", "varIntA = 0xFFFFFFFF00000000;"));
        }
    }
}
