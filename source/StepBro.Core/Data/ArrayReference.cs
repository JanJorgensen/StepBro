using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    public class ArrayReference<T> : IEnumerable<T>
    {
        private ArrayReference<T> m_parentArray;
        private T[] m_reference;
        private int m_start;
        private int m_length;

        public ArrayReference(T[] reference, int start = 0, int length = -1)
        {
            m_parentArray = null;
            m_reference = reference;
            if (start > (reference.Length - 1) || start < 0) throw new ArgumentOutOfRangeException("start");
            m_start = start;
            if (length < 0)
            {
                m_length = m_reference.Length - start;
            }
            else
            {
                if (length > (reference.Length - start)) throw new ArgumentOutOfRangeException("length");
                m_length = length;
            }
        }

        private ArrayReference(ArrayReference<T> parent, int start, int length)
        {
            m_reference = parent.m_reference;
            m_parentArray = parent;
            m_start = start + m_parentArray.m_start;
            m_length = length;
        }

        //private T[] TheArray { get { return (m_reference != null) ? m_reference : m_parentArray.m_reference; } }

        public T this[int index] { get { return m_reference[m_start + index]; } set { m_reference[m_start + index] = value; } }

        public ArrayReference<T> GetSubArray(int index = 0, int length = -1)
        {
            int l = (length == -1) ? (m_length - index) : length;
            if (index < 0 || index >= m_start+m_length) throw new ArgumentOutOfRangeException("start");
            if (l < 0 || l > (m_length-index)) throw new ArgumentOutOfRangeException("length");
            return new ArrayReference<T>(this, index, l);
        }

        #region Enumeration Functionality

        private class Enumerator : IEnumerator<T>
        {
            private int i = 0;
            private T[] m_reference;
            private int m_start;
            private int m_end;

            public Enumerator(T[] reference, int start, int length)
            {
                m_reference = reference;
                m_start = start;
                m_end = start + length;
                i = start;
            }

            public T Current
            {
                get
                {
                    if (i >= m_end) throw new IndexOutOfRangeException();
                    return m_reference[i];
                }
            }

            public void Dispose()
            {
                m_reference = null;
            }

            object System.Collections.IEnumerator.Current
            {
                get
                {
                    if (i >= m_end) throw new IndexOutOfRangeException();
                    return m_reference[i];
                }
            }

            public bool MoveNext()
            {
                i++;
                return i < m_end;
            }

            public void Reset()
            {
                i = m_start;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(m_reference, m_start, m_length);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new Enumerator(m_reference, m_start, m_length);
        }

        #endregion
    }
}
