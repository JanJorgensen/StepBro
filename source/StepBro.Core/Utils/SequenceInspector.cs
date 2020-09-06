using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSharp.Utils
{
    public class SequenceInspector<T>
    {
        protected IEnumerable<T> m_source;
        private Converter<T, string> m_descriptor;
        private int m_nextIndex = 0;
        private IEnumerator<T> m_enumerator = null;
        private IValueWithDescription<T> m_last = null;
        private Action<string> m_expectFailureHandler = null;

        public SequenceInspector(IEnumerable<T> source, Converter<T, string> descriptor = null)
        {
            m_source = source;
            m_descriptor = descriptor;
        }

        public void SetExpectFailureAction(Action<string> handler)
        {
            m_expectFailureHandler = handler;
        }

        [StepBro.Core.Attributes.AsProperty]
        public IValueWithDescription<T> Next()
        {
            if (m_enumerator == null)
            {
                m_enumerator = m_source.GetEnumerator();
            }
            if (m_enumerator.MoveNext())
            {
                m_last = new ValueWithDescription<T>(
                    m_enumerator.Current,
                    m_descriptor != null ? m_descriptor(m_enumerator.Current) : null,
                    m_nextIndex++);
                return m_last;
            }
            else
            {
                return null;
            }
        }

        [StepBro.Core.Attributes.AsProperty]
        public T NextValue()
        {
            var v = this.Next();
            if (v != null)
            {
                return v.Value;
            }
            else
            {
                throw new IndexOutOfRangeException("No more entries in the sequence.");
            }
        }

        public IValueWithDescription<T> Last { get { return m_last; } }

        public int NextIndex
        {
            get { return m_nextIndex; }
        }

        public string LastDescription()
        {
            return (m_last != null) ? m_last.Description : null;
        }

        public int Index
        {
            get { return (m_last != null) ? m_last.Index : -1; }
        }

        /// <summary>
        /// Checks if there are more entries left in the sequence.
        /// </summary>
        /// <remarks>This method will 'eat' the next entry if there are more entries left.</remarks>
        /// <returns>Whether there are more entries left.</returns>
        public bool ExpectEnd()
        {
            if (this.Next() == null)
            {
                return true;
            }
            else
            {
                m_expectFailureHandler?.Invoke(String.Format("Expected end of log. Next entry was #{0}.", m_last.Index));
                return false;
            }
        }

        public bool ExpectNext(T value)
        {
            var next = this.Next();
            if (next == null)
            {
                m_expectFailureHandler?.Invoke("Sequence was empty.");
                return false;
            }
            if (Object.Equals(value, next.Value))
            {
                return true;
            }
            else
            {
                m_expectFailureHandler?.Invoke(String.Format("Expected: \"{0}\" but found \"{1}\" at index {2}", value.ToString(), next.Value.ToString(), next.Index));
                return false;
            }
        }

        public void Reset()
        {
            if (m_enumerator != null)
            {
                m_enumerator.Reset();
            }
        }
    }
}
