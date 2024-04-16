using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data.SerializationHelp
{
    public class TypedValue
    {
        public TypedValue() { }

        public TypedValue(object value)
        {
            if (value == null) throw new ArgumentNullException("value");
            else if (value is string) { this.Type = "string"; this.Value = value as string; }
            else if (value is long || value is int) { this.Type = "int"; this.Value = value.ToString(); }
            else if (value is bool) { this.Type = "bool"; this.Value = value.ToString(); }
            else if (value is Identifier) { this.Type = "identifier"; this.Value = ((Identifier)value).Name; }
            else
            {
                throw new ArgumentException("value");
            }
        }
        public string Type { get; set; } = null;
        public string Value { get; set; } = null;

        public object GetValue()
        {
            if (this.Type == "int") return Convert.ToInt64(Value);
            if (this.Type == "string") return Value;
            if (this.Type == "bool") return Convert.ToBoolean(Value);
            if (this.Type == "identifier") return new Identifier(Value);
            throw new NotImplementedException();
        }
    }
}
