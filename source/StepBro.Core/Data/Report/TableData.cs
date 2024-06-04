using System;

namespace StepBro.Core.Data.Report
{
    public class TableData : ReportData
    {
        public TableData() : base(DateTime.UtcNow, ReportDataType.DataTable)
        { }

        public void Headers(params string[] headers)
        {

        }

        public void AddRow(params object[] cells)
        {

        }
    }
}
