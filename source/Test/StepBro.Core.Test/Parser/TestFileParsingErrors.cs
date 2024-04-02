using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core.Logging;
using StepBro.Core.Parser;
using StepBro.Core.ScriptData;
using StepBroCoreTest;
using StepBroCoreTest.Parser;
using System;
using System.Linq;
using System.Text;

namespace StepBro.Core.Test.Parser
{
    [TestClass]
    public class TestFileParsingErrors
    {
        [TestMethod]
        public void FileParsing_AccessPrivateVariableSameNamespace()
        {
            var files = FileBuilder.ParseFiles((ILogger)null,
                new Tuple<string, string>("andrea.sbs", "using \"betty.sbs\"; namespace Anders; procedure int Absalon(){ \r\n int i = varPrivate; \r\n return i; }"),
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

        [TestMethod]
        public void FileParsing_AccessVariableProtectedDifferentNamespace()
        {
            var files = FileBuilder.ParseFiles((ILogger)null,
                new Tuple<string, string>("andrea.sbs", "using \"betty.sbs\"; namespace Anders; procedure int Absalon(){ \r\n int i = varProtected;\r\n return i; }"),
                new Tuple<string, string>("betty.sbs", "namespace Bent; public int varPublic = 5; protected int varProtected = 8; private int varPrivate = 11;"));
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

        [TestMethod]
        public void FileParsing_AccessVariablePrivateDifferentNamespace()
        {
            var files = FileBuilder.ParseFiles((ILogger)null,
                new Tuple<string, string>("andrea.sbs", "using \"betty.sbs\"; namespace Anders; procedure int Absalon(){ \r\n int i = varPrivate;\r\n return i; }"),
                new Tuple<string, string>("betty.sbs", "namespace Bent; public int varPublic = 5; protected int varProtected = 8; private int varPrivate = 11;"));
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

        [TestMethod]
        public void FileParsing_UnresolvedVariable()
        {
            var file = new StringBuilder();
            file.AppendLine("procedure void Proc()");
            file.AppendLine("{");
            file.AppendLine("    myTool.Action(27);");
            file.AppendLine("}");

            var files = FileBuilder.ParseFiles((ILogger)null, this.GetType().Assembly,
                new Tuple<string, string>("test.sbs", file.ToString()));
            Assert.AreEqual(1, files.Length);
            Assert.AreEqual(1, files[0].Errors.ErrorCount);
            Assert.IsTrue(files[0].Errors[0].Message.Contains("unresolved identifier", StringComparison.InvariantCultureIgnoreCase));
        }


        [TestMethod]
        public void FileParsing_UnresolvedVariableInFlowStatements()
        {
            var file = new StringBuilder();
            file.AppendLine("procedure void Proc()");
            file.AppendLine("{");
            file.AppendLine("    if ( myTool.Action(27) )");
            file.AppendLine("    {");
            file.AppendLine("        log (\"plup\");");
            file.AppendLine("    }");
            file.AppendLine("}");

            var files = FileBuilder.ParseFiles((ILogger)null, this.GetType().Assembly,
                new Tuple<string, string>("test.sbs", file.ToString()));
            Assert.AreEqual(1, files.Length);
            Assert.AreEqual(1, files[0].Errors.ErrorCount);
            Assert.IsTrue(files[0].Errors[0].Message.Contains("unresolved identifier", StringComparison.InvariantCultureIgnoreCase));
        }

        [TestMethod]
        public void FileParsing_UnresolvedProcedure()
        {
            var file = new StringBuilder();
            file.AppendLine("procedure void Proc()");
            file.AppendLine("{");
            file.AppendLine("    bool b = false;");
            file.AppendLine("    b = BadProc(27);");
            file.AppendLine("}");

            var files = FileBuilder.ParseFiles((ILogger)null, this.GetType().Assembly,
                new Tuple<string, string>("test.sbs", file.ToString()));
            Assert.AreEqual(1, files.Length);
            Assert.AreEqual(1, files[0].Errors.ErrorCount);
            Assert.IsTrue(files[0].Errors[0].Message.Contains("unresolved identifier", StringComparison.InvariantCultureIgnoreCase));


            file = new StringBuilder();
            file.AppendLine("procedure void Proc()");
            file.AppendLine("{");
            file.AppendLine("    bool b = false;");
            file.AppendLine("    !BadProc(27);");
            file.AppendLine("}");

            files = FileBuilder.ParseFiles((ILogger)null, this.GetType().Assembly,
                new Tuple<string, string>("test.sbs", file.ToString()));
            Assert.AreEqual(1, files.Length);
            Assert.AreEqual(1, files[0].Errors.ErrorCount);
            Assert.IsTrue(files[0].Errors[0].Message.Contains("unresolved identifier", StringComparison.InvariantCultureIgnoreCase));
        }

        [TestMethod]
        public void FileParsing_MissingSemicolon()
        {
            var file = new StringBuilder();
            file.AppendLine("procedure void Proc()");
            file.AppendLine("{");
            file.AppendLine("    int i = 0;");
            file.AppendLine("    while(i < 20)");
            file.AppendLine("    {");
            file.AppendLine("        log(\"Hello\");");
            file.AppendLine("        i++");                 // Missing semicolon here.
            file.AppendLine("    }");
            file.AppendLine("}");

            var files = FileBuilder.ParseFiles((ILogger)null, this.GetType().Assembly,
                new Tuple<string, string>("test.sbs", file.ToString()));
            Assert.AreEqual(1, files.Length);
            Assert.AreEqual(1, files[0].Errors.ErrorCount);
            Assert.IsTrue(files[0].Errors[0].Message.Contains("missing ';'", StringComparison.InvariantCultureIgnoreCase), "Found: " + files[0].Errors[0].Message);
        }

        [TestMethod]
        public void FileParsing_NamespaceBeforeUsing()
        {
            var file = new StringBuilder();
            file.AppendLine("namespace test;");
            file.AppendLine("using StepBro.ToolBarCreator;");

            var files = FileBuilder.ParseFiles((ILogger)null, this.GetType().Assembly,
                new Tuple<string, string>("test.sbs", file.ToString()));
            Assert.AreEqual(1, files.Length);
            Assert.AreEqual(1, files[0].Errors.ErrorCount);
            Assert.IsTrue(files[0].Errors[0].Message.Contains("Namespace should be declared after the last \"using\" statement.", StringComparison.InvariantCultureIgnoreCase));
        }

        [TestMethod]
        public void FileParsing_NamespaceBeforeMultipleUsing()
        {
            var file = new StringBuilder();
            file.AppendLine("namespace test;");
            file.AppendLine("using StepBro.ToolBarCreator;");
            file.AppendLine("using System;");

            var files = FileBuilder.ParseFiles((ILogger)null, this.GetType().Assembly,
                new Tuple<string, string>("test.sbs", file.ToString()));
            Assert.AreEqual(1, files.Length);
            Assert.AreEqual(1, files[0].Errors.ErrorCount);
            Assert.IsTrue(files[0].Errors[0].Message.Contains("Namespace should be declared after the last \"using\" statement.", StringComparison.InvariantCultureIgnoreCase));
        }

        [TestMethod]
        public void FileParsing_NamespaceBetweenUsings()
        {
            var file = new StringBuilder();
            file.AppendLine("using System;");
            file.AppendLine("namespace test;");
            file.AppendLine("using StepBro.ToolBarCreator;");

            var files = FileBuilder.ParseFiles((ILogger)null, this.GetType().Assembly,
                new Tuple<string, string>("test.sbs", file.ToString()));
            Assert.AreEqual(1, files.Length);
            Assert.AreEqual(1, files[0].Errors.ErrorCount);
            Assert.IsTrue(files[0].Errors[0].Message.Contains("Namespace should be declared after the last \"using\" statement.", StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
