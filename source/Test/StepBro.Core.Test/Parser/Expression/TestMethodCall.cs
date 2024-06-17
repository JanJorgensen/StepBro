using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core.Parser;
using System;
using System.Linq.Expressions;
using StepBroCoreTest.Data;
using static StepBroCoreTest.Data.DummyClass;
using static StepBroCoreTest.Parser.ExpressionParser;
using StepBro.Core.Execution;

namespace StepBroCoreTest.Parser
{
    [TestClass]
    public class TestMethodCall
    {
        [TestMethod]
        public void StaticMethodArgsVoid()
        {
            var result = ExpressionParser.Parse(
                typeof(DummyClass).FullName + ".MethodStaticLongOut1()");
            Assert.IsTrue(result.IsExpression);
            Assert.AreEqual(typeof(long), result.DataType.Type);

            var value = Expression.Lambda<Func<long>>(result.ExpressionCode).Compile()();
            Assert.AreEqual(4001L, value);
        }

        [TestMethod]
        public void StaticMethodArgsSeveralNormal()
        {
            var result = ExpressionParser.Parse(
                typeof(DummyClass).FullName + ".MethodStaticLongOut4(72, \"Mee\", 27m)");
            Assert.IsTrue(result.IsExpression);
            Assert.AreEqual(typeof(long), result.DataType.Type);

            var value = Expression.Lambda<Func<long>>(result.ExpressionCode).Compile()();
            Assert.AreEqual(4076L, value);
        }

        [TestMethod]
        public void StaticMethodArgEnum()
        {
            var result = ExpressionParser.Parse(
                typeof(DummyClass).FullName + ".MethodStaticLongOut7(" + typeof(DummyEnum).FullName + ".Second)");
            Assert.IsTrue(result.IsExpression);
            Assert.AreEqual(typeof(long), result.DataType.Type);

            var value = Expression.Lambda<Func<long>>(result.ExpressionCode).Compile()();
            Assert.AreEqual(154L, value);
        }

        [TestMethod]
        public void StaticMethodArgObject()
        {
            var result = ExpressionParser.Parse(
                typeof(DummyClass).FullName + ".MethodStaticLongOut8(null)");
            Assert.IsTrue(result.IsExpression);
            Assert.AreEqual(typeof(long), result.DataType.Type);
            var value = Expression.Lambda<Func<long>>(result.ExpressionCode).Compile()();
            Assert.AreEqual(-1L, value);

            result = ExpressionParser.Parse(
                typeof(DummyClass).FullName + ".MethodStaticLongOut8(123)");
            Assert.IsTrue(result.IsExpression);
            Assert.AreEqual(typeof(long), result.DataType.Type);
            value = Expression.Lambda<Func<long>>(result.ExpressionCode).Compile()();
            Assert.AreEqual(3L, value);
        }

        [TestMethod]
        public void StaticMethodArgsSeveralNormal_WithUsing()
        {
            var result = ExpressionParser.ParseUsingDummyClass(
                "(MethodStaticLongOut4(72, \"Mee\", 27m))");
            Assert.IsTrue(result.IsExpression);
            Assert.AreEqual(typeof(long), result.DataType.Type);

            var value = Expression.Lambda<Func<long>>(result.ExpressionCode).Compile()();
            Assert.AreEqual(4076L, value);
        }

        [TestMethod]
        public void MethodArgsSeveralNormal()
        {
            var result = ExpressionParser.Parse(
                typeof(DummyClass).FullName + ".OneInstance.MethodLongOut4(72, \"Mee\", 27m)");
            Assert.IsTrue(result.IsExpression);
            Assert.AreEqual(typeof(long), result.DataType.Type);

            var value = Expression.Lambda<Func<long>>(result.ExpressionCode).Compile()();
            Assert.AreEqual(5076L, value);
        }

        [TestMethod]
        public void StaticMethodsWithDifferentSignatures()
        {
            // Test selection between two methods where one has no parameters, and the other a single 'params' parameter.
            Assert.AreEqual(
                "No parameters",
                ParseAndRunExp<string>(nameof(MethodStaticParamsStringOrNone) + "()"));
        }
        [TestMethod]
        public void InstanceMethodsWithDifferentSignatures()
        {
            // Test selection between two methods where one has no parameters, and the other a single 'params' parameter.
            var result = ExpressionParser.Parse(
                typeof(DummyClass).FullName + ".OneInstance.MethodInstanceParamsStringOrNone()");
            Assert.IsTrue(result.IsExpression);
            Assert.AreEqual(typeof(string), result.DataType.Type);

            var value = Expression.Lambda<Func<string>>(result.ExpressionCode).Compile()();
            Assert.AreEqual("Here, with no parameters", value);
        }

