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
                new Tuple<string, string>("betty.sbs", "namespace Anders; public procedure int Bethlehem(){ return 7161; }"));
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
        public void FileParsing_AccessVariablesSameNamespace()
        {
            var files = FileBuilder.ParseFiles((ILogger)null,
                new Tuple<string, string>("andrea.sbs", "using \"betty.sbs\"; namespace Anders; procedure int Absalon(){ \r\n int i = varPublic;\r\n i += varProtected;\r\n return i; }"),
                new Tuple<string, string>("betty.sbs", "namespace Anders; public int varPublic = 5; protected int varProtected = 8; private int varPrivate = 11;"));
            Assert.AreEqual(2, files.Length);
            Assert.AreEqual("andrea.sbs", files[0].FileName);
            Assert.AreEqual("betty.sbs", files[1].FileName);
            Assert.AreEqual(0, files[0].Errors.ErrorCount);
            Assert.AreEqual(0, files[1].Errors.ErrorCount);
            var procedureA = files[0].ListElements().First(p => p.Name == "Absalon") as IFileProcedure;
            Assert.IsNotNull(procedureA);
            var element = files[1].ListElements().First(p => p.Name == "varPublic") as IFileElement;
            Assert.IsNotNull(element);
            element = files[1].ListElements().First(p => p.Name == "varProtected") as IFileElement;
            Assert.IsNotNull(element);
            element = files[1].ListElements().First(p => p.Name == "varPrivate") as IFileElement;
            Assert.IsNotNull(element);

            var taskContext = ExecutionHelper.ExeContext(services: FileBuilder.LastServiceManager.Manager);

            var result = taskContext.CallProcedure(procedureA);
            Assert.AreEqual(13L, result);
        }

        [TestMethod]
        public void FileParsing_AccessPrivateVariableSameNamespace()
        {
            var files = FileBuilder.ParseFiles((ILogger)null,
                new Tuple<string, string>("andrea.sbs", "using \"betty.sbs\"; namespace Anders; procedure int Absalon(){ \r\n int i = varPublic; \r\n i += varProtected; \r\n i += varPrivate;\r\n return i; }"),
                new Tuple<string, string>("betty.sbs", "namespace Anders; public int varPublic = 5; protected int varProtected = 8; private int varPrivate = 11;"));
            Assert.AreEqual(2, files.Length);
            Assert.AreEqual("andrea.sbs", files[0].FileName);
            Assert.AreEqual("betty.sbs", files[1].FileName);
            Assert.AreEqual(1, files[0].Errors.ErrorCount);
            Assert.AreEqual(0, files[1].Errors.ErrorCount);
            var procedureA = files[0].ListElements().First(p => p.Name == "Absalon") as IFileProcedure;
            Assert.IsNotNull(procedureA);
            var element = files[1].ListElements().First(p => p.Name == "varPublic") as IFileElement;
            Assert.IsNotNull(element);
            element = files[1].ListElements().First(p => p.Name == "varProtected") as IFileElement;
            Assert.IsNotNull(element);
            element = files[1].ListElements().First(p => p.Name == "varPrivate") as IFileElement;
            Assert.IsNotNull(element);
        }

        [TestMethod, Ignore]    // public using is not working yet.
        public void FileParsing_AccessProcedureInUsedFilesPublicUsingFile()
        {
            var files = FileBuilder.ParseFiles((ILogger)null,
                new Tuple<string, string>("andrea.sbs", "using \"betty.sbs\"; procedure int Absalon(){ int i = 0; i = Christian(); return i; }"),
                new Tuple<string, string>("betty.sbs", "public using \"chrissy.sbs\";"),
                new Tuple<string, string>("chrissy.sbs", "public int Christian() { return 82; }"));
            Assert.AreEqual(3, files.Length);
            Assert.AreEqual("andrea.sbs", files[0].FileName);
            Assert.AreEqual("betty.sbs", files[1].FileName);
            Assert.AreEqual("chrissy.sbs", files[2].FileName);
            Assert.AreEqual(0, files[0].Errors.ErrorCount);
            Assert.AreEqual(0, files[1].Errors.ErrorCount);
            Assert.AreEqual(0, files[2].Errors.ErrorCount);
            var procedureA = files[0].ListElements().First(p => p.Name == "Absalon") as IFileProcedure;
            Assert.IsNotNull(procedureA);
        }

        [TestMethod, Description("Testing the behaviour of generated code when parsing, and then re-parsing the code.")]
        public void TestFileParsingWithReParsing()
        {
            var f = new StringBuilder();
            f.AppendLine("using " + typeof(DummyInstrumentClass).Namespace + ";");  // An object with the IResettable interface.
            f.AppendLine("namespace ObjectUsing;");
            f.AppendLine("public " + typeof(DummyInstrumentClass).Name + " myTool");
            f.AppendLine("{");
            f.AppendLine("   BoolA: true,");
            f.AppendLine("   IntA:  19");
            f.AppendLine("}");
            f.AppendLine("procedure " + typeof(DummyInstrumentClass).Name + " GetObject() { return myTool; }");

            DummyInstrumentClass.m_nextInstanceID = 10;

            var files = FileBuilder.ParseFiles((ILogger)null, this.GetType().Assembly, new Tuple<string, string>("myfile.sbs", f.ToString()));
            var file = files[0];
            Assert.AreEqual("myfile.sbs", files.ElementAt(0).FileName);
            var procGetObject = files[0].ListElements().First(p => p.Name == "GetObject") as IFileProcedure;
            Assert.AreEqual("GetObject", procGetObject.Name);

            var taskContext = ExecutionHelper.ExeContext(services: FileBuilder.LastServiceManager.Manager);

            var obj1 = taskContext.CallProcedure(procGetObject) as DummyInstrumentClass;
            Assert.IsNotNull(obj1);
            Assert.AreEqual(1, obj1.ResetCounts);
            Assert.AreEqual(10, obj1.ID);
            Assert.AreEqual(19, obj1.IntA);

            file.ResetBeforeParsing(true);
            file.SetParserFileStream(f.ToString());
            var errorCount = FileBuilder.ParseFiles(FileBuilder.LastServiceManager.Manager, null, file);
            procGetObject = file.ListElements().First(p => p.Name == "GetObject") as IFileProcedure;
            Assert.IsNotNull(procGetObject);
            var obj2 = taskContext.CallProcedure(procGetObject) as DummyInstrumentClass;
            Assert.IsNotNull(obj2);
            Assert.IsTrue(Object.ReferenceEquals(obj1, obj2));
            Assert.AreEqual(1, obj2.ResetCounts);
            Assert.AreEqual(10, obj2.ID);
            Assert.AreEqual(19, obj2.IntA);


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
            var obj3 = taskContext.CallProcedure(procGetObject) as DummyInstrumentClass;
            Assert.IsNotNull(obj3);
            Assert.IsTrue(Object.ReferenceEquals(obj1, obj3));
            Assert.AreEqual(2, obj3.ResetCounts);
            Assert.AreEqual(10, obj3.ID);
            Assert.AreEqual(37, obj3.IntA);
        }

        [TestMethod]
        public void FileParsing_OverrideVariable()
        {
            var f1 = new StringBuilder();
            f1.AppendLine("using " + typeof(DummyInstrumentClass).Namespace + ";");  // An object with the IResettable interface.
            f1.AppendLine("using \"libfile.sbs\";");
            f1.AppendLine("namespace ObjectOverride;");
            f1.AppendLine("public override " + typeof(DummyInstrumentClass).Name + " myTool");
            f1.AppendLine("{");
            f1.AppendLine("   IntA:  72");
            f1.AppendLine("}");
            f1.AppendLine("procedure int TopGetValue() { return myTool.IntA; }");

            var f2 = new StringBuilder();
            f2.AppendLine("using " + typeof(DummyInstrumentClass).Namespace + ";");  // An object with the IResettable interface.
            f2.AppendLine("namespace ObjectOverride;");
            f2.AppendLine("public " + typeof(DummyInstrumentClass).Name + " myTool");
            f2.AppendLine("{");
            f2.AppendLine("   BoolA: true,");
            f2.AppendLine("   IntA:  36");
            f2.AppendLine("}");
            f2.AppendLine("procedure int LibGetValue() { return myTool.IntA; }");

            var files = FileBuilder.ParseFiles((ILogger)null, this.GetType().Assembly,
                new Tuple<string, string>("topfile.sbs", f1.ToString()),
                new Tuple<string, string>("libfile.sbs", f2.ToString()));
            Assert.AreEqual(2, files.Length);
            Assert.AreEqual("topfile.sbs", files[0].FileName);
            Assert.AreEqual("libfile.sbs", files[1].FileName);
            Assert.AreEqual(0, files[0].Errors.ErrorCount);
            Assert.AreEqual(0, files[1].Errors.ErrorCount);
            var procedureTop = files[0].ListElements().First(p => p.Name == "TopGetValue") as IFileProcedure;
            Assert.IsNotNull(procedureTop);
            var procedureLib = files[1].ListElements().First(p => p.Name == "LibGetValue") as IFileProcedure;
            Assert.IsNotNull(procedureLib);
            var element = files[1].ListElements().First(p => p.Name == "myTool") as IFileElement;
            Assert.IsNotNull(element);

            var taskContext = ExecutionHelper.ExeContext(services: FileBuilder.LastServiceManager.Manager);

            var result = taskContext.CallProcedure(procedureTop);
            Assert.AreEqual(72L, result);
            result = taskContext.CallProcedure(procedureLib);
            Assert.AreEqual(72L, result);
        }
    }
}

