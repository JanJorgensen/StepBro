﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core;
using StepBro.Core.Parser;
using StepBro.Core.ScriptData;
using StepBroCoreTest.Data;
using StepBroCoreTest.Utils;

namespace StepBroCoreTest.Execution
{
    [TestClass]
    public class TestLogStatement
    {
        [TestInitialize]
        public void Setup()
        {
            ServiceManager.Dispose();
        }

        [TestMethod]
        public void LogConstantString()
        {
            var procedure = FileBuilder.ParseProcedureExpectNoErrors(
                "procedure void Anders() {",
                "   log (\"Ello!\");",
                "}");

            var taskContext = ExecutionHelper.ExeContext();
            taskContext.CallProcedure(procedure);
            var log = new LogInspector(taskContext.Logger);
            log.ExpectNext("0 - Pre - TestRun - Starting");
            log.ExpectNext("1 - Pre - Anders - <no arguments>");
            log.ExpectNext("2 - Normal - 2 Log - Ello!");
            log.ExpectNext("2 - Post");
            log.ExpectEnd();
        }

        [TestMethod]
        public void LogSimpleNonStringTypes()
        {
            var procedure = FileBuilder.ParseProcedureExpectNoErrors(
                "procedure void Anders() {",
                "   log (true);",
                "   log (36);",
                "   log (pass);",
                "}");

            var taskContext = ExecutionHelper.ExeContext();
            taskContext.CallProcedure(procedure);
            var log = new LogInspector(taskContext.Logger);
            log.ExpectNext("0 - Pre - TestRun - Starting");
            log.ExpectNext("1 - Pre - Anders - <no arguments>");
            log.ExpectNext("2 - Normal - 2 Log - True");
            log.ExpectNext("2 - Normal - 3 Log - 36");
            log.ExpectNext("2 - Normal - 4 Log - Pass");
            log.ExpectNext("2 - Post");
            log.ExpectEnd();
        }

        [TestMethod]
        public void LogStringEnumerable()
        {
            var procedure = FileBuilder.ParseProcedure(typeof(DummyClass),
                "procedure void Anders() {",
                "   log (DummyClass.MethodListSomeNames());",
                "}");

            var taskContext = ExecutionHelper.ExeContext();
            taskContext.CallProcedure(procedure);
            var log = new LogInspector(taskContext.Logger);
            log.ExpectNext("0 - Pre - TestRun - Starting");
            log.ExpectNext("1 - Pre - Anders - <no arguments>");
            log.ExpectNext("2 - Normal - 2 - Anders");
            log.ExpectNext("2 - Normal - 2 - Berditto");
            log.ExpectNext("2 - Normal - 2 - Chrushtor");
            log.ExpectNext("2 - Normal - 2 - Dowfick");
            log.ExpectNext("2 - Post");
            log.ExpectEnd();
        }
    }
}
