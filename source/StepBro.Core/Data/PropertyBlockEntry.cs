using System;
using System.Text;

namespace StepBro.Core.Data
{
    public abstract class PropertyBlockEntry
    {
        private readonly int m_line;
        private string m_specifiedDataType;
        private string m_name;
        private bool m_isArrayEntry = false;
        private bool? m_isUsedOrApproved;
        private bool m_isAdditionAssignment = false;

        protected PropertyBlockEntry(int line, PropertyBlockEntryType type, string name = null)
        {
            m_line = line;
            Name = name;
            BlockEntryType = type;
        }

        public int Line { get { return m_line; } }

        public string Name
        {
            get
            {
                if (IsArrayEntry) throw new NotSupportedException("Entry is an array element, and therefore has no name.");
                return m_name;
            }
            set
            {
                if (m_name != null) throw new InvalidOperationException("Name property has already been set.");
                if (m_isArrayEntry) throw new InvalidOperationException("Entry is an array element, and therefore has no name.");
                m_name = value;
            }
        }

        public string SpecifiedTypeName
        {
            get
            {
                if (IsArrayEntry) throw new NotSupportedException("Entry is an array element, and therefore has no specified type.");
                return m_specifiedDataType;
            }
            set
            {
                if (m_specifiedDataType != null) throw new InvalidOperationException("SpecifiedTypeName property has already been set.");
                if (m_isArrayEntry) throw new InvalidOperationException("Entry is an array element, and therefore has no specified type.");
                m_specifiedDataType = value;
            }
        }

        public bool IsArrayEntry
        {
            get { return m_isArrayEntry; }
            internal set
            {
                if (value == true && m_name != null) throw new InvalidOperationException("Entry already has a name, and can therefore not be an array element.");
                m_isArrayEntry = value;
            }
        }

        public bool IsAdditionAssignment
        {
            get { return m_isAdditionAssignment; }
        }

        internal void MarkAsAdditionAssignment()
        {
            m_isAdditionAssignment = true;
        }

        public PropertyBlockEntryType BlockEntryType { get; private set; }

        public object Tag { get; set; }

        public abstract void GetTestString(StringBuilder text);

        public string GetTestString()
        {
            var sb = new StringBuilder();
            GetTestString(sb);
            return sb.ToString();
        }

        public bool Is(string name, PropertyBlockEntryType type)
        {
            return String.Equals(m_name, name) && BlockEntryType == type;
        }

        public bool IsUsedOrApproved
        {
            get { return m_isUsedOrApproved.HasValue ? m_isUsedOrApproved.Value : false; }
            set
            {
                if (!value) throw new ArgumentOutOfRangeException();
                m_isUsedOrApproved = true;
            }
        }
        public abstract PropertyBlockEntry Clone();

        internal PropertyBlockEntry CloneBase(PropertyBlockEntry baseElement)
        {
            m_name = baseElement.m_name;
            m_specifiedDataType = baseElement.m_specifiedDataType;
            m_isAdditionAssignment = baseElement.m_isAdditionAssignment;
            return this;
        }
    }
}