        [TestMethod]
        public void StaticMethodStringParams()
        {
            // First with no 'params' arguments. 
            Assert.AreEqual(
                "F: Anders. Args: <none>",
                ParseAndRunExp<string>(nameof(MethodStaticStringParamsString) + "(\"Anders\")"));

            // Then with a single 'params' argument.
            Assert.AreEqual(
                "F: Abraham. Args: Faster",
                ParseAndRunExp<string>("MethodStaticStringParamsString(\"Abraham\", \"Faster\")"));

            // Then with several 'params' arguments.
            Assert.AreEqual(
                "F: Asbjorn. Args: Anja, Betina, Charlotte, Diana",
                ParseAndRunExp<string>("MethodStaticStringParamsString(\"Asbjorn\", \"Anja\", \"Betina\", \"Charlotte\", \"Diana\")"));
        }

        [TestMethod]
        public void StaticMethodObjectParams()
        {
            // First with no 'params' arguments. 
            Assert.AreEqual(
                "F: Anders. Args: <none>",
                ParseAndRunExp<string>("MethodStaticStringParamsObject(\"Anders\")"));

            // Then with a single 'params' argument.
            Assert.AreEqual(
                "F: Abraham. Args: Faster",
                ParseAndRunExp<string>("MethodStaticStringParamsObject(\"Abraham\", \"Faster\")"));

            // Then with several 'params' arguments.
            Assert.AreEqual(
                "F: Asbjorn. Args: Anja, Betina, Charlotte, Diana",
                ParseAndRunExp<string>("MethodStaticStringParamsObject(\"Asbjorn\", \"Anja\", \"Betina\", \"Charlotte\", \"Diana\")"));
        }

        [TestMethod]
        public void StaticMethodStringParamsWithWrongArguments()
        {
            ExpressionParser.ParseUsingDummyClass("(MethodStaticLongOut22(4, 6))", 0);
            ExpressionParser.ParseUsingDummyClass("(MethodStaticLongOut22(4 6))", 1);
        }

        [TestMethod]
        public void MethodExtensionSimple()
        {
            Assert.AreEqual(938L, ParseAndRun<long>(
                typeof(DummyClass).FullName + ".OneInstance.Plus10()"));

            Assert.AreEqual(942L, ParseAndRun<long>(
                typeof(DummyClass).FullName + ".OneInstance.Plus10Plus(4)"));

            //var t = typeof(DummyClass);
            //var result = TSharpFileBuilder.ParseExpression(this.GetType().Assembly,
            //    "TSharpCoreTest.Parser.DummyClass.OneInstance.Plus10()");

            //Assert.IsTrue(result.ReferencedType == TSExpressionType.Expression);
            //Assert.AreEqual(typeof(long), result.DataType);

            //var value = Expression.Lambda<Func<long>>(result.ExpressionCode).Compile()();
            //Assert.AreEqual(938L, value);
        }

        [TestMethod]
        public void ExtensionMethodEnumerableSimple()
        {
            Assert.AreEqual(26L, ParseAndRun<long>(
                "value",
                "var arr = [ 25, 26, 27 ]; " +
                "var value = arr.MySecondOrDefault();",
                false));

            Assert.AreEqual(26L, ParseAndRun<long>(
                "value",
                "var arr = [ 25, 26, 27 ]; " +
                "var value = arr.MySecondOrDefault();",
                false));
        }

        [TestMethod]
        public void ExtensionMethodListTypes()
        {
            Assert.AreEqual(52L, ParseAndRun<long>(
                "value",
                "var arr = [ 25, 26, 27 ]; " +
                "var value = arr.MyExtMethodListLong();",
                false));

            Assert.AreEqual(54L, ParseAndRun<long>(
                "value",
                "var arr = [ 25, 26, 27 ]; " +
                "var value = arr.MyExtMethodIListLong();",
                false));

            Assert.AreEqual(39L, ParseAndRun<long>(
                "value",
                "var arr = [ 25, 26, 27 ]; " +
                "var value = arr.MyExtMethodIEnumLong();",
                false));
        }

        [TestMethod]
        [Ignore]    // Selecting between two alternatives not implemented yet
        public void ExtensionMethodFromTwoAlternatives()
        {
            // There are two ToArray methods to choose from.

            Assert.AreEqual(26L, ParseAndRun<long>(
                "value",
                "var arr = [ 25, 26, 27 ]; " +
                "var value = arr.ToArray().MyExtMethodArrayLong();",
                false));
        }

        [TestMethod]
        public void MethodExtensionEnumerableSimpleOverloaded()
        {
            Assert.AreEqual(25L, ParseAndRun<long>(
                "value",
                "var arr = [ 25, 26, 27 ]; " +
                "var value = arr.FirstOrDefault();",
                false));
        }

