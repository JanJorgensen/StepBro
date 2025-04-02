using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Logging
{
    public interface IComponentLoggerSource
    {
        string Name { get; }
        /// <summary>
        /// Set the enabled state of the logger source.
        /// </summary>
        /// <param name="value">Whether to set the state to enabled (true) or disabled (false).</param>
        /// <returns>Whether the source changed the enabled state to the requested value.</returns>
        bool SetEnabled(bool value);
        bool Enabled { get; }
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
        DateTime LogState(string message);
        DateTime LogSent(string message);
        DateTime LogReceived(string message);
        DateTime LogError(string text);
    }

    internal class ComponentLoggerService : ServiceBase<IComponentLoggerService, ComponentLoggerService>, IComponentLoggerService
    {
        private class Logger : IComponentLogging
        {
            IComponentLoggerSource m_source;

            public Logger(IComponentLoggerSource source)
            {
                m_source = source;
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

            public DateTime LogError(string text)
            {
                return default(DateTime);
            }

            public DateTime LogReceived(string message)
            {
                return default(DateTime);
            }

            public DateTime LogSent(string message)
            {
                return default(DateTime);
            }

            public DateTime LogState(string message)
            {
                return default(DateTime);
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
            var logger = new Logger(source);
            m_loggers.Add(logger);
            return logger as IComponentLogging;
        }

        public IEnumerable<IComponentLogging> ListLoggers()
        {
            foreach (var logger in m_loggers) { yield return logger; }
        }
    }
}
