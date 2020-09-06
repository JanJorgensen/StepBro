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
    public class TestArrayExpressions
    {
        [TestMethod]
        public void TestSimpleSpecifiedArraySetValue()
        {
            Assert.AreEqual(62L, ParseAndRun<long>("myArr[1]", "int[] myArr = [25, 26, 27]; myArr[1] = 62;", false));
            Assert.AreEqual(true, ParseAndRun<bool>("myArr[2]", "bool[] myArr = [true, false, false, true]; myArr[2] = true;", false));
            Assert.AreEqual(3.14, ParseAndRun<double>("myArr[3]", "decimal[] myArr = [1.2, 2.4, 3.6, 4.8, 5.0]; myArr[3] = 3.14;", false));
            Assert.AreEqual("Nix", ParseAndRun<string>("myArr[1]", "string[] myArr = [\"Anders\", \"Bent\", \"Christian\"]; myArr[1] = \"Nix\";", false));
            Assert.AreEqual(Verdict.Inconclusive, ParseAndRun<Verdict>("myArr[1]", "verdict[] myArr = [unset, pass, fail]; myArr[1] = inconclusive;", true));

            Assert.AreEqual(TimeSpan.FromMilliseconds(4000), ParseAndRun<TimeSpan>("myArr[2]", "timespan[] myArr = [1s, 25ms, 2.3s, 87ms]; myArr[2] = 4s;", true));

            Assert.AreEqual(new DateTime(2017, 6, 22, 12, 31, 0),
                ParseAndRun<DateTime>(
                    "myArr[1]",
                    "datetime[] myArr = [@2016-03-07 12:30:00, @2017-02-25 12:30:10, @2015-07-20 12:30:00]; myArr[1] = @2017-06-22 12:31:00;",
                    true));
        }

        [TestMethod]
        public void TestSimpleSpecifiedArrayAssignOperation()
        {
            Assert.AreEqual(27L, ParseAndRun<long>("myArr[1]", "int[] myArr = [10, 26, 80]; myArr[1]++;", false));
            Assert.AreEqual(25L, ParseAndRun<long>("myArr[1]", "int[] myArr = [10, 26, 80]; myArr[1]--;", false));

            Assert.AreEqual(27L, ParseAndRun<long>("myArr[1]", "int[] myArr = [10, 26, 80]; ++myArr[1];", false));
            Assert.AreEqual(25L, ParseAndRun<long>("myArr[1]", "int[] myArr = [10, 26, 80]; --myArr[1];", false));

            Assert.IsTrue(ParseAndRun<bool>("myArr[1] == 27 && a == 26", "int[] myArr = [10, 26, 80]; int a = myArr[1]++;", false));
            Assert.IsTrue(ParseAndRun<bool>("myArr[1] == 25 && a == 26", "int[] myArr = [10, 26, 80]; int a = myArr[1]--;", false));

            Assert.IsTrue(ParseAndRun<bool>("myArr[1] == 27 && a == 27", "int[] myArr = [10, 26, 80]; int a = ++myArr[1];", false));
            Assert.IsTrue(ParseAndRun<bool>("myArr[1] == 25 && a == 25", "int[] myArr = [10, 26, 80]; int a = --myArr[1];", false));

            Assert.AreEqual(26L, ParseAndRun<long>("myArr[1]++", "int[] myArr = [10, 26, 80];", false));
            Assert.AreEqual(27L, ParseAndRun<long>("++myArr[1]", "int[] myArr = [10, 26, 80];", false));

            Assert.AreEqual(26L, ParseAndRun<long>("myArr[1]--", "int[] myArr = [10, 26, 80];", false));
            Assert.AreEqual(25L, ParseAndRun<long>("--myArr[1]", "int[] myArr = [10, 26, 80];", false));


            //Assert.AreEqual(62L, ParseAndRun<long>("myArr[1]", "int[] myArr = [25, 26, 27]; myArr[1] = 62;", false));
            //Assert.AreEqual(true, ParseAndRun<bool>("myArr[2]", "bool[] myArr = [true, false, false, true]; myArr[2] = true;", false));
            //Assert.AreEqual(3.14, ParseAndRun<double>("myArr[3]", "decimal[] myArr = [1.2, 2.4, 3.6, 4.8, 5.0]; myArr[3] = 3.14;", false));
            //Assert.AreEqual("Nix", ParseAndRun<string>("myArr[1]", "string[] myArr = [\"Anders\", \"Bent\", \"Christian\"]; myArr[1] = \"Nix\";", false));
            //Assert.AreEqual(Verdict.Inconclusive, ParseAndRun<Verdict>("myArr[1]", "verdict[] myArr = [unset, pass, fail]; myArr[1] = inconclusive;", true));

            //Assert.AreEqual(TimeSpan.FromMilliseconds(4000), ParseAndRun<TimeSpan>("myArr[2]", "timespan[] myArr = [1s, 25ms, 2.3s, 87ms]; myArr[2] = 4s", true));

            //Assert.AreEqual(new DateTime(2017, 6, 22, 12, 31, 0),
            //    ParseAndRun<DateTime>(
            //        "myArr[1]",
            //        "datetime[] myArr = [@2016-03-07 12:30:00, @2017-02-25 12:30:10, @2015-07-20 12:30:00]; myArr[1] = @2017-06-22 12:31:00;",
            //        true));
        }
    }
}
