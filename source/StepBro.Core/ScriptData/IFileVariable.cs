using StepBro.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.ScriptData
{
    public interface IFileVariable : IFileElement
    {
        IValueContainer Value { get; }
    }
}
