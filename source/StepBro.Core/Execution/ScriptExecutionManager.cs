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
        private ILoggerScope m_logger = null;
        private ILoadedFilesManager m_loadedFilesManager = null;
        //private ILogSinkManager m_logSinkManager = null;
        private TaskManager m_taskManager = null;
        private readonly List<WeakReference<ScriptExecutionTask>> m_tasks = new List<WeakReference<ScriptExecutionTask>>();

        public ScriptExecutionManager(out IService serviceAccess) :
            base(nameof(ScriptExecutionManager), out serviceAccess, 
                typeof(ILogger), typeof(ILoadedFilesManager), typeof(TaskManager), typeof(IConfigurationFileManager))
        {
        }

        protected override void Start(ServiceManager manager, ITaskContext context)
        {
            m_logger = manager.Get<ILogger>() as ILoggerScope;
            m_loadedFilesManager = manager.Get<ILoadedFilesManager>();
            //m_logSinkManager = manager.Get<ILogSinkManager>();
            m_taskManager = manager.Get<TaskManager>();
        }

        public IScriptExecution ExecuteFileElement(
            IFileElement element,
            IPartner partner,
            params object[] arguments)
        {
            this.ExpectServiceStarted();

            var exeTask = (ScriptExecutionTask)CreateFileElementExecution(element, partner, arguments);
            exeTask.ExecuteSynchronous();

            return exeTask;
        }

        public IScriptExecution CreateFileElementExecution(
            IFileElement element,
            IPartner partner,
            params object[] arguments)
        {
            this.ExpectServiceStarted();

            var scriptTask = new ScriptExecutionTask(m_logger, /*m_logSinkManager,*/ m_loadedFilesManager, m_taskManager, element, arguments);
            m_tasks.Add(new WeakReference<ScriptExecutionTask>(scriptTask));
            return scriptTask;
        }
    }
}
