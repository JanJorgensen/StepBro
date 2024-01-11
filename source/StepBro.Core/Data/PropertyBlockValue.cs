using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace StepBro.Core.Data
{
    [JsonDerivedType(typeof(Identifier), typeDiscriminator: "id")]
    public class PropertyBlockValue : PropertyBlockEntry
    {
        private object m_value;
        private PropertyBlockValueSolver m_solver;

        public PropertyBlockValue(int line, string name, object value) : this(line, name, value, null)
        {
        }
        public PropertyBlockValue(int line, string name, PropertyBlockValueSolver solver) : this(line, name, null, solver)
        {
        }

        private PropertyBlockValue(int line, string name, object value, PropertyBlockValueSolver solver) : base(line, PropertyBlockEntryType.Value, name)
        {
            m_value = value;
            m_solver = solver;
        }

        public object Value { get { return m_value; } set { m_value = value; } }


        [JsonIgnore]
        public bool IsStringOrIdentifier { get { return m_value is string || m_value is Identifier; } }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("Value: ");
            this.GetTestString(sb);
            return sb.ToString();
        }

        public override void GetTestString(StringBuilder text)
        {
            if (this.IsArrayEntry)
            {
                if (m_solver != null)
                {
                    text.Append("<solver>");
                }
                else if (m_value == null)
                {
                    text.Append("<null>");
                }
                else
                {
                    text.Append(Convert.ToString(m_value, System.Globalization.CultureInfo.InvariantCulture));
                }
            }
            else
            {
                var spectype = String.IsNullOrEmpty(this.SpecifiedTypeName) ? "" : (this.SpecifiedTypeName + " ");
                string assignment = this.IsAdditionAssignment ? "+=" : "=";
                if (m_solver != null)
                {
                    text.AppendFormat("{0}{1}{2}<solver>", spectype, this.Name, assignment);
                }
                else if (m_value == null)
                {
                    text.AppendFormat("{0}{1}{2}<null>", spectype, this.Name, assignment);
                }
                else
                {
                    text.AppendFormat("{0}{1}{2}{3}",
                        spectype,
                        this.Name,
                        assignment,
                        Convert.ToString(m_value, System.Globalization.CultureInfo.InvariantCulture));
                }
            }
        }

        public override PropertyBlockEntry Clone(bool skipUsedOrApproved = false)
        {
            return new PropertyBlockValue(this.Line, null, m_value, m_solver).CloneBase(this);
        }

        public override SerializablePropertyBlockEntry CloneForSerialization()
        {
            string name = this.IsArrayEntry ? null : this.Name;
            string type = this.IsArrayEntry ? null : this.SpecifiedTypeName;
            if (m_value == null)
            {
                return new SerializablePropertyBlockValueNull()
                {
                    Name = name,
                    SpecifiedType = type
                };
            }
            else if (m_value is bool)
            {
                return new SerializablePropertyBlockValueBool()
                {
                    Name = name,
                    SpecifiedType = type,
                    Value = (bool)m_value
                };
            }
            else if (m_value is bool)
            {
                return new SerializablePropertyBlockValueBool()
                {
                    Name = name,
                    SpecifiedType = type,
                    Value = (bool)m_value
                };
            }
            else if (m_value is long)
            {
                return new SerializablePropertyBlockValueInt()
                {
                    Name = name,
                    SpecifiedType = type,
                    Value = (long)m_value
                };
            }
            else if (m_value is string)
            {
                return new SerializablePropertyBlockValueString()
                {
                    Name = name,
                    SpecifiedType = type,
                    Value = (string)m_value
                };
            }
            else if (m_value is Identifier)
            {
                return new SerializablePropertyBlockValueString()
                {
                    Name = name,
                    SpecifiedType = type,
                    Value = ((Identifier)m_value).Name
                };
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }

    public delegate object PropertyBlockValueSolver();
}
