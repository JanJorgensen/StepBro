using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core.Logging;
using StepBro.Core.Parser;
using StepBro.Core.ScriptData;
using System;
using System.Linq;
using System.Text;

namespace StepBroCoreTest.Parser
{
    [TestClass]
    public class TestFileParsing
    {
        [TestMethod]
        public void TestFileParsingSimpleFileUsings()
        {
            var files = FileBuilder.ParseFiles((ILogger)null,
                new Tuple<string, string>("andrea.sbs", "using \"betty.sbs\"; namespace Anders; procedure int Absalon(){ int i = 0; i = Bethlehem(); return i; }"),
                new Tuple<string, string>("betty.sbs", "namespace Anders; procedure int Bethlehem(){ return 7161; }"));
            Assert.AreEqual(2, files.Length);
            Assert.AreEqual("andrea.sbs", files.ElementAt(0).FileName);
            Assert.AreEqual("betty.sbs", files.ElementAt(1).FileName);
            var procedureA = files[0].ListElements().First(p => p.Name == "Absalon") as IFileProcedure;
            Assert.AreEqual("Absalon", procedureA.Name);
            var procedureB = files[1].ListElements().First(p => p.Name == "Bethlehem") as IFileProcedure;
            Assert.AreEqual("Bethlehem", procedureB.Name);

            var taskContext = ExecutionHelper.ExeContext(services: FileBuilder.LastServiceManager.Manager);

            var result = taskContext.CallProcedure(procedureA);
            Assert.AreEqual(7161L, result);
        }

        [TestMethod]
        public void TestFileParsingWithReParsing()
        {
            var f = new StringBuilder();
            f.AppendLine("using " + typeof(DummyInstrumentClass).Namespace + ";");
            f.AppendLine("namespace ObjectUsing;");
            f.AppendLine("public " + typeof(DummyInstrumentClass).Name + " myTool");
            f.AppendLine("{");
            f.AppendLine("   BoolA: true,");
            f.AppendLine("   IntA:  19");
            f.AppendLine("}");
            f.AppendLine("procedure " + typeof(DummyInstrumentClass).Name + " GetObject() { return myTool; }");

            var files = FileBuilder.ParseFiles((ILogger)null, this.GetType().Assembly, new Tuple<string, string>("myfile.sbs", f.ToString()));
            var file = files[0];
            Assert.AreEqual("myfile.sbs", files.ElementAt(0).FileName);
            var procGetObject = files[0].ListElements().First(p => p.Name == "GetObject") as IFileProcedure;
            Assert.AreEqual("GetObject", procGetObject.Name);

            var taskContext = ExecutionHelper.ExeContext(services: FileBuilder.LastServiceManager.Manager);

            var obj = taskContext.CallProcedure(procGetObject) as DummyInstrumentClass;
            Assert.IsNotNull(obj);
            Assert.AreEqual(10, obj.ID);
            Assert.AreEqual(19, obj.IntA);

            file.ResetBeforeParsing(true);
            file.SetParserFileStream(f.ToString());
            var errorCount = FileBuilder.ParseFiles(FileBuilder.LastServiceManager.Manager, null, file);
            procGetObject = file.ListElements().First(p => p.Name == "GetObject") as IFileProcedure;
            Assert.IsNotNull(procGetObject);
            obj = taskContext.CallProcedure(procGetObject) as DummyInstrumentClass;
            Assert.IsNotNull(obj);
            Assert.AreEqual(1, obj.ResetCounts);
            Assert.AreEqual(10, obj.ID);
            Assert.AreEqual(19, obj.IntA);


            f = new StringBuilder();
            f.AppendLine("using " + typeof(DummyInstrumentClass).Namespace + ";");
            f.AppendLine("namespace ObjectUsing;");
            f.AppendLine("public " + typeof(DummyInstrumentClass).Name + " myTool");
            f.AppendLine("{");
            f.AppendLine("   BoolA: true,");
            f.AppendLine("   IntA:  37");
            f.AppendLine("}");
            f.AppendLine("procedure " + typeof(DummyInstrumentClass).Name + " GetObject() { return myTool; }");
            file.ResetBeforeParsing(true);
            file.SetParserFileStream(f.ToString());
            errorCount = FileBuilder.ParseFiles(FileBuilder.LastServiceManager.Manager, null, file);
            procGetObject = file.ListElements().First(p => p.Name == "GetObject") as IFileProcedure;
            Assert.IsNotNull(procGetObject);
            obj = taskContext.CallProcedure(procGetObject) as DummyInstrumentClass;
            Assert.IsNotNull(obj);
            Assert.AreEqual(2, obj.ResetCounts);
            Assert.AreEqual(10, obj.ID);
            Assert.AreEqual(37, obj.IntA);
        }
    }
}

