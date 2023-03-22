using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core.Data;
using StepBro.Core.Parser;
using System;

namespace StepBroCoreTest.Parser
{
    [TestClass]
    public class TestConstExpression
    {
        [TestMethod]
        public void TestExpressionConstantLiteral()
        {
            this.AssertConstantExpression(16L, "16");
            this.AssertConstantExpression(0x44L, "0x44");
            this.AssertConstantExpression(0x7BL, "7Bh");
            this.AssertConstantExpression(new Identifier("blabber"), "'blabber'");
        }

        [TestMethod]
        public void TestExpressionConstantMult()
        {
            this.AssertConstantExpression(16L * 200L, "16 * 200");
            this.AssertConstantExpression(5000L * 1234L, "5K * 1234");
            this.AssertConstantExpression(12.54 * 0.83, "12.54 * 0.83");
            this.AssertConstantExpression(50.0, "5.0 * 10");
            this.AssertConstantExpression(11.0 * 2.4, "11 * 2.4");
            this.AssertConstantExpression("JanJanJanJanJan", "\"Jan\" * 5");

            this.AssertConstantExpression(TimeSpan.FromMilliseconds(7200), "2.4s * 3");
            this.AssertConstantExpression(TimeSpan.FromMilliseconds(12400), "4 * 3.1s");
            this.AssertConstantExpression(TimeSpan.FromMilliseconds(2520), "2.1s * 1.2");
            this.AssertConstantExpression(TimeSpan.FromMilliseconds(760), "0.2 * 3.8s");
        }

        [TestMethod]
        public void TestExpressionConstantDiv()
        {
            this.AssertConstantExpression(64L / 8L, "64 / 8");
            this.AssertConstantExpression(5000L / 25L, "5K / 25");
            this.AssertConstantExpression(12.54 / 4.2, "12.54 / 4.2");
            this.AssertConstantExpression(50.0 / 4.0, "50.0 / 4");
            this.AssertConstantExpression(72.0 / 2.4, "72 / 2.4");
        }

        [TestMethod]
        public void TestExpressionConstantMod()
        {
            this.AssertConstantExpression(8L % 3L, "8 % 3");
            this.AssertConstantExpression(8.4 % 3.7, "8.4 % 3.7");
            this.AssertConstantExpression(8.0 % 3.7, "8 % 3.7");
            this.AssertConstantExpression(8.4 % 3.0, "8.4 % 3");

            this.AssertConstantExpression(TimeSpan.FromTicks((1200L * TimeSpan.TicksPerMillisecond) % 54), "1200ms % 54");
        }

        [TestMethod]
        public void TestExpressionConstantAdd()
        {
            this.AssertConstantExpression(216L, "16 + 200");
            this.AssertConstantExpression(6234L, "5K + 1234");
            this.AssertConstantExpression(13.37, "12.54 + 0.83");
            this.AssertConstantExpression(15.0, "5.0 + 10");
            this.AssertConstantExpression(13.4, "11 + 2.4");
            this.AssertConstantExpression("Jansen", "\"Jan\" + \"sen\"");
            this.AssertConstantExpression("Nummer: 29", "\"Nummer: \" + 29");
        }

        [TestMethod]
        public void TestExpressionConstantSub()
        {
            this.AssertConstantExpression(32L - 43L, "32 - 43");
            this.AssertConstantExpression(5000L - 1234L, "5K - 1234");
            this.AssertConstantExpression(12.54 - 0.83, "12.54 - 0.83");
            this.AssertConstantExpression(-5.0, "5.0 - 10");
            this.AssertConstantExpression(11.0 - 2.4, "11 - 2.4");
            this.AssertConstantExpression("Jasse", "\"Janesnese\" - \"ne\"");
            this.AssertConstantExpression(TimeSpan.FromMilliseconds(13800), "15s - 1200ms");
            this.AssertConstantExpression(new DateTime(2017, 4, 26, 19, 43, 7, 600), "@2017-04-26 19:43:10 - 2400ms");
        }

        #region Comparison

        [TestMethod]
        public void TestExpressionConstantComparison_Equal()
        {
            this.AssertConstantExpression(false, "5 == 4");
            this.AssertConstantExpression(true, "4 == 4");
            this.AssertConstantExpression(false, "4 == 5");

            this.AssertConstantExpression(false, "5.4 == 4.6");
            this.AssertConstantExpression(true, "4.6 == 4.6");
            this.AssertConstantExpression(false, "4.6 == 5.4");

            this.AssertConstantExpression(false, "5.4s == 4.6s");
            this.AssertConstantExpression(true, "4.6s == 4.6s");
            this.AssertConstantExpression(false, "4.6s == 5.4s");

            this.AssertConstantExpression(false, "@2017-10-20 13:23:00 == @2017-10-20 13:20:00");
            this.AssertConstantExpression(true, "@2017-10-20 13:20:00 == @2017-10-20 13:20:00");
            this.AssertConstantExpression(false, "@2017-10-20 13:20:00 == @2017-10-20 13:23:00");
        }


