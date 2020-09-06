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

namespace StepBroCoreTest.Parser
{
    [TestClass]
    public class TestProcedureReference
    {
        [TestMethod]
        public void TestProcedureReferenceCasting()
        {
            var f = new StringBuilder();
            f.AppendLine("namespace MyFile;");
            f.AppendLine("int ProcA(int i) { return i * 10; }");
            f.AppendLine("int ProcB() { int i = 0; procedure ref1 = ProcA; ProcA ref2 = ref1; return ref2(4); }");
            var file = FileBuilder.ParseFile(null, f.ToString());
            Assert.AreEqual(2, file.ListElements().Where(e => e.ElementType == FileElementType.ProcedureDeclaration).Count());
            var procedure = file.ListElements().First(p => p.Name == "ProcB") as IFileProcedure;
            Assert.AreEqual("ProcB", procedure.Name);

            var taskContext = ExecutionHelper.ExeContext();

            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(40, (long)result);
        }
    }
}