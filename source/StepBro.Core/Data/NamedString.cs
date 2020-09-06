using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    public struct NamedString
    {
        public readonly string Name;
        public readonly string Value;
        public NamedString(string name, string value) { this.Name = name; this.Value = value; }
    }
}
