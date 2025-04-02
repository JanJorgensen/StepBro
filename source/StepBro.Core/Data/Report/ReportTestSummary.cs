using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StepBro.Core.Data.Report
{
    public class ReportTestSummary : ReportData
    {
        public class SummaryData
        {
            public string Title { get; set; } = null;
            public ReportGroup Group { get; set; } = null;
            public ProcedureResult Result { get; set; } = null;

            public string GetName()
            {
                if (Group != null) return Group.Name;
                else return Title;
            }


            public override string ToString()
            {
                if (this.Result != null)
                {
                    return this.GetName() + " - " + this.Result.ToString();
                }
                else
                {
                    return this.GetName() + " ...";
                }
            }
        }

        private List<SummaryData> m_procedureResults = new List<SummaryData>();
        private object m_sync = new object();

        public ReportTestSummary(DateTime timestamp) : base(timestamp, ReportDataType.TestSummary)
        {
        }

        public event EventHandler SummaryUpdated;

        public void AddEntryBeforeResult(string name)
        {
            lock (m_sync)
            {
                m_procedureResults.Add(new SummaryData { Title = name });
            }
            this.SummaryUpdated?.Invoke(this, new EventArgs());
        }

        public void AddEntryBeforeResult(ReportGroup reference)
        {
            lock (m_sync)
            {
                m_procedureResults.Add(new SummaryData { Group = reference, Title = reference.Name });
            }
            this.SummaryUpdated?.Invoke(this, new EventArgs());
        }

        public void AddResult(string reference, ProcedureResult result)
        {
            try
            {
                lock (m_sync)
                {
                    if (m_procedureResults.Count > 0)
                    {
                        var last = m_procedureResults[m_procedureResults.Count - 1];
                        if ((Object.ReferenceEquals(last.Title, reference) || String.Equals(reference, last.Title)) && last.Result == null)
                        {
                            m_procedureResults[m_procedureResults.Count - 1].Result = result;   // Override with the result.
                            return;
                        }
                    }
                    m_procedureResults.Add(new SummaryData { Title = reference, Result = result });
                }
            }
            finally
            {
                this.SummaryUpdated?.Invoke(this, new EventArgs());
            }
        }

        public void AddResult(ReportGroup reference, ProcedureResult result)
        {
            try
            {
                lock (m_sync)
                {
                    if (m_procedureResults.Count > 0)
                    {
                        var last = m_procedureResults[m_procedureResults.Count - 1];
                        if (Object.ReferenceEquals(last.Group, reference) && last.Result == null)
                        {
                            m_procedureResults[m_procedureResults.Count - 1].Result = result;   // Override with the result.
                            return;
                        }
                    }
                    m_procedureResults.Add(new SummaryData { Title = reference.Name, Group = reference, Result = result });
                }
            }
            finally
            {
                this.SummaryUpdated?.Invoke(this, new EventArgs());
            }
        }

        public IEnumerable<SummaryData> ListResults()
        {
            lock (m_sync)
            {
                foreach (var r in m_procedureResults)
                {
                    yield return r;
                }
            }
        }

        public int CountSubFails()
        {
            lock (m_sync)
            {
                return m_procedureResults.Count(r => r.Result == null || r.Result.Verdict == Verdict.Fail);
            }
        }
        public int CountSubErrors()
        {
            lock (m_sync)
            {
                return m_procedureResults.Count(r => r.Result == null || r.Result.Verdict == Verdict.Error);
            }
        }

        public override string ToString()
        {
            return this.GetResultDescription();
        }

        public Verdict GetResultVerdict()
        {
            Verdict result = Verdict.Pass;
            lock (m_sync)
            {
                foreach (var r in m_procedureResults)
                {
                    if (r.Result == null)
                    {
                        result = Verdict.Error;
                        break;
                    }
                    else if (r.Result.Verdict > result) result = r.Result.Verdict;
                }
            }
            return result;
        }

        public string GetResultDescription()
        {
            lock (m_sync)
            {
                return $"Tests: {m_procedureResults.Count}, Fails: {this.CountSubFails()}, Errors: {this.CountSubErrors()}";
            }
        }
    }
}
