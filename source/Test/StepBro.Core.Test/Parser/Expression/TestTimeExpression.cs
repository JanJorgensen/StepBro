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
    public class TestTimeExpression
    {
        [TestMethod]
        public void TestTimespanSimpleConstantMath()
        {
            Assert.AreEqual(TimeSpan.FromMilliseconds(8300), ParseAndRun<TimeSpan>("5s + 3.3s"));
            Assert.AreEqual(TimeSpan.FromMilliseconds(64700), ParseAndRun<TimeSpan>("@1:08 - 3.3s"));
        }

        [TestMethod]
        public void TestTimespanSimpleVariableMath()
        {
            Assert.AreEqual(TimeSpan.FromMilliseconds(7900), ParseAndRun<TimeSpan>("varTimespanA"));
            Assert.AreEqual(TimeSpan.FromMilliseconds(1250), ParseAndRun<TimeSpan>("varTimespanB"));
            Assert.AreEqual(TimeSpan.FromMilliseconds(7925), ParseAndRun<TimeSpan>("varTimespanA + 25ms"));
            Assert.AreEqual(TimeSpan.FromMilliseconds(750), ParseAndRun<TimeSpan>("2s - varTimespanB"));
            Assert.AreEqual(TimeSpan.FromMilliseconds(8900), ParseAndRun<TimeSpan>("varTimespanA + varTimespanB - 250ms"));
            Assert.AreEqual(TimeSpan.FromMilliseconds(7847), ParseAndRun<TimeSpan>("varTimespanA - varDecB"));

            Assert.AreEqual(TimeSpan.FromMilliseconds(7900 * 14), ParseAndRun<TimeSpan>("varTimespanA * varIntA"));
            Assert.AreEqual(TimeSpan.FromMilliseconds(7900 * 14), ParseAndRun<TimeSpan>("varIntA * varTimespanA"));
            Assert.AreEqual(TimeSpan.FromMilliseconds(7900 * 3.7), ParseAndRun<TimeSpan>("varTimespanA * varDecA"));
            Assert.AreEqual(TimeSpan.FromMilliseconds(7900 * 3.7), ParseAndRun<TimeSpan>("varDecA * varTimespanA"));

            Assert.AreEqual(TimeSpan.FromTicks((TimeSpan.TicksPerMillisecond * 7900L) / 14L), ParseAndRun<TimeSpan>("varTimespanA / varIntA"));
            Assert.AreEqual(TimeSpan.FromTicks(Convert.ToInt64((TimeSpan.TicksPerMillisecond * 7900) / 3.7)), ParseAndRun<TimeSpan>("varTimespanA / varDecA"));

            Assert.AreEqual(TimeSpan.FromTicks((TimeSpan.TicksPerMillisecond * 7900L) % 14L), ParseAndRun<TimeSpan>("varTimespanA % varIntA"));
        }
    }
}
