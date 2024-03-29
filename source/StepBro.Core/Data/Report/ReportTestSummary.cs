﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data.Report
{
    public class ReportTestSummary : ReportData
    {
        private List<Tuple<string, ProcedureResult>> m_procedureResults = new List<Tuple<string, ProcedureResult>>();
        public ReportTestSummary(DateTime timestamp) : base(timestamp, ReportDataType.TestSummary)
        {
        }

        public void AddEntryBeforeResult(string reference)
        {
            m_procedureResults.Add(new Tuple<string, ProcedureResult>(reference, null));
        }

        public void AddResult(string reference, ProcedureResult result)
        {
            if (m_procedureResults.Count > 0)
            {
                var last = m_procedureResults[m_procedureResults.Count - 1];
                if (String.Equals(last.Item1, reference) && last.Item2 == null)
                {
                    m_procedureResults[m_procedureResults.Count - 1] = new Tuple<string, ProcedureResult>(reference, result);   // Override with the result.
                    return;
                }
            }
            m_procedureResults.Add(new Tuple<string, ProcedureResult>(reference, result));
        }

        public IEnumerable<Tuple<string, ProcedureResult>> ListResults()
        {
            foreach (var r in m_procedureResults)
            {
                yield return r;
            }
        }

        public int CountSubFails()
        {
            return m_procedureResults.Count(r => r.Item2 == null || r.Item2.Verdict == Verdict.Fail);
        }
        public int CountSubErrors()
        {
            return m_procedureResults.Count(r => r.Item2 == null || r.Item2.Verdict == Verdict.Error);
        }

        public override string ToString()
        {
            return this.GetResultDescription();
        }

        public Verdict GetResultVerdict()
        {
            Verdict result = Verdict.Pass;
            foreach (var r in m_procedureResults)
            {
                if (r.Item2 == null)
                {
                    result = Verdict.Error;
                    break;
                }
                else if (r.Item2.Verdict > result) result = r.Item2.Verdict;
            }
            return result;
        }

        public string GetResultDescription()
        {
            return $"Tests: {m_procedureResults.Count}, Fails: {this.CountSubFails()}, Errors: {this.CountSubErrors()}";
        }
    }
}
