using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core.Parser;
using StepBro.Core.ScriptData;

namespace StepBroCoreTest.Execution
{
    [TestClass]
    public class TestPartners
    {
        [TestMethod]
        public void TestProcedurePartnerDirectCall()
        {
            var f = new StringBuilder();
            f.AppendLine("int ParkProc() { return 1582; }");
            f.AppendLine("function int Anton() :");
            f.AppendLine("    partner Park : ParkProc;");
            f.AppendLine("int Moskus() {");
            f.AppendLine("   int n = 0;");
            f.AppendLine("   n = Anton.Park();");
            f.AppendLine("   return n;");
            f.AppendLine("}");
            var file = FileBuilder.ParseFile(null, f.ToString());
            var procedure = file["Moskus"] as IFileProcedure;

            var taskContext = ExecutionHelper.ExeContext();
            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(1582, (long)result);
        }

        [TestMethod]
        public void TestTestListPartnerDirectCall()
        {
            var f = new StringBuilder();
            f.AppendLine("int ParkProc() { return 752; }");
            f.AppendLine("testlist Anton :");
            f.AppendLine("    partner Park : ParkProc;");
            f.AppendLine("int Moskus() {");
            f.AppendLine("   int n = 0;");
            f.AppendLine("   n = Anton.Park();");
            f.AppendLine("   return n;");
            f.AppendLine("}");
            var file = FileBuilder.ParseFile(null, f.ToString());
            var procedure = file["Moskus"] as IFileProcedure;

            var taskContext = ExecutionHelper.ExeContext();
            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(752, (long)result);
        }

        [TestMethod]
        public void TestProcedurePartnerOverrideCall()
        {
            var f = new StringBuilder();
            f.AppendLine("int ParkProc1() { return 7722; }");
            f.AppendLine("int ParkProc2() { return 6633; }");
            f.AppendLine("function int Anton() :");
            f.AppendLine("    partner Park : ParkProc1;");
            f.AppendLine("function int Benny() : Anton,");
            f.AppendLine("    partner override Park : ParkProc2;");
            f.AppendLine("int Moskus() {");
            f.AppendLine("   int n = 0;");
            f.AppendLine("   n = Benny.Park();");
            f.AppendLine("   return n;");
            f.AppendLine("}");
            var file = FileBuilder.ParseFile(null, f.ToString());
            var procedure = file["Moskus"] as IFileProcedure;

            var taskContext = ExecutionHelper.ExeContext();
            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(6633, (long)result);
        }

        [TestMethod]
        public void TestTestListPartnerOverrideCall()
        {
            var f = new StringBuilder();
            f.AppendLine("int ParkProc1() { return 543; }");
            f.AppendLine("int ParkProc2() { return 432; }");
            f.AppendLine("testlist Anton :");
            f.AppendLine("    partner Park : ParkProc1;");
            f.AppendLine("testlist Benny : Anton,");
            f.AppendLine("    partner override Park : ParkProc2;");
            f.AppendLine("int Moskus() {");
            f.AppendLine("   int n = 0;");
            f.AppendLine("   n = Benny.Park();");
            f.AppendLine("   return n;");
            f.AppendLine("}");
            var file = FileBuilder.ParseFile(null, f.ToString());
            var procedure = file["Moskus"] as IFileProcedure;

            var taskContext = ExecutionHelper.ExeContext();
            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(432, (long)result);
        }

        [TestMethod]
        public void TestProcedureReferencePartnerOverrideCall()
        {
            var f = new StringBuilder();
            f.AppendLine("int ParkProc1() { return 7722; }");
            f.AppendLine("int ParkProc2() { return 6633; }");
            f.AppendLine("function int Anton() :");
            f.AppendLine("    partner Park : ParkProc1;");
            f.AppendLine("function int Benny() : Anton,");
            f.AppendLine("    partner override Park : ParkProc2;");
            f.AppendLine("int Moskus() {");
            f.AppendLine("   Anton proc = Benny;");
            f.AppendLine("   int n = 0;");
            f.AppendLine("   n = proc.Park();");
            f.AppendLine("   return n;");
            f.AppendLine("}");
            var file = FileBuilder.ParseFile(null, f.ToString());
            var procedure = file["Moskus"] as IFileProcedure;

            var taskContext = ExecutionHelper.ExeContext();
            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(6633, (long)result);
        }

        [TestMethod]
        public void TestTestListReferencePartnerOverrideCall()
        {
            var f = new StringBuilder();
            f.AppendLine("int ParkProc1() { return 9992; }");
            f.AppendLine("int ParkProc2() { return 2229; }");
            f.AppendLine("testlist Anton :");
            f.AppendLine("    partner Park : ParkProc1;");
            f.AppendLine("testlist Benny : Anton,");
            f.AppendLine("    partner override Park : ParkProc2;");
            f.AppendLine("int Moskus() {");
            f.AppendLine("   Anton list = Benny;");
            f.AppendLine("   int n = 0;");
            f.AppendLine("   n = list.Park();");
            f.AppendLine("   return n;");
            f.AppendLine("}");
            var file = FileBuilder.ParseFile(null, f.ToString());
            var procedure = file["Moskus"] as IFileProcedure;

            var taskContext = ExecutionHelper.ExeContext();
            var result = taskContext.CallProcedure(procedure);
            Assert.AreEqual(2229, (long)result);
        }
    }
}
