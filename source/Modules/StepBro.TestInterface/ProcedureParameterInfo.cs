using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.TestInterface
{
    public sealed class ProcedureParameterInfo
    {
        public ProcedureParameterInfo(string name, string description, Type type, bool hasDefaultValue, bool isParamsArray)
        {
            this.Name = name;
            this.Description = description;
            this.Type = type;
            this.HasDefaultValue = hasDefaultValue;
            this.IsParamsArray = isParamsArray;
        }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public Type Type { get; private set; }
        public bool HasDefaultValue { get; private set; }
        public bool IsParamsArray { get; private set; }
    }
}
