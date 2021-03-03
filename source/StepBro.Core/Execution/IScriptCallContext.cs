using System;
using StepBro.Core.Data;
using StepBro.Core.ScriptData;
using StepBro.Core.General;
using StepBro.Core.Logging;
using System.Collections.Generic;

namespace StepBro.Core.Execution
{
    internal interface IScriptCallContext : ICallContext, IInternalDispose
    {
        IFileProcedure Self { get; }
        IProcedureThis This { get; }
        int CurrentScriptFileLine { get; set; }
        ContextLogOption CallLoggingOption { get; }

        void EnterTestStep(int line, int column, int index, string title);
        void EnterStatement(int line, int column);

        IScriptCallContext EnterNewScriptContext(IFileProcedure procedure, ContextLogOption procedureLoggingOption, bool isDynamicCall);
        IScriptCallContext EnterNewScriptContext(IProcedureReference procedure, ContextLogOption procedureLoggingOption, bool isDynamicCall);

        bool SetResultFromSub(IScriptCallContext sub);
        ProcedureResult Result { get; }

        void Log(string text);
        void LogDetail(string text);
        void LogError(string text);

        ILoadedFilesManager LoadedFiles { get; }
    }
}