        [TestMethod]
        public void MethodAwaitAsyncVoid()
        {
            var t = ParseAndRun<TimeSpan>("Now() - ts;", "var ts = Now();\n    await " + nameof(DummyClass.MethodStaticAsyncVoid) + "();", false, true);
            Assert.IsTrue(t > TimeSpan.FromMilliseconds(130 - 1), "The execution time for the statement shoult be approx. 130ms");
            Assert.IsTrue(t < TimeSpan.FromMilliseconds(160));
        }

        [TestMethod]
        public void MethodAwaitAsyncTyped()
        {
            var result = ParseAndRun<long>("await " + nameof(DummyClass.MethodStaticAsyncTyped) + "();", "", false, true);
            Assert.AreEqual(12321L, result);

            var t = ParseAndRun<TimeSpan>("Now() - ts;", "var ts = Now();\n    await " + nameof(DummyClass.MethodStaticAsyncTyped) + "();", false, true);
            Assert.IsTrue(t > TimeSpan.FromMilliseconds(110 - 1), "The execution time for the statement shoult be approx. 110ms");
            Assert.IsTrue(t < TimeSpan.FromMilliseconds(140));
        }

        [TestMethod]
        public void StaticMethodSignaturesWithAllParametersSet()
        {
            Assert.AreEqual(4001L, ParseAndRunExp<long>(nameof(MethodStaticLongOut1) + "()"));
            Assert.AreEqual(4042L, ParseAndRunExp<long>(nameof(MethodStaticLongOut2) + "(40)"));
            Assert.AreEqual(4053L, ParseAndRunExp<long>(nameof(MethodStaticLongOut3) + "(50)"));
            Assert.AreEqual(4064L, ParseAndRunExp<long>(nameof(MethodStaticLongOut4) + "(60, \"Yes\", 12.3)"));
            Assert.AreEqual(4075L, ParseAndRunExp<long>(nameof(MethodStaticLongOut5) + "(70, true, 15s)"));
            Assert.AreEqual(4086L, ParseAndRunExp<long>(nameof(MethodStaticLongOut6) + "(80, false, 400ms)"));
        }

        [TestMethod]
        public void StaticMethodSignaturesWithDefaultValues()
        {
            Assert.AreEqual(4008L, ParseAndRunExp<long>(nameof(MethodStaticLongOut3) + "()"));
            Assert.AreEqual(4010L, ParseAndRunExp<long>(nameof(MethodStaticLongOut5) + "()"));
            Assert.AreEqual(4028L, ParseAndRunExp<long>(nameof(MethodStaticLongOut6) + "(22)"));
            Assert.AreEqual(2072L, ParseAndRunExp<long>(nameof(MethodStaticLongSevaralArgs) + "()"));
        }

        [TestMethod]
        public void TestMethodSignaturesWithNamedArguments()
        {
            Assert.AreEqual(4015L, ParseAndRunExp<long>(nameof(MethodStaticLongOut2) + "(a: 13)"));

            Assert.AreEqual(4035L, ParseAndRunExp<long>(nameof(MethodStaticLongOut3) + "(a: 32)"));

            Assert.AreEqual(4042L, ParseAndRunExp<long>(nameof(MethodStaticLongOut4) + "(a: 38, b: \"Mom\", c: 0.52)"));
            Assert.AreEqual(4022L, ParseAndRunExp<long>(nameof(MethodStaticLongOut4) + "(c: 90.4, a: 18, b: \"fjk\")"));
            Assert.AreEqual(4008L, ParseAndRunExp<long>(nameof(MethodStaticLongOut4) + "(c: 9.8, b: \"Dab\", a: 4)"));
            Assert.AreEqual(4108L, ParseAndRunExp<long>(nameof(MethodStaticLongOut4) + "(104, b: \"KAS\", c: 72.02)"));

            Assert.AreEqual(4019L, ParseAndRunExp<long>(nameof(MethodStaticLongOut5) + "(a: 14, b: true, c: 100ms)"));

            Assert.AreEqual(4106L, ParseAndRunExp<long>(nameof(MethodStaticLongOut6) + "(100, false, c: 123ms)"));
            Assert.AreEqual(4107L, ParseAndRunExp<long>(nameof(MethodStaticLongOut6) + "(101, b: true, c: 126ms)"));
            Assert.AreEqual(4108L, ParseAndRunExp<long>(nameof(MethodStaticLongOut6) + "(102, c: 129ms, b: true)"));
            Assert.AreEqual(4109L, ParseAndRunExp<long>(nameof(MethodStaticLongOut6) + "(a: 103, b: true, c: 1ms)"));
            Assert.AreEqual(4110L, ParseAndRunExp<long>(nameof(MethodStaticLongOut6) + "(b: true, c: 3003ms, a: 104)"));
            Assert.AreEqual(4111L, ParseAndRunExp<long>(nameof(MethodStaticLongOut6) + "(c: 12s, a: 105, b: true)"));
        }

