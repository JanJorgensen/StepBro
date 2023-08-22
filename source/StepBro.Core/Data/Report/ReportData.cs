using StepBro.Core.Api;
using StepBro.Core.Execution;
using System;

namespace StepBro.Core
{
    public abstract class ReportData
    {
        private readonly DateTime m_timestamp;
        private readonly ReportDataType m_type;
        private bool m_isLocked = false;

        protected ReportData(DateTime timestamp, ReportDataType type)
        {
            m_timestamp = timestamp;
            m_type = type;
        }

        public DateTime TimeStamp { get { return m_timestamp; } }
        public ReportDataType Type { get { return m_type; } }

        public void Lock()
        {
            if (m_isLocked) throw new InvalidOperationException("The report group is already locked");
            m_isLocked = true;
        }

        public bool IsLocked { get { return m_isLocked; } }

        public bool IsStillOpen([Implicit] ICallContext context)
        {
            if (m_isLocked)
            {
                context.ReportError("The data report element is locked, and therefore cannot be modified anymore.");
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
