using StepBro.Core;
using StepBro.Core.Data;
using StepBro.Core.Execution;
using StepBro.Core.General;
using StepBro.Core.Logging;
using StepBro.Core.ScriptData;
using StepBro.Core.Tasks;
using System;
using System.Collections.Generic;

namespace StepBroCoreTest
{
    internal static class ExecutionHelper
    {
        public class RuntimeErrorCollector
        {
            public List<Tuple<IFileProcedure, int, ErrorID, string, Exception>> Errors { get; set; } = new List<Tuple<IFileProcedure, int, ErrorID, string, Exception>>();
            public void ReportError(IFileProcedure procedure, int line, ErrorID error, string description, Exception exception)
            {
                this.Errors.Add(new Tuple<IFileProcedure, int, ErrorID, string, Exception>(procedure, line, error, description, exception));
            }
        }
        public static RuntimeErrorCollector RuntimeErrors { get; set; }


        public static object Call(this IFileProcedure procedure, params object[] args)
        {
            return DoDynamicInvoke(procedure, false, args);
        }

        public static object CallDebug(this IFileProcedure procedure, params object[] args)
        {
            return DoDynamicInvoke(procedure, true, args);
        }

        public static object Call(this IProcedureReference procedure, params object[] args)
        {
            return DoDynamicInvoke(procedure.ProcedureData, false, args);
        }

        public static object CallDebug(this IProcedureReference procedure, params object[] args)
        {
            return DoDynamicInvoke(procedure.ProcedureData, true, args);
        }

        private static object DoDynamicInvoke(IFileProcedure procedure, bool debug, params object[] args)
        {
            var taskContext = ExeContext();
            LoggerRoot.Root(taskContext.Logger).IsDebugging = debug;

            return taskContext.CallProcedure(procedure, args);
        }

        public static ScriptTaskContext ExeContext(bool debugging = true)
        {
            ScriptTaskContext taskContext = new ScriptTaskContext();
            RuntimeErrors = new RuntimeErrorCollector();
            taskContext.SetErrorListener(RuntimeErrors.ReportError);
            var logger = new LoggerRoot("", false, "TestRun", "Starting");
            if (debugging)
            {
                logger.IsDebugging = true;
            }
            var taskStatusUpdater = new Mocks.ExecutionScopeStatusUpdaterMock();
            IService service;
            LoadedFilesManager loadedFiles = new LoadedFilesManager(out service);
            TaskManager taskManager = new TaskManager(out service);
            taskContext.Setup(logger.m_rootLogger, ContextLogOption.Normal, taskStatusUpdater, loadedFiles, taskManager);
            return taskContext;
        }
    }
}
