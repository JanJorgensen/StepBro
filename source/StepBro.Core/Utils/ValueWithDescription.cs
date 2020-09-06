using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSharp.Utils
{
    public sealed class ValueWithDescription<T> : IValueWithDescription<T>
    {
        public ValueWithDescription(T value, string description, int index = -1)
        {
            this.Value = value;
            this.Description = description;
            this.Index = index;
        }

        public T Value { get; private set; }
        public string Description { get; private set; }
        public int Index { get; private set; }
    }
}
