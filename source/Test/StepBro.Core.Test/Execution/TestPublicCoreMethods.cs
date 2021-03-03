﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core.Parser;
using StepBro.Core.ScriptData;

namespace StepBroCoreTest
{
    [TestClass]
    public class TestPublicCoreMethods
    {
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
    }
}