using StepBro.Core.Api;
using StepBro.Core.Execution;
using System;
using System.Collections.Concurrent;

namespace StepBro.CAN
{
    public class ReceiveQueue
    {
        private readonly string m_name;
        private BlockingCollection<IMessage> m_queue = new BlockingCollection<IMessage>();
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

        public bool TryAddToQueue(IMessage message)
        {
            if (m_filter == null || m_filter(message))
            {
                m_queue.Add(message);
                return true;
            }
            else return true;
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

        public void Flush([Implicit]ICallContext context)
        {
            while (m_queue.TryTake(out IMessage msg, 0)) ;
        }
    }
}
