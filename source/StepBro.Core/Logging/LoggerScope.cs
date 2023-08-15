using System;

namespace StepBro.Core.Logging
{
    internal class LoggerScope : ServiceBase<ILogger, LoggerScope>, IProtectedLogger
    {
        private enum DisposeOption
        {
            PublicDisposal, 
            ProtectedDisposal, 
            NoDisposal, 
            DisposedOrEnded
        }
        private Logger m_logger;
        private readonly LogEntry m_scopeStartEntry;
        private DisposeOption m_disposeOption;
        private int m_threadID = -1;
        private string m_location;
        private LoggerDynamicLocationSource m_dynamicLocation;

        internal LoggerScope(Logger logger, LogEntry scopeStartEntry, bool disposeProtected = false, LoggerDynamicLocationSource dynamicLocation = null) : base()
        {
            m_logger = logger;
            m_scopeStartEntry = scopeStartEntry;
            m_disposeOption = disposeProtected ? DisposeOption.ProtectedDisposal : DisposeOption.PublicDisposal;
            m_threadID = scopeStartEntry.ThreadId;
            m_location = scopeStartEntry.Location;
            if (dynamicLocation != null)
            {
                m_dynamicLocation = dynamicLocation;
            }
            else
            {
                m_dynamicLocation = new LoggerDynamicLocationSource(GetStaticLocation);
            }
        }

        private string GetStaticLocation() { return m_location; }

        internal LoggerScope(Logger logger, LogEntry scopeStartEntry, out IService service) : base("LoggerRootScope", out service)
        {
            m_logger = logger;
            m_scopeStartEntry = scopeStartEntry;
            m_disposeOption = DisposeOption.ProtectedDisposal;
            m_threadID = scopeStartEntry.ThreadId;
            m_location = scopeStartEntry.Location;
            m_dynamicLocation = new LoggerDynamicLocationSource(GetStaticLocation);
        }

        internal Logger Logger { get { return m_logger; } }

        public bool IsDebugging
        {
            get
            {
                return m_logger.IsDebugging;
            }
        }

        public void Dispose()
        {
            if (m_disposeOption == DisposeOption.ProtectedDisposal)
            {
                throw new NotSupportedException("This logger cannot be disposed by the Dispose method.");
            }
            this.DoDispose();
        }

        public void DisposeProtected()
        {
            this.DoDispose();
        }

        private void DoDispose()
        {
            if (m_disposeOption < DisposeOption.NoDisposal)
            {
                this.Log(LogEntry.Type.Post, null);
            }
            m_disposeOption = DisposeOption.DisposedOrEnded;
        }

        public string Location { get { return m_dynamicLocation(); } }

        public ILoggerScope LogEntering(string location, string text)
        {
            var entry = m_logger.Log(m_scopeStartEntry, LogEntry.Type.Pre, DateTime.Now, m_threadID, location, text);
            return new LoggerScope(m_logger, entry);
        }

        public ILoggerScope CreateSubLocation(string location)
        {
            var scope = new LoggerScope(m_logger, m_scopeStartEntry);
            scope.m_location = location;
            return scope;
        }

        public ILoggerScope LogEntering(bool isHighLevel, string location, string text, LoggerDynamicLocationSource dynamicLocation)
        {
            var entry = m_logger.Log(m_scopeStartEntry, isHighLevel ? LogEntry.Type.PreHighLevel : LogEntry.Type.Pre, DateTime.Now, m_threadID, location, text);
            return new LoggerScope(m_logger, entry, dynamicLocation: dynamicLocation);
        }

        public void LogExit(string text)
        {
            if (m_disposeOption == DisposeOption.DisposedOrEnded)
            {
                this.Log(LogEntry.Type.Error, "<already ended> " + text);
            }
            else
            {
                this.Log(LogEntry.Type.Post, text);
                m_disposeOption = DisposeOption.DisposedOrEnded;
            }
        }

        public void Log(string text)
        {
            this.Log(LogEntry.Type.Normal, text);
        }

        public void LogDetail(string text)
        {
            this.Log(LogEntry.Type.Detail, text);
        }

        public void LogError(string text)
        {
            this.Log(LogEntry.Type.Error, text);
        }

        public void LogAsync(string text)
        {
            this.Log(LogEntry.Type.Async, text);
        }

        public void LogUserAction(string text)
        {
            this.Log(LogEntry.Type.UserAction, text);
        }

        public void LogSystem(string text)
        {
            this.Log(LogEntry.Type.System, text);
        }

        internal LogEntry Log(LogEntry.Type type, string text)
        {
            return m_logger.Log(m_scopeStartEntry, type, DateTime.Now, m_threadID, (type != LogEntry.Type.Post) ? m_dynamicLocation() : null, text);
        }

        public void EnteredParallelTask(string text)
        {
            System.Diagnostics.Debug.Assert(m_threadID == m_scopeStartEntry.ThreadId);  // Assert this one has not been called before.
            m_threadID = System.Threading.Thread.CurrentThread.ManagedThreadId;
            System.Diagnostics.Debug.Assert(m_threadID != m_scopeStartEntry.ThreadId);  // Assert this is on a different thread than the task that started this task.
            this.Log(LogEntry.Type.TaskEntry, text);
        }

        public IProtectedLogger GetProtectedLogger()
        {
            m_disposeOption = DisposeOption.ProtectedDisposal;
            return (IProtectedLogger)this;
        }

        public object FirstLogEntryInScope
        {
            get { return m_scopeStartEntry; }
        }

        //private class ProtectedLogger : ILogger
        //{
        //    private Logger m_scope;
        //    public ProtectedLogger(Logger scope)
        //    {
        //        m_scope = scope;
        //    }

        //    public bool IsDebugging
        //    {
        //        get
        //        {
        //            return m_scope.IsDebugging;
        //        }
        //    }

        //    public void Log(string location, string text)
        //    {
        //        m_scope.Log(location, text);
        //    }

        //    public void LogDetail(string location, string text)
        //    {
        //        m_scope.Log(location, text);
        //    }

        //    public ILoggerScope LogEntering(string location, string text)
        //    {
        //        return m_scope.LogEntering(location, text);
        //    }

        //    public void LogError(string location, string text)
        //    {
        //        m_scope.LogError(location, text);
        //    }

        //    public void LogSystem(string location, string text)
        //    {
        //        m_scope.LogSystem(location, text);
        //    }

        //    public void LogUserAction(string location, string text)
        //    {
        //        m_scope.LogUserAction(location, text);
        //    }
        //}
    }
}
