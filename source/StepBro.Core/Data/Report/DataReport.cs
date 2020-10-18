using StepBro.Core.Api;
using StepBro.Core.Data.Report;
using System.Collections.Generic;

namespace StepBro.Core
{
    [Public]
    public class DataReport
    {
        private readonly string m_id;
        private readonly List<ReportData> m_data = new List<ReportData>();

        public DataReport(string id)
        {
            m_id = id;
        }

        public string ID { get { return m_id; } }

        public IEnumerable<ReportData> ListData()
        {
            foreach (var d in m_data) yield return d;
        }

        public void AddData(ReportData data)
        {
            m_data.Add(data);
        }
    }
}
