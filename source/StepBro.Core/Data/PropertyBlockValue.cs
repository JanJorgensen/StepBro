using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    public class PropertyBlockValue : PropertyBlockEntry
    {
        private object m_value;
        private PropertyBlockValueSolver m_solver;

        public PropertyBlockValue(int line, string name, object value) : base(line, PropertyBlockEntryType.Value, name)
        {
            m_value = value;
            m_solver = null;
        }
        public PropertyBlockValue(int line, string name, PropertyBlockValueSolver solver) : base(line, PropertyBlockEntryType.Value, name)
        {
            m_value = null;
            m_solver = solver;
        }

        public object Value { get { return m_value; } }

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
                if (m_solver != null)
                {
                    text.AppendFormat("{0}{1}=<solver>", spectype, this.Name);
                }
                else if (m_value == null)
                {
                    text.AppendFormat("{0}{1}=<null>", spectype, this.Name);
                }
                else
                {
                    text.AppendFormat("{0}{1}={2}", 
                        spectype, 
                        this.Name, 
                        Convert.ToString(m_value, System.Globalization.CultureInfo.InvariantCulture));
                }
            }
        }
    }

    public delegate object PropertyBlockValueSolver();
}
