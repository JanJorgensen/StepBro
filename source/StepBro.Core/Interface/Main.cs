using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Execution;
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
        private static ILogger m_logRootScope = null;
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
        private static UICalculator m_uiCalculator = null;
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
            m_logRootScope = m_mainLogger.RootLogger;
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
                    foreach (var f in Directory.GetFiles(modulesFolder, "*.dll"))
                    {
                        m.LoadAssembly(f, false);
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

            m_uiCalculator = new UICalculator(out service);
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
                            context.UpdateStatus($"Resetting files ({(force ? "forced" : "not forced")})");
                            foreach (var f in m_loadedFilesManager.ListFiles<ScriptFile>())
                            {
                                f.ResetBeforeParsing(preserveUpdateableElements: force == false);
                            }

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

        private static bool CheckIfFileParsingNeeded()
        {
            foreach (var f in m_loadedFilesManager.ListFiles<ScriptFile>())
            {
                if (f.LastParsing == DateTime.MinValue || f.LastParsing < f.LastFileChange || f.HasFileChanged())
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
                    logger = m_logRootScope.LogEntering("StepBro.Main.FileParsing", "Starting file parsing");
                    foreach (var f in m_loadedFilesManager.ListFiles<ScriptFile>())
                    {
                        f.ResetBeforeParsing(preserveUpdateableElements: force == false);
                    }

                    m_lastParsingErrorCount = FileBuilder.ParseFiles(m_serviceManagerAdmin.Manager, logger, (IScriptFile)null);
                }
                finally
                {
                    logger.LogExit($"Ended file parsing. {m_lastParsingErrorCount} errors.");
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
                var element = file.ListElements().First(
                    p => String.Equals(name, p.Name, StringComparison.InvariantCultureIgnoreCase) ||
                    String.Equals(name, p.FullName, StringComparison.InvariantCultureIgnoreCase));
                if (element != null)
                {
                    return element;
                }
            }
            return null;
        }

        public static IExecutionResult ExecuteProcedure(IFileProcedure procedure, params object[] arguments)
        {
            return m_scriptExecutionManager.ExecuteFileElement(procedure, null, arguments);
        }

        public static IScriptExecution StartProcedureExecution(IFileProcedure procedure, params object[] arguments)
        {
            var execution = m_scriptExecutionManager.CreateFileElementExecution(procedure, null, arguments);
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

        private class ExecutionScopeStatusUpdaterMock : StepBro.Core.Execution.IExecutionScopeStatusUpdate
        {
            private int m_level = 0;
            public int m_disposeCount = 0;
            public ExecutionScopeStatusUpdaterMock m_child = null;
            public string m_text = null;
            public TimeSpan m_expectedTime = default(TimeSpan);
            public long m_progressMax = -1;
            public long m_progress = -1;
            public long m_progressPokeCount = 0;
            public Func<long, string> m_progressFormatter = null;
            public List<Tuple<string, Func<bool, bool>>> m_buttons = new List<Tuple<string, Func<bool, bool>>>();

            public AttentionColor ProgressColor
            {
                get;
                set;
            }

            public bool PauseRequested
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public event EventHandler Disposed;
            public event EventHandler ExpectedTimeExceeded;

            public void AddActionButton(string title, Func<bool, bool> activationAction)
            {
                throw new AccessViolationException();
                //MiniLogger.Instance.Add("TaskUpdate(" + m_level + ").AddActionButton: " + title);
                //m_buttons.Add(new Tuple<string, ButtonActivationType, Action<bool>>(title, type, activationAction));
            }

            public void ClearSublevels()
            {
                if (m_child != null)
                {
                    m_child.Dispose();
                }
            }

            public IExecutionScopeStatusUpdate CreateProgressReporter(string text = "", TimeSpan expectedTime = default(TimeSpan), long progressMax = -1L, Func<long, string> progressFormatter = null)
            {
                if (m_child != null) throw new Exception("Child status already active.");
                //MiniLogger.Instance.Add("TaskUpdate(" + m_level + ").CreateProgressReporter( " + text + " )");
                m_child = new ExecutionScopeStatusUpdaterMock();
                m_child.m_level = m_level + 1;
                m_child.m_text = text;
                m_child.m_expectedTime = expectedTime;
                m_child.m_progressMax = progressMax;
                m_child.m_progressFormatter = progressFormatter;
                m_child.Disposed += M_child_Disposed;
                return m_child;
            }

            private void M_child_Disposed(object sender, EventArgs e)
            {
                if (Object.ReferenceEquals(sender, m_child))
                {
                    m_child.Disposed -= M_child_Disposed;
                    m_child = null;
                }
            }

            public void Dispose()
            {
                m_disposeCount++;
                if (m_disposeCount == 1)
                {
                    if (Disposed != null) Disposed(this, EventArgs.Empty);
                }
            }

            public void ProgressAliveSignal()
            {
                //MiniLogger.Instance.Add("TaskUpdate(" + m_level + ").ProgressAliveSignal");
                m_progressPokeCount++;
            }

            public void UpdateStatus(string text = null, long progress = -1)
            {
                if (text != null) m_text = text;
                if (progress >= 0) m_progress = progress;
                if (progress == 99999) ExpectedTimeExceeded?.Invoke(this, EventArgs.Empty);    // TODO
                //MiniLogger.Instance.Add(String.Format("TaskUpdate({0}).UpdateStatus: {1}, {2}", m_level, String.IsNullOrEmpty(text) ? "<no text>" : text, (progress >= 0) ? progress.ToString() : "<no progress>"));
            }

            public bool EnterPauseIfRequested(string state)
            {
                throw new NotImplementedException();
            }

            public void ProgressSetup(long start, long length, Func<long, string> toText)
            {
                throw new NotImplementedException();
            }
        }

        public static void DumpCurrentObjectsToLog()
        {
            var logger = m_mainLogger.RootLogger;
            foreach (var oc in m_dynamicObjectManager.ListKnownObjects())
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
