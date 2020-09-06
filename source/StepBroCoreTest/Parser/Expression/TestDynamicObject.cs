using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core.Data;
using StepBro.Core.General;
using System;
using static StepBroCoreTest.Parser.ExpressionParser;

namespace StepBroCoreTest.Parser
{
    [TestClass]
    public class TestDynamicObject
    {
        [TestMethod]
        public void CallUnknownDynamicMethod()
        {
            Assert.AreEqual(0L, ParseAndRun<long>(
                "obj.TheHorse(12, true)",
                nameof(DummyDynamicObject) + " obj = " + nameof(DummyDynamicObject) + ".NewInitialized();",
                varDummyClass: true));
            Assert.AreEqual(1, ExecutionHelper.RuntimeErrors.Errors.Count);
            Assert.IsNotNull(ExecutionHelper.RuntimeErrors.Errors[0].Item3);
            Assert.IsInstanceOfType(ExecutionHelper.RuntimeErrors.Errors[0].Item3, typeof(DynamicMethodNotFoundError));
        }

        [TestMethod]
        public void CallSimpleDynamicMethod()
        {
            Assert.AreEqual(726L, ParseAndRun<long>(
                "obj.Anderson(12, \"Somikelo\", 3ms)",
                nameof(DummyDynamicObject) + " obj = " + nameof(DummyDynamicObject) + ".NewInitialized();",
                varDummyClass: true));
            Assert.AreEqual(0, ExecutionHelper.RuntimeErrors.Errors.Count);
        }

        [TestMethod]
        public void CallDynamicMethodWithIdentifier()
        {
            Assert.AreEqual(true, ParseAndRun<bool>(
                "obj.Bengtson('Somikelo', 42)",
                nameof(DummyDynamicObject) + " obj = " + nameof(DummyDynamicObject) + ".NewInitialized();",
                varDummyClass: true));
            Assert.AreEqual(0, ExecutionHelper.RuntimeErrors.Errors.Count);
        }

        [TestMethod, Ignore]    // Identifier as a type has not been implemented yet.
        public void CallDynamicMethodReturningIdentifier()
        {
            Assert.AreEqual(true, ParseAndRun<Identifier>(
                "obj.Christianson()",
                nameof(DummyDynamicObject) + " obj = " + nameof(DummyDynamicObject) + ".NewInitialized();",
                varDummyClass: true));
            Assert.AreEqual(0, ExecutionHelper.RuntimeErrors.Errors.Count);
        }

        [TestMethod]
        public void CallSimpleDynamicMethodWithWrongArguments()
        {
            Assert.AreEqual(0L, ParseAndRun<long>(
                "obj.Anderson(12, true)",
                nameof(DummyDynamicObject) + " obj = " + nameof(DummyDynamicObject) + ".NewInitialized();",
                varDummyClass: true));
            Assert.AreEqual(1, ExecutionHelper.RuntimeErrors.Errors.Count);
            Assert.IsNotNull(ExecutionHelper.RuntimeErrors.Errors[0].Item5);
            Assert.IsInstanceOfType(ExecutionHelper.RuntimeErrors.Errors[0].Item5, typeof(ArgumentException));

            Assert.AreEqual(0L, ParseAndRun<long>(
                "obj.Anderson(12, \"Somikelo\")",
                nameof(DummyDynamicObject) + " obj = " + nameof(DummyDynamicObject) + ".NewInitialized();",
                varDummyClass: true));
            Assert.AreEqual(1, ExecutionHelper.RuntimeErrors.Errors.Count);
            Assert.IsNotNull(ExecutionHelper.RuntimeErrors.Errors[0].Item5);
            Assert.IsInstanceOfType(ExecutionHelper.RuntimeErrors.Errors[0].Item5, typeof(ArgumentException));

            Assert.AreEqual(0L, ParseAndRun<long>(
                "obj.Anderson(\"Somikelo\", 8s, 91)",
                nameof(DummyDynamicObject) + " obj = " + nameof(DummyDynamicObject) + ".NewInitialized();",
                varDummyClass: true));
            Assert.AreEqual(1, ExecutionHelper.RuntimeErrors.Errors.Count);
            Assert.IsNotNull(ExecutionHelper.RuntimeErrors.Errors[0].Item5);
            Assert.IsInstanceOfType(ExecutionHelper.RuntimeErrors.Errors[0].Item5, typeof(ArgumentException));
        }


        //[TestMethod]
        //public void MethodAwaitAsyncVoid()
        //{
        //    var t = ParseAndRun<TimeSpan>("Now() - ts;", "var ts = Now();\n    await " + nameof(DummyClass.MethodStaticAsyncVoid) + "();", false, true);
        //    Assert.IsTrue(t > TimeSpan.FromMilliseconds(130 - 1), "The execution time for the statement shoult be approx. 130ms");
        //    Assert.IsTrue(t < TimeSpan.FromMilliseconds(160));
        //}

        //[TestMethod]
        //public void MethodAwaitAsyncTyped()
        //{
        //    var result = ParseAndRun<long>("await " + nameof(DummyClass.MethodStaticAsyncTyped) + "();", "", false, true);
        //    Assert.AreEqual(12321L, result);

        //    var t = ParseAndRun<TimeSpan>("Now() - ts;", "var ts = Now();\n    await " + nameof(DummyClass.MethodStaticAsyncTyped) + "();", false, true);
        //    Assert.IsTrue(t > TimeSpan.FromMilliseconds(110 - 1), "The execution time for the statement shoult be approx. 110ms");
        //    Assert.IsTrue(t < TimeSpan.FromMilliseconds(140));
        //}
    }
}