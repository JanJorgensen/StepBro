using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    public sealed class TestResult
    {
        private Verdict m_verdict;
        private ErrorID m_errorID;
        private int m_stepIndex;

        public Verdict Verdict { get { return m_verdict; } }
        public ErrorID ErrorID { get { return m_errorID; } }
        public int StepIndex { get { return m_stepIndex; } }

        public TestResult(Verdict verdict, ErrorID error, int stepIndex)
        {
            m_verdict = verdict;
            m_errorID = error;
            m_stepIndex = stepIndex;
        }
    }
}
