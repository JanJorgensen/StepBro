﻿using System;
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
            return m_name == name && BlockEntryType == type;
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
    }
}
