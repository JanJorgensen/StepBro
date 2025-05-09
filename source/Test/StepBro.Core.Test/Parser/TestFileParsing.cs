using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using StepBro.Core.Data;
using StepBro.Core.Logging;
using StepBro.Core.Parser;
using StepBro.Core.ScriptData;
using StepBroCoreTest.Utils;
using System;
using System.Collections.Generic;
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
        public void TestFileParsingProceduresOfSameNameInDifferentNamespaces()
        {
            string f1 =
                """
                using "betty.sbs";
                namespace Anders;
                public void Absalon()
                {
                    Fly();
                    Anders.Fly();
                    Bent.Fly();
                    Christian.Fly();
                }
                public void Fly(){}
                """;

            var files = FileBuilder.ParseFiles((ILogger)null,
                new Tuple<string, string>("andrea.sbs", f1),
                new Tuple<string, string>("betty.sbs",      "public using \"chrissy.sbs\";  namespace Bent;         public void Fly(){}"),
                new Tuple<string, string>("chrissy.sbs",    "                               namespace Christian;    public void Fly(){}"));
            Assert.AreEqual(3, files.Length);
            Assert.AreEqual(0, files[0].Errors.ErrorCount);
            Assert.AreEqual(0, files[1].Errors.ErrorCount);
            Assert.AreEqual(0, files[2].Errors.ErrorCount);
            var procedureA = files[0].ListElements().First(p => p.Name == "Absalon") as IFileProcedure;
            Assert.AreEqual("Absalon", procedureA.Name);

            var taskContext = ExecutionHelper.ExeContext(services: FileBuilder.LastServiceManager.Manager);

            taskContext.CallProcedure(procedureA);

            var log = new LogInspector(taskContext.Logger);
            log.DebugDump();
            log.ExpectNext("0 - Pre - TestRun - Starting");
            log.ExpectNext("1 - Pre - Anders.Absalon - <no arguments>");
            log.ExpectNext("2 - Pre - 5 Anders.Fly - <no arguments>");
            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - Pre - 6 Anders.Fly - <no arguments>");
            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - Pre - 7 Bent.Fly - <no arguments>");
            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - Pre - 8 Christian.Fly - <no arguments>");
            log.ExpectNext("3 - Post");
            log.ExpectNext("2 - Post");
            log.ExpectEnd();
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
        public void FileParsing_AccessVariablesDifferentNamespace()
        {
            var files = FileBuilder.ParseFiles((ILogger)null,
                new Tuple<string, string>("andrea.sbs", "using \"betty.sbs\"; namespace Anders; procedure int Absalon(){ \r\n int i = varPublic;\r\n return i; }"),
                new Tuple<string, string>("betty.sbs", "namespace Bent; public int varPublic = 5; protected int varProtected = 8; private int varPrivate = 11;"));
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
            Assert.AreEqual(5L, result);
        }

        [TestMethod]
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

            var taskContext = ExecutionHelper.ExeContext(services: FileBuilder.LastServiceManager.Manager);

            var result = taskContext.CallProcedure(procedureA);
            Assert.AreEqual(82L, result);
        }

        [TestMethod]
        public void TestFileParsingHandlingDublicateFileUsing()
        {
            var files = FileBuilder.ParseFiles((ILogger)null,
                new Tuple<string, string>("andrea.sbs", "using \"betty.sbs\"; using \"betty.sbs\"; namespace Anders; procedure int Absalon(){ int i = 0; i = Bethlehem(); return i; }"),
                new Tuple<string, string>("betty.sbs", "namespace Anders; public procedure int Bethlehem(){ return 7161; }"));
            Assert.AreEqual(2, files.Length);
            Assert.AreEqual(1, files.ElementAt(0).Errors.ErrorCount);
            Assert.AreEqual("File using already added (betty.sbs).", files.ElementAt(0).Errors[0].Message);
            Assert.AreEqual(0, files.ElementAt(1).Errors.ErrorCount);
        }


        [TestMethod, Description("Testing the behaviour of generated code when parsing, and then re-parsing the code.")]
        public void TestFileParsingWithReParsing()
        {
            var f = new StringBuilder();
            f.AppendLine("using " + typeof(DummyInstrumentClass).Namespace + ";");  // An object with the IResettable interface.
            f.AppendLine("namespace ObjectUsing;");
            f.AppendLine("public " + typeof(DummyInstrumentClass).Name + " myTool = " + typeof(DummyInstrumentClass).Name);
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
            f.AppendLine("public " + typeof(DummyInstrumentClass).Name + " myTool = " + typeof(DummyInstrumentClass).Name);
            f.AppendLine("{");
            f.AppendLine("   BoolA: true,");
            f.AppendLine("   IntA:  37");
            f.AppendLine("}");
            f.AppendLine("procedure " + typeof(DummyInstrumentClass).Name + " GetObject() { return myTool; }");
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
        public void FileParsing_ProcedureAsExtension()
        {
            var f1 = new StringBuilder();
            f1.AppendLine("using " + typeof(DummyInstrumentClass).Namespace + ";");  // An object with the IResettable interface.
            f1.AppendLine(typeof(DummyInstrumentClass).Name + " myTool = " + typeof(DummyInstrumentClass).Name);
            f1.AppendLine("{");
            f1.AppendLine("   IntA: 783");
            f1.AppendLine("}");
            f1.AppendLine("procedure int TopCaller() { int value = 0 ; value = myTool.GetValue(); return value; }");
            f1.AppendLine("procedure int GetValue(this " + typeof(DummyInstrumentClass).Name + " instance) { return instance.IntA; }");

            var files = FileBuilder.ParseFiles((ILogger)null, this.GetType().Assembly,
                new Tuple<string, string>("myfile.sbs", f1.ToString()));
            Assert.AreEqual(1, files.Length);
            Assert.AreEqual(0, files[0].Errors.ErrorCount);
            var procedure = files[0].ListElements().FirstOrDefault(p => p.Name == "TopCaller") as IFileProcedure;
            Assert.IsNotNull(procedure);
            var element = files[0].ListElements().FirstOrDefault(p => p.Name == "myTool");
            Assert.IsNotNull(element);

            var taskContext = ExecutionHelper.ExeContext(services: FileBuilder.LastServiceManager.Manager);

            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(783L, result);
        }


        [TestMethod]
        public void FileParsing_CustomObjectData()
        {
            var source = new StringBuilder();
            source.AppendLine("using " + typeof(DummyInstrumentClass).Namespace + ";");
            source.AppendLine("namespace ObjectOverride;");
            source.AppendLine("public " + typeof(DummyInstrumentClass).Name + " myTool1 = " + typeof(DummyInstrumentClass).Name);
            source.AppendLine("{");
            source.AppendLine("   BoolA: true,");
            source.AppendLine("   IntA:  36,");
            source.AppendLine("   ExtraData:  { a: 8227, b: \"Bee\" }");    // Accepted extra data
            source.AppendLine("}");
            source.AppendLine("public " + typeof(DummyInstrumentClass).Name + " myTool2 = " + typeof(DummyInstrumentClass).Name);
            source.AppendLine("{");
            source.AppendLine("   BoolA: true,");
            source.AppendLine("   IntA:  36,");
            source.AppendLine("   Mogens:  { m1: true, m2: Jepper }");      // Unknown extra data
            source.AppendLine("}");

            var logger = new StepBro.Core.Logging.Logger("", false, "TestRun", "Starting");
            var taskContext = ExecutionHelper.ExeContext(logger: logger);

            var files = FileBuilder.ParseFiles(taskContext.Logger, this.GetType().Assembly,
                new Tuple<string, string>("script.sbs", source.ToString()));
            Assert.AreEqual(1, files.Length);
            Assert.AreEqual("script.sbs", files[0].FileName);
            Assert.AreEqual(1, files[0].Errors.ErrorCount);

            var log = new LogInspector(taskContext.Logger);
            log.DebugDump();
            log.ExpectNext("0 - Pre - TestRun - Starting");
            log.ExpectNext("1 - Detail - TestRun - Variable myTool1 - Create data");
            log.ExpectNext("1 - Detail - TestRun - Variable myTool1 init: Reset and initialize, data: { BoolA=True, IntA=36, ExtraData = { a=8227, b=Bee } }");
            log.ExpectNext("1 - Normal - TestRun - Entry: Value: a=8227");
            log.ExpectNext("1 - Normal - TestRun - Entry: Value: b=Bee");
            log.ExpectNext("1 - Detail - TestRun - Variable myTool2 - Create data");
            log.ExpectNext("1 - Detail - TestRun - Variable myTool2 init: Reset and initialize, data: { BoolA=True, IntA=36, Mogens = { m1=True, m2=Identifier: Jepper } }");
            log.ExpectNext("1 - Error - TestRun - Unknown data field: \"Mogens\", line 13");
            log.ExpectEnd();
        }

        [TestMethod]
        public void FileParsing_OverrideVariable()
        {
            var f1 = new StringBuilder();
            f1.AppendLine("using " + typeof(DummyInstrumentClass).Namespace + ";");
            f1.AppendLine("using \"libfile.sbs\";");
            f1.AppendLine("namespace ObjectOverride;");
            f1.AppendLine("override myTool");
            f1.AppendLine("{");
            f1.AppendLine("   IntA:  72");
            f1.AppendLine("}");
            f1.AppendLine("procedure int TopGetValue() { return myTool.IntA; }");

            var f2 = new StringBuilder();
            f2.AppendLine("using " + typeof(DummyInstrumentClass).Namespace + ";");
            f2.AppendLine("namespace ObjectOverride;");
            f2.AppendLine("public " + typeof(DummyInstrumentClass).Name + " myTool = " + typeof(DummyInstrumentClass).Name);
            f2.AppendLine("{");
            f2.AppendLine("   BoolA: true,");
            f2.AppendLine("   IntA:  36");
            f2.AppendLine("}");

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
            var element = files[1].ListElements().First(p => p.Name == "myTool") as IFileElement;
            Assert.IsNotNull(element);

            var taskContext = ExecutionHelper.ExeContext(services: FileBuilder.LastServiceManager.Manager);

            var result = taskContext.CallProcedure(procedureTop);
            Assert.AreEqual(72L, result);
        }

        [TestMethod]
        public void FileParsing_TypeDefSimple()
        {
            var f1 = new StringBuilder();
            f1.AppendLine("using " + typeof(DummyInstrumentClass).Namespace + ";");  // An object with the IResettable interface.
            f1.AppendLine("type MyToolType : " + typeof(DummyInstrumentClass).Name + ";");
            f1.AppendLine("MyToolType myTool = MyToolType");
            f1.AppendLine("{");
            f1.AppendLine("   IntA: 72");
            f1.AppendLine("}");
            f1.AppendLine("procedure int TopGetValue() { return myTool.IntA; }");

            var files = FileBuilder.ParseFiles((ILogger)null, this.GetType().Assembly,
                new Tuple<string, string>("myfile.sbs", f1.ToString()));
            Assert.AreEqual(1, files.Length);
            Assert.AreEqual(0, files[0].Errors.ErrorCount);
            var typedef = files[0].ListElements().FirstOrDefault(p => p.Name == "MyToolType");
            Assert.IsNotNull(typedef);
            Assert.IsNotNull(typedef.DataType);
            var procedure = files[0].ListElements().FirstOrDefault(p => p.Name == "TopGetValue") as IFileProcedure;
            Assert.IsNotNull(procedure);
            var element = files[0].ListElements().FirstOrDefault(p => p.Name == "myTool");
            Assert.IsNotNull(element);

            var taskContext = ExecutionHelper.ExeContext(services: FileBuilder.LastServiceManager.Manager);

            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(72L, result);
        }

        [TestMethod]
        public void FileParsing_TypeDef_ExtensionProcedure()
        {
            var files = CreateTypedefTestFiles(
                "procedure int TopGetValue() { var val = 0; val = myTool.GetValue(); return val; }",
                "procedure int GetValue(this MyThirdToolType instance) { return instance.IntA; }");
            Assert.AreEqual(4, files.Length);
            Assert.AreEqual("topfile.sbs", files[0].FileName);
            Assert.AreEqual("lib3file.sbs", files[1].FileName);
            Assert.AreEqual("lib2file.sbs", files[2].FileName);
            Assert.AreEqual("lib1file.sbs", files[3].FileName);
            Assert.AreEqual(0, files[0].Errors.ErrorCount);
            Assert.AreEqual(0, files[1].Errors.ErrorCount);
            Assert.AreEqual(0, files[2].Errors.ErrorCount);
            Assert.AreEqual(0, files[3].Errors.ErrorCount);
            var procedureTop = files[0].ListElements().First(p => p.Name == "TopGetValue") as IFileProcedure;
            Assert.IsNotNull(procedureTop);
            var tool = files[0].ListElements().First(p => p.Name == "myTool");
            Assert.IsNotNull(tool);

            var taskContext = ExecutionHelper.ExeContext(services: FileBuilder.LastServiceManager.Manager);
            var result = taskContext.CallProcedure(procedureTop);
            Assert.AreEqual(91L, result);
        }

        [TestMethod]
        public void FileParsing_TypeDef_ExtensionProcedure_IllegalParameterType()
        {
            var files = CreateTypedefTestFiles(
                "procedure int TopGetValue() { var val = 0; val = myTool.GetValue(); return val; }",    // The GetValue procedure has a different parameter type.
                "procedure int GetValue(this HisThirdToolType instance) { return instance.IntA; }");
            Assert.AreEqual(4, files.Length);
            Assert.AreEqual("topfile.sbs", files[0].FileName);
            Assert.AreEqual("lib3file.sbs", files[1].FileName);
            Assert.AreEqual("lib2file.sbs", files[2].FileName);
            Assert.AreEqual("lib1file.sbs", files[3].FileName);
            Assert.AreEqual(1, files[0].Errors.ErrorCount);
            Assert.AreEqual(0, files[1].Errors.ErrorCount);
            Assert.AreEqual(0, files[2].Errors.ErrorCount);
            Assert.AreEqual(0, files[3].Errors.ErrorCount);
            var procedureTop = files[0].ListElements().First(p => p.Name == "TopGetValue") as IFileProcedure;
            Assert.IsNotNull(procedureTop);
            var tool = files[0].ListElements().First(p => p.Name == "myTool");
            Assert.IsNotNull(tool);
        }

        [TestMethod]
        public void FileParsing_TypeDef_VariableAssignment()
        {
            var files = CreateTypedefTestFiles(
                "procedure int AssignAndGet() {",
                "    MyThirdToolType v = myTool;",
                "    return v.IntA;",
                "}"
                );    // The GetValue procedure has a different parameter type.
            Assert.AreEqual(4, files.Length);
            Assert.AreEqual("topfile.sbs", files[0].FileName);
            Assert.AreEqual("lib3file.sbs", files[1].FileName);
            Assert.AreEqual("lib2file.sbs", files[2].FileName);
            Assert.AreEqual("lib1file.sbs", files[3].FileName);
            Assert.AreEqual(0, files[0].Errors.ErrorCount);
            Assert.AreEqual(0, files[1].Errors.ErrorCount);
            Assert.AreEqual(0, files[2].Errors.ErrorCount);
            Assert.AreEqual(0, files[3].Errors.ErrorCount);
            var procedureTop = files[0].ListElements().First(p => p.Name == "AssignAndGet") as IFileProcedure;
            Assert.IsNotNull(procedureTop);
            var tool = files[0].ListElements().First(p => p.Name == "myTool");
            Assert.IsNotNull(tool);

            var taskContext = ExecutionHelper.ExeContext(services: FileBuilder.LastServiceManager.Manager);
            var result = taskContext.CallProcedure(procedureTop);
            Assert.AreEqual(91L, result);
        }

        [TestMethod]
        public void FileParsing_TypeDef_VariableAssignment_IllegalType()
        {
            var files = CreateTypedefTestFiles(
                "procedure int AssignToIllegalType() {",
                "    HisThirdToolType v = myTool;",
                "    return v.IntA;",
                "}"
                );    // The GetValue procedure has a different parameter type.
            Assert.AreEqual(4, files.Length);
            Assert.AreEqual("topfile.sbs", files[0].FileName);
            Assert.AreEqual("lib3file.sbs", files[1].FileName);
            Assert.AreEqual("lib2file.sbs", files[2].FileName);
            Assert.AreEqual("lib1file.sbs", files[3].FileName);
            Assert.AreEqual(2, files[0].Errors.ErrorCount);
            Assert.AreEqual(0, files[1].Errors.ErrorCount);
            Assert.AreEqual(0, files[2].Errors.ErrorCount);
            Assert.AreEqual(0, files[3].Errors.ErrorCount);
        }

        ScriptFile[] CreateTypedefTestFiles(params string[] testLines)
        {
            var f1 = new StringBuilder();
            f1.AppendLine("using \"lib3file.sbs\";");
            f1.AppendLine("namespace TypedefTest;");
            f1.AppendLine("MyThirdToolType myTool = MyThirdToolType");
            f1.AppendLine("{");
            f1.AppendLine("    IntA: 91");
            f1.AppendLine("}");
            f1.AppendLine("procedure int CreateVar() { ");
            f1.AppendLine("    MyThirdToolType v = myTool;");
            f1.AppendLine("    return v.IntA;");
            f1.AppendLine("}");
            foreach (var line in testLines) { f1.AppendLine(line); }

            var f2 = new StringBuilder();
            f2.AppendLine("using \"lib2file.sbs\";");
            f2.AppendLine("namespace TypedefTest;");
            f2.AppendLine("type MyThirdToolType : MySecondToolType;");
            f2.AppendLine("type HisThirdToolType : MySecondToolType;");

            var f3 = new StringBuilder();
            f3.AppendLine("using \"lib1file.sbs\";");
            f3.AppendLine("namespace TypedefTest;");
            f3.AppendLine("type MySecondToolType : MyFirstToolType;");

            var f4 = new StringBuilder();
            f4.AppendLine("using " + typeof(DummyInstrumentClass).Namespace + ";");  // An object with the IResettable interface.
            f4.AppendLine("namespace TypedefTest;");
            f4.AppendLine("type MyFirstToolType : " + typeof(DummyInstrumentClass).Name + ";");

            var files = FileBuilder.ParseFiles((ILogger)null, this.GetType().Assembly,
                new Tuple<string, string>("topfile.sbs", f1.ToString()),
                new Tuple<string, string>("lib3file.sbs", f2.ToString()),
                new Tuple<string, string>("lib2file.sbs", f3.ToString()),
                new Tuple<string, string>("lib1file.sbs", f4.ToString()));

            return files;
        }

        [TestMethod]
        public void FileParsing_TypeDefGenericList()
        {
            var f1 = new StringBuilder();
            f1.AppendLine("using StringList = System.Collections.Generic.List<string>;");
            f1.AppendLine("procedure string MyProc() {");
            f1.AppendLine("    StringList list;");
            f1.AppendLine("    list.Add(\"Anders\");");
            f1.AppendLine("    list.Add(\"Bent\");");
            f1.AppendLine("    list.Add(\"Christian\");");
            f1.AppendLine("    return list[1];");
            f1.AppendLine("}");

            var files = FileBuilder.ParseFiles((ILogger)null, this.GetType().Assembly,
                new Tuple<string, string>("topfile.sbs", f1.ToString()));
            Assert.AreEqual(1, files.Length);
            Assert.AreEqual("topfile.sbs", files[0].FileName);
            Assert.AreEqual(0, files[0].Errors.ErrorCount);
            var proc = files[0].ListElements().First(p => p.Name == "MyProc") as IFileProcedure;
            Assert.IsNotNull(proc);

            var taskContext = ExecutionHelper.ExeContext(services: FileBuilder.LastServiceManager.Manager);

            var result = taskContext.CallProcedure(proc);
            Assert.AreEqual("Bent", result);
        }

        [TestMethod]
        public void FileParsing_TypeDefGenericTuple()
        {
            var f1 = new StringBuilder();
            f1.AppendLine("using MyTuple = System.Tuple<string, int>;");
            f1.AppendLine("procedure MyTuple TopGetValue() {");
            f1.AppendLine("    var data = MyTuple(\"Wombat\", 17);");
            f1.AppendLine("    return data;");
            f1.AppendLine("}");

            var files = FileBuilder.ParseFiles((ILogger)null, this.GetType().Assembly,
                new Tuple<string, string>("topfile.sbs", f1.ToString()));
            Assert.AreEqual(1, files.Length);
            Assert.AreEqual("topfile.sbs", files[0].FileName);
            Assert.AreEqual(0, files[0].Errors.ErrorCount);
            var proc = files[0].ListElements().First(p => p.Name == "TopGetValue") as IFileProcedure;
            Assert.IsNotNull(proc);

            var taskContext = ExecutionHelper.ExeContext(services: FileBuilder.LastServiceManager.Manager);

            var result = taskContext.CallProcedure(proc);
            Assert.AreEqual("Wombat", ((Tuple<string, long>)result).Item1);
            Assert.AreEqual(17, ((Tuple<string, long>)result).Item2);
        }

        [TestMethod]
        public void FileParsing_OverrideVariableAsTypedef_ChangeData()
        {
            var f1 = new StringBuilder();
            f1.AppendLine("using " + typeof(DummyInstrumentClass).Namespace + ";");  // An object with the IResettable interface.
            f1.AppendLine("using \"libfile.sbs\";");
            f1.AppendLine("namespace ObjectOverride;");
            f1.AppendLine("type DumberInstrument : " + typeof(DummyInstrumentClass).Name + ";");
            f1.AppendLine("override myTool as DumberInstrument");
            f1.AppendLine("{");
            f1.AppendLine("   IntA:  72");
            f1.AppendLine("}");
            f1.AppendLine("procedure int TopGetValue()");
            f1.AppendLine("{");
            f1.AppendLine("   myTool.DoProcB();");
            f1.AppendLine("   return myTool.IntA;");
            f1.AppendLine("}");
            f1.AppendLine("procedure void DoProcB(this DumberInstrument instrument)");
            f1.AppendLine("{");
            f1.AppendLine("   instrument.DoSomethingElse();");
            f1.AppendLine("}");

            var f2 = new StringBuilder();
            f2.AppendLine("using " + typeof(DummyInstrumentClass).Namespace + ";");  // An object with the IResettable interface.
            f2.AppendLine("namespace ObjectOverride;");
            f2.AppendLine("public " + typeof(DummyInstrumentClass).Name + " myTool = " + typeof(DummyInstrumentClass).Name);
            f2.AppendLine("{");
            f2.AppendLine("   BoolA: true,");
            f2.AppendLine("   IntA:  36");
            f2.AppendLine("}");
            f2.AppendLine("procedure int LibGetValue()");
            f2.AppendLine("{");
            f2.AppendLine("   myTool.DoProcA();");
            f2.AppendLine("   return myTool.IntA;");
            f2.AppendLine("}");
            f2.AppendLine("procedure void DoProcA(this " + typeof(DummyInstrumentClass).Name + " instrument)");
            f2.AppendLine("{");
            f2.AppendLine("   instrument.DoSomething();");
            f2.AppendLine("}");

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
            var toolTop = files[0].ListElements().First(p => p.Name == "myTool") as IFileElement;
            Assert.IsNotNull(toolTop);
            Assert.IsNotNull(toolTop.DataType);
            Assert.AreEqual(typeof(TypeDef), toolTop.DataType.DynamicType.GetType());
            Assert.AreEqual("DumberInstrument", (toolTop.DataType.DynamicType as TypeDef).Name);
            var toolLib = files[1].ListElements().First(p => p.Name == "myTool") as IFileElement;
            Assert.IsNotNull(toolLib);

            var taskContext = ExecutionHelper.ExeContext(services: FileBuilder.LastServiceManager.Manager);

            var result = taskContext.CallProcedure(procedureTop);
            Assert.AreEqual(18L, result);
            result = taskContext.CallProcedure(procedureLib);
            Assert.AreEqual(1018L, result);
        }


        [TestMethod]
        public void FileParsing_OverrideVariableAsTypedef_NoDataChange_DataAsInstance()
        {
            var f1 = new StringBuilder();
            f1.AppendLine("using " + typeof(DummyInstrumentClass).Namespace + ";");  // An object with the IResettable interface.
            f1.AppendLine("using \"midfile.sbs\";");
            f1.AppendLine("namespace ObjectOverride;");
            f1.AppendLine("procedure int TopGetValue()");
            f1.AppendLine("{");
            f1.AppendLine("   myTool.IntA = 2000;");
            f1.AppendLine("   myTool.DoProcB();");
            f1.AppendLine("   return myTool.IntA;");
            f1.AppendLine("}");

            var f2 = new StringBuilder();
            f2.AppendLine("using " + typeof(DummyInstrumentClass).Namespace + ";");  // An object with the IResettable interface.
            f2.AppendLine("public using \"libfile.sbs\";");
            f2.AppendLine("namespace ObjectOverride;");
            f2.AppendLine("type DumberInstrument : " + typeof(DummyInstrumentClass).Name + ";");
            f2.AppendLine("override myTool as DumberInstrument;");
            f2.AppendLine("procedure int MidGetValue()");
            f2.AppendLine("{");
            f2.AppendLine("   myTool.DoProcB();");
            f2.AppendLine("   return myTool.IntA;");
            f2.AppendLine("}");
            f2.AppendLine("procedure void DoProcB(this DumberInstrument instrument)");
            f2.AppendLine("{");
            f2.AppendLine("   instrument.DoSomethingElse();");
            f2.AppendLine("}");

            var f3 = new StringBuilder();
            f3.AppendLine("using " + typeof(DummyInstrumentClass).Namespace + ";");  // An object with the IResettable interface.
            f3.AppendLine("namespace ObjectOverride;");
            f3.AppendLine("public " + typeof(DummyInstrumentClass).Name + " myTool = " + typeof(DummyInstrumentClass).Name);
            f3.AppendLine("{");
            f3.AppendLine("   BoolA: true,");
            f3.AppendLine("   IntA:  36");
            f3.AppendLine("}");

            var files = FileBuilder.ParseFiles((ILogger)null, this.GetType().Assembly,
                new Tuple<string, string>("topfile.sbs", f1.ToString()),
                new Tuple<string, string>("midfile.sbs", f2.ToString()),
                new Tuple<string, string>("libfile.sbs", f3.ToString()));
            Assert.AreEqual(3, files.Length);
            Assert.AreEqual("topfile.sbs", files[0].FileName);
            Assert.AreEqual("midfile.sbs", files[1].FileName);
            Assert.AreEqual("libfile.sbs", files[2].FileName);
            Assert.AreEqual(0, files[0].Errors.ErrorCount);
            Assert.AreEqual(0, files[1].Errors.ErrorCount);
            Assert.AreEqual(0, files[2].Errors.ErrorCount);
            var procedureTop = files[0].ListElements().First(p => p.Name == "TopGetValue") as IFileProcedure;
            Assert.IsNotNull(procedureTop);
            var procedureMid = files[1].ListElements().First(p => p.Name == "MidGetValue") as IFileProcedure;
            Assert.IsNotNull(procedureMid);
            var toolMid = files[1].ListElements().First(p => p.Name == "myTool") as IFileElement;
            Assert.IsNotNull(toolMid);
            Assert.IsNotNull(toolMid.ElementType == FileElementType.Override);
            var toolLib = files[2].ListElements().First(p => p.Name == "myTool") as IFileElement;
            Assert.IsNotNull(toolLib);
            Assert.IsNotNull(toolLib.ElementType == FileElementType.FileVariable);

            var taskContext = ExecutionHelper.ExeContext(services: FileBuilder.LastServiceManager.Manager);

            var result = taskContext.CallProcedure(procedureTop);
            Assert.AreEqual(500L, result);

            result = taskContext.CallProcedure(procedureMid);
            Assert.AreEqual(125L, result);
        }

        [TestMethod]
        public void FileParsing_OverrideVariableAsTypedef_NoDataChange_SimpleDataAsInstance()
        {
            var f1 = new StringBuilder();
            f1.AppendLine("using " + typeof(DummyInstrumentClass).Namespace + ";");  // An object with the IResettable interface.
            f1.AppendLine("using \"midfile.sbs\";");
            f1.AppendLine("namespace ObjectOverride;");
            f1.AppendLine("procedure void Main()");
            f1.AppendLine("{");
            f1.AppendLine("   myTool.DoProcB();");
            f1.AppendLine("}");

            var f2 = new StringBuilder();
            f2.AppendLine("using " + typeof(DummyInstrumentClass).Namespace + ";");  // An object with the IResettable interface.
            f2.AppendLine("public using \"libfile.sbs\";");
            f2.AppendLine("namespace ObjectOverride;");
            f2.AppendLine("type DumberInstrument : " + typeof(DummyInstrumentClass).Name + ";");
            f2.AppendLine("override myTool as DumberInstrument;");
            f2.AppendLine("procedure void DoProcB(this " + typeof(DummyInstrumentClass).Name + " instrument)");
            f2.AppendLine("{");
            f2.AppendLine("   instrument.IntA = 992;");
            f2.AppendLine("}");

            var f3 = new StringBuilder();
            f3.AppendLine("using " + typeof(DummyInstrumentClass).Namespace + ";");  // An object with the IResettable interface.
            f3.AppendLine("namespace ObjectOverride;");
            f3.AppendLine("public " + typeof(DummyInstrumentClass).Name + " myTool = " + typeof(DummyInstrumentClass).Name);
            f3.AppendLine("{");
            f3.AppendLine("   BoolA: true,");
            f3.AppendLine("   IntA:  36");
            f3.AppendLine("}");

            var files = FileBuilder.ParseFiles((ILogger)null, this.GetType().Assembly,
                new Tuple<string, string>("topfile.sbs", f1.ToString()),
                new Tuple<string, string>("midfile.sbs", f2.ToString()),
                new Tuple<string, string>("libfile.sbs", f3.ToString()));
            Assert.AreEqual(3, files.Length);
            Assert.AreEqual("topfile.sbs", files[0].FileName);
            Assert.AreEqual("midfile.sbs", files[1].FileName);
            Assert.AreEqual("libfile.sbs", files[2].FileName);
            Assert.AreEqual(0, files[0].Errors.ErrorCount);
            Assert.AreEqual(0, files[1].Errors.ErrorCount);
            Assert.AreEqual(0, files[2].Errors.ErrorCount);
            var procedureMain = files[0].ListElements().First(p => p.Name == "Main") as IFileProcedure;
            Assert.IsNotNull(procedureMain);
            var toolMid = files[1].ListElements().First(p => p.Name == "myTool") as IFileElement;
            Assert.IsNotNull(toolMid);
            Assert.IsNotNull(toolMid.ElementType == FileElementType.Override);
            var toolLib = files[2].ListElements().First(p => p.Name == "myTool") as IFileElement;
            Assert.IsNotNull(toolLib);
            Assert.IsNotNull(toolLib.ElementType == FileElementType.FileVariable);

            var taskContext = ExecutionHelper.ExeContext(services: FileBuilder.LastServiceManager.Manager);
            taskContext.CallProcedure(procedureMain);
        }

        [TestMethod]
        public void FileParsing_OverrideVariableAsTypedef_NoDataChange_DataAsArgument()
        {
            var f1 = new StringBuilder();
            f1.AppendLine("using " + typeof(DummyInstrumentClass).Namespace + ";");  // An object with the IResettable interface.
            f1.AppendLine("using \"midfile.sbs\";");
            f1.AppendLine("namespace ObjectOverride;");
            f1.AppendLine("procedure void Main()");
            f1.AppendLine("{");
            f1.AppendLine("   DoProcB(myTool);");
            f1.AppendLine("}");

            var f2 = new StringBuilder();
            f2.AppendLine("using " + typeof(DummyInstrumentClass).Namespace + ";");  // An object with the IResettable interface.
            f2.AppendLine("public using \"libfile.sbs\";");
            f2.AppendLine("namespace ObjectOverride;");
            f2.AppendLine("type DumberInstrument : " + typeof(DummyInstrumentClass).Name + ";");
            f2.AppendLine("override myTool as DumberInstrument;");
            f2.AppendLine("procedure void DoProcB(" + typeof(DummyInstrumentClass).Name + " instrument)");
            f2.AppendLine("{");
            f2.AppendLine("   instrument.IntA = 992;");
            f2.AppendLine("}");

            var f3 = new StringBuilder();
            f3.AppendLine("using " + typeof(DummyInstrumentClass).Namespace + ";");  // An object with the IResettable interface.
            f3.AppendLine("namespace ObjectOverride;");
            f3.AppendLine("public " + typeof(DummyInstrumentClass).Name + " myTool = " + typeof(DummyInstrumentClass).Name);
            f3.AppendLine("{");
            f3.AppendLine("   BoolA: true,");
            f3.AppendLine("   IntA:  36");
            f3.AppendLine("}");

            var files = FileBuilder.ParseFiles((ILogger)null, this.GetType().Assembly,
                new Tuple<string, string>("topfile.sbs", f1.ToString()),
                new Tuple<string, string>("midfile.sbs", f2.ToString()),
                new Tuple<string, string>("libfile.sbs", f3.ToString()));
            Assert.AreEqual(3, files.Length);
            Assert.AreEqual("topfile.sbs", files[0].FileName);
            Assert.AreEqual("midfile.sbs", files[1].FileName);
            Assert.AreEqual("libfile.sbs", files[2].FileName);
            Assert.AreEqual(0, files[0].Errors.ErrorCount);
            Assert.AreEqual(0, files[1].Errors.ErrorCount);
            Assert.AreEqual(0, files[2].Errors.ErrorCount);
            var procedureMain = files[0].ListElements().First(p => p.Name == "Main") as IFileProcedure;
            Assert.IsNotNull(procedureMain);
            var toolMid = files[1].ListElements().First(p => p.Name == "myTool") as IFileElement;
            Assert.IsNotNull(toolMid);
            Assert.IsNotNull(toolMid.ElementType == FileElementType.Override);
            var toolLib = files[2].ListElements().First(p => p.Name == "myTool") as IFileElement;
            Assert.IsNotNull(toolLib);
            Assert.IsNotNull(toolLib.ElementType == FileElementType.FileVariable);
            var obj = (toolLib as FileVariable).Value.Object as DummyInstrumentClass;

            var taskContext = ExecutionHelper.ExeContext(services: FileBuilder.LastServiceManager.Manager);
            taskContext.CallProcedure(procedureMain);
            Assert.AreEqual(992L, obj.IntA);
        }

        [TestMethod]
        public void FileParsing_TypeDefUseConstructorOneAbstraction()
        {
            string f1 =
                """
                using System;

                type TupleInts : Tuple<int, int>;

                procedure TupleInts test()
                {
                    TupleInts a = testTuple();
                    return a;
                }

                function TupleInts testTuple()
                {
                    TupleInts toReturn = TupleInts(5, 7);
                    return toReturn;
                }
                """;

            var files = FileBuilder.ParseFiles((ILogger)null, this.GetType().Assembly,
                new Tuple<string, string>("myfile.sbs", f1.ToString()));
            Assert.AreEqual(1, files.Length);
            Assert.AreEqual(0, files[0].Errors.ErrorCount);
            var typedef = files[0].ListElements().FirstOrDefault(p => p.Name == "TupleInts");
            Assert.IsNotNull(typedef);
            Assert.IsNotNull(typedef.DataType);
            var procedure = files[0].ListElements().FirstOrDefault(p => p.Name == "test") as IFileProcedure;
            Assert.IsNotNull(procedure);

            var taskContext = ExecutionHelper.ExeContext(services: FileBuilder.LastServiceManager.Manager);

            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(5, ((Tuple<long, long>)result).Item1);
            Assert.AreEqual(7, ((Tuple<long, long>)result).Item2);
        }

        [TestMethod]
        public void FileParsing_TypeDefUseOtherTypeDef()
        {
            string f1 =
                """
                using System;
                
                type TupleInts : Tuple<int, int>;
                type TupleList : System.Collections.Generic.List<TupleInts>;

                procedure TupleList test()
                {
                    var list  = TupleList();
                    var entry = TupleInts(12, 16);
                    list.Add(entry);
                    list.Add(TupleInts(22, 28));
                    return list;
                }
                """;

            var files = FileBuilder.ParseFiles((ILogger)null, this.GetType().Assembly,
                new Tuple<string, string>("myfile.sbs", f1.ToString()));
            Assert.AreEqual(1, files.Length);
            Assert.AreEqual(0, files[0].Errors.ErrorCount);
            var typedef = files[0].ListElements().FirstOrDefault(p => p.Name == "TupleInts");
            Assert.IsNotNull(typedef);
            Assert.IsNotNull(typedef.DataType);
            var procedure = files[0].ListElements().FirstOrDefault(p => p.Name == "test") as IFileProcedure;
            Assert.IsNotNull(procedure);

            var taskContext = ExecutionHelper.ExeContext(services: FileBuilder.LastServiceManager.Manager);

            var result = taskContext.CallProcedure(procedure);
            Assert.IsInstanceOfType(result, typeof(List<Tuple<long, long>>));
            var list = result as List<Tuple<long, long>>;
            Assert.AreEqual(2, list.Count);
            Assert.AreEqual(12L, (list[0]).Item1);
            Assert.AreEqual(16L, (list[0]).Item2);
            Assert.AreEqual(22L, (list[1]).Item1);
            Assert.AreEqual(28L, (list[1]).Item2);
        }

        [TestMethod]
        public void FileParsing_TypeDefUsePropertyWhereExtensionMethodExistsWithSameName()
        {
            string f1 =
                """
                using System;
                
                type EntryType : Tuple<int,int>;
                type MyList : System.Collections.Generic.List<EntryType>;

                procedure int test()
                {
                    var list = MyList();
                    list.Add(EntryType(0, 30));
                    list.Add(EntryType(1, 31));
                    list.Add(EntryType(2, 32));
                    return list.Count;
                }
                """;

            var files = FileBuilder.ParseFiles((ILogger)null, this.GetType().Assembly,
                new Tuple<string, string>("myfile.sbs", f1.ToString()));
            Assert.AreEqual(1, files.Length);
            Assert.AreEqual(0, files[0].Errors.ErrorCount);
            var typedef = files[0].ListElements().FirstOrDefault(p => p.Name == "MyList");
            Assert.IsNotNull(typedef);
            Assert.IsNotNull(typedef.DataType);
            var procedure = files[0].ListElements().FirstOrDefault(p => p.Name == "test") as IFileProcedure;
            Assert.IsNotNull(procedure);

            var taskContext = ExecutionHelper.ExeContext(services: FileBuilder.LastServiceManager.Manager);

            var result = taskContext.CallProcedure(procedure);
            Assert.IsInstanceOfType(result, typeof(long));
            Assert.AreEqual(3L, (long)result);
        }

        [TestMethod, Ignore("Finding generic type List<> via a using statement is not implemented.")]
        public void FileParsing_TypeDefByNamespaceUsing()
        {
            string f1 =
                """
                using System;
                using System.Collections.Generic;   // Make this work.
                                
                type TupleInts : Tuple<int, int>;
                type TupleList : List<TupleInts>;   // Make this work.

                procedure TupleList test()
                {
                    var list  = TupleList();
                    var entry = TupleInts(12, 16);
                    list.Add(entry);
                    list.Add(TupleInts(22, 28));
                    return list;
                }
                """;

            var files = FileBuilder.ParseFiles((ILogger)null, this.GetType().Assembly,
                new Tuple<string, string>("myfile.sbs", f1.ToString()));
            Assert.AreEqual(1, files.Length);
            Assert.AreEqual(0, files[0].Errors.ErrorCount);
            var typedef = files[0].ListElements().FirstOrDefault(p => p.Name == "TupleInts");
            Assert.IsNotNull(typedef);
            Assert.IsNotNull(typedef.DataType);
            var procedure = files[0].ListElements().FirstOrDefault(p => p.Name == "test") as IFileProcedure;
            Assert.IsNotNull(procedure);

            var taskContext = ExecutionHelper.ExeContext(services: FileBuilder.LastServiceManager.Manager);

            var result = taskContext.CallProcedure(procedure);
            Assert.IsInstanceOfType(result, typeof(List<Tuple<long, long>>));
            var list = result as List<Tuple<long, long>>;
            Assert.AreEqual(2, list.Count);
            Assert.AreEqual(12L, (list[0]).Item1);
            Assert.AreEqual(16L, (list[0]).Item2);
            Assert.AreEqual(22L, (list[1]).Item1);
            Assert.AreEqual(28L, (list[1]).Item2);
        }

        [TestMethod]
        public void FileParsing_TypeDefUseConstructorTwoAbstractions()
        {
            string f1 =
                """
                using System;

                type TupleInts : Tuple<int, int>;
                type TupleTest : TupleInts;

                procedure TupleInts test()
                {
                    TupleInts a = testTuple();
                    return a;
                }

                function TupleTest testTuple()
                {
                    TupleTest toReturn = TupleTest(5, 7);
                    return toReturn;
                }
                """;

            var files = FileBuilder.ParseFiles((ILogger)null, this.GetType().Assembly,
                new Tuple<string, string>("myfile.sbs", f1.ToString()));
            Assert.AreEqual(1, files.Length);
            Assert.AreEqual(0, files[0].Errors.ErrorCount);
            var typedef = files[0].ListElements().FirstOrDefault(p => p.Name == "TupleInts");
            Assert.IsNotNull(typedef);
            Assert.IsNotNull(typedef.DataType);
            var procedure = files[0].ListElements().FirstOrDefault(p => p.Name == "test") as IFileProcedure;
            Assert.IsNotNull(procedure);

            var taskContext = ExecutionHelper.ExeContext(services: FileBuilder.LastServiceManager.Manager);

            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(5, ((Tuple<long, long>)result).Item1);
            Assert.AreEqual(7, ((Tuple<long, long>)result).Item2);
        }

        [TestMethod]
        public void FileParsing_TypeDefUseConstructorNarrowTwoAbstractions()
        {
            string f1 =
                """
                using System;

                type TupleInts : Tuple<int, int>;
                type TupleTest : TupleInts;

                procedure TupleInts test()
                {
                    TupleInts a = testTuple();
                    return a;
                }

                function TupleInts testTuple()
                {
                    TupleInts toReturn = TupleTest(5, 7);
                    return toReturn;
                }
                """;

            var files = FileBuilder.ParseFiles((ILogger)null, this.GetType().Assembly,
                new Tuple<string, string>("myfile.sbs", f1.ToString()));
            Assert.AreEqual(1, files.Length);
            Assert.AreEqual(0, files[0].Errors.ErrorCount);
            var typedef = files[0].ListElements().FirstOrDefault(p => p.Name == "TupleInts");
            Assert.IsNotNull(typedef);
            Assert.IsNotNull(typedef.DataType);
            var procedure = files[0].ListElements().FirstOrDefault(p => p.Name == "test") as IFileProcedure;
            Assert.IsNotNull(procedure);

            var taskContext = ExecutionHelper.ExeContext(services: FileBuilder.LastServiceManager.Manager);

            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(5, ((Tuple<long, long>)result).Item1);
            Assert.AreEqual(7, ((Tuple<long, long>)result).Item2);
        }

        [TestMethod]
        public void FileParsing_TypeDefUseConstructorBroadenTwoAbstractions()
        {
            string f1 =
                """
                using System;

                type TupleInts : Tuple<int, int>;
                type TupleTest : TupleInts;

                procedure TupleInts test()
                {
                    TupleInts a = testTuple();
                    return a;
                }

                function TupleInts testTuple()
                {
                    // Here we try to assign TupleInts to a TupleTest variable, using a constructor
                    // This is an error because TupleTest is narrower than TupleInts
                    TupleTest toReturn = TupleInts(5, 7);
                    return toReturn;
                }
                """;

            var files = FileBuilder.ParseFiles((ILogger)null, this.GetType().Assembly,
                new Tuple<string, string>("myfile.sbs", f1.ToString()));
            Assert.AreEqual(1, files.Length);

            // When we are assigning a broad type to a specific type we expect an error, like if we have
            // List<int> a = new List<object>();
            // We expect an error, this is the case in the code above.
            Assert.AreEqual(1, files[0].Errors.ErrorCount);
            Assert.IsTrue(files[0].Errors[0].Message.Contains("incompatible type"));
        }

        [TestMethod]
        public void FileParsing_TypeDefUseConstructorTwoAbstractionsError()
        {
            string f1 =
                """
                using System;

                type TupleInts : Tuple<int, int>;
                type TupleTest : TupleInts;

                procedure TupleTest test()
                {
                    // Here we try to assign TupleInts to a TupleTest variable using a function call
                    // This is an error because TupleTest is narrower than TupleInts
                    TupleTest a = testTuple();
                    return a;
                }

                function TupleInts testTuple()
                {
                    TupleInts toReturn = TupleInts(5, 7);
                    return toReturn;
                }
                """;

            var files = FileBuilder.ParseFiles((ILogger)null, this.GetType().Assembly,
                new Tuple<string, string>("myfile.sbs", f1.ToString()));
            Assert.AreEqual(1, files.Length);

            // When we are assigning a broad type to a specific type we expect an error, like if we have
            // List<int> a = new List<object>();
            // We expect an error, this is the case in the code above.
            Assert.AreEqual(1, files[0].Errors.ErrorCount);
            Assert.IsTrue(files[0].Errors[0].Message.Contains("incompatible type"));
        }


        [TestMethod]
        public void FileParsing_ProcedureExtensionCallingMethodWithSameName()
        {
            var f1 = new StringBuilder();
            f1.AppendLine("using " + typeof(DummyInstrumentClass).Namespace + ";");  // An object with the IResettable interface.
            f1.AppendLine("namespace Extensions;");
            f1.AppendLine("public " + typeof(DummyInstrumentClass).Name + " myTool = " + typeof(DummyInstrumentClass).Name);
            f1.AppendLine("{");
            f1.AppendLine("   BoolA: true,");
            f1.AppendLine("   IntA:  36");
            f1.AppendLine("}");
            f1.AppendLine("procedure int Top()");
            f1.AppendLine("{");
            f1.AppendLine("   myTool.Work();");         // CALL THE PROCEDURE.
            f1.AppendLine("   return myTool.IntA;");
            f1.AppendLine("}");
            f1.AppendLine("procedure void Work(this " + typeof(DummyInstrumentClass).Name + " instrument)");
            f1.AppendLine("{");
            f1.AppendLine("   instrument.DoSomething();");
            f1.AppendLine("   instrument.@Work();");    // CALL THE OBJECT METHOD.
            f1.AppendLine("}");

            var files = FileBuilder.ParseFiles((ILogger)null, this.GetType().Assembly,
                new Tuple<string, string>("topfile.sbs", f1.ToString()));
            Assert.AreEqual(1, files.Length);
            Assert.AreEqual("topfile.sbs", files[0].FileName);
            Assert.AreEqual(0, files[0].Errors.ErrorCount);
            var procedureTop = files[0].ListElements().First(p => p.Name == "Top") as IFileProcedure;
            Assert.IsNotNull(procedureTop);
            var toolTop = files[0].ListElements().First(p => p.Name == "myTool") as IFileElement;
            Assert.IsNotNull(toolTop);

            var taskContext = ExecutionHelper.ExeContext(services: FileBuilder.LastServiceManager.Manager);

            var result = taskContext.CallProcedure(procedureTop);
            Assert.AreEqual(3108L, result);
        }

        [TestMethod]
        public void FileParsing_DocumentationElement()
        {
            string f1 =
                """
                /// summary: Some documentation for the file.
                /// And some more <br/> And yet some _more_.
                documentation JustSomeDocLineWithoutAnyDocumentation;
                void main()
                {
                    log("");
                }
                """;

            var files = FileBuilder.ParseFiles((ILogger)null, this.GetType().Assembly,
                new Tuple<string, string>("myfile.sbs", f1.ToString()));

            Assert.AreEqual(0, files[0].Errors.ErrorCount);
            var docElement = files[0].ListElements().FirstOrDefault(e => e.ElementType == FileElementType.Documentation) as FileElementDocumentation;
            Assert.IsNotNull(docElement);
            Assert.AreEqual(3, docElement.Line);
            var documentation = docElement.GetDocumentation();
            Assert.AreEqual(2, documentation.Count);
        }

        [TestMethod]
        public void FileParsing_ArgumentErrorInProcedureCall()
        {
            string f1 =
                """
                void main()
                {
                    sub(mogens);
                }
                void sub(int x)
                {
                }
                """;

            var files = FileBuilder.ParseFiles((ILogger)null, this.GetType().Assembly,
                new Tuple<string, string>("myfile.sbs", f1.ToString()));

            Assert.AreEqual(1, files[0].Errors.ErrorCount);
            Assert.AreEqual("Unknown identifier: mogens", files[0].Errors[0].Message);
        }

        [TestMethod]
        public void FileParsing_UsingMethodInStaticClass()
        {
            string f1 =
                """
                int main()
                {
                    var v = 10.8;
                    var i = System.Convert.ToInt64(v);
                    return i;
                }
                """;

            var files = FileBuilder.ParseFiles((ILogger)null, this.GetType().Assembly,
                new Tuple<string, string>("myfile.sbs", f1.ToString()));

            Assert.AreEqual(0, files[0].Errors.ErrorCount);

            var procedure = files[0].ListElements().First(p => p.Name == "main") as IFileProcedure;
            Assert.IsNotNull(procedure);

            var taskContext = ExecutionHelper.ExeContext(services: FileBuilder.LastServiceManager.Manager);
            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(11L, result);
        }

        [TestMethod]
        public void FileParsing_FilePathVariable()
        {
            string source =
                $$"""
                using {{typeof(DummyInstrumentClass).Namespace}};
                namespace SayMyName;
                const filepath myFile = @"[this]\sub\zup\script.sbs";
                public string UseVariable()
                {
                    {{nameof(DummyInstrumentClass)}}.ShowString(myFile);
                    return myFile;
                }
                """;

            var files = FileBuilder.ParseFiles((ILogger)null, this.GetType().Assembly,
                new Tuple<string, string>("myfile.sbs", source.ToString()));
            Assert.AreEqual(1, files.Length);
            Assert.AreEqual("myfile.sbs", files[0].FileName);
            Assert.AreEqual(0, files[0].Errors.ErrorCount);
            var procedure = files[0].ListElements().FirstOrDefault(p => p.Name == "UseVariable") as IFileProcedure;

            var taskContext = ExecutionHelper.ExeContext(services: FileBuilder.LastServiceManager.Manager);
            var result = taskContext.CallProcedure(procedure);
            Assert.IsNotNull(result);
            Assert.IsTrue(result is string);
        }

        [TestMethod]
        public void FileParsing_UsingPropertyAsMethod()
        {
            string source =
                $$"""
                using {{typeof(DummyInstrumentClass).Namespace}};
                namespace ErrorCheck;
                {{nameof(DummyInstrumentClass)}} tool = {{nameof(DummyInstrumentClass)}}(10);
                procedure bool Func()
                {
                    return tool.BoolA();    // This should give an error.
                }
                """;

            var files = FileBuilder.ParseFiles((ILogger)null, this.GetType().Assembly,
                new Tuple<string, string>("myfile.sbs", source.ToString()));
            Assert.AreEqual(1, files.Length);
            Assert.AreEqual("myfile.sbs", files[0].FileName);
            Assert.AreEqual(1, files[0].Errors.ErrorCount);         // The one error.        
            Assert.IsTrue(files[0].Errors[0].Message.Contains("property \"BoolA\""));
            Assert.IsTrue(files[0].Errors[0].Message.Contains("can not be used as a method"));
        }
    }
}

