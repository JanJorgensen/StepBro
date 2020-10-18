using System;

namespace StepBro.Core.Data.Report
{
    public class ExpectResultData : ReportData
    {
        private readonly string m_location;
        private readonly string m_id;
        private readonly string m_expected;
        private readonly string m_actual;
        private readonly Verdict m_verdict;

        public ExpectResultData(string location, string id, string expected, string actual, Verdict verdict) :
            base(DateTime.Now, ReportDataType.ExpectResult)
        {
            m_location = location;
            m_id = id;
            m_expected = expected;
            m_actual = actual;
            m_verdict = verdict;
        }

        public string Location { get { return m_location; } }
        public string ID { get { return m_id; } }
        public string Expected { get { return m_expected; } }
        public string Actual { get { return m_actual; } }
        public Verdict Verdict { get { return m_verdict; } }
    }
}
