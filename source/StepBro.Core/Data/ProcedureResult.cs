using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    public sealed class ProcedureResult
    {
        private Verdict m_verdict;
        private string m_description;
        private ErrorID m_errorID;
        private int m_stepIndex;

        public Verdict Verdict { get { return m_verdict; } }
        public int StepIndex { get { return m_stepIndex; } }
        public string Description { get { return m_description; } }
        public ErrorID ErrorID { get { return m_errorID; } }

        public ProcedureResult(Verdict verdict, int stepIndex, string description, ErrorID error = null)
        {
            m_verdict = verdict;
            m_stepIndex = stepIndex;
            m_description = description;
            m_errorID = error;
        }
    }
}
