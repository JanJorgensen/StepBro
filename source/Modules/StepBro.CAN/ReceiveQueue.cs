using StepBro.Core.Api;
using StepBro.Core.Execution;
using System;
using System.Collections.Concurrent;

namespace StepBro.CAN
{
    public class ReceiveQueue : IReceiveEntity
    {
        private readonly string m_name;
        private BlockingCollection<IMessage> m_queue = new BlockingCollection<IMessage>();
        private long m_totalCount = 0L;
        private TimeSpan m_defaultTimeout = TimeSpan.MaxValue;
        private readonly Predicate<IMessage> m_filter = null;

        public ReceiveQueue(string name)
        {
            m_name = name;
        }

        public ReceiveQueue(string name, Predicate<IMessage> filter) : this(name)
        {
            m_filter = filter;
        }

        public string Name { get { return m_name; } }

        public bool TryAddHandover(IMessage message)
        {
            if (m_filter == null || m_filter(message))
            {
                m_totalCount++;
                m_queue.Add(message);
                return true;
            }
            else return false;
        }

        public IMessage GetNext([Implicit]ICallContext context, TimeSpan timeout)
        {
            if (m_queue.TryTake(out IMessage message, timeout)) return message;
            return null;
        }

        public IMessage GetNext([Implicit]ICallContext context)
        {
            return this.GetNext(context, m_defaultTimeout);
        }

        public void Flush(bool clearStats)
        {
            while (m_queue.TryTake(out IMessage msg, 0)) ;
            if (clearStats) m_totalCount = 0L;
        }

        public string GetStatusText()
        {
            int count = m_queue.Count;
            return $"{count} in queue. Received: {m_totalCount}";
        }

        public long TotalCount { get { return m_totalCount; } }
    }
}