        [TestMethod]
        public void TestInstanceMethodSignaturesWithContext()
        {
            Assert.AreEqual(740L + 928L, ParseAndRun<long>("varDummyA.MethodWithCallContextA()", varDummyClass: true));
            Assert.AreEqual(740L + 726L, ParseAndRun<long>("varDummyB.MethodWithCallContextA()", varDummyClass: true));
        }

        [TestMethod]
        public void TestInstanceMethodSignaturesWithContextAndParameter()
        {
            Assert.AreEqual(70L + 928L, ParseAndRun<long>("varDummyA.MethodWithCallContextB(\"Upsan\")", varDummyClass: true));
            Assert.AreEqual(70L + 726L, ParseAndRun<long>("varDummyB.MethodWithCallContextB(\"Upsan\")", varDummyClass: true));
        }

        [TestMethod, Ignore]
        public void TestSystemRandom()
        {
            Assert.AreEqual(25L, ParseAndRun<long>(
                "value",
                "var rnd = new Random(); " +
                "var value = rnd.Next(0, 100);",
                false));
        }

        [TestMethod]
        public void TestIntegerReturnValueAutoConvert()
        {
            var proc = FileBuilder.ParseProcedure(typeof(DummyClass),
                "int Func()",
                "{",
                    "int i = DummyClass.MethodStaticLongToInt(14);",
                    "expect(i == 14);",
                    "i = DummyClass.MethodStaticLongToInt(17);",
                    "expect(i == 17);",
                    "return i;",
                "}");
            Assert.AreEqual(typeof(long), proc.ReturnType.Type);
            Assert.AreEqual(0, proc.Parameters.Length);
            var result = proc.Call();
            Assert.IsTrue(ScriptTaskContext.LastContext.Result.Verdict <= StepBro.Core.Data.Verdict.Pass);
            Assert.IsInstanceOfType(result, typeof(long));
            Assert.AreEqual(17L, (long)result);
        }


        [TestMethod]
        public void TestOutVariableIntSuccess()
        {
            var proc = FileBuilder.ParseProcedureExpectNoErrors(
                """
                int Func()
                {
                    int a = 0;
                    bool success = Int64.TryParse("7", out a);
                    return a;
                }
                """);
            Assert.AreEqual(typeof(long), proc.ReturnType.Type);
            Assert.AreEqual(0, proc.Parameters.Length);
            object result = proc.Call();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(long));
            Assert.AreEqual(7L, (long)result);
        }

        [TestMethod]
        public void TestOutVariableIntFailure()
        {
            var proc = FileBuilder.ParseProcedureExpectNoErrors(
                """
                bool Func()
                {
                    int a = 0;
                    bool success = Int64.TryParse("abc", out a);
                    return success;
                }
                """);
            Assert.AreEqual(typeof(bool), proc.ReturnType.Type);
            Assert.AreEqual(0, proc.Parameters.Length);
            object result = proc.Call();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.AreEqual(false, (bool)result);
        }

        [TestMethod]
        public void TestOutVariableBoolSuccess()
        {
            var proc = FileBuilder.ParseProcedureExpectNoErrors(
                """
                bool Func()
                {
                    bool a = false;
                    bool success = Boolean.TryParse("true", out a);
                    return a;
                }
                """);
            Assert.AreEqual(typeof(bool), proc.ReturnType.Type);
            Assert.AreEqual(0, proc.Parameters.Length);
            object result = proc.Call();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.AreEqual(true, (bool)result);
        }

        [TestMethod]
        public void TestOutVariableBoolFailure()
        {
            var proc = FileBuilder.ParseProcedureExpectNoErrors(
                """
                bool Func()
                {
                    bool a = false;
                    bool success = Boolean.TryParse("123", out a);
                    return success;
                }
                """);
            Assert.AreEqual(typeof(bool), proc.ReturnType.Type);
            Assert.AreEqual(0, proc.Parameters.Length);
            object result = proc.Call();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.AreEqual(false, (bool)result);
        }

        [TestMethod]
        public void TestMultipleMethodsChooseCorrect()
        {
            var proc = FileBuilder.ParseProcedureExpectNoErrors(
                """
                int Func()
                {
                    int a = 15;
                    int correctNumber = Math.Min(a, 13);
                    return correctNumber;
                }
                """);
            Assert.AreEqual(typeof(long), proc.ReturnType.Type);
            Assert.AreEqual(0, proc.Parameters.Length);
            object result = proc.Call();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(long));
            Assert.AreEqual(13L, (long)result);
        }
    }
}