using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    public struct NamedData<T>
    {
        public readonly string Name;
        public readonly T Value;
        public NamedData(string name, T value) { this.Name = name; this.Value = value; }
    }
}
