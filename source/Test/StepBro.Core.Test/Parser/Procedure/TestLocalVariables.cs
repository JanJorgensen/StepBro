using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core.Data;
using StepBro.Core.Parser;
using StepBroCoreTest.Data;
using static StepBroCoreTest.Parser.ExpressionParser;

namespace StepBroCoreTest.Parser
{
    [TestClass]
    public class TestLocalVariables
    {
        [TestMethod]
        public void TestSimpleSpecifiedTypeVariables()
        {
            Assert.AreEqual(728L, ParseAndRun<long>("myVar", "int myVar = 728;"));
            Assert.AreEqual(13.43, ParseAndRun<double>("myVar", "decimal myVar = 13.43;"));
            Assert.AreEqual(true, ParseAndRun<bool>("myVar", "bool myVar = true;"));
            Assert.AreEqual("Snappy", ParseAndRun<string>("myVar", "string myVar = \"Snappy\";"));
            Assert.AreEqual(Verdict.Inconclusive, ParseAndRun<Verdict>("myVar", "verdict myVar = inconclusive;"));
        }

        [TestMethod]
        public void TestSimpleConstantVarTypeVariables()
        {
            Assert.AreEqual(728L, ParseAndRun<long>("myVar", "var myVar = 728;"));
            Assert.AreEqual(true, ParseAndRun<bool>("myVar", "var myVar = true;"));
            Assert.AreEqual("Snappy", ParseAndRun<string>("myVar", "var myVar = \"Snappy\";"));
            Assert.AreEqual(13.43, ParseAndRun<double>("myVar", "var myVar = 13.43;"));
            Assert.AreEqual(Verdict.Inconclusive, ParseAndRun<Verdict>("myVar", "var myVar = inconclusive;"));
        }

        [TestMethod]
        public void TestSimpleSpecifiedArrayTypeVariables()
        {
            Assert.AreEqual(26L, ParseAndRun<long>("myArr[1]", "int[] myArr = [25, 26, 27];", false));
            Assert.AreEqual(false, ParseAndRun<bool>("myArr[2]", "bool[] myArr = [true, false, false, true];", false));
            Assert.AreEqual(4.8, ParseAndRun<double>("myArr[3]", "decimal[] myArr = [1.2, 2.4, 3.6, 4.8, 5.0];", false));
            Assert.AreEqual("Bent", ParseAndRun<string>("myArr[1]", "string[] myArr = [\"Anders\", \"Bent\", \"Christian\"];", false));
            Assert.AreEqual(Verdict.Pass, ParseAndRun<Verdict>("myArr[1]", "verdict[] myArr = [unset, pass, fail];", true));

            Assert.AreEqual(TimeSpan.FromMilliseconds(2300), ParseAndRun<TimeSpan>("myArr[2]", "timespan[] myArr = [1s, 25ms, 2.3s, 87ms];", true));

            Assert.AreEqual(new DateTime(2017, 2, 25, 12, 30, 10),
                ParseAndRun<DateTime>(
                    "myArr[1]",
                    "datetime[] myArr = [@2016-03-07 12:30:00, @2017-02-25 12:30:10, @2015-07-20 12:30:00];",
                    true));
        }

        [TestMethod]
        public void TestSimpleVarAssignment()
        {
            Assert.AreEqual(727L, ParseAndRun<long>("myVar", "var myVar = 0; myVar = 727;", false));
            Assert.AreEqual(true, ParseAndRun<bool>("myVar", "var myVar = false; myVar = true;", false));
            Assert.AreEqual("Snappy", ParseAndRun<string>("myVar", "var myVar = \"\"; myVar = \"Snappy\";", false));
            Assert.AreEqual(13.45, ParseAndRun<double>("myVar", "var myVar = 0.0; myVar = 13.45;", false));
            Assert.AreEqual(Verdict.Error, ParseAndRun<Verdict>("myVar", "var myVar = unset; myVar = error;", false));
        }


        [TestMethod]
        public void TestClassTypeVariables()
        {
            Assert.AreEqual(8822L, ParseAndRun<long>("obj.PropInt", "DummyClass obj; obj.PropInt = 8822;", false, true));
            //Assert.AreEqual(8822L, ParseAndRun<long>("obj.PropInt", "var obj = DummyClass(); obj.PropInt = 8822;", false, true));     // TODO !!
        }
    }
}
