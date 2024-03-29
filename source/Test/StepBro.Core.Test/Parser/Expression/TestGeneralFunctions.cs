﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core.Parser;
using StepBro.Core.ScriptData;
using System;
using static StepBroCoreTest.Parser.ExpressionParser;

namespace StepBroCoreTest.Parser
{
    [TestClass]
    public class TestGeneralFunctions
    {
        [TestMethod]
        public void ExecuteDelay()
        {
            ParseAndRun<long>(statements: "delay(200ms); ", varGeneration: false);
            Assert.IsTrue(LastExecutionTime >= TimeSpan.FromTicks(TimeSpan.TicksPerMillisecond * 200));
            Assert.IsTrue(LastExecutionTime < TimeSpan.FromTicks(TimeSpan.TicksPerMillisecond * 220));
        }

        [TestMethod]
        public void ExecuteDelay2()
        {
            var proc = FileBuilder.ParseProcedureExpectNoErrors(
                "void Func(){ delay(200ms); }");

            Assert.AreEqual(typeof(void), proc.ReturnType.Type);
            Assert.AreEqual(0, proc.Parameters.Length);

            object result = proc.Call();
            Assert.IsNull(result);
        }

        [TestMethod]
        public void StringParseToInteger()
        {
            var proc = FileBuilder.ParseProcedureExpectNoErrors("int Func(){ var v = \"726\".ToInt(); return v; }");

            Assert.AreEqual(typeof(long), proc.ReturnType.Type);
            Assert.AreEqual(0, proc.Parameters.Length);

            object result = proc.Call();
            Assert.IsNotNull(result);
            Assert.IsTrue(result.GetType() == typeof(long));
            Assert.AreEqual(726L, (long)result);

            proc = FileBuilder.ParseProcedureExpectNoErrors("int Func(){ var v = \"*d7\".ToInt(); return v; }");

            Assert.AreEqual(typeof(long), proc.ReturnType.Type);
            Assert.AreEqual(0, proc.Parameters.Length);

            result = proc.Call();
            Assert.IsNotNull(result);
            Assert.IsTrue(result.GetType() == typeof(long));
            Assert.AreEqual(0L, (long)result);                  // Default value because the parsing failed.
        }

        [TestMethod]
        public void TestContainsMatch()
        {
            var proc = FileBuilder.ParseProcedureExpectNoErrors(
                "bool Func(){ var strings = [\"Anders\", \"Benny\", \"Chris\", \"Dennis\"]; bool b = strings.ContainsMatch(\"Be*\"); return b; }");
            Assert.AreEqual(typeof(bool), proc.ReturnType.Type);
            Assert.AreEqual(0, proc.Parameters.Length);
            object result = proc.Call();
            Assert.IsTrue(result is bool);
            Assert.IsTrue((bool)result);


            proc = FileBuilder.ParseProcedureExpectNoErrors(
                "bool Func(){ var strings = [\"Anders\", \"Benny\", \"Chris\", \"Dennis\"]; bool b = strings.ContainsMatch(\"Bes*\"); return b; }");
            Assert.AreEqual(typeof(bool), proc.ReturnType.Type);
            Assert.AreEqual(0, proc.Parameters.Length);
            result = proc.Call();
            Assert.IsTrue(result is bool);
            Assert.IsFalse((bool)result);
        }

        [TestMethod]
        public void TestContainsMatchDirectlyOnStringArray()
        {
            var file = FileBuilder.ParseFile(null,
                """
                bool Func()
                {
                    string[] strings = ["Anders", "Benny", "Chris", "Dennis"];
                    bool b = strings.Contains("Benny");
                    return b;
                }
                """);
            var proc = file.GetFileElement<IFileProcedure>("Func");
            Assert.AreEqual(typeof(bool), proc.ReturnType.Type);
            Assert.AreEqual(0, proc.Parameters.Length);
            object result = proc.Call();
            Assert.IsTrue(result is bool);
            Assert.IsTrue((bool)result);

            file = FileBuilder.ParseFile(null,
                """
                bool Func()
                {
                    string[] strings = ["Anders", "Benny", "Chris", "Dennis"];
                    bool b = strings.Contains("Bes");
                    return b;
                }
                """);
            proc = file.GetFileElement<IFileProcedure>("Func");
            Assert.AreEqual(typeof(bool), proc.ReturnType.Type);
            Assert.AreEqual(0, proc.Parameters.Length);
            result = proc.Call();
            Assert.IsTrue(result is bool);
            Assert.IsFalse((bool)result);
        }

        [TestMethod]
        public void TestContainsMatchDirectlyOnString()
        {
            var file = FileBuilder.ParseFile(null,
                """
                bool Func()
                {
                    string str = "Benny";
                    bool b = str.Contains("Be");
                    return b;
                }
                """);
            var proc = file.GetFileElement<IFileProcedure>("Func");
            Assert.AreEqual(typeof(bool), proc.ReturnType.Type);
            Assert.AreEqual(0, proc.Parameters.Length);
            object result = proc.Call();
            Assert.IsTrue(result is bool);
            Assert.IsTrue((bool)result);

            file = FileBuilder.ParseFile(null,
                """
                bool Func()
                {
                    string str = "Benny";
                    bool b = str.Contains("Bes");
                    return b;
                }
                """);
            proc = file.GetFileElement<IFileProcedure>("Func");
            Assert.AreEqual(typeof(bool), proc.ReturnType.Type);
            Assert.AreEqual(0, proc.Parameters.Length);
            result = proc.Call();
            Assert.IsTrue(result is bool);
            Assert.IsFalse((bool)result);
        }

        [TestMethod]
        public void TestFindMatch()
        {
            var proc = FileBuilder.ParseProcedureExpectNoErrors(
                "string Func(){ var strings = [\"Anders\", \"Benny\", \"Chris\", \"Dennis\"]; string r = strings.FindMatch(\"Be*\"); return r; }");
            Assert.AreEqual(typeof(string), proc.ReturnType.Type);
            Assert.AreEqual(0, proc.Parameters.Length);
            object result = proc.Call();
            Assert.IsTrue(result is string);
            Assert.AreEqual("Benny", (string)result);


            proc = FileBuilder.ParseProcedureExpectNoErrors(
                "string Func(){ var strings = [\"Anders\", \"Benny\", \"Chris\", \"Dennis\"]; string r = strings.FindMatch(\"Bes*\"); return r; }");
            Assert.AreEqual(typeof(string), proc.ReturnType.Type);
            Assert.AreEqual(0, proc.Parameters.Length);
            result = proc.Call();
            Assert.IsNull(result);
        }
    }
}