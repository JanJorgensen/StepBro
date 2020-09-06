using System;
using System.Threading;
using System.Threading.Tasks;

namespace StepBro.Core.Tasks
{
    public class TaskToAsyncResult<TResult> : IAsyncResult<TResult>
    {
        private readonly Task<TResult> m_task;
        public TaskToAsyncResult(Task<TResult> task)
        {
            m_task = task;
        }

        public static explicit operator TaskToAsyncResult<TResult>(Task<TResult> task) => new TaskToAsyncResult<TResult>(task);

        public TResult Result { get { return m_task.Result; } }

        public bool IsCompleted { get { return m_task.IsCompleted; } }

        public WaitHandle AsyncWaitHandle { get { return ((IAsyncResult)m_task).AsyncWaitHandle; } }

        public object AsyncState { get { return m_task.AsyncState; } }

        public bool CompletedSynchronously { get { return ((IAsyncResult)m_task).CompletedSynchronously; } }

        public bool IsFaulted { get { return m_task.IsFaulted; } }
    }
}
