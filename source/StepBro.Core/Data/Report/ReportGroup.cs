using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data.Report
{
    public class ReportGroup
    {
        private string m_name;
        private string m_description;
        private readonly DateTime m_starttime;
        private readonly List<ReportData> m_data = new List<ReportData>();

        public ReportGroup(string name, string description)
        {
            m_name = name;
            m_description = description;
            m_starttime = DateTime.Now;
        }

        public string Name { get { return m_name; } }
        public string Description { get { return m_description; } }
        public DateTime StartTime { get { return m_starttime; } }

        public void AddData(ReportData data)
        {
            m_data.Add(data);
        }

        public IEnumerable<ReportData> ListData()
        {
            foreach (var d in m_data) yield return d;
        }

    }
}
