using System;
using StepBro.Core.Data;
using StepBro.Core.ScriptData;
using StepBro.Core.General;
using StepBro.Core.Logging;

namespace StepBro.Core.Execution
{
    internal interface IScriptCallContext : ICallContext, IInternalDispose
    {
        //void Setup(IFileProcedure procedure, ContextLogOption procedureLoggingOption, bool separateStateLevel);
        //IScriptFileInfo CurrentScriptFile { get; }
        IFileProcedure Self { get; }
        IProcedureThis This { get; }
        int CurrentScriptFileLine { get; set; }
        ContextLogOption CallLoggingOption { get; }

        void EnterTestStep(int line, int column, int index, string title);
        void EnterStatement(int line, int column);

        IScriptCallContext EnterNewScriptContext(IFileProcedure procedure, ContextLogOption procedureLoggingOption, bool isDynamicCall);
        IScriptCallContext EnterNewScriptContext(IProcedureReference procedure, ContextLogOption procedureLoggingOption, bool isDynamicCall);

        TestResult CurrentProcedureResult { get; }

        void Log(string text);
        void LogDetail(string text);
        void LogError(string text);

        ILoadedFilesManager LoadedFiles { get; }
    }
}
