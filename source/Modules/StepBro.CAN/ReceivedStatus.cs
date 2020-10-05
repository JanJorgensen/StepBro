using System;

namespace StepBro.CAN
{
    public class ReceivedStatus : IReceiveEntity
    {
        private readonly string m_name;
        private IMessage m_last = null;
        private long m_totalCount = 0L;
        private readonly Predicate<IMessage> m_filter = null;

        public ReceivedStatus(string name, Predicate<IMessage> filter)
        {
            m_name = name;
            m_filter = filter;
        }

        public string Name { get { return m_name; } }

        public bool TryAddHandover(IMessage message)
        {
            if (m_filter == null || m_filter(message))
            {
                m_totalCount++;
                m_last = message;
                return true;
            }
            else return false;
        }

        public string GetStatusText()
        {
            var last = m_last;
            if (last != null)
            {
                return last.GetDataAsString();
            }
            else
            {
                return "Not Received";
            }
        }

        public void Flush(bool clearStats)
        {
            m_last = null;
            if (clearStats) m_totalCount = 0L;
        }

        public IMessage LastReceived { get { return m_last; } }

        public long TotalCount { get { return m_totalCount; } }
    }
}
