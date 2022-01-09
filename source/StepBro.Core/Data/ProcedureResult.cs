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
        private List<ProcedureResult> m_subResults;

        public string Reference { get { return m_reference; } }
        public Verdict Verdict { get { return m_verdict; } }
        public int StepIndex { get { return m_stepIndex; } }
        public string Description { get { return m_description; } }
        public ErrorID ErrorID { get { return m_errorID; } }
        public int SubResultCount { get { return m_subResults.Count; } }

        public ProcedureResult(string reference, Verdict verdict, int stepIndex, string description, ErrorID error, List<ProcedureResult> subResults)
        {
            m_reference = reference;
            m_verdict = verdict;
            m_stepIndex = stepIndex;
            m_description = description;
            m_errorID = error;
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
    }
}
