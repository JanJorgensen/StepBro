using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core;
using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Parser;
using StepBroCoreTest.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StepBroCoreTest.Parser
{
    public static class ExpressionParser
    {
        public static TimeSpan LastExecutionTime { get; private set; }

        internal static IEnumerable<string> GetTokens(string content)
        {
            return FileBuilder.GetTokens(content);
        }

        internal static SBExpressionData Parse(string expression)
        {
            var builder = FileBuilder.ParseExpression(null, [typeof(ExpressionParser)], expression);
            Assert.AreEqual(0, builder.Errors.ErrorCount);
            return builder.Listener.GetExpressionResult();
        }

        internal static IErrorCollector ParseError(string expression)
        {
            var builder = FileBuilder.ParseExpression(null, [typeof(ExpressionParser)], expression);
            return builder.Errors;
        }

        internal static SBExpressionData ParseUsingDummyClass(string expression, int expectedErrorCount = 0)
        {
            var builder = FileBuilder.ParseParExpression(typeof(DummyClass), null, expression);
            Assert.AreEqual(expectedErrorCount, builder.Errors.ErrorCount);
            return builder.Listener.GetExpressionResult();
        }

        internal static T ParseAndRunExp<T>(string expression)
        {
            var builder = FileBuilder.ParseExpression(typeof(DummyClass), null, expression);
            Assert.AreEqual(0, builder.Errors.ErrorCount);
            var result = builder.Listener.GetExpressionResult();
            Assert.IsTrue(result.IsExpression);
            Assert.AreEqual(typeof(T), result.DataType.Type);


            var compiled = System.Linq.Expressions.Expression.Lambda<Func<T>>(result.ExpressionCode).Compile();
            var before = DateTime.Now;
            try
            {
                return compiled();
            }
            finally
            {
                LastExecutionTime = DateTime.Now - before;
            }
        }

        public static FileBuilder Parse<T>(
            string returnValueExpression = "0",
            string statements = "",
            bool varGeneration = true,
            bool varDummyClass = false)
        {
            string typeName = "";
            if (typeof(T) == typeof(long)) typeName = "int";
            else if (typeof(T) == typeof(double)) typeName = "decimal";
            else if (typeof(T) == typeof(bool)) typeName = "bool";
            else if (typeof(T) == typeof(string)) typeName = "string";
            else if (typeof(T) == typeof(TimeSpan)) typeName = "timespan";
            else if (typeof(T) == typeof(DateTime)) typeName = "datetime";
            else if (typeof(T) == typeof(Verdict)) typeName = "verdict";
            else if (typeof(T) == typeof(DummyDataClass)) typeName = "DummyDataClass";
            else if (typeof(T) == typeof(DataReport)) typeName = "DataReport";
            else throw new NotImplementedException();
            StringBuilder source = new StringBuilder();
            source.AppendLine(typeName + " ExpressionProcedure(){");
            if (varGeneration || varDummyClass) AddLocalVariables(source, varDummyClass);
            source.AppendLine(statements);
            source.AppendLine(typeName + " result = " + returnValueExpression + ";");
            source.AppendLine("return result;");
            source.AppendLine("}");
            IAddonManager addons = AddonManager.Create();
            addons.AddAssembly(typeof(Enumerable).Assembly, false);
            addons.AddAssembly(typeof(ExpressionParser).Assembly, false);
            return FileBuilder.ParseProcedure(
                addons,
                new string[] { typeof(DataReport).Namespace, typeof(DummyClass).FullName, typeof(DummyClass).Namespace },
                null,
                source.ToString());
        }

        public static T ParseAndRun<T>(
            string returnValueExpression = "0",
            string statements = "",
            bool varGeneration = true,
            bool varDummyClass = false)
        {
            var builder = Parse<T>(returnValueExpression, statements, varGeneration, varDummyClass);
            DummyClass.ResetTestData();
            var taskContext = ExecutionHelper.ExeContext();
            var before = DateTime.Now;
            object result = taskContext.CallProcedure(builder.Listener.LastParsedProcedure);
            LastExecutionTime = DateTime.Now - before;
            if (taskContext.ExecutionExeception == null)
            {
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(T));
                return (T)result;
            }
            else
            {
                return default(T);
            }
        }

        private static void AddLocalVariables(StringBuilder source, bool addDummyInstance)
        {
            source.AppendLine("int varIntA = 14;");
            source.AppendLine("int varIntB = -29;");
            source.AppendLine("decimal varDecA = 3.7;");
            source.AppendLine("decimal varDecB = 53m;");
            source.AppendLine("bool varBoolA = true;");
            source.AppendLine("bool varBoolB = false;");
            source.AppendLine("string varStringA = \"Vaffel\";");
            source.AppendLine("string varStringB = \"Jan\";");
            source.AppendLine("string varStringC = \"\";");
            source.AppendLine("string varStringD = null;");
            source.AppendLine("timespan varTimespanA = 7.9s;");
            source.AppendLine("timespan varTimespanB = @0:01.250;");
            if (addDummyInstance) source.AppendLine("var varDummyA = OneInstance;");
            if (addDummyInstance) source.AppendLine("var varDummyB = OneInstanceInterface;");
        }
    }
}
