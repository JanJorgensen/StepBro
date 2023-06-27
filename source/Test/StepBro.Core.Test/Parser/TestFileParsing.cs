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
            f.AppendLine("public " + typeof(DummyInstrumentClass).Name + " myTool = " + typeof(DummyInstrumentClass).Name);
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
        public void FileParsing_TypeDefGeneric()
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
        public void FileParsing_OverrideVariableAsTypedef()
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
            f1.AppendLine("procedure int TopGetValue() { return myTool.IntA; }");
            f1.AppendLine("procedure void DoProcB(this DumberInstrument instrument) { instrument.DoSomethingElse(); }");

            var f2 = new StringBuilder();
            f2.AppendLine("using " + typeof(DummyInstrumentClass).Namespace + ";");  // An object with the IResettable interface.
            f2.AppendLine("namespace ObjectOverride;");
            f2.AppendLine("public " + typeof(DummyInstrumentClass).Name + " myTool = " + typeof(DummyInstrumentClass).Name);
            f2.AppendLine("{");
            f2.AppendLine("   BoolA: true,");
            f2.AppendLine("   IntA:  36");
            f2.AppendLine("}");
            f2.AppendLine("procedure int LibGetValue() { return myTool.IntA; }");
            f2.AppendLine("procedure void DoProcA(this " + typeof(DummyInstrumentClass).Name + " instrument) { instrument.DoSomething(); }");

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
            var toolLib = files[1].ListElements().First(p => p.Name == "myTool") as IFileElement;
            Assert.IsNotNull(toolLib);

            var taskContext = ExecutionHelper.ExeContext(services: FileBuilder.LastServiceManager.Manager);

            var result = taskContext.CallProcedure(procedureTop);
            Assert.AreEqual(72L, result);
            result = taskContext.CallProcedure(procedureLib);
            Assert.AreEqual(72L, result);
        }
    }
}

