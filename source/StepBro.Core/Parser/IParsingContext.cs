using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StepBro.Core.Data;

namespace StepBro.Core.Parser
{
    [Flags]
    internal enum EntryModifiers : uint
    {
        Private = 0x01,
        Static = 0x02,
        Execution = 0x04
    }

    internal interface IParsingContext
    {
        //void AddVariable(string name, Type type, TSExpressionData assignment, EntryModifiers modifiers);
        IEnumerable<IIdentifierInfo> KnownIdentifiers();
    }
}
