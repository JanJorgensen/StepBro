using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    public class PropertyBlockArray : PropertyBlockEntry, IList<PropertyBlockEntry>
    {
        List<PropertyBlockEntry> m_entries = new List<PropertyBlockEntry>();

        public PropertyBlockArray(string name = null) : base(PropertyBlockEntryType.Array, name)
        {
        }

        public void AddRange(IEnumerable<PropertyBlockEntry> children)
        {
            m_entries.AddRange(children);
            foreach (var entry in children)
            {
                entry.IsArrayEntry = true;
            }
        }

        public override void GetTestString(StringBuilder text)
        {
            if (String.IsNullOrEmpty(this.Name))
            {
                text.Append("[ ");
            }
            else
            {
                if (String.IsNullOrEmpty(this.SpecifiedTypeName))
                {
                    text.AppendFormat("{0} = [ ", this.Name);
                }
                else
                {
                    text.AppendFormat("{0} {1} = [ ", this.SpecifiedTypeName, this.Name);
                }
            }

            bool first = true;
            foreach (var child in m_entries)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    text.Append(", ");
                }
                child.GetTestString(text);
            }
            text.Append(" ]");
        }

        #region IList Implementation

        public PropertyBlockEntry this[int index]
        {
            get
            {
                return ((IList<PropertyBlockEntry>)m_entries)[index];
            }

            set
            {
                ((IList<PropertyBlockEntry>)m_entries)[index] = value;
                value.IsArrayEntry = true;
            }
        }

        public int Count
        {
            get
            {
                return ((IList<PropertyBlockEntry>)m_entries).Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return ((IList<PropertyBlockEntry>)m_entries).IsReadOnly;
            }
        }

        public void Add(PropertyBlockEntry item)
        {
            ((IList<PropertyBlockEntry>)m_entries).Add(item);
            item.IsArrayEntry = true;
        }

        public void Clear()
        {
            ((IList<PropertyBlockEntry>)m_entries).Clear();
        }

        public bool Contains(PropertyBlockEntry item)
        {
            return ((IList<PropertyBlockEntry>)m_entries).Contains(item);
        }

        public void CopyTo(PropertyBlockEntry[] array, int arrayIndex)
        {
            throw new NotImplementedException();
            //((IList<PropertyBlockEntry>)m_entries).CopyTo(array, arrayIndex);
        }

        public IEnumerator<PropertyBlockEntry> GetEnumerator()
        {
            return ((IList<PropertyBlockEntry>)m_entries).GetEnumerator();
        }

        public int IndexOf(PropertyBlockEntry item)
        {
            return ((IList<PropertyBlockEntry>)m_entries).IndexOf(item);
        }

        public void Insert(int index, PropertyBlockEntry item)
        {
            ((IList<PropertyBlockEntry>)m_entries).Insert(index, item);
            item.IsArrayEntry = true;
        }

        public bool Remove(PropertyBlockEntry item)
        {
            return ((IList<PropertyBlockEntry>)m_entries).Remove(item);
        }

        public void RemoveAt(int index)
        {
            ((IList<PropertyBlockEntry>)m_entries).RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList<PropertyBlockEntry>)m_entries).GetEnumerator();
        }

        #endregion
    }
}
