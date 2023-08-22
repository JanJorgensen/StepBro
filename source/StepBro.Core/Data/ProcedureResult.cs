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

        public string Reference { get { return m_reference; } }
        public Verdict Verdict { get { return m_verdict; } }
        public int StepIndex { get { return m_stepIndex; } }
        public string Description { get { return m_description; } }
        public ErrorID ErrorID { get { return m_errorID; } }

        internal ProcedureResult(string reference, Verdict verdict, int stepIndex, string description, ErrorID error, DateTime start, DateTime end)
        {
            m_reference = reference;
            m_verdict = verdict;
            m_stepIndex = stepIndex;
            m_description = description;
            m_errorID = error;
            m_startTime = start;
            m_endTime = end;
        }

        public ProcedureResult SelectIfWorse(ProcedureResult otherResult)
        {
            if (this.Verdict >= otherResult.Verdict) return this;
            return otherResult;
        }

        public string ToString(bool includeReference)
        {
            List<string> parts = new List<string>();
            if (includeReference && !String.IsNullOrEmpty(m_reference)) parts.Add(m_reference);
            if (m_stepIndex > 0) parts.Add("step " + m_stepIndex.ToString());
            parts.Add(m_verdict.ToString());
            if (!String.IsNullOrEmpty(m_description)) parts.Add("\"" + m_description + "\"");

            return String.Join(", ", parts);
        }

        public override string ToString()
        {
            return this.ToString(true);
        }
    }
}
