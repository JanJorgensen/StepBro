using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Logging
{
    public interface ISpecialLoggerSource
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

    public interface ISpecialLoggerService
    {
        ISpecialLogging CreateSpecialLogger(ISpecialLoggerSource source);
        IEnumerable<ISpecialLogging> ListLoggers();
    }

    public interface ISpecialLogging : IDisposable
    {
        bool Enabled { get; set; }
        event EventHandler EnabledChanged;
        string Name { get; }
        DateTime LogState(string message);
        DateTime LogSent(string message);
        DateTime LogReceived(string message);
        DateTime LogError(string text);
    }

    internal class SpecialLoggerService : ServiceBase<ISpecialLoggerService, SpecialLoggerService>, ISpecialLoggerService
    {
        private class Logger : ISpecialLogging
        {
            ISpecialLoggerSource m_source;

            public Logger(ISpecialLoggerSource source)
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

        public SpecialLoggerService(out IService serviceAccess, StepBro.Core.Logging.Logger logger) :
            base("SpecialLoggerService", out serviceAccess)
        {
            m_mainLogger = logger;
        }

        public ISpecialLogging CreateSpecialLogger(ISpecialLoggerSource source)
        {
            var logger = new Logger(source);
            m_loggers.Add(logger);
            return logger as ISpecialLogging;
        }

        public IEnumerable<ISpecialLogging> ListLoggers()
        {
            foreach (var logger in m_loggers) { yield return logger; }
        }
    }
}
