using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core;
using StepBro.Core.Parser;
using StepBro.Core.ScriptData;
using StepBro.Core.Logging;

namespace StepBroCoreTest
{
    [TestClass]
    public class TestPublicCoreMethods
    {
        [TestInitialize]
        public void Setup()
        {
            ServiceManager.Dispose();
        }

        [TestMethod]
        public void TestGetFullPath()
        {

            var f = new StringBuilder();
            f.AppendLine("string MyProcedure(string path) {");
            f.AppendLine("   string r = System.String.Concat(\"\", path.GetFullPath(\"Erik\"));");
            f.AppendLine("   return r;");
            f.AppendLine("}");
            var files = FileBuilder.ParseFiles((ILogger)null, this.GetType().Assembly, new Tuple<string, string>("myfile.sbs", f.ToString()));
            var procedure = files[0].GetFileElement<IFileProcedure>("MyProcedure");
            Assert.AreEqual(0, files[0].Errors.ErrorCount);

            var taskContext = ExecutionHelper.ExeContext();

            if (System.OperatingSystem.IsWindows())
            {
                var result = taskContext.CallProcedure(procedure, "c:\\knud");
                Assert.AreEqual("c:\\knud\\Erik", (string)result);
            }
            else
            {
                var result = taskContext.CallProcedure(procedure, "/home/runner/work/knud");
                Assert.AreEqual("/home/runner/work/knud/Erik", (string)result);
            }
        }

        [TestMethod]
        public void TestLineReaderAwait()
        {
            var f = new StringBuilder();
            f.AppendLine("string MyProcedure() {");
            f.AppendLine("   string[] myArr = [\"Anders\", \"Bent\", \"Christian\", \"Dennis\"];");
            f.AppendLine("   var reader = myArr.ToLineReader();");
            f.AppendLine("   var found = reader.Await(\"Dennis\", 2s);");
            f.AppendLine("   return found;");
            f.AppendLine("}");
            var file = FileBuilder.ParseFile(null, f.ToString());
            var procedure = file.GetFileElement<IFileProcedure>("MyProcedure"); ;

            var taskContext = ExecutionHelper.ExeContext();
            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual("Dennis", (string)result);
        }

        [TestMethod]
        public void TestLineReader()
        {
            var f = new StringBuilder();
            f.AppendLine("string MyProcedure() {");
            f.AppendLine("   string[] myArr = [\"Anders\", \"Bent\", \"Christian\", \"Dennis\"];");
            f.AppendLine("   var reader = myArr.ToLineReader();");
            f.AppendLine("   expect (reader.Current.Text == \"Anders\");");
            f.AppendLine("   expect (reader.HasMore);");
            f.AppendLine("   reader.Next();");
            f.AppendLine("   reader.Next();");
            f.AppendLine("   expect (reader.HasMore);");
            f.AppendLine("   return reader.Current.Text;");
            f.AppendLine("}");
            var file = FileBuilder.ParseFile(null, f.ToString());
            var procedure = file.GetFileElement<IFileProcedure>("MyProcedure"); ;

            var taskContext = ExecutionHelper.ExeContext();
            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual("Christian", (string)result);
        }

        [TestMethod]
        public void TestLineReaderFind()
        {
            var f = new StringBuilder();
            f.AppendLine("string MyProcedure() {");
            f.AppendLine("   string[] myArr = [\"Anders\", \"Bent\", \"Christian\", \"Dennis\"];");
            f.AppendLine("   var reader = myArr.ToLineReader();");
            f.AppendLine("   expect (reader.Current.Text == \"Anders\");");
            f.AppendLine("   expect (reader.HasMore);");
            f.AppendLine("   string findTest = reader.Find(\"Christian\");");
            f.AppendLine("   expect (findTest == \"Christian\");");
            f.AppendLine("   return reader.Current.Text;");
            f.AppendLine("}");
            var file = FileBuilder.ParseFile(null, f.ToString());
            var procedure = file.GetFileElement<IFileProcedure>("MyProcedure"); ;

            var taskContext = ExecutionHelper.ExeContext();
            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual("Christian", (string)result);
        }

        [TestMethod]
        public void TestLineReaderFindNullOnRightSide()
        {
            var f = new StringBuilder();
            f.AppendLine("string MyProcedure() {");
            f.AppendLine("   string[] myArr = [\"Anders\", \"Bent\", \"Christian\", \"Dennis\"];");
            f.AppendLine("   var reader = myArr.ToLineReader();");
            f.AppendLine("   expect (reader.Current.Text == \"Anders\");");
            f.AppendLine("   expect (reader.HasMore);");
            f.AppendLine("   string findTest = reader.Find(\"Jens\", true);");
            f.AppendLine("   expect (findTest == null);");
            f.AppendLine("   return reader.Current.Text;");
            f.AppendLine("}");
            var file = FileBuilder.ParseFile(null, f.ToString());
            var procedure = file.GetFileElement<IFileProcedure>("MyProcedure"); ;

            var taskContext = ExecutionHelper.ExeContext();
            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(null, result);
        }

        [TestMethod]
        public void TestLineReaderFindNullOnLeftSide()
        {
            var f = new StringBuilder();
            f.AppendLine("string MyProcedure() {");
            f.AppendLine("   string[] myArr = [\"Anders\", \"Bent\", \"Christian\", \"Dennis\"];");
            f.AppendLine("   var reader = myArr.ToLineReader();");
            f.AppendLine("   expect (reader.Current.Text == \"Anders\");");
            f.AppendLine("   expect (reader.HasMore);");
            f.AppendLine("   string findTest = reader.Find(\"Jens\", true);");
            f.AppendLine("   expect (null == findTest);");
            f.AppendLine("   return reader.Current.Text;");
            f.AppendLine("}");
            var file = FileBuilder.ParseFile(null, f.ToString());
            var procedure = file.GetFileElement<IFileProcedure>("MyProcedure"); ;

            var taskContext = ExecutionHelper.ExeContext();
            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(null, result);
        }

        [TestMethod]
        public void TestLineReaderFindNullString()
        {
            var f = new StringBuilder();
            f.AppendLine("string MyProcedure() {");
            f.AppendLine("   string[] myArr = [\"Anders\", \"Bent\", \"null\", \"Dennis\"];");
            f.AppendLine("   var reader = myArr.ToLineReader();");
            f.AppendLine("   expect (reader.Current.Text == \"Anders\");");
            f.AppendLine("   expect (reader.HasMore);");
            f.AppendLine("   string findTest = reader.Find(\"null\", true);");
            f.AppendLine("   expect (\"null\" == findTest);");
            f.AppendLine("   return reader.Current.Text;");
            f.AppendLine("}");
            var file = FileBuilder.ParseFile(null, f.ToString());
            var procedure = file.GetFileElement<IFileProcedure>("MyProcedure"); ;

            var taskContext = ExecutionHelper.ExeContext();
            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual("null", result);
        }
    }
}