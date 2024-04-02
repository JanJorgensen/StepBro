using System;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace StepBro.Core.Data
{
    public abstract class PropertyBlockEntry
    {
        private readonly int m_line;
        private string m_specifiedDataType = null;
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

        public bool HasTypeSpecified { get { return !String.IsNullOrEmpty(m_specifiedDataType); } }

        public string TypeOrName { get { return this.HasTypeSpecified ? m_specifiedDataType : m_name; } }

        public bool IsArrayEntry
        {
            get { return m_isArrayEntry; }
            internal set
            {
                if (value == true && m_name != null) throw new InvalidOperationException("Entry already has a name, and can therefore not be an array element.");
                m_isArrayEntry = value;
            }
        }

        public bool IsAdditionAssignment { get; set; } = false;

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
            return string.Equals(m_name, name) && BlockEntryType == type;
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
        public abstract PropertyBlockEntry Clone(bool skipUsedOrApproved = false);

        internal PropertyBlockEntry CloneBase(PropertyBlockEntry baseElement)
        {
            m_name = baseElement.m_name;
            m_specifiedDataType = baseElement.m_specifiedDataType;
            this.IsAdditionAssignment = baseElement.IsAdditionAssignment;
            return this;
        }

        public abstract SerializablePropertyBlockEntry CloneForSerialization();
    }

    #region Serialization

    [JsonDerivedType(typeof(SerializablePropertyBlockFlag), typeDiscriminator: "flag")]
    [JsonDerivedType(typeof(SerializablePropertyBlockValueNull), typeDiscriminator: "null")]
    [JsonDerivedType(typeof(SerializablePropertyBlockValueBool), typeDiscriminator: "bool")]
    [JsonDerivedType(typeof(SerializablePropertyBlockValueString), typeDiscriminator: "string")]
    [JsonDerivedType(typeof(SerializablePropertyBlockValueInt), typeDiscriminator: "int")]
    [JsonDerivedType(typeof(SerializablePropertyBlockValueIdentifier), typeDiscriminator: "identifier")]
    [JsonDerivedType(typeof(SerializablePropertyBlock), typeDiscriminator: "block")]
    [JsonDerivedType(typeof(SerializablePropertyBlockArray), typeDiscriminator: "array")]
    [JsonDerivedType(typeof(SerializablePropertyBlockEvent), typeDiscriminator: "event")]
    public abstract class SerializablePropertyBlockEntry
    {
        [JsonPropertyOrder(0)]
        public string Name { get; set; } = null;
        [JsonPropertyOrder(1)]
        public string SpecifiedType { get; set; } = null;
        public int Line { get; set; } = -1;
        public abstract PropertyBlockEntry CloneAsPropertyBlockEntry();
    }
    public class SerializablePropertyBlockFlag : SerializablePropertyBlockEntry
    {
        public override PropertyBlockEntry CloneAsPropertyBlockEntry()
        {
            var data = new PropertyBlockFlag(this.Line, this.Name);
            data.SpecifiedTypeName = SpecifiedType;
            return data;
        }
    }
    public class SerializablePropertyBlockValueNull : SerializablePropertyBlockEntry
    {
        public override PropertyBlockEntry CloneAsPropertyBlockEntry()
        {
            var data = new PropertyBlockValue(this.Line, this.Name, null);
            data.SpecifiedTypeName = SpecifiedType;
            return data;
        }
    }
    public class SerializablePropertyBlockValueBool : SerializablePropertyBlockEntry
    {
        [JsonPropertyOrder(2)]
        public bool Value { get; set; }

        public override PropertyBlockEntry CloneAsPropertyBlockEntry()
        {
            var data = new PropertyBlockValue(this.Line, this.Name, this.Value);
            data.SpecifiedTypeName = SpecifiedType;
            return data;
        }
    }
    public class SerializablePropertyBlockValueString : SerializablePropertyBlockEntry
    {
        [JsonPropertyOrder(2)]
        public string Value { get; set; }

        public override PropertyBlockEntry CloneAsPropertyBlockEntry()
        {
            var data = new PropertyBlockValue(this.Line, this.Name, this.Value);
            data.SpecifiedTypeName = SpecifiedType;
            return data;
        }
    }
    public class SerializablePropertyBlockValueInt : SerializablePropertyBlockEntry
    {
        [JsonPropertyOrder(2)]
        public long Value { get; set; }

        public override PropertyBlockEntry CloneAsPropertyBlockEntry()
        {
            var data = new PropertyBlockValue(this.Line, this.Name, this.Value);
            data.SpecifiedTypeName = SpecifiedType;
            return data;
        }
    }
    public class SerializablePropertyBlockValueIdentifier : SerializablePropertyBlockEntry
    {
        [JsonPropertyOrder(2)]
        public string Value { get; set; }

        public override PropertyBlockEntry CloneAsPropertyBlockEntry()
        {
            var data = new PropertyBlockValue(this.Line, this.Name, (Identifier)this.Value);
            data.SpecifiedTypeName = SpecifiedType;
            return data;
        }
    }
    public class SerializablePropertyBlock : SerializablePropertyBlockEntry
    {
        [JsonPropertyOrder(2)]
        public SerializablePropertyBlockEntry[] Entries { get; set; }

        public override PropertyBlockEntry CloneAsPropertyBlockEntry()
        {
            var block = new PropertyBlock(this.Line, this.Name);
            block.SpecifiedTypeName = SpecifiedType;
            block.AddRange(this.Entries.Select(e => e.CloneAsPropertyBlockEntry()));
            return block;
        }
    }
    public class SerializablePropertyBlockArray : SerializablePropertyBlockEntry
    {
        [JsonPropertyOrder(2)]
        public SerializablePropertyBlockEntry[] Entries { get; set; }

        public override PropertyBlockEntry CloneAsPropertyBlockEntry()
        {
            var block = new PropertyBlockArray(this.Line, this.Name);
            block.SpecifiedTypeName = SpecifiedType;
            block.AddRange(this.Entries.Select(e => e.CloneAsPropertyBlockEntry()));
            return block;
        }
    }
    public class SerializablePropertyBlockEvent : SerializablePropertyBlockEntry
    {
        [JsonPropertyOrder(2)]
        public Verdict Verdict { get; set; }

        public override PropertyBlockEntry CloneAsPropertyBlockEntry()
        {
            var data = new PropertyBlockEvent(this.Line, this.Name, this.Verdict);
            data.SpecifiedTypeName = SpecifiedType;
            return data;
        }
    }

    #endregion
}
