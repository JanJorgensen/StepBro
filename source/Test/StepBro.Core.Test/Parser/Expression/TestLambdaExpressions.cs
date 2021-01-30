using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBroCoreTest.Data;
using static StepBroCoreTest.Parser.ExpressionParser;

namespace StepBroCoreTest.Parser
{
    [TestClass]
    public class TestLambdaExpressions
    {
        [TestMethod]
        public void TestSimpleStaticMethodWithFilter()
        {
            Assert.AreEqual(124L, ParseAndRun<long>(
                "value",
                "var value = " + typeof(DummyClass).FullName + ".GenerateNumber1( a => (a % 2) == 0 );",
                false));
        }

        [TestMethod]
        public void TestSimpleStaticMethodWithFilterAndAnotherParameter()
        {
            Assert.AreEqual(1124L, ParseAndRun<long>(
                "value",
                "var value = " + typeof(DummyClass).FullName + ".GenerateNumber2( 1000, a => (a % 2) == 0 );",
                false));
        }

        [TestMethod]
        public void TestSimpleSpecifiedArrayGetValue()
        {
            Assert.AreEqual(26L, ParseAndRun<long>(
                "value",
                "var arr = [ 25, 26, 27 ]; " +
                "var value = arr.FirstOrDefault(a => (a % 2) == 0);",
                false));
        }

        [TestMethod]
        public void TestMethodSelectFromLambdaResult()
        {
            // Challenge: Select method has delegate with two generic parameters, 
            // where the type of the second one is determined by the type of the alpha-expression result.

            Assert.AreEqual(28L, ParseAndRun<long>(
                "value",
                "var arr = [ 25, 26, 27 ]; " +
                "var list = arr.Select(a => a + 2);" +
                "var value = list.FirstOrDefault(a => (a % 2) == 0);",
                false));

            Assert.AreEqual(38L, ParseAndRun<long>(
                "value",
                "var arr = [ 35, 36, 37 ]; " +
                "var value = arr.Select(a => a + 2).FirstOrDefault(a => (a % 2) == 0);",
                false));

            Assert.AreEqual(58L, ParseAndRun<long>(
                "value",
                "var arr = [ 55, 56, 57 ]; " +
                "var list = arr.Select(a => a + 2);" +
                "var value = list.FirstOrDefault(a => (a % 2) == 0);",
                false));

            Assert.AreEqual(68L, ParseAndRun<long>(
                "value",
                "var arr = [ 65, 66, 67 ]; " +
                "var value = arr.Select(a => a + 2).FirstOrDefault(a => (a % 2) == 0);",
                false));
        }

        [TestMethod]
        public void TestLocalVariableFromLambdaWithParameters()
        {
            Assert.AreEqual(12L, ParseAndRun<long>(
                "value",
                "var lambda = (int a) => a * 4; " +
                "var value = lambda(3);",
                false));
        }

        [TestMethod]
        public void TestLocalVariableFromLambdaWithoutParameters()
        {
            Assert.AreEqual(5L, ParseAndRun<long>(
                "value",
                "var lambda = () => 5; " +
                "var value = lambda();",
                false));
        }

        [TestMethod]
        public void TestStaticMethodWithObjectTypeInFilter()
        {
            var value = ParseAndRun<DummyDataClass>(
                "value",
                "var value = " + typeof(DummyClass).FullName + ".GetAnObject( obj => (obj.IntProp == -38) );",
                false);
            Assert.IsNotNull(value);
            Assert.IsInstanceOfType(value, typeof(DummyDataClass));
            Assert.AreEqual(42u, value.UIntProp);
        }
    }
}
