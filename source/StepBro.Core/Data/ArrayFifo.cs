using StepBro.Core.Logging;
using System;
using System.Linq;
using System.Threading;

namespace StepBro.Core.Data
{
    public class ArrayFifo<T> : IReadBuffer<T>
    {
        private class MyDebugLogEntry : Logging.DebugLogEntry
        {
            public enum Action { Add, Get, Index, IndexError, Await, AwaitImmediate, AwaitEvent, AwaitTimeout, Eat }
            private readonly string m_instance;
            private readonly Action m_action;
            private readonly int m_head, m_tail, m_count;
            private readonly string m_data;
            public MyDebugLogEntry(string instance, Action action, int head, int tail, int count, string data = null) : base()
            {
                m_instance = instance;  
                m_action = action;
                m_head = head;
                m_tail = tail;
                m_count = count;
                m_data = data;
            }
            public override string ToString()
            {
                if (m_data == null)
                {
                    return $"{m_instance} {m_action}: {m_head}, {m_tail}, {m_count}";
                }
                else
                {
                    return $"{m_instance} {m_action}: {m_head}, {m_tail}, {m_count}, '{m_data}'";
                }
            }
        }

        private static int m_instanceIndex = 0;
        private string m_instanceName;
        private readonly object m_lock = new object();
        private AutoResetEvent m_newDataEvent;
        private bool m_waitingForData = false;
        private T[] m_buffer;
        private int m_size;
        private int m_head, m_tail;

        public ArrayFifo(int size = 16 * 1024)
        {
            m_newDataEvent = new AutoResetEvent(false);
            m_instanceName = "ArrayFifo" + (++m_instanceIndex);
            Setup(size);
        }

        public void Setup(int size)
        {
            lock (m_lock)
            {
                m_buffer = new T[size];
                m_size = size;
                m_head = 0;
                m_tail = 0;
            }
        }

        public int Count { get { lock (m_lock) { return m_head - m_tail; } } }

        public void Add(T[] block, int start, int length)
        {
            lock (m_lock)
            {
                MakeSpace(length);
                Array.Copy(block, start, m_buffer, m_head, length);
                DebugLogEntry.Register(new MyDebugLogEntry(m_instanceName, MyDebugLogEntry.Action.Add, m_head, m_tail, length, DataToString(block, start, length)));
                m_head += length;
                if (m_waitingForData)
                {
                    m_newDataEvent.Set();
                    m_waitingForData = false;
                }
            }
        }

        public virtual string DataToString(T[] block, int start, int length)
        {
            return String.Join(", ", block.Skip(start).Take(length).Select(e => e.ToString()));
        }

        public virtual string ValueString(T value)
        {
            return value.ToString();
        }

        public void Add(Func<T[], int, int, int> getter, int length)
        {
            lock (m_lock)
            {
                MakeSpace(length);
                var count = getter(m_buffer, m_head, length);
                DebugLogEntry.Register(new MyDebugLogEntry(m_instanceName, MyDebugLogEntry.Action.Add, m_head, m_tail, count, DataToString(m_buffer, m_head, length)));
                m_head += count;
                if (m_waitingForData)
                {
                    m_newDataEvent.Set();
                    m_waitingForData = false;
                }
            }
        }

        public bool AwaitNewData(int knownCount, int timeout = 0)
        {
            lock (m_lock)
            {
                if (Count > knownCount)
                {
                    DebugLogEntry.Register(new MyDebugLogEntry(m_instanceName, MyDebugLogEntry.Action.AwaitImmediate, m_head, m_tail, -1));
                    return true;
                }
                m_waitingForData = true;
            }
            DebugLogEntry.Register(new MyDebugLogEntry(m_instanceName, MyDebugLogEntry.Action.Await, m_head, m_tail, -1, timeout.ToString()));
            if (m_newDataEvent.WaitOne(timeout))
            {
                DebugLogEntry.Register(new MyDebugLogEntry(m_instanceName, MyDebugLogEntry.Action.AwaitEvent, m_head, m_tail, -1));
                return true;
            }
            else
            {
                DebugLogEntry.Register(new MyDebugLogEntry(m_instanceName, MyDebugLogEntry.Action.AwaitTimeout, m_head, m_tail, -1));
                return false;
            }
        }

