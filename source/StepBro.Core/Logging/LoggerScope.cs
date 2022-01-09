using System;

namespace StepBro.Core.Logging
{
    internal class LoggerScope : ServiceBase<ILogger, LoggerScope>, IProtectedLogger
    {
        private Logger m_logger;
        private readonly LogEntry m_scopeStartEntry;
        private bool m_disposeProtected;
        private bool m_ended = false;
        private int m_threadID = -1;

        internal LoggerScope(Logger logger, LogEntry scopeStartEntry, bool disposeProtected = false) : base()
        {
            m_logger = logger;
            m_scopeStartEntry = scopeStartEntry;
            m_disposeProtected = disposeProtected;
            m_threadID = scopeStartEntry.ThreadId;
        }

        internal LoggerScope(Logger logger, LogEntry scopeStartEntry, out IService service) : base("LoggerRootScope", out service)
        {
            m_logger = logger;
            m_scopeStartEntry = scopeStartEntry;
            m_disposeProtected = true;
            m_threadID = scopeStartEntry.ThreadId;
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
            if (m_disposeProtected)
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
            if (!m_ended)
            {
                this.Log(LogEntry.Type.Post, null, null);
                m_ended = true;
            }
        }

        public ILoggerScope LogEntering(string location, string text)
        {
            var entry = this.Log(LogEntry.Type.Pre, location, text);
            return new LoggerScope(m_logger, entry);
        }

        public void LogExit(string location, string text)
        {
            if (m_ended)
            {
                this.Log(LogEntry.Type.Error, location, "<already ended> " + text);
            }
            else
            {
                this.Log(LogEntry.Type.Post, location, text);
                m_ended = true;
            }
        }

        public void Log(string location, string text)
        {
            this.Log(LogEntry.Type.Normal, location, text);
        }

        public void LogDetail(string location, string text)
        {
            this.Log(LogEntry.Type.Detail, location, text);
        }

        public void LogError(string location, string text)
        {
            this.Log(LogEntry.Type.Error, location, text);
        }

        public void LogAsync(string location, string text)
        {
            this.Log(LogEntry.Type.Async, location, text);
        }

        public void LogUserAction(string location, string text)
        {
            this.Log(LogEntry.Type.UserAction, location, text);
        }

        public void LogSystem(string location, string text)
        {
            this.Log(LogEntry.Type.System, location, text);
        }

        private LogEntry Log(LogEntry.Type type, string location, string text)
        {
            return m_logger.Log(m_scopeStartEntry, type, DateTime.Now, m_threadID, location, text);
        }


        public void EnteredParallelTask(string location, string text)
        {
            System.Diagnostics.Debug.Assert(m_threadID == m_scopeStartEntry.ThreadId);  // Assert this one has not been called before.
            m_threadID = System.Threading.Thread.CurrentThread.ManagedThreadId;
            System.Diagnostics.Debug.Assert(m_threadID != m_scopeStartEntry.ThreadId);  // Assert this is on a different thread than the task that started this task.
            this.Log(LogEntry.Type.TaskEntry, location, text);
        }

        public IProtectedLogger GetProtectedLogger()
        {
            m_disposeProtected = true;
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
