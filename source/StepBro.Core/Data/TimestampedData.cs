using StepBro.Core.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    [Public]
    public class TimestampedData<T>
    {
        private DateTime m_time;
        private T m_data;

        public TimestampedData(DateTime time, T data)
        {
            m_time = time;
            m_data = data;
        }

        public DateTime TimeStamp { get { return m_time; } }

        public T Data { get { return m_data; } }
    }

    [Public]
    public class TimestampedString : TimestampedData<string>
    {
        public TimestampedString(DateTime time, string value) : base(time, value) { }
    }
}
