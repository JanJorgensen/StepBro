using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using StepBroCoreTest.Data;

namespace StepBroCoreTest.Parser
{
    [TestClass]
    public class TestExpDotIdentifier
    {
        [TestMethod]
        public void ParseSimpleEnum()
        {
            var result = ExpressionParser.Parse("ConsoleColor.Cyan");
            Assert.IsTrue(result.IsConstant);
            Assert.IsTrue(result.Value is ConsoleColor);
            Assert.AreEqual(ConsoleColor.Cyan, (ConsoleColor)result.Value);
        }

        [TestMethod]
        public void ParseIllegalEnumValue()
        {
            var errors = ExpressionParser.ParseError("ConsoleColor.12");
            Assert.AreEqual(1, errors.ErrorCount);
            Assert.AreEqual("mismatched input '12' expecting IDENTIFIER", errors[0].Message);
        }

        [TestMethod]
        public void ParseUnknownEnumVanue()
        {
            var errors = ExpressionParser.ParseError("ConsoleColor.Muffi");
            Assert.AreEqual(1, errors.ErrorCount);
            Assert.AreEqual("Nonexisting enum value 'Muffi' for enum type 'ConsoleColor'.", errors[0].Message);
        }

        [TestMethod]
        public void ParseStaticProperty()
        {
            var result = ExpressionParser.Parse("Console.BufferWidth");
            Assert.IsTrue(result.IsPropertyReference);
            Assert.AreEqual(typeof(int), result.DataType.Type);
            Assert.AreEqual(typeof(Console).GetProperty("BufferWidth"), result.Value);
            var value = Expression.Lambda<Func<int>>(result.ExpressionCode).Compile()();
            Assert.AreEqual(Console.BufferWidth, value);
        }

        [TestMethod]
        public void ParseStaticMethod()
        {
            var result = ExpressionParser.Parse("Math.Sin");
            Assert.IsTrue(result.IsMethodReference);
            Assert.AreEqual(typeof(Math), result.DataType.Type);
            Assert.IsInstanceOfType(result.Value, typeof(IEnumerable<System.Reflection.MethodInfo>));
            var methods = (IEnumerable<System.Reflection.MethodInfo>)result.Value;
            Assert.AreEqual(1, methods.Count());
            Assert.AreEqual("Sin", methods.FirstOrDefault().Name);
        }

        [TestMethod]
        public void AccessStaticAndInstanceProps()
        {
            var result = ExpressionParser.Parse(typeof(DummyClass).FullName+ ".OneInstance.Self.Self.PropInt");
            Assert.IsTrue(result.IsPropertyReference);
            Assert.AreEqual(typeof(long), result.DataType.Type);
            Assert.AreEqual(typeof(DummyClass).GetProperty("PropInt"), result.Value);

            var value = Expression.Lambda<Func<long>>(result.ExpressionCode).Compile()();
            Assert.AreEqual(928L, value);
        }

        [TestMethod]
        public void AccessStaticAndInstancePropsAndInstanceMethod()
        {
            var result = ExpressionParser.Parse(typeof(DummyClass).FullName + ".OneInstance.GetTimespan()");
            Assert.IsTrue(result.IsExpression);
            Assert.AreEqual(typeof(TimeSpan), result.DataType.Type);

            var value = Expression.Lambda<Func<TimeSpan>>(result.ExpressionCode).Compile()();
            Assert.AreEqual(TimeSpan.FromTicks(TimeSpan.TicksPerMillisecond * 514L), value);
        }

        [TestMethod]
        public void AccessStaticAndInstancePropsDelegate()
        {
            var result = ExpressionParser.Parse(typeof(DummyClass).FullName + ".OneInstance.DelegateLong()");
            Assert.IsTrue(result.IsExpression);
            Assert.AreEqual(typeof(long), result.DataType.Type);

            var value = Expression.Lambda<Func<long>>(result.ExpressionCode).Compile()();
            Assert.AreEqual(21L, value);
        }
    }
}