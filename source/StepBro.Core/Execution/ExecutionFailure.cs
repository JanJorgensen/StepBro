using System;

namespace StepBro.Core.Execution
{
    public class ExecutionFailure
    {
        private readonly string m_description;

        protected ExecutionFailure(string description)
        {
            m_description = description;
        }

        public string Description { get { return m_description; } }
    }

    public class ExecutionError : ExecutionFailure
    {
        private readonly Exception m_exception;
        protected ExecutionError(string description, Exception exception = null) : base(description)
        {
            m_exception = exception;
        }

        public Exception Exception { get { return m_exception; } }
    }

    public class ExpectFailed : ExecutionFailure
    {
        private readonly string m_expected;
        private readonly string m_value;
        public ExpectFailed(string expected, string value, string description) : base(description)
        {
            m_expected = expected;
            m_value = value;
        }
        public string Expected { get { return m_expected; } }
        public string Value { get { return m_value; } }
    }

    public class ExpectError : ExecutionError
    {
        private readonly string m_expected;
        private readonly string m_value;
        public ExpectError(string expected, string value, string description) : base(description)
        {
            m_expected = expected;
            m_value = value;
        }
        public string Expected { get { return m_expected; } }
        public string Value { get { return m_value; } }
    }
}
