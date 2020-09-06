using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.ScriptData
{
    public interface IScriptProperty : IFileElement
    {
        //IFileElement Parent { get; }
        object Value { get; }
    }

    public interface IScriptProperty<T> : IScriptProperty
    {
        new T Value { get; }
    }
}
