using System;
using StepBro.Core.Data;

namespace StepBro.Core.Execution
{
    public interface IExecutionResult
    {
        Verdict Verdict { get; }
        TimeSpan ExecutionTime { get; }
        ExecutionFailure Failure { get; }
        object ReturnValue { get; }
    }

    internal class ExecutionResult : IExecutionResult
    {
        private readonly Verdict m_verdict;
        private readonly object m_returnValue;
        private readonly TimeSpan m_executionTime;
        private readonly ExecutionFailure m_failure;

        public ExecutionResult(Verdict verdict, TimeSpan executionTime, ExecutionFailure failure = null)
        {
            m_verdict = verdict;
            m_returnValue = null;
            m_executionTime = executionTime;
            m_failure = failure;
        }

        public ExecutionResult(object value, TimeSpan executionTime)
        {
            m_returnValue = value;
            m_verdict = Verdict.Unset;
            m_executionTime = executionTime;
            m_failure = null;
        }

        public Verdict Verdict { get { return m_verdict; } }

        public TimeSpan ExecutionTime { get { return m_executionTime; } }

        public ExecutionFailure Failure { get { return m_failure; } }

        public object ReturnValue { get { return m_returnValue; } }
    }
}
