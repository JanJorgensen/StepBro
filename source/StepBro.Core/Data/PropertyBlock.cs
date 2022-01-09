using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    public class PropertyBlock : PropertyBlockEntry, IList<PropertyBlockEntry>
    {
        List<PropertyBlockEntry> m_children = new List<PropertyBlockEntry>();

        public PropertyBlock(int line, string name = null) : base(line, PropertyBlockEntryType.Block, name)
        {
        }

        public PropertyBlock(int line, string name, IEnumerable<PropertyBlockEntry> children) : base(line, PropertyBlockEntryType.Block, name)
        {
            if (children != null)
            {
                m_children.AddRange(children);
            }
        }

        public PropertyBlock(int line, IEnumerable<PropertyBlockEntry> children) : base(line, PropertyBlockEntryType.Block)
        {
            m_children.AddRange(children);
        }

        public void AddRange(IEnumerable<PropertyBlockEntry> children)
        {
            m_children.AddRange(children);
        }

        public override void GetTestString(StringBuilder text)
        {
            if (IsArrayEntry || String.IsNullOrEmpty(this.Name))
            {
                text.Append("{ ");
            }
            else
            {
                if (String.IsNullOrEmpty(this.SpecifiedTypeName))
                {
                    text.AppendFormat("{0} = {{ ", this.Name);
                }
                else
                {
                    text.AppendFormat("{0} {1} = {{ ", this.SpecifiedTypeName, this.Name);
                }
            }

            bool first = true;
            foreach (var child in m_children)
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
            text.Append(" }");
        }

        #region IList Interface

        public PropertyBlockEntry this[int index]
        {
            get
            {
                return ((IList<PropertyBlockEntry>)m_children)[index];
            }

            set
            {
                ((IList<PropertyBlockEntry>)m_children)[index] = value;
            }
        }

        public int Count
        {
            get
            {
                return ((IList<PropertyBlockEntry>)m_children).Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return ((IList<PropertyBlockEntry>)m_children).IsReadOnly;
            }
        }

        public void Add(PropertyBlockEntry item)
        {
            ((IList<PropertyBlockEntry>)m_children).Add(item);
        }

        public void Clear()
        {
            ((IList<PropertyBlockEntry>)m_children).Clear();
        }

        public bool Contains(PropertyBlockEntry item)
        {
            return ((IList<PropertyBlockEntry>)m_children).Contains(item);
        }

        public void CopyTo(PropertyBlockEntry[] array, int arrayIndex)
        {
            ((IList<PropertyBlockEntry>)m_children).CopyTo(array, arrayIndex);
        }

        public IEnumerator<PropertyBlockEntry> GetEnumerator()
        {
            return ((IList<PropertyBlockEntry>)m_children).GetEnumerator();
        }

        public int IndexOf(PropertyBlockEntry item)
        {
            return ((IList<PropertyBlockEntry>)m_children).IndexOf(item);
        }

        public void Insert(int index, PropertyBlockEntry item)
        {
            ((IList<PropertyBlockEntry>)m_children).Insert(index, item);
        }

        public bool Remove(PropertyBlockEntry item)
        {
            return ((IList<PropertyBlockEntry>)m_children).Remove(item);
        }

        public void RemoveAt(int index)
        {
            ((IList<PropertyBlockEntry>)m_children).RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList<PropertyBlockEntry>)m_children).GetEnumerator();
        }

        #endregion
    }
}
