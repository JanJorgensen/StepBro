using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core;
using StepBro.Core.Parser;
using System.Linq;

namespace StepBroCoreTest.Execution
{
    [TestClass]
    public class TestReportCreation
    {
        [TestMethod]
        public void CreateReportInProcedure()
        {
            var proc = FileBuilder.ParseProcedureExpectNoErrors(
                "DataReport Func(){ var data = StartReport(\"daType\", \"daTitle\"); return data; }");

            Assert.AreEqual(typeof(DataReport), proc.ReturnType.Type);
            Assert.AreEqual(0, proc.Parameters.Length);

            object result = proc.Call();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(DataReport));
            var report = result as DataReport;
            Assert.AreEqual("daType", report.Type);
            Assert.AreEqual("daTitle", report.Title);
            Assert.AreEqual(0, report.ListData().Count());
        }

        [TestMethod]
        public void CreateReportWithData()
        {
            var proc = FileBuilder.ParseProcedureExpectNoErrors(
                "DataReport Func(){ step 1, \"Setup\"; var data = StartReport(\"daType\", \"Test#6\"); expect (5 > 4); return data; }");

            Assert.AreEqual(typeof(DataReport), proc.ReturnType.Type);
            Assert.AreEqual(0, proc.Parameters.Length);

            object result = proc.Call();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(DataReport));
            var report = result as DataReport;
            Assert.AreEqual("daType", report.Type);
            Assert.AreEqual("Test#6", report.Title);
            Assert.AreEqual(1, report.ListData().Count());
            var entry = report.ListData().ElementAt(0);
            Assert.AreEqual(StepBro.Core.Data.Report.ReportDataType.ExpectResult, entry.Type);
        }

        [TestMethod]
        public void CreateReportWithinUsing()
        {
            var proc = FileBuilder.ParseProcedureExpectNoErrors(
                "DataReport Func(){ DataReport result = null; using ( var r = StartReport(\"daType\", \"Test#9\")) { expect (5 > 4); result = r; } return result; }");

            Assert.AreEqual(typeof(DataReport), proc.ReturnType.Type);
            Assert.AreEqual(0, proc.Parameters.Length);

            object result = proc.Call();
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(DataReport));
            var report = result as DataReport;
            Assert.AreEqual("daType", report.Type);
            Assert.AreEqual("Test#9", report.Title);
            Assert.AreEqual(1, report.ListData().Count());
            var entry = report.ListData().ElementAt(0);
            Assert.AreEqual(StepBro.Core.Data.Report.ReportDataType.ExpectResult, entry.Type);
        }
    }
}
