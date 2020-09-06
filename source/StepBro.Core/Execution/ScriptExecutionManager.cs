using StepBro.Core.General;
using StepBro.Core.Logging;
using StepBro.Core.ScriptData;
using StepBro.Core.Tasks;
using System;
using System.Collections.Generic;

namespace StepBro.Core.Execution
{
    internal class ScriptExecutionManager : ServiceBase<IScriptExecutionManager, ScriptExecutionManager>, IScriptExecutionManager
    {
        private ILogger m_logger = null;
        private ILoadedFilesManager m_loadedFilesManager = null;
        private ILogSinkManager m_logSinkManager = null;
        private TaskManager m_taskManager = null;
        private readonly List<WeakReference<ScriptExecutionTask>> m_tasks = new List<WeakReference<ScriptExecutionTask>>();

        public ScriptExecutionManager(out IService serviceAccess) :
            base(nameof(ScriptExecutionManager), out serviceAccess, typeof(IMainLogger), typeof(ILoadedFilesManager), typeof(TaskManager))
        {
        }

        protected override void Start(ServiceManager manager, ITaskContext context)
        {
            m_logger = manager.Get<IMainLogger>().Logger.RootLogger;
            m_loadedFilesManager = manager.Get<ILoadedFilesManager>();
            m_logSinkManager = manager.Get<ILogSinkManager>();
            m_taskManager = manager.Get<TaskManager>();
        }

        public IExecutionResult ExecuteFileElement(
            IFileElement element,
            IPartner partner,
            params object[] arguments)
        {
            this.ExpectServiceStarted();

            var exeTask = new ScriptExecutionTask(m_logger, m_logSinkManager, m_loadedFilesManager, m_taskManager, element, arguments);
            m_tasks.Add(new WeakReference<ScriptExecutionTask>(exeTask));
            exeTask.ExecuteSynchronous();

            throw new NotImplementedException();
        }

        public IScriptExecution CreateFileElementExecution(
            IFileElement element,
            IPartner partner,
            params object[] arguments)
        {
            this.ExpectServiceStarted();

            var scriptTask = new ScriptExecutionTask(m_logger, m_logSinkManager, m_loadedFilesManager, m_taskManager, element, arguments);
            m_tasks.Add(new WeakReference<ScriptExecutionTask>(scriptTask));
            return scriptTask;
        }
    }
}
