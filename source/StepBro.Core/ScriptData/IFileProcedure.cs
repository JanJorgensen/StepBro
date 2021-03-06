﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StepBro.Core.Data;
using StepBro.Core.Execution;
using StepBro.Core.Logging;

namespace StepBro.Core.ScriptData
{
    [Flags]
    public enum ProcedureFlags
    {
        None = 0,
        IsFunction = 0x01,
        ContinueOnFail = 0x02,
        GotoCleanupOnFail = 0x04,
        GotoCleanupOnError = 0x08,
        SeparateStateLevel = 0x10,
        /// <summary>
        /// The result from called sub-procedures will not affect procedure result directly.
        /// </summary>
        NoSubResultInheritance = 0x20,
    }
    public interface IFileProcedure : IFileElement
    {
        TypeReference ReturnType { get; }
        NamedData<TypeReference>[] Parameters { get; }
        IProcedureReference ProcedureReference { get; }
        ProcedureFlags Flags { get; }
        ContextLogOption LogOption { get; }
        //bool SeparateStateLevel { get; }
        //bool IsFunction { get; }
        IEnumerable<int> ListBreakpoints();
        bool IsBreakpointOnLine(int line);
    }
}
