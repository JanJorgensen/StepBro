using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using StepBro.Core.Data;
using StepBro.Core.Parser;
using static StepBroCoreTest.Parser.ExpressionParser;

namespace StepBroCoreTest.Parser
{
    [TestClass]
    public class TestTypeParsing
    {
        [TestMethod]
        public void TestBasicTypes()
        {
            Assert.AreEqual(typeof(void), FileBuilder.ParseType("void").Type);

            Assert.AreEqual(typeof(bool), FileBuilder.ParseType("bool").Type);
            Assert.AreEqual(typeof(long), FileBuilder.ParseType("int").Type);
            Assert.AreEqual(typeof(long), FileBuilder.ParseType("integer").Type);
            Assert.AreEqual(typeof(double), FileBuilder.ParseType("decimal").Type);
            Assert.AreEqual(typeof(double), FileBuilder.ParseType("double").Type);
            Assert.AreEqual(typeof(string), FileBuilder.ParseType("string").Type);
            Assert.AreEqual(typeof(Verdict), FileBuilder.ParseType("verdict").Type);
            Assert.AreEqual(typeof(TimeSpan), FileBuilder.ParseType("timespan").Type);
            Assert.AreEqual(typeof(DateTime), FileBuilder.ParseType("datetime").Type);
            Assert.AreEqual(typeof(object), FileBuilder.ParseType("object").Type);
        }

        [TestMethod]
        public void TestBasicArrayTypes()
        {
            Assert.AreEqual(typeof(List<bool>), FileBuilder.ParseType("bool[]").Type);
            Assert.AreEqual(typeof(List<long>), FileBuilder.ParseType("int[]").Type);
            Assert.AreEqual(typeof(List<long>), FileBuilder.ParseType("integer[]").Type);
            Assert.AreEqual(typeof(List<double>), FileBuilder.ParseType("decimal[]").Type);
            Assert.AreEqual(typeof(List<double>), FileBuilder.ParseType("double[]").Type);
            Assert.AreEqual(typeof(List<string>), FileBuilder.ParseType("string[]").Type);
            Assert.AreEqual(typeof(List<Verdict>), FileBuilder.ParseType("verdict[]").Type);
            Assert.AreEqual(typeof(List<TimeSpan>), FileBuilder.ParseType("timespan[]").Type);
            Assert.AreEqual(typeof(List<DateTime>), FileBuilder.ParseType("datetime[]").Type);
            Assert.AreEqual(typeof(List<object>), FileBuilder.ParseType("object[]").Type);
        }
    }
}