        public bool AwaitNewData(int knownCount, TimeSpan timeout)
        {
            lock (m_lock)
            {
                if (Count > knownCount)
                {
                    DebugLogEntry.Register(new MyDebugLogEntry(m_instanceName, MyDebugLogEntry.Action.AwaitImmediate, m_head, m_tail, -1));
                    return true;
                }
                m_waitingForData = true;
            }
            DebugLogEntry.Register(new MyDebugLogEntry(m_instanceName, MyDebugLogEntry.Action.Await, m_head, m_tail, -1, timeout.ToString()));
            if (m_newDataEvent.WaitOne(timeout))
            {
                DebugLogEntry.Register(new MyDebugLogEntry(m_instanceName, MyDebugLogEntry.Action.AwaitEvent, m_head, m_tail, -1));
                return true;
            }
            else
            {
                DebugLogEntry.Register(new MyDebugLogEntry(m_instanceName, MyDebugLogEntry.Action.AwaitTimeout, m_head, m_tail, -1));
                return false;
            }
        }

        private void MakeSpace(int size)
        {
            if (size > (m_size - m_head) && m_tail > 0)
            {
                while (((m_head - m_tail) + size) * 2 >= m_size)
                {
                    m_size *= 2;
                }
                var newBuffer = new T[m_size];
                Array.Copy(m_buffer, m_tail, newBuffer, 0, (m_head - m_tail));
                m_buffer = newBuffer;
                m_head = m_head - m_tail;
                m_tail = 0;
            }
        }

        public T this[int index]
        {
            get
            {
                lock (m_lock)
                {
                    try
                    {
                        if (m_tail + index >= m_head)
                        {
                            throw new ArgumentOutOfRangeException("index");
                        }
                        var value = m_buffer[m_tail + index];
                        DebugLogEntry.Register(new MyDebugLogEntry(m_instanceName, MyDebugLogEntry.Action.Index, m_head, m_tail, index, ValueString(value)));
                        return value;
                    }
                    catch (Exception)
                    {
                        DebugLogEntry.Register(new MyDebugLogEntry(m_instanceName, MyDebugLogEntry.Action.IndexError, m_head, m_tail, index));
                        DebugLogUtils.DumpToFile();
                        throw;
                    }
                }
            }
        }

        protected virtual void OnDataPulled()
        {
        }

        public void Eat(int length)
        {
            lock (m_lock)
            {
                if (length > (m_head - m_tail)) throw new ArgumentOutOfRangeException("length");
                else if (length < 0)
                {
                    m_tail = 0;
                    m_head = 0;
                }
                else
                {
                    m_tail += length;
                    if (m_tail == m_head)
                    {
                        m_tail = 0;
                        m_head = 0;
                    }
                }
                DebugLogEntry.Register(new MyDebugLogEntry(m_instanceName, MyDebugLogEntry.Action.Eat, m_head, m_tail, length));
                OnDataPulled();
            }
        }

        public void Get(int index, int length, int total, T[] targetbuffer, int targetindex)
        {
            lock (m_lock)
            {
                if (index > (m_head - m_tail) || index < 0) throw new ArgumentOutOfRangeException("index");
                if (length > ((m_head - m_tail) + index) || length < 0) throw new ArgumentOutOfRangeException("length");
                if (total > (m_head - m_tail) || total < (length + index) || total <= 0) throw new ArgumentOutOfRangeException("total");
                Array.Copy(m_buffer, m_tail + index, targetbuffer, targetindex, length);
                m_tail += total;
                if (m_tail == m_head)
                {
                    m_tail = 0;
                    m_head = 0;
                }
                DebugLogEntry.Register(new MyDebugLogEntry(m_instanceName, MyDebugLogEntry.Action.Get, m_head, m_tail, length));
                OnDataPulled();
            }
        }

        public T[] Get(int index, int length, int total)
        {
            T[] block = new T[length];
            Get(index, length, total, block, 0);
            return block;
        }

        public T[] Get(int length)
        {
            return Get(length, 0, length);
        }
    }
}
