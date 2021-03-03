using StepBro.Core.Logging;
using StepBro.Core.Tasks;
using StepBroCoreTest.Utils;
using System;

namespace StepBroCoreTest.Mocks
{
    public class TaskContextMock : ITaskContext, ILoggerScope
    {
        private MiniLogger m_logger;
        private readonly bool m_logProgressSetup = false;
        public TaskContextMock(MiniLogger logger)
        {
            if (logger == null) logger = new MiniLogger();
            m_logger = logger;
        }

        public bool PauseRequested { get; set; } = false;

        public ILoggerScope Logger { get { return (ILoggerScope)this; } }

        public bool EnterPauseIfRequested(string state)
        {
            return false;
        }

        public void ProgressAliveSignal()
        {
            m_logger.Add("progress alive");
        }

        public void ProgressSetup(long start, long length, Func<long, string> toText)
        {
            if (m_logProgressSetup)
            {
                m_logger.Add($"progress setup, start: {start}, length: {length}");
            }
        }

        public void UpdateStatus(string text = null, long progress = -1)
        {
            if (text == null)
            {
                if (progress >= 0)
                {
                    m_logger.Add($"Progress: {progress}");
                }
                else
                {
                    m_logger.Add("<no status>");
                }
            }
            else
            {
                if (progress >= 0)
                {
                    m_logger.Add($"{text} - Progress: {progress}");
                }
                else
                {
                    m_logger.Add(text);
                }
            }
        }

        #region ILoggerScope
        object ILoggerScope.FirstLogEntryInScope => throw new NotImplementedException();

        bool ILogger.IsDebugging => throw new NotImplementedException();

        void ILoggerScope.EnteredParallelTask(string location, string text)
        {
            throw new NotImplementedException();
        }

        void ILogger.Log(string location, string text)
        {
            throw new NotImplementedException();
        }

        void ILogger.LogDetail(string location, string text)
        {
            throw new NotImplementedException();
        }

        ILoggerScope ILogger.LogEntering(string location, string text)
        {
            throw new NotImplementedException();
        }

        void ILogger.LogError(string location, string text)
        {
            throw new NotImplementedException();
        }

        void ILoggerScope.LogExit(string location, string text)
        {
            throw new NotImplementedException();
        }

        void ILogger.LogSystem(string location, string text)
        {
            throw new NotImplementedException();
        }

        void ILogger.LogUserAction(string location, string text)
        {
            throw new NotImplementedException();
        }

        void ILogger.LogAsync(string location, string text)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~TaskContextMock() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
