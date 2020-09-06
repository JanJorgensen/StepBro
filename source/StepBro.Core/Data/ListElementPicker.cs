using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    public class ListElementPicker<T>
    {
        private IList<T> m_list;
        private List<bool> m_pickedList;
        private int m_current = 0;
        private int m_currentUnpickedIndex = 0;
        private int m_pickedCount = 0;

        public ListElementPicker(IList<T> list)
        {
            m_list = list;
            m_pickedList = new List<bool>(new bool[list.Count]);
        }

        //private class Enumerator : IEnumerator<T>, IEnumerator
        //{
        //    int m_index = 0;
        //    ListElementPicker<T> m_parent;

        //    public Enumerator(ListElementPicker<T> parent)
        //    {
        //        m_parent = parent;
        //    }

        //    public T Current
        //    {
        //        get
        //        {
        //            return m_parent.m_list[m_index];
        //        }
        //    }

        //    object IEnumerator.Current
        //    {
        //        get
        //        {
        //            return m_parent.m_list[m_index];
        //        }
        //    }

        //    public void Dispose()
        //    {
        //        m_parent = null;
        //    }

        //    public bool MoveNext()
        //    {
        //        m_index++;
        //        return m_index < (m_parent.m_list.Count - 1);
        //    }

        //    public void Reset()
        //    {
        //        m_index = 0;
        //    }
        //}

        //public IEnumerator<T> GetEnumerator()
        //{
        //    return new Enumerator(this);
        //}

        //IEnumerator IEnumerable.GetEnumerator()
        //{
        //    return new Enumerator(this);
        //}

        public IEnumerable<T> ListUnpicked()
        {
            for (int i = 0; i < m_list.Count; i++)
            {
                if (m_pickedList[i] == false)
                {
                    m_current = i;
                    yield return m_list[i];
                }
            }
        }

        /// <summary>
        /// The current element.
        /// </summary>
        public T Current { get { return m_list[m_current]; } }
        /// <summary>
        /// The index of the curremt element in the source list.
        /// </summary>
        public int CurrentIndex { get { return m_current; } }
        /// <summary>
        /// The index of the current element in the list of unpicked elements.
        /// </summary>
        public int CurrentIndexOfUnpicked { get { return m_currentUnpickedIndex; } }
        public int PickedCount { get { return m_pickedCount; } }
        public int UnpickedCount { get { return m_list.Count - m_pickedCount; } }

        public bool IsCurrentUnpicked()
        {
            return (m_pickedCount < m_list.Count) &&
                (m_current >= 0) &&
                (m_current < m_list.Count) &&
                (m_pickedList[m_current] == false);
        }

        public bool SkipToNextUnpicked()
        {
            for (int i = m_current + 1; i < m_list.Count; i++)
            {
                if (m_pickedList[i] == false)
                {
                    m_current = i;
                    m_currentUnpickedIndex++;
                    return true;
                }
            }
            // Didn't fint another; no pickable selected now. Use SelectFirstUnpicked() to start from the beginning again.
            m_current = -1;
            m_currentUnpickedIndex = -1;
            return false;
        }

        public T Pick()
        {
            if (m_current < 0 || m_current >= m_list.Count) throw new IndexOutOfRangeException();

            T val = m_list[m_current];
            m_pickedList[m_current] = true;
            m_pickedCount++;
            m_currentUnpickedIndex--;
            SkipToNextUnpicked();
            return val;
        }

        public void SelectFirst()
        {
            m_current = 0;
            m_currentUnpickedIndex = m_pickedList[0] ? -1 : 0;
        }

        public bool SelectFirstUnpicked()
        {
            m_current = 0;
            m_currentUnpickedIndex = -1;
            foreach (var p in m_pickedList)
            {
                if (m_pickedList[m_current] == false)
                {
                    m_currentUnpickedIndex = 0;
                    return true;
                }
                m_current++;
            }
            m_current = -1;
            return false;
        }

        public int IndexToUnpickedIndex(int index)
        {
            int ui = -1;
            for (int i = 0; i < m_list.Count; i++)
            {
                if (!m_pickedList[i]) ui++;
                if (i == index)
                {
                    return m_pickedList[i] ? -1 : ui;
                }
            }
            return -1;
        }

        public bool AllBeforeCurrentArePickedAndOthersUnpicked()
        {
            for (int i = 0; i < m_pickedList.Count; i++)
            {
                if (m_pickedList[i] != i < m_current) return false;
            }
            return true;
        }

        public bool FindUnpicked(Predicate<T> matcher)
        {
            for (int i = 0; i < m_list.Count; i++)
            {
                if (m_pickedList[i] == false)
                {
                    if (matcher(m_list[i]))
                    {
                        m_currentUnpickedIndex = this.IndexToUnpickedIndex(i);
                        m_current = i;
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
