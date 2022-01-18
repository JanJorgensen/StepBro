using System;
using StepBro.Core.Data;
using StepBro.Core.General;
using StepBro.Core.Logging;
using StepBro.Core.ScriptData;
using StepBro.Core.Tasks;

namespace StepBro.Core.Execution
{
    internal class ScriptTaskContext
    {
        private object m_value = null;
        private ILoggerScope m_logger = null;
        private ContextLogOption m_logOption = ContextLogOption.Normal;
        private IExecutionScopeStatusUpdate m_statusUpdate = null;
        private ILoadedFilesManager m_loadedFilesManager = null;
        private TaskManager m_taskManager;
        private RuntimeErrorListener m_errorListener = null;
        private Exception m_executionException = null;
        private ProcedureResult m_result = null;

        public void Setup(
            ILoggerScope logger, 
            ContextLogOption callerLoggingOption, 
            IExecutionScopeStatusUpdate statusUpdate, 
            ILoadedFilesManager loadedFilesManager,
            TaskManager taskManager)
        {
            m_logger = logger;
            m_logOption = callerLoggingOption;
            m_statusUpdate = statusUpdate;
            m_loadedFilesManager = loadedFilesManager;
            m_taskManager = taskManager;
        }

        public void SetErrorListener(RuntimeErrorListener listener)
        {
            m_errorListener = listener;
        }

        //public object CallProcedure(IFileProcedure procedure, LoggerRoot logger, ContextLogOption callerLoggingOption, IExecutionScopeStatusUpdate statusUpdate, params object[] arguments)
        //{
        //    this.Setup(logger, callerLoggingOption, statusUpdate);

        //    return this.DoCallProcedure(procedure, arguments);
        //}

        private object DoCallProcedure(IFileProcedure procedure, object[] arguments)
        {
            Delegate runtimeProcedure = ((FileProcedure)procedure).RuntimeProcedure;
            var invokeArguments = new object[arguments.Length + 1];
            Array.Copy(arguments, 0, invokeArguments, 1, arguments.Length);

            ScriptCallContext context = null;
            try
            {
                context = new ScriptCallContext(this, m_logger, m_logOption, m_statusUpdate, procedure, m_taskManager, arguments);
                context.SetErrorListener(m_errorListener);
                invokeArguments[0] = context;
                m_value = runtimeProcedure.DynamicInvoke(invokeArguments);
                m_result = context.Result;
                return m_value;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                context.InternalDispose();
            }
        }

        //public object CallProcedure(IProcedureReference procedure, LoggerRoot logger, ContextLogOption callerLoggingOption, IExecutionScopeStatusUpdate statusUpdate, params object[] arguments)
        //{
        //    this.Setup(logger, callerLoggingOption, statusUpdate);
        //    return this.DoCallProcedure(procedure.ProcedureInfo, arguments);
        //}

        public object CallProcedure(IProcedureReference procedure, params object[] arguments)
        {
            return this.DoCallProcedure(procedure.ProcedureData, arguments);
        }

        public object CallProcedure(IFileProcedure procedure, params object[] arguments)
        {
            try
            {
                m_value = this.DoCallProcedure(procedure, arguments);
                return m_value;
            }
            catch (Exception ex)
            {
                Logging.Logger.Root(this.Logger).DebugDump();
                m_executionException = ex;
                return null;
            }
        }

        public ILogger Logger { get { return m_logger; } }

        public object ReturnValue { get { return m_value; } }
        public ProcedureResult Result { get { return m_result; } }
        public Exception ExecutionExeception { get { return m_executionException; } }

        public ILoadedFilesManager LoadedFilesManager { get { return m_loadedFilesManager; } }
    }
}
