using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Execution;
using StepBro.Core.File;
using StepBro.Core.General;
using StepBro.Core.Host;
using StepBro.Core.Logging;
using StepBro.Core.Parser;
using StepBro.Core.ScriptData;
using StepBro.Core.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace StepBro.Core
{
    public static class Main
    {
        private static int m_initIndex = 0;
        public static readonly string StepBroFileExtension = "sbs";
        private static bool m_initialized = false;
        private static ServiceManager.IServiceManagerAdministration m_serviceManagerAdmin = null;
        private static Logger m_mainLogger = null;
        private static ILoggerScope m_logRootScope = null;
        private static ILoadedFilesManager m_loadedFilesManager = null;
        private static IConfigurationFileManager m_configurationFileManager = null;
        private static ITextFileSystem m_textFileSystem = null;
        private static IAddonManager m_addonManager = null;
        //private static ILogSinkManager m_logSinkManager = null;
        private static TaskManager m_taskManager = null;
        private static HostApplicationActionQueue m_hostActions = null;
        //private static List<ScriptFile> m_loadedFiles = new List<ScriptFile>();
        private static ScriptExecutionManager m_scriptExecutionManager = null;
        private static DynamicObjectManager m_dynamicObjectManager = null;
        private static LogFileCreationManager m_logFileCreationManager = null;
        private static UICalculator m_uiCalculator = null;
        private static FolderManager m_folderShortcuts = null;
        //private static readonly object m_mainObject = new object();
        //private static readonly bool m_isInDebugMode = true;
        //private static Queue<Task> m_runningTasks = new Queue<Task>();
        //private static readonly object m_runningTasksSync = new object();
        private static ITask m_parsingInQueue = null;
        private static int m_lastParsingErrorCount = 0;

        //public static object ExecutionHelper { get; private set; }

        static Main()
        {
            m_serviceManagerAdmin = ServiceManager.Create();
        }

        public static bool IsInitialized { get { return m_initialized; } }

        public static void Initialize(params IService[] hostServices)
        {
            m_mainLogger = new Logger("", false, "StepBro", "Main logger created");
            IService service = m_mainLogger.RootScopeService;
            m_logRootScope = m_mainLogger.RootLogger as ILoggerScope;
            m_serviceManagerAdmin.Manager.Register(service);

            m_loadedFilesManager = new LoadedFilesManager(out service);
            m_serviceManagerAdmin.Manager.Register(service);
            m_configurationFileManager = new ConfigurationFileManager(out service);
            m_serviceManagerAdmin.Manager.Register(service);
            m_textFileSystem = new TextFileSystem(out service);
            m_serviceManagerAdmin.Manager.Register(service);

            m_addonManager = new AddonManager(
                (IAddonManager m) =>
                {
                    // **** LOAD THE ADDON MODULES ****

                    m.AddAssembly(typeof(DateTime).Assembly, false);
                    m.AddAssembly(typeof(Enumerable).Assembly, false);
                    m.AddAssembly(typeof(Math).Assembly, false);
                    m.AddAssembly(AddonManager.StepBroCoreAssembly, true);
                    var modulesFolder = Path.Combine(Path.GetDirectoryName(typeof(AddonManager).Assembly.Location), "Modules");
                    var modulesListFile = Path.Combine(modulesFolder, Constants.PLUGINS_LIST_FILE);
                    if (System.IO.File.Exists(modulesListFile))
                    {
                        var modulesListData = modulesListFile.GetPropertyBlockFromFile();
                        var list = modulesListData[0] as PropertyBlockArray;
                        foreach (PropertyBlockValue entry in list)
                        {
                            var filepath = entry.Value as string;
                            if (!System.IO.Path.IsPathFullyQualified(filepath))
                            {
                                filepath = Path.Combine(modulesFolder, filepath);
                            }
                            m.LoadAssembly(filepath, false);
                        }
                    }
                    else
                    {
                        foreach (var f in Directory.GetFiles(modulesFolder, "*.dll"))
                        {
                            m.LoadAssembly(f, false);
                        }
                    }
                },
                out service);
            m_serviceManagerAdmin.Manager.Register(service);

            //m_logSinkManager = new LogSinkManager(out service);
            //m_serviceManagerAdmin.Manager.Register(service);

            m_taskManager = new TaskManager(out service);
            m_serviceManagerAdmin.Manager.Register(service);

            m_hostActions = new HostApplicationActionQueue(out service);
            m_serviceManagerAdmin.Manager.Register(service);

            m_scriptExecutionManager = new ScriptExecutionManager(out service);
            m_serviceManagerAdmin.Manager.Register(service);

            m_dynamicObjectManager = new DynamicObjectManager(out service);
            m_serviceManagerAdmin.Manager.Register(service);

            m_logFileCreationManager = new LogFileCreationManager(out service);
            m_serviceManagerAdmin.Manager.Register(service);

            m_uiCalculator = new UICalculator(out service);
            m_serviceManagerAdmin.Manager.Register(service);

            m_folderShortcuts = new FolderManager(out service);
            m_serviceManagerAdmin.Manager.Register(service);

            if (hostServices != null)
            {
                foreach (var hs in hostServices)
                {
                    m_serviceManagerAdmin.Manager.Register(hs);
                }
            }

            TaskContextDummy taskContext = new TaskContextDummy((ILoggerScope)m_logRootScope);

            try
            {
                m_serviceManagerAdmin.StartServices(taskContext);
            }
            catch (Exception ex)
            {
                // Roll back (stop) the services that actually did start.
                try
                {
                    m_serviceManagerAdmin.StopServices(taskContext);
                }
                catch { /* Ignore exceptions during stop */ }
                throw new Exception("Failed to start all services. " + ex.GetType().Name + ", " + ex.ToString());
            }

            m_initialized = true;
            m_initIndex++;
        }

        public static void Deinitialize()
        {
            DeinitializeInternal(false);
        }

        internal static void DeinitializeInternal(bool reset)
        {
            m_loadedFilesManager.UnloadAllFilesWithoutDependants();

            TaskContextDummy taskContext = new TaskContextDummy();
            m_serviceManagerAdmin.StopServices(taskContext, reset);

            if (reset)
            {
                m_serviceManagerAdmin = null;
                m_mainLogger = null;
                m_loadedFilesManager = null;
                m_addonManager = null;
                //m_logSinkManager = null;
                m_scriptExecutionManager = null;
                m_dynamicObjectManager = null;
                m_uiCalculator = null;
                m_folderShortcuts = null;

                m_serviceManagerAdmin = ServiceManager.Create();
            }
        }

        public static T GetService<T>() where T : class
        {
            return m_serviceManagerAdmin.Manager.Get<T>();
        }

        public static Logger Logger { get { return m_mainLogger; } }

        public static ILogger RootLogger { get { return m_logRootScope; } }

        public static ServiceManager ServiceManager { get { return m_serviceManagerAdmin.Manager; } }

        public static ILoadedFilesManager GetLoadedFilesManager()
        {
            return m_loadedFilesManager;
        }

        public static void RegisterService(IService service, bool forceReplace)
        {
            m_serviceManagerAdmin.Manager.Register(service);
        }

        public static IScriptFile LoadScriptFile(object user, string filepath)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (String.IsNullOrWhiteSpace(filepath)) throw new ArgumentNullException("filepath");
            if (m_loadedFilesManager.ListFiles<IScriptFile>().FirstOrDefault(f => String.Equals(f.FilePath, filepath)) != null)
            {
                throw new FileAlreadyLoadedException(filepath);
            }
            var extension = System.IO.Path.GetExtension(filepath);
            if (extension.Equals(".sbs", StringComparison.InvariantCulture))
            {
                var file = new ScriptFile(filepath);
                file.RegisterDependant(user);
                m_loadedFilesManager.RegisterLoadedFile(file);
                return file;
            }
            else
            {
                return null;
            }
        }

        public static IScriptFile CreateScriptFileObject(string filepath)
        {
            return new ScriptFile(filepath);
        }

        public static bool UnregisterFileUsage(object user, ILoadedFile file, bool throwIfNotFound = true)
        {
            var wasUnregistered = file.UnregisterDependant(user, throwIfNotFound);
            m_loadedFilesManager.UnloadAllFilesWithoutDependants();
            return wasUnregistered;
        }

        public static IHostApplicationActionQueue Actions { get { return m_hostActions; } }

        //private static IAsyncResult EnqueueTask(Action action)
        //{
        //    var combinedActions = new DoubleAction(action, OnRunningTaskEnd);
        //    var task = new Task(combinedActions.ActionDoSecondAlways);
        //    lock (m_runningTasksSync)
        //    {
        //        if (m_runningTasks.Count == 0)
        //        {
        //            task.Start();
        //        }
        //        m_runningTasks.Enqueue(task);
        //    }
        //    return task;
        //}

        //private static void OnRunningTaskEnd()
        //{
        //    lock (m_runningTasksSync)
        //    {
        //        m_runningTasks.Dequeue();   // Remove task that just ended.
        //        if (m_runningTasks.Count > 0)
        //        {
        //            var next = m_runningTasks.Peek();
        //            next.Start();
        //        }
        //    }
        //}

        /// <summary>
        /// Starts a task parsing all loaded script files.
        /// </summary>
        /// <param name="force">Set to true to force a parsing of all files.</param>
        /// <returns>True if parsing succeeded (no errors or parsing skipped).</returns>
        public static ITask StartFileParsing(bool force)
        {
            if (m_parsingInQueue != null) return m_parsingInQueue;
            if (force || CheckIfFileParsingNeeded())
            {
                m_parsingInQueue = m_hostActions.AddTask("Script File Parsing", true, null, (context) =>
                    {
                        try
                        {
                            context.UpdateStatus("Parsing files");
                            m_lastParsingErrorCount = FileBuilder.ParseFiles(m_serviceManagerAdmin.Manager, context.Logger, (IScriptFile)null);
                        }
                        catch (Exception ex)
                        {
                            m_lastParsingErrorCount = 1;    // At least one...
                            context.Logger.LogError(ex.ToString());
                        }
                        finally
                        {
                            context.UpdateStatus($"Finished parsing. {m_lastParsingErrorCount} parsing error(s).");
                            m_parsingInQueue = null;
                            ParsingCompleted?.Invoke(null, EventArgs.Empty);
                        }
                    });
                return m_parsingInQueue;
            }
            else
            {
                return null;
            }
        }

        public static bool CheckIfFileParsingNeeded(bool alsoIfFileNotFound = false)
        {
            foreach (var f in m_loadedFilesManager.ListFiles<ScriptFile>())
            {
                if (f.LastParsing == DateTime.MinValue || f.LastParsing < f.LastFileChange || f.HasFileChanged(alsoIfFileNotFound))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool ParseFiles(bool force)
        {
            if (force || CheckIfFileParsingNeeded())
            {
                ILoggerScope logger = null;
                try
                {
                    logger = m_logRootScope.LogEntering(true, "StepBro.Main.FileParsing", "Starting file parsing", null);
                    m_lastParsingErrorCount = FileBuilder.ParseFiles(m_serviceManagerAdmin.Manager, logger, (IScriptFile)null);
                }
                finally
                {
                    if (m_lastParsingErrorCount > 0)
                    {
                        logger.LogError($"Ended file parsing. {m_lastParsingErrorCount} errors.");
                    }
                    else
                    {
                        logger.Log($"Ended file parsing. No errors.");
                    }
                }
                return (m_lastParsingErrorCount == 0);
            }
            else
            {
                return true;    // Success (at least no failures) ...
            }
        }

        public static event EventHandler ParsingCompleted;

        public static int LastParsingErrorCount { get { return m_lastParsingErrorCount; } }

        public static IFileProcedure TryFindProcedure(string name)
        {
            foreach (var file in m_loadedFilesManager.ListFiles<ScriptFile>())
            {
                IFileProcedure procedure = file.ListElements().First(
                    p => String.Equals(name, p.Name, StringComparison.InvariantCultureIgnoreCase) ||
                    String.Equals(name, p.FullName, StringComparison.InvariantCultureIgnoreCase)) as IFileProcedure;
                if (procedure != null)
                {
                    return procedure;
                }
            }
            return null;
        }

        public static IFileElement TryFindFileElement(string name)
        {
            foreach (var file in m_loadedFilesManager.ListFiles<ScriptFile>())
            {
                var element = file.ListElements().FirstOrDefault(
                    p => String.Equals(name, p.Name, StringComparison.InvariantCultureIgnoreCase) ||
                    String.Equals(name, p.FullName, StringComparison.InvariantCultureIgnoreCase));
                if (element != null)
                {
                    return element;
                }
            }
            return null;
        }

        public static IScriptExecution ExecuteProcedure(IFileElement element, IPartner partner, params object[] arguments)
        {
            return m_scriptExecutionManager.ExecuteFileElement(element, partner, arguments);
        }

        public static IScriptExecution StartProcedureExecution(IFileElement element, IPartner partner, params object[] arguments)
        {
            var execution = m_scriptExecutionManager.CreateFileElementExecution(element, partner, arguments);
            execution.StartExecution();
            return execution;
        }

        public static ICallContext CreateUICallContext()
        {
            return new UICallContext(m_serviceManagerAdmin.Manager);
        }

        public static object ParseExpression(IScriptFile fileContext, string expression)
        {
            var builder = FileBuilder.ParseExpression(fileContext as ScriptFile, m_addonManager, expression);
            if (builder.Errors.ErrorCount > 0)
            {
                throw new Exception("Error parsing expression: " + builder.Errors[0].Message);
            }
            var result = builder.Listener.GetExpressionResult();
            if (result.IsUnresolvedIdentifier)
            {
                result = new SBExpressionData(Expression.Constant(result.Value));
            }
            var expressionAsObject = Expression.Convert(result.ExpressionCode, typeof(object));
            return Expression.Lambda<Func<object>>(expressionAsObject).Compile()();
        }

        public static void DumpCurrentObjectsToLog()
        {
            var logger = m_mainLogger.RootLogger;
            foreach (var oc in m_dynamicObjectManager.GetObjectCollection())
            {
                logger.Log("Dynamic object: " + oc.FullName);
            }
        }
    }

    internal class TaskContextDummy : ITaskContext
    {
        public TaskContextDummy(ILoggerScope logger = null) { Logger = logger; }

        public bool PauseRequested
        {
            get { return false; }
        }

        public ILoggerScope Logger
        {
            get;
            private set;
        }

        public bool EnterPauseIfRequested(string state)
        {
            return false;
        }

        public void ProgressAliveSignal()
        {
        }

        public void ProgressSetup(long start, long length, Func<long, string> toText)
        {
        }

        public void UpdateStatus(string text = null, long progress = -1)
        {
        }
    }
}
