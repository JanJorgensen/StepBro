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
    public class TestComparisonExpression
    {
        [TestMethod]
        public void TestBetweenOperationsDecimal()
        {
            DoTestBetweenOperationsDecimal(false);
        }

        [TestMethod]
        public void TestBetweenOperationsDecimalWithIntegerLimits()
        {
            DoTestBetweenOperationsDecimal(true);
        }

        private void DoTestBetweenOperationsDecimal(bool integerLimits)
        {
            var comparison = integerLimits ? "10 < a < 20" : "10.0 < a < 20.0";
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "decimal a = 9.0;"));
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "decimal a = 10.0;"));

            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "decimal a = 10.0000000001;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "decimal a = 15.0;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "decimal a = 19.9999999999;"));

            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "decimal a = 20.0;"));
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "decimal a = 21.0;"));

            comparison = integerLimits ? "10 <= a <= 20" : "10.0 <= a <= 20.0";
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "decimal a = 9.0;"));
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "decimal a = 9.9999999999;"));

            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "decimal a = 10.0;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "decimal a = 15.0;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "decimal a = 20.0;"));

            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "decimal a = 20.0000000001;"));
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "decimal a = 21.0;"));

            comparison = integerLimits ? "10 <~ a <~ 20" : "10.0 <~ a <~ 20.0";
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "decimal a = 9.0;"));
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "decimal a = 9.9999999999;"));

            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "decimal a = 9.99999999999999;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "decimal a = 10.0;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "decimal a = 15.0;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "decimal a = 20.0;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "decimal a = 20.00000000000001;"));

            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "decimal a = 20.0000000001;"));
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "decimal a = 21.0;"));
        }

        [TestMethod]
        public void TestBetweenOperationsInteger()
        {
            var comparison = "10 < a < 20";
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "int a = 9;"));
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "int a = 10;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "int a = 15;"));
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "int a = 20;"));
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "int a = 21;"));

            comparison = "10 <= a <= 20";
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "int a = 9;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "int a = 10;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "int a = 15;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "int a = 20;"));
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "int a = 21;"));

            comparison = "10 <~ a <~ 20";
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "int a = 9;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "int a = 10;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "int a = 15;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "int a = 20;"));
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "int a = 21;"));
        }

        [TestMethod]
        public void TestBetweenOperationsTimespan()
        {
            DoTestBetweenOperationsTimespan(false);
        }

        [TestMethod]
        public void TestBetweenOperationsTimespanWithDecimalLimits()
        {
            DoTestBetweenOperationsTimespan(true);
        }

        private void DoTestBetweenOperationsTimespan(bool decimalLimits)
        {
            var comparison = decimalLimits ? "10.0 < a < 20.0" : "10s < a < 20s";
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "timespan a = 9s;"));
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "timespan a = 10s;"));

            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "timespan a = 10.00001s;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "timespan a = 15s;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "timespan a = 19.99999s;"));

            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "timespan a = 20s;"));
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "timespan a = 21s;"));

            comparison = decimalLimits ? "10.0 <= a <= 20.0" : "10s <= a <= 20s";
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "timespan a = 9s;"));
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "timespan a = 9.99999s;"));

            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "timespan a = 10s;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "timespan a = 15s;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "timespan a = 20s;"));

            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "timespan a = 20.00001s;"));
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "timespan a = 21s;"));

            comparison = decimalLimits ? "10.0 <~ a <~ 20.0" : "10s <~ a <~ 20s";
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "timespan a = 9s;"));
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "timespan a = 9.99999s;"));

            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "timespan a = 9.9999996s;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "timespan a = 10s;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "timespan a = 15s;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "timespan a = 20s;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "timespan a = 20.0000004s;"));

            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "timespan a = 20.00001s;"));
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "timespan a = 21s;"));
        }

        [TestMethod]
        public void TestEqualsWithToleranceInteger()
        {
            //var ch ='±';
            //int v = Convert.ToInt32(ch);
            //var tokens = GetTokens("a == 10 ± 5").ToList();

            var comparison = "a == 10 ± 5";
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "int a = 4;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "int a = 5;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "int a = 6;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "int a = 14;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "int a = 15;"));
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "int a = 16;"));

            comparison = "a ~= 10 ± 5";
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "int a = 4;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "int a = 5;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "int a = 6;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "int a = 14;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "int a = 15;"));
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "int a = 16;"));
        }

        [TestMethod]
        public void TestEqualsWithToleranceDecimal()
        {
            var comparison = "a == 10.0 ± 5.0";
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "decimal a = 4.0;"));
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "decimal a = 4.99999999999;"));
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "decimal a =  4.99999999999999;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "decimal a = 5.0;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "decimal a = 6.0;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "decimal a = 14.0;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "decimal a = 15.0;"));
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "decimal a = 15.00000000000001;"));
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "decimal a = 15.00000000001;"));
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "decimal a = 16.0;"));

            comparison = "a ~= 10.0 ± 5.0";
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "decimal a = 4.0;"));
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "decimal a = 4.99999999999;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "decimal a =  4.99999999999999;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "decimal a = 5.0;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "decimal a = 6.0;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "decimal a = 14.0;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "decimal a = 15.0;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "decimal a = 15.00000000000001;"));
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "decimal a = 15.00000000001;"));
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "decimal a = 16.0;"));
        }

        [TestMethod]
        public void TestEqualsWithToleranceTimespan()
        {
            var comparison = "a == 10s ± 5s";

            var exp = Parse("6s == 10s ± 5s");

            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "timespan a = 4s;"));
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "timespan a = 4.99999s;"));
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "timespan a =  4.9999996s;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "timespan a = 5s;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "timespan a = 6s;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "timespan a = 14s;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "timespan a = 15s;"));
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "timespan a = 15.0000004s;"));
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "timespan a = 15.00001s;"));
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "timespan a = 16s;"));

            comparison = "a ~= 10s ± 5s";
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "timespan a = 4s;"));
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "timespan a = 4.99999s;"));
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "timespan a =  4.9999996s;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "timespan a = 5s;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "timespan a = 6s;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "timespan a = 14s;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "timespan a = 15s;"));
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "timespan a = 15.0000004s;"));
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "timespan a = 15.00001s;"));
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "timespan a = 16s;"));
        }

        [TestMethod]
        public void TestEqualsWithToleranceDateTime()
        {
            var comparison = "a == @2000-01-01 12:45:00 ± 30s";
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "datetime a = @2000-01-01 12:44:00;"));
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "datetime a = @2000-01-01 12:44:29.99999;"));
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "datetime a = @2000-01-01 12:44:29.9999996;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "datetime a = @2000-01-01 12:44:30;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "datetime a = @2000-01-01 12:44:40;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "datetime a = @2000-01-01 12:45:00;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "datetime a = @2000-01-01 12:45:20;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "datetime a = @2000-01-01 12:45:30;"));
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "datetime a = @2000-01-01 12:45:30.0000004;"));
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "datetime a = @2000-01-01 12:45:30.00001;"));
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "datetime a = @2000-01-01 12:46:00;"));


            comparison = "a ~= @2000-01-01 12:45:00 ± 30s";
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "datetime a = @2000-01-01 12:44:00;"));
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "datetime a = @2000-01-01 12:44:29.99999;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "datetime a = @2000-01-01 12:44:29.9999996;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "datetime a = @2000-01-01 12:44:30;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "datetime a = @2000-01-01 12:44:40;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "datetime a = @2000-01-01 12:45:00;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "datetime a = @2000-01-01 12:45:20;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "datetime a = @2000-01-01 12:45:30;"));
            Assert.AreEqual(true, ParseAndRun<bool>(comparison, "datetime a = @2000-01-01 12:45:30.0000004;"));
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "datetime a = @2000-01-01 12:45:30.00001;"));
            Assert.AreEqual(false, ParseAndRun<bool>(comparison, "datetime a = @2000-01-01 12:46:00;"));
        }

        [TestMethod]
        public void TestEqualsString()
        {
            Assert.AreEqual(true, ParseAndRun<bool>("s1 == \"Henry\"", "var s1 = \"Henry\";"));
            Assert.AreEqual(false, ParseAndRun<bool>("s1 != \"Henry\"", "var s1 = \"Henry\";"));
            Assert.AreEqual(false, ParseAndRun<bool>("s1 == \"Henny\"", "var s1 = \"Henry\";"));
            Assert.AreEqual(true, ParseAndRun<bool>("s1 != \"Henny\"", "var s1 = \"Henry\";"));
        }
    }
}
