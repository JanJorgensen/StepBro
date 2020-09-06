using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StepBro.Core.Data;
using StepBro.Core.Execution;
using StepBro.Core.Logging;

namespace StepBro.Core.ScriptData
{
    public interface IFileProcedure : IFileElement
    {
        TypeReference ReturnType { get; }
        NamedData<TypeReference>[] Parameters { get; }
        IProcedureReference ProcedureReference { get; }
        ContextLogOption LogOption { get; }
        bool SeparateStateLevel { get; }
        bool IsFunction { get; }
        IEnumerable<int> ListBreakpoints();
        bool IsBreakpointOnLine(int line);
    }
}
