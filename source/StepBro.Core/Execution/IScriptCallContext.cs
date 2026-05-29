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

        int CurrentTestStepIndex { get; }
        string CurrentTestStepTitle { get; }

        void EnterTestStep(int line, int column, int index, string title);
        void EnterStatement(int line, int column);

        bool IsSimpleExpectStatementWithValue{ get; set; }
        string ExpectStatementValue { get; set; }


        IScriptCallContext EnterNewScriptContext(IFileProcedure procedure, ContextLogOption procedureLoggingOption, bool isDynamicCall, object[] arguments);
        IScriptCallContext EnterNewScriptContext(IProcedureReference procedure, ContextLogOption procedureLoggingOption, bool isDynamicCall, object[] arguments);

        bool SetResultFromSub(IScriptCallContext sub);
        ProcedureResult Result { get; }

        void LogStatement(string text);
        void Log(string text);
        void LogDetail(string text);
        void LogError(string text);
        string GetLogLocation();

        ILoadedFilesManager LoadedFiles { get; }

        void SetLoopExitReason(string reason);
    }
}
