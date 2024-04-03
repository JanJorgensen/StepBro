using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq.Expressions;

namespace StepBroCoreTest.Parser
{
    [TestClass]
    public class TestSelectExpression
    {
        [TestMethod]
        public void TestSelectBoolean()
        {
            var result = ExpressionParser.Parse("(5 > 10) ? false : true");
            Assert.IsTrue(result.IsExpression);
            Assert.AreEqual(typeof(bool), result.DataType.Type);

            var value = Expression.Lambda<Func<bool>>(result.ExpressionCode).Compile()();
            Assert.AreEqual(true, value);

            result = ExpressionParser.Parse("(12 > 10) ? false : true");
            Assert.IsTrue(result.IsExpression);
            Assert.AreEqual(typeof(bool), result.DataType.Type);

            value = Expression.Lambda<Func<bool>>(result.ExpressionCode).Compile()();
            Assert.AreEqual(false, value);
        }

        [TestMethod]
        public void TestSelectInt()
        {
            var result = ExpressionParser.Parse("(5 > 10) ? 30 : 72");
            Assert.IsTrue(result.IsExpression);
            Assert.AreEqual(typeof(long), result.DataType.Type);

            var value = Expression.Lambda<Func<long>>(result.ExpressionCode).Compile()();
            Assert.AreEqual(72L, value);

            result = ExpressionParser.Parse("(12 > 10) ? 82 : 96");
            Assert.IsTrue(result.IsExpression);
            Assert.AreEqual(typeof(long), result.DataType.Type);

            value = Expression.Lambda<Func<long>>(result.ExpressionCode).Compile()();
            Assert.AreEqual(82L, value);
        }

        [TestMethod]
        public void TestSelectString()
        {
            var result = ExpressionParser.Parse("(5 > 10) ? \"Anders\" : \"Bent\"");
            Assert.IsTrue(result.IsExpression);
            Assert.AreEqual(typeof(string), result.DataType.Type);

            var value = Expression.Lambda<Func<string>>(result.ExpressionCode).Compile()();
            Assert.AreEqual("Bent", value);

            result = ExpressionParser.Parse("(12 > 10) ? \"Anders\" : \"Bent\"");
            Assert.IsTrue(result.IsExpression);
            Assert.AreEqual(typeof(string), result.DataType.Type);

            value = Expression.Lambda<Func<string>>(result.ExpressionCode).Compile()();
            Assert.AreEqual("Anders", value);
        }
    }
}
