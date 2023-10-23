using System;
using StepBro.Core.Data;

namespace StepBro.Core.Execution
{
    public interface IExecutionResult
    {
        ProcedureResult ProcedureResult { get; }
        TimeSpan ExecutionTime { get; }
        Exception Exception { get; }
        object ReturnValue { get; }
    }

    internal class ExecutionResult : IExecutionResult
    {
        private readonly ProcedureResult m_result;
        private readonly object m_returnValue;
        private readonly TimeSpan m_executionTime;
        private readonly Exception m_exception;

        public ExecutionResult(ProcedureResult result, TimeSpan executionTime, object value = null, Exception exception = null)
        {
            m_result = result;
            m_returnValue = value;
            m_executionTime = executionTime;
            m_exception = exception;
        }


        public ProcedureResult ProcedureResult { get { return m_result; } }

        public TimeSpan ExecutionTime { get { return m_executionTime; } }

        public object ReturnValue { get { return m_returnValue; } }

        public Exception Exception { get { return m_exception; } }
    }
}
