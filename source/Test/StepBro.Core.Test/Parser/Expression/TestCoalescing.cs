using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBroCoreTest.Data;
using static StepBroCoreTest.Parser.ExpressionParser;

namespace StepBroCoreTest.Parser
{
    [TestClass]
    public class TestCoalescing
    {
        [TestMethod]
        public void TestCoalescingInteger()
        {
            Assert.AreEqual(625L, ParseAndRun<long>(typeof(DummyClass).FullName + "." + nameof(DummyClass.MethodNullableIntNull) + "() ?? 625", varGeneration: false));

            Assert.AreEqual(1735L, ParseAndRun<long>(typeof(DummyClass).FullName + "." + nameof(DummyClass.MethodNullableIntValue) + "() ?? 442", varGeneration: false));
        }

        [TestMethod]
        public void TestCoalescingString()
        {
            Assert.AreEqual("What?", ParseAndRun<string>(typeof(DummyClass).FullName + "." + nameof(DummyClass.MethodNullableStringNull) + "() ?? \"What?\"", varGeneration: false));

            Assert.AreEqual("Jenson", ParseAndRun<string>(typeof(DummyClass).FullName + "." + nameof(DummyClass.MethodNullableStringValue) + "() ?? \"Nein!\"", varGeneration: false));
        }
    }
}
