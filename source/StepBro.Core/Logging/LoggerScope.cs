﻿using System;

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
        private static LoggerScope m_root = null;
        private Logger m_logger;
        private readonly LogEntry m_scopeStartEntry;
        private LogEntry m_scopeEndEntry = null;
        private DisposeOption m_disposeOption;
        private int m_threadID = -1;
        private string m_location;
        private LoggerDynamicLocationSource m_dynamicLocation;
        private bool m_errorsLogged = false;

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
            //if (m_root != null) throw new InvalidOperationException("A root logger is already created!");
            m_root = this;
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
            var entry = m_logger.Log(m_scopeStartEntry, LogEntry.Type.Pre, DateTime.UtcNow, m_threadID, location, text);
            return new LoggerScope(m_logger, entry);
        }

        public ILoggerScope CreateSubLocation(string location)
        {
            var scope = new LoggerScope(m_logger, m_scopeStartEntry);
            scope.m_location = location;
            return scope;
        }

        public ILoggerScope LogEntering(LogEntry.Type type, string location, string text, LoggerDynamicLocationSource dynamicLocation = null)
        {
            var thread = m_threadID;
            if (type == LogEntry.Type.TaskEntry)
            {
                thread = System.Threading.Thread.CurrentThread.ManagedThreadId;
            }
            var entry = m_logger.Log(m_scopeStartEntry, type, DateTime.UtcNow, thread, location, text);
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
                m_disposeOption = DisposeOption.DisposedOrEnded;
                m_scopeEndEntry = this.Log(LogEntry.Type.Post, text);
            }
        }

        public ITimestampedData Log(string text)
        {
            return this.Log(LogEntry.Type.Normal, text);
        }

        public void LogDetail(string text)
        {
            this.Log(LogEntry.Type.Detail, text);
        }

        public void LogError(string text)
        {
            m_errorsLogged = true;
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

        public void LogCommSent(string text)
        {
            this.Log(LogEntry.Type.CommunicationOut, text);
        }

        public void LogCommReceived(string text)
        {
            this.Log(LogEntry.Type.CommunicationIn, text);
        }

        internal LogEntry Log(LogEntry.Type type, string text)
        {
            return m_logger.Log(m_scopeStartEntry, type, DateTime.UtcNow, m_threadID, (type != LogEntry.Type.Post) ? m_dynamicLocation() : null, text);
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

        public ITimestampedData FirstLogEntryInScope
        {
            get { return m_scopeStartEntry; }
        }

        public ITimestampedData LastLogEntryInScope
        {
            get { return m_scopeEndEntry; }
        }

        public bool ErrorsLogged { get { return m_errorsLogged; } }
    }
}
