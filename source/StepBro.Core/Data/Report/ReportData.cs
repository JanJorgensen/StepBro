using System;

namespace StepBro.Core.Data.Report
{
    public abstract class ReportData
    {
        private readonly DateTime m_timestamp;
        private readonly ReportDataType m_type;

        protected ReportData(DateTime timestamp, ReportDataType type)
        {
            m_timestamp = timestamp;
            m_type = type;
        }

        public DateTime TimeStamp { get { return m_timestamp; } }
        public ReportDataType Type { get { return m_type; } }
    }
}
