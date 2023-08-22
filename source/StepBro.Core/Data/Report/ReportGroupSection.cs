using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data.Report
{
    public class ReportGroupSection : ReportData
    {
        private readonly string m_header;
        private readonly string m_subheader;

        public ReportGroupSection(DateTime timestamp, string header, string subheader) : base(timestamp, ReportDataType.Section)
        {
            m_header = header;
            m_subheader = subheader;
        }

        public string Header { get { return m_header; } }

        public string SubHeader { get { return m_subheader; } }

        public override string ToString()
        {
            return "Section: " + m_header + " - " + m_subheader;
        }
    }
}
