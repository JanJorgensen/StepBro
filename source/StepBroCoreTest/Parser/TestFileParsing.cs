using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core.Data;
using StepBro.Core.Parser;
using StepBro.Core.ScriptData;
using StepBro.Core.Execution;
using StepBro.Core.Logging;
using StepBro.Core.Api;

namespace StepBroCoreTest.Parser
{
    [TestClass]
    public class TestFileParsing
    {
        [TestMethod]
        public void TestFileParsingSimpleFileUsings()
        {
            var files = FileBuilder.ParseFiles((ILogger)null,
                new Tuple<string, string>("andrea.tss", "using \"betty.tss\"; namespace Anders; procedure int Absalon(){ int i = 0; i = Bethlehem(); return i; }"),
                new Tuple<string, string>("betty.tss", "namespace Anders; procedure int Bethlehem(){ return 7161; }"));
            Assert.AreEqual(2, files.Length);
            Assert.AreEqual("andrea.tss", files.ElementAt(0).FileName);
            Assert.AreEqual("betty.tss", files.ElementAt(1).FileName);
            var procedure = files[0].ListElements().First(p => p.Name == "Absalon") as IFileProcedure;
            Assert.AreEqual("Absalon", procedure.Name);
            procedure = files[1].ListElements().First(p => p.Name == "Bethlehem") as IFileProcedure;
            Assert.AreEqual("Bethlehem", procedure.Name);

            var taskContext = ExecutionHelper.ExeContext();

            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(7161L, result);
        }

    }
}

