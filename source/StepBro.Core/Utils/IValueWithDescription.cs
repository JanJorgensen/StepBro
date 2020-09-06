using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Utils
{
    public interface IValueWithDescription<T>
    {
        T Value { get; }
        string Description { get; }
        int Index { get; }
    }
}
