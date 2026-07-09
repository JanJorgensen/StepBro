using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace StepBro.Core.Logging
{
    public interface IComponentLoggerSource
    {
        const string CAT_TEXT = "TEXT";

        string Name { get; }
        /// <summary>
        /// Set the enabled state of the logger source.
        /// </summary>
        /// <param name="value">Whether to set the state to enabled (true) or disabled (false).</param>
        /// <returns>Whether the source changed the enabled state to the requested value.</returns>
        bool SetEnabled(bool value);
        bool Enabled { get; }
        /// <summary>
        /// The type of data for communication logging.
        /// </summary>
        string CommDataCategory { get; }
    }

    public interface IComponentLoggerService
    {
        IComponentLogging CreateComponentLogger(IComponentLoggerSource source);
        IEnumerable<IComponentLogging> ListLoggers();
    }

    public interface IComponentLogging : IDisposable
    {
        bool Enabled { get; set; }
        event EventHandler EnabledChanged;
        string Name { get; }
        void LogState(string text);
        void LogSent(string message);
        void LogReceived(string message);
        void LogError(string text);
    }

    internal class ComponentLoggerService : ServiceBase<IComponentLoggerService, ComponentLoggerService>, IComponentLoggerService
    {
        private class Logger : IComponentLogging
        {
            private IComponentLoggerSource m_source;
            private ILoggerScope m_asyncLogger = null;

            private Logger(IComponentLoggerSource source)
            {
                m_source = source;
            }

            private void SetupLogger()
            {
                if (m_asyncLogger == null)
                {
                    m_asyncLogger = ((ILoggerScope)Core.Main.GetService<ILogger>()).LogEntering(LogEntry.Type.Component, m_source.Name, m_source.CommDataCategory, null);
                }
            }

            public static Logger Create(IComponentLoggerSource source)
            {
                var logger = new Logger(source);
                logger.SetupLogger();
                return logger;
            }

            public string Name { get { return m_source.Name; } }

            public bool Enabled
            {
                get { return m_source.Enabled; }
                set
                {
                    if (value != m_source.Enabled)
                    {
                        if (m_source.SetEnabled(value))
                        {
                            this.EnabledChanged?.Invoke(this, new EventArgs());
                        }
                    }
                }
            }

            public event EventHandler EnabledChanged;

            public void Dispose()
            {
            }

            public void LogState(string text)
            {
                m_asyncLogger.Log(text);
            }

            public void LogSent(string message)
            {
                m_asyncLogger.LogCommSent(message);
            }

            public void LogReceived(string message)
            {
                m_asyncLogger.LogCommReceived(message);
            }

            public void LogError(string text)
            {
                m_asyncLogger.LogError(text);
            }
        }

        StepBro.Core.Logging.Logger m_mainLogger;
        private List<Logger> m_loggers = new List<Logger>();

        public ComponentLoggerService(out IService serviceAccess, StepBro.Core.Logging.Logger logger) :
            base("ComponentLoggerService", out serviceAccess)
        {
            m_mainLogger = logger;
        }

        public IComponentLogging CreateComponentLogger(IComponentLoggerSource source)
        {
            var logger = Logger.Create(source);
            m_loggers.Add(logger);
            return logger as IComponentLogging;
        }

        public IEnumerable<IComponentLogging> ListLoggers()
        {
            foreach (var logger in m_loggers) { yield return logger; }
        }
    }
}
