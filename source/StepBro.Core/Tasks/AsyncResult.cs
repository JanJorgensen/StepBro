using StepBro.Core.Data;
using System;
using System.Threading;

namespace StepBro.Core.Tasks
{
    public class AsyncResult<T> : IAsyncResult<T>
    {
        private T m_value = default(T);
        private bool m_completed;
        private bool m_faulted;
        private ErrorID m_error = null;
        private Exception m_exception = null;
        private readonly bool m_synccompleted;
        private readonly AutoResetEvent m_event;

        public AsyncResult()
        {
            m_event = new AutoResetEvent(false);
            m_synccompleted = false;
            m_completed = false;
            m_faulted = false;
        }
        public AsyncResult(T value)
        {
            m_value = value;
            m_event = new AutoResetEvent(true);
            m_synccompleted = true;
            m_completed = true;
            m_faulted = false;
        }

        public T Result { get { return m_value; } }
        public ErrorID Error { get { return m_error; } }
        public Exception Exception { get { return m_exception; } }

        public bool IsCompleted { get { return m_completed; } }

        public WaitHandle AsyncWaitHandle { get { return m_event; } }

        public object AsyncState { get { return null; } }

        public bool CompletedSynchronously { get { return m_synccompleted; } }

        public bool IsFaulted { get { return m_faulted; } }

        public void SetFaulted(ErrorID error = null, Exception exception = null)
        {
            if (m_completed)
            {
                throw new System.InvalidOperationException("Already completed.");
            }
            else
            {
                m_faulted = false;
                m_error = error;
                m_exception = exception;
                m_completed = true;
                m_event.Set();
            }
        }
        public void SetResult(T value)
        {
            if (m_completed)
            {
                throw new System.InvalidOperationException("Already completed.");
            }
            else
            {
                m_value = value;
                m_completed = true;
                m_event.Set();
            }
        }
    }
}
