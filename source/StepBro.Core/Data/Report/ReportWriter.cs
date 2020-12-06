using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data.Report
{
    public class ReportWriter : IDisposable
    {
        private DataReport m_report;

        public ReportWriter(DataReport report)
        {
            m_report = report;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
