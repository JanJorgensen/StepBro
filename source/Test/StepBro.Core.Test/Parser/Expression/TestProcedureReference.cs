using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core;
using StepBro.Core.Data;
using StepBro.Core.Parser;
using StepBro.Core.Api;
using static StepBroCoreTest.Parser.ExpressionParser;
using StepBro.Core.ScriptData;
using StepBro.Core.Logging;

namespace StepBroCoreTest.Parser
{
    [TestClass]
    public class TestProcedureReference
    {
        [TestMethod]
        public void TestProcedureReferenceGoodCasting()
        {
            var f = new StringBuilder();
            f.AppendLine("namespace MyFile;");
            f.AppendLine("int ProcA(int i) { return i * 10; }");
            f.AppendLine("int ProcB() { ");
            f.AppendLine("    int i = 0;");
            f.AppendLine("    procedure ref1 = ProcA;");
            f.AppendLine("    ProcA ref2 = ref1 as ProcA;");
            f.AppendLine("    return ref2(4);");
            f.AppendLine("}");
            var file = FileBuilder.ParseFiles((ILogger)null, new Tuple<string, string>("myfile.tss", f.ToString()))[0];
            Assert.AreEqual(2, file.ListElements().Where(e => e.ElementType == FileElementType.ProcedureDeclaration).Count());
            var procedure = file.ListElements().First(p => p.Name == "ProcB") as IFileProcedure;
            Assert.AreEqual("ProcB", procedure.Name);

            var taskContext = ExecutionHelper.ExeContext();

            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(40, (long)result);
        }

        [TestMethod]
        public void TestProcedureReferenceBadCasting()
        {
            var f = new StringBuilder();
            f.AppendLine("namespace MyFile;");
            f.AppendLine("int ProcA(int i) { return i * 10; }");
            f.AppendLine("int ProcB() { ");
            f.AppendLine("    int i = 0;");
            f.AppendLine("    procedure ref1 = ProcA;");
            f.AppendLine("    ProcA ref2 = ref1;");     // No implicit casting - should result in parsing error.
            f.AppendLine("    return ref2(4);");
            f.AppendLine("}");
            try
            {
                FileBuilder.ParseFile(null, f.ToString());

                Assert.Fail("No exception thrown.");
            }
            catch (ParsingErrorException ex)
            {
                Assert.AreEqual("Variable assignment of incompatible type.", ex.Message);
            }
        }
    }
}