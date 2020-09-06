using System;
using System.Threading;

namespace StepBro.Core.Data
{
    public class ArrayFifo<T> : IReadBuffer<T>
    {
        private readonly object m_lock = new object();
        private AutoResetEvent m_newDataEvent;
        private bool m_waitingForData = false;
        private T[] m_buffer;
        private int m_size;
        private int m_head, m_tail;

        public ArrayFifo(int size = 16 * 1024)
        {
            m_newDataEvent = new AutoResetEvent(false);
            this.Setup(size);
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
                this.MakeSpace(length);
                Array.Copy(m_buffer, start, m_buffer, m_head, length);
                m_head += length;
                if (m_waitingForData)
                {
                    m_newDataEvent.Set();
                    m_waitingForData = false;
                }
            }
        }

        public void Add(Func<T[], int, int, int> getter, int length)
        {
            lock (m_lock)
            {
                this.MakeSpace(length);
                m_head += getter(m_buffer, m_head, length);
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
                if (this.Count > knownCount)
                {
                    return true;
                }
                m_waitingForData = true;
            }
            return m_newDataEvent.WaitOne(timeout);
        }

        public bool AwaitNewData(int knownCount, TimeSpan timeout)
        {
            lock (m_lock)
            {
                if (this.Count > knownCount) return true;
                m_waitingForData = true;
            }
            return m_newDataEvent.WaitOne(timeout);
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
                    if (m_tail + index >= m_head) throw new ArgumentOutOfRangeException("index");
                    return m_buffer[m_tail + index];
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
                if (length > (m_head - m_tail) || length <= 0) throw new ArgumentOutOfRangeException("length");
                m_tail += length;
                if (m_tail == m_head)
                {
                    m_tail = 0;
                    m_head = 0;
                }
                this.OnDataPulled();
            }
        }

        public T[] Get(int index, int length, int total)
        {
            T[] block = new T[length];
            this.Get(index, length, total, block, 0);
            return block;
        }

        public T[] Get(int length)
        {
            return this.Get(length, 0, length);
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
                this.OnDataPulled();
            }
        }
    }
}
