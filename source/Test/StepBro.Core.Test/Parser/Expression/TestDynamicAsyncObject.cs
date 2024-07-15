using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core.Data;
using StepBro.Core.General;
using System;
using StepBroCoreTest.Data;
using static StepBroCoreTest.Parser.ExpressionParser;

namespace StepBroCoreTest.Parser
{
    [TestClass]
    public class TestDynamicAsyncObject
    {
        [TestMethod]
        public void AsyncCallUnknownDynamicMethod()
        {
            Assert.AreEqual(0L, ParseAndRun<long>(
                "await obj.TheHorse(12, true)",
                nameof(DummyDynamicAsyncObject) + " obj = " + nameof(DummyDynamicAsyncObject) + ".NewInitialized();"));
            Assert.AreEqual(1, ExecutionHelper.RuntimeErrors.Errors.Count);
            Assert.IsNotNull(ExecutionHelper.RuntimeErrors.Errors[0].Item3);
            Assert.IsInstanceOfType(ExecutionHelper.RuntimeErrors.Errors[0].Item3, typeof(DynamicMethodNotFoundError));
        }

        [TestMethod]
        public void AsyncCallVoidVoidDynamicMethod()
        {
            Assert.AreEqual(1L, ParseAndRun<long>(
                "obj.InvokeCount",
                nameof(DummyDynamicAsyncObject) + " obj = " + nameof(DummyDynamicAsyncObject) + ".NewInitialized();" + Environment.NewLine+
                "await obj.Esildro();"));
            Assert.AreEqual(0, ExecutionHelper.RuntimeErrors.Errors.Count);
        }

        [TestMethod]
        public void AsyncCallSimpleDynamicMethod()
        {
            Assert.AreEqual(726L, ParseAndRun<long>(
                "await obj.Anderson(12, \"Somikelo\", 3ms)",
                nameof(DummyDynamicAsyncObject) + " obj = " + nameof(DummyDynamicAsyncObject) + ".NewInitialized();",
                varDummyClass: true));
            Assert.AreEqual(0, ExecutionHelper.RuntimeErrors.Errors.Count);
        }

        [TestMethod]
        public void AsyncCallDynamicMethodWithIdentifier()
        {
            Assert.AreEqual(true, ParseAndRun<bool>(
                "await obj.Bengtson('Somikelo', 42)",
                nameof(DummyDynamicAsyncObject) + " obj = " + nameof(DummyDynamicAsyncObject) + ".NewInitialized();",
                varDummyClass: true));
            Assert.AreEqual(0, ExecutionHelper.RuntimeErrors.Errors.Count);
        }

        [TestMethod, Ignore]    // Identifier as a type has not been implemented yet.
        public void AsyncCallDynamicMethodReturningIdentifier()
        {
            //Assert.AreEqual(true, ParseAndRun<Identifier>(
            //    "await obj.Christianson()",
            //    nameof(DummyDynamicAsyncObject) + " obj = " + nameof(DummyDynamicAsyncObject) + ".NewInitialized();",
            //    varDummyClass: true));
            //Assert.AreEqual(0, ExecutionHelper.RuntimeErrors.Errors.Count);
        }

        [TestMethod]
        public void AsyncCallSimpleDynamicMethodWithWrongArguments()
        {
            Assert.AreEqual(0L, ParseAndRun<long>(
                "await obj.Anderson(12, true)",
                nameof(DummyDynamicAsyncObject) + " obj = " + nameof(DummyDynamicAsyncObject) + ".NewInitialized();",
                varDummyClass: true));
            Assert.AreEqual(1, ExecutionHelper.RuntimeErrors.Errors.Count);
            Assert.IsNotNull(ExecutionHelper.RuntimeErrors.Errors[0].Item5);
            Assert.IsInstanceOfType(ExecutionHelper.RuntimeErrors.Errors[0].Item5, typeof(ArgumentException));

            Assert.AreEqual(0L, ParseAndRun<long>(
                "await obj.Anderson(12, \"Somikelo\")",
                nameof(DummyDynamicAsyncObject) + " obj = " + nameof(DummyDynamicAsyncObject) + ".NewInitialized();",
                varDummyClass: true));
            Assert.AreEqual(1, ExecutionHelper.RuntimeErrors.Errors.Count);
            Assert.IsNotNull(ExecutionHelper.RuntimeErrors.Errors[0].Item5);
            Assert.IsInstanceOfType(ExecutionHelper.RuntimeErrors.Errors[0].Item5, typeof(ArgumentException));

            Assert.AreEqual(0L, ParseAndRun<long>(
                "await obj.Anderson(\"Somikelo\", 8s, 91)",
                nameof(DummyDynamicAsyncObject) + " obj = " + nameof(DummyDynamicAsyncObject) + ".NewInitialized();",
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