        [TestMethod]
        public void TestExpressionConstantComparison_GreaterThan()
        {
            this.AssertConstantExpression(true, "5 > 4");
            this.AssertConstantExpression(false, "4 > 4");
            this.AssertConstantExpression(false, "4 > 5");

            this.AssertConstantExpression(true, "5.4 > 4.6");
            this.AssertConstantExpression(false, "4.6 > 4.6");
            this.AssertConstantExpression(false, "4.6 > 5.4");

            this.AssertConstantExpression(true, "5.4s > 4.6s");
            this.AssertConstantExpression(false, "4.6s > 4.6s");
            this.AssertConstantExpression(false, "4.6s > 5.4s");

            this.AssertConstantExpression(true, "@2017-10-20 13:23:00 > @2017-10-20 13:20:00");
            this.AssertConstantExpression(false, "@2017-10-20 13:20:00 > @2017-10-20 13:20:00");
            this.AssertConstantExpression(false, "@2017-10-20 13:20:00 > @2017-10-20 13:23:00");
        }

        [TestMethod]
        public void TestExpressionConstantComparison_LessThan()
        {
            this.AssertConstantExpression(true, "4 < 5");
            this.AssertConstantExpression(false, "4 < 4");
            this.AssertConstantExpression(false, "5 < 4");

            this.AssertConstantExpression(true, "4.6 < 5.2");
            this.AssertConstantExpression(false, "4.6 < 4.6");
            this.AssertConstantExpression(false, "5.5 < 4.6");

            this.AssertConstantExpression(true, "4.6s < 5.2s");
            this.AssertConstantExpression(false, "4.6s < 4.6s");
            this.AssertConstantExpression(false, "5.5s < 4.6s");

            this.AssertConstantExpression(true, "@2017-10-20 13:20:00 < @2017-10-20 13:23:00");
            this.AssertConstantExpression(false, "@2017-10-20 13:20:00 < @2017-10-20 13:20:00");
            this.AssertConstantExpression(false, "@2017-10-20 13:23:00 < @2017-10-20 13:23:00");
        }

        [TestMethod]
        public void TestExpressionConstantComparison_GreaterThanOrApprox()
        {
            this.AssertConstantExpression(true, "5 >~ 4");
            this.AssertConstantExpression(true, "4 >~ 4");
            this.AssertConstantExpression(false, "4 >~ 5");

            this.AssertConstantExpression(true, "5.4 >~ 4.6");
            this.AssertConstantExpression(true, "4.6 >~ 4.6");
            this.AssertConstantExpression(false, "4.6 >~ 5.4");

            //AssertConstantExpression(true, "5.4s >~ 4.6s");
            //AssertConstantExpression(false, "4.6s >~ 4.6s");
            //AssertConstantExpression(false, "4.6s >~ 5.4s");

            //AssertConstantExpression(true, "@2017-10-20 13:23:00 > @2017-10-20 13:20:00");
            //AssertConstantExpression(false, "@2017-10-20 13:20:00 > @2017-10-20 13:20:00");
            //AssertConstantExpression(false, "@2017-10-20 13:20:00 > @2017-10-20 13:23:00");
        }

        [TestMethod]
        public void TestExpressionConstantComparison_LessThanOrApprox()
        {
            this.AssertConstantExpression(true, "4 <~ 5");
            this.AssertConstantExpression(true, "4 <~ 4");
            this.AssertConstantExpression(false, "5 <~ 4");

            this.AssertConstantExpression(true, "4.6 <~ 5.2");
            this.AssertConstantExpression(true, "4.6 <~ 4.6");
            this.AssertConstantExpression(false, "5.5 <~ 4.6");

            //AssertConstantExpression(true, "4.6s < 5.2s");
            //AssertConstantExpression(false, "4.6s < 4.6s");
            //AssertConstantExpression(false, "5.5s < 4.6s");

            //AssertConstantExpression(true, "@2017-10-20 13:20:00 < @2017-10-20 13:23:00");
            //AssertConstantExpression(false, "@2017-10-20 13:20:00 < @2017-10-20 13:20:00");
            //AssertConstantExpression(false, "@2017-10-20 13:23:00 < @2017-10-20 13:23:00");
        }

        #endregion

        [TestMethod]
        public void TestExpressionConstantUnaryOperators()
        {
            this.AssertConstantExpression(-426L, "-426");
            this.AssertConstantExpression(TimeSpan.FromMilliseconds(-250), "-250ms");

            this.AssertConstantExpression(false, "!true");
            this.AssertConstantExpression(true, "not false");
        }

        private void AssertConstantExpression<T>(T expected, string expression)
        {
            var builder = FileBuilder.ParseExpression(expression);
            var result = builder.Listener.GetExpressionResult();
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is T);
            Assert.AreEqual(expected, (T)result.Value);
        }
    }
}
