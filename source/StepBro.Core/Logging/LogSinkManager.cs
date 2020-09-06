using System;
using System.Collections.Generic;
using StepBro.Core.Tasks;

namespace StepBro.Core.Logging
{
    public interface ILogSinkManager : ILogSink
    {
        void Add(ILogSink sink);
        void Remove(ILogSink sink);
    }

    public class LogSinkManager : ServiceBase<ILogSinkManager, LogSinkManager>, ILogSinkManager, ILogSink
    {
        private ILogSink[] m_sinks = new ILogSink[] { };
        private bool m_started;
        private readonly object m_sync = new object();
        private ServiceManager m_manager = null;

        public LogSinkManager(out IService serviceAccess, bool started = true) : base("LogSinkManager", out serviceAccess, typeof(IMainLogger))
        {
            m_started = started;
        }

        protected override void Start(ServiceManager manager, ITaskContext context)
        {
            m_manager = manager;
            manager.Get<IMainLogger>().Logger.SetSink(this);
        }

        protected override void Stop(ServiceManager manager, ITaskContext context)
        {
            manager.Get<IMainLogger>().Logger.RemoveSink(this);
            ((ILogSink)this).Stop();
        }

        public void Add(ILogSink sink)
        {
            lock (m_sync)
            {
                var logger = m_manager.Get<IMainLogger>();
                logger.Logger.RootLogger.LogSystem("LogSinkManager", $"Adding sink '{sink.GetType().FullName}'.");
                List<ILogSink> sinks = new List<ILogSink>(m_sinks);
                sinks.Add(sink);
                m_sinks = sinks.ToArray();
                if (m_started)
                {
                    sink.Start(logger.Logger.GetOldestEntry());
                }
            }
        }

        public void Remove(ILogSink sink)
        {
            lock (m_sync)
            {
                int i = Array.IndexOf(m_sinks, sink);
                if (i >= 0)
                {
                    m_manager.Get<IMainLogger>().Logger.RootLogger.LogSystem("LogSinkManager", $"Removing sink '{sink.GetType().FullName}'.");
                    List<ILogSink> sinks = new List<ILogSink>(m_sinks);
                    if (m_started)
                    {
                        sinks[i].Stop();
                    }
                    sinks.RemoveAt(i);
                    m_sinks = sinks.ToArray();
                }
                else
                {
                    m_manager.Get<IMainLogger>().Logger.RootLogger.LogSystem("LogSinkManager", $"ERROR. Removing sink '{sink.GetType().FullName}', but it is not registered.");
                }
            }
        }

        void ILogSink.Add(LogEntry entry)
        {
            lock (m_sync)
            {
                if (m_started)
                {
                    foreach (var s in m_sinks)
                    {
                        s.Add(entry);
                    }
                }
            }
        }

        void ILogSink.Start(LogEntry entry)
        {
            lock (m_sync)
            {
                if (m_started) throw new Exception("Oops!");
                foreach (var s in m_sinks)
                {
                    s.Start(entry);
                }
                m_started = true;
            }
        }

        void ILogSink.Stop()
        {
            lock (m_sync)
            {
                if (m_started)
                {
                    foreach (var s in m_sinks)
                    {
                        s.Stop();
                    }
                    m_started = false;
                }
            }
        }
    }
}
