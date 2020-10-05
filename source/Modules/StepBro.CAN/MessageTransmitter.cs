using StepBro.Core.Data;
using System;

namespace StepBro.CAN
{
    public class MessageTransmitter : ITimedAction
    {
        private readonly string m_name;
        private readonly MessageType m_type;
        private readonly uint m_ID;
        private readonly byte[] m_data;
        private TimeSpan m_periodicTime;
        private DateTime m_nextTransmit;
        private readonly bool m_periodic;
        private bool m_active;

        public MessageTransmitter(
            string name,
            MessageType type, uint ID, byte[] data,
            bool periodic, TimeSpan time,
            DateTime next, bool active)
        {
            m_name = name;
            m_type = type;
            m_ID = ID;
            m_data = data;
            m_periodicTime = time;
            m_nextTransmit = next;
            m_periodic = periodic;
            m_active = active;
        }

        public MessageTransmitter(string name, MessageType type, uint ID, byte[] data, TimeSpan time, bool startNow) :
            this(name, type, ID, data, true, time, startNow ? (DateTime.Now + time) : DateTime.MaxValue, true)
        {

        }

        public MessageTransmitter(string name, MessageType type, uint ID, byte[] data, DateTime time) :
            this(name, type, ID, data, true, TimeSpan.MaxValue, time, true)
        {

        }

        public bool IsActive { get { return m_active; } }

        public bool Periodic { get { return m_periodic; } }

        public TimeSpan PeriodicTime { get { return m_periodicTime; } }

        public DateTime NextActionTime { get { return m_nextTransmit; } }

        public MessageType Type { get { return m_type; } }

        public uint ID { get { return m_ID; } }

        public byte[] Data { get { return m_data; } }

        public bool UpdateWhenHandling(DateTime now)
        {
            if (m_active && m_periodic)
            {
                m_nextTransmit += m_periodicTime;
                return true;
            }
            else
            {
                m_active = false;
                return false;
            }
        }

        public void Reset()
        {
            m_active = false;
        }

        public void Activate()
        {
            if (!m_active && m_periodic)
            {
                m_nextTransmit = DateTime.Now + m_periodicTime;
            }
        }

        public void Deactivate()
        {
            if (m_active)
            {
                m_active = false;
            }
        }
    }
}
