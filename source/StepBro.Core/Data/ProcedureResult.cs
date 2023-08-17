using StepBro.Core.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    [Public]
    public sealed class ProcedureResult
    {
        private string m_reference;
        private Verdict m_verdict;
        private string m_description;
        private ErrorID m_errorID;
        private int m_stepIndex;
        private DateTime m_startTime;
        private DateTime m_endTime;
        private List<ProcedureResult> m_subResults;

        public string Reference { get { return m_reference; } }
        public Verdict Verdict { get { return m_verdict; } }
        public int StepIndex { get { return m_stepIndex; } }
        public string Description { get { return m_description; } }
        public ErrorID ErrorID { get { return m_errorID; } }
        public int SubResultCount { get { return m_subResults.Count; } }

        internal ProcedureResult(string reference, Verdict verdict, int stepIndex, string description, ErrorID error, DateTime start, DateTime end, List<ProcedureResult> subResults)
        {
            m_reference = reference;
            m_verdict = verdict;
            m_stepIndex = stepIndex;
            m_description = description;
            m_errorID = error;
            m_startTime = start;
            m_endTime = end;
            m_subResults = new List<ProcedureResult>(subResults);
        }

        public IEnumerable<ProcedureResult> ListSubResults()
        {
            foreach (var r in m_subResults) yield return r;
        }

        public int CountSubFails()
        {
            return m_subResults.Count(r => r.Verdict == Verdict.Fail);
        }
        public int CountSubErrors()
        {
            return m_subResults.Count(r => r.Verdict == Verdict.Error);
        }

        public ProcedureResult SelectIfWorse(ProcedureResult otherResult)
        {
            if (this.Verdict >= otherResult.Verdict) return this;
            return otherResult;
        }

        public override string ToString()
        {
            List<string> parts = new List<string>();
            if (!String.IsNullOrEmpty(m_reference)) parts.Add(m_reference);
            if (m_stepIndex > 0) parts.Add("step " + m_stepIndex.ToString());
            parts.Add(m_verdict.ToString());
            if (!String.IsNullOrEmpty(m_description)) parts.Add("\"" + m_description + "\"");
            if (m_subResults != null && m_subResults.Count > 0)
            {
                parts.Add("Subresults: " + m_subResults.Count.ToString());
                if (this.CountSubErrors() > 0) { parts.Add("errors: " + this.CountSubErrors().ToString()); }
                else if (this.CountSubFails() > 0) { parts.Add("fails: " + this.CountSubFails().ToString()); }
            }

            return String.Join(", ", parts);
        }
    }
}
