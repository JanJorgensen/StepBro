using Microsoft.Extensions.Logging;
using StepBro.Core.File;
using StepBro.Core.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Logging
{
    public static class MicrosoftExtensionLogging // : Microsoft.Extensions.Logging.ILogger
    {
        static public Microsoft.Extensions.Logging.ILogger<T> AsMicrosoftExtensionLogger<T>(this ILoggerScope logger)
        {
            return new Wrapper<T>(logger);
        }

        private class Wrapper<T> : Microsoft.Extensions.Logging.ILogger<T>
        {
            private ILoggerScope m_logger;

            public Wrapper(ILoggerScope logger)
            {
                m_logger = logger;
            }

            IDisposable Microsoft.Extensions.Logging.ILogger.BeginScope<TState>(TState state)
            {
                throw new NotImplementedException();
            }

            bool Microsoft.Extensions.Logging.ILogger.IsEnabled(LogLevel logLevel)
            {
                if (m_logger.IsDebugging) return true;
                else
                {
                    switch (logLevel)
                    {
                        case LogLevel.Trace:
                        case LogLevel.Debug:
                            return false;
                        default:
                            return true;
                    }
                }
            }

            void Microsoft.Extensions.Logging.ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                switch (logLevel)
                {
                    case LogLevel.Trace:
                    case LogLevel.Debug:
                        m_logger.LogDetail(formatter(state, exception));
                        break;
                    case LogLevel.Information:
                        m_logger.Log(formatter(state, exception));
                        break;
                    case LogLevel.Warning:
                        m_logger.Log("Warning!" + formatter(state, exception));
                        break;
                    case LogLevel.Error:
                    case LogLevel.Critical:
                        m_logger.LogError(formatter(state, exception));
                        break;
                    case LogLevel.None:
                    default:
                        break;
                }
            }
        }
    }
}
