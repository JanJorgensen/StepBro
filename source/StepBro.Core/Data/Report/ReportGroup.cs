using StepBro.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data.Report
{
    public class ReportGroup
    {
        private bool m_isLocked = false;
        private string m_name;
        private string m_description;
        //private readonly DateTime m_starttime;
        private readonly LogEntry m_logStart;
        private LogEntry m_logEnd = null;
        private readonly List<ReportData> m_data = new List<ReportData>();

        public ReportGroup(string name, string description, LogEntry logStart)
        {
            m_name = name;
            m_description = description;
            m_logStart = logStart;
        }

        public string Name { get { return m_name; } }
        public string Description { get { return m_description; } }
        public DateTime StartTime { get { return m_logStart.Timestamp; } }
        public LogEntry LogStart { get { return m_logStart; } }
        public LogEntry LogEnd { get { return m_logEnd; } }

        public void AddData(ReportData data)
        {
            if (m_isLocked) throw new InvalidOperationException("The report group is locked/closed.");
            m_data.Add(data);
        }

        public IEnumerable<ReportData> ListData()
        {
            foreach (var d in m_data) yield return d;
        }

        public void Lock(LogEntry end)
        {
            if (m_isLocked) throw new InvalidOperationException("The report group is already locked");
            m_logEnd = end;
            foreach (var d in m_data)
            {
                d.Lock();
            }
            m_isLocked = true;
        }

        public bool IsLocked { get { return m_isLocked; } }
    }
}
