using Microsoft.Build.Framework;
using StepBro.Core.Addons;
using StepBro.Core.Data;
using StepBro.Core.General;
using StepBro.Core.ScriptData;
using StepBro.Core.Tasks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StepBro.Core.Logging
{
    internal class LogFileCreationManager : ServiceBase<ILogFileCreationManager, LogFileCreationManager>, ILogFileCreationManager
    {
        private class FileCreator : ILogFileCreator
        {
            private LogFileCreationManager m_parent;
            private IOutputFormatter m_formatter;
            private bool m_includePast = true;
            private bool m_stop = false;
            private bool m_awaitSilence = true;
            private System.Threading.Thread m_thread = null;
            private DateTime m_zeroTime = DateTime.Now;

            internal FileCreator(LogFileCreationManager parent, IOutputFormatter formatter)
            {
                m_parent = parent;
                m_formatter = formatter;
            }

            internal void Start(bool includePast)
            {
                m_includePast = includePast;

                m_thread = new System.Threading.Thread(this.LoggerThread);
                m_thread.Priority = ThreadPriority.BelowNormal;
                m_thread.Start();
            }

            private void LoggerThread()
            {
                LogEntry entry = StepBro.Core.Main.Logger.GetOldestEntry();
                if (!m_includePast)
                {
                    entry = StepBro.Core.Main.Logger.GetNewestEntry();
                    // Start at the beginning of a scope.
                    while (entry.Parent != null && entry.Parent.Parent != null)
                    {
                        entry = entry.Parent;
                    }
                }
                m_zeroTime = entry.Timestamp;

                while (entry != null)
                {
                    m_formatter.WriteLogEntry(entry, m_zeroTime);
                    while (true)
                    {
                        if (m_stop)
                        {
                            if (m_awaitSilence && entry.Next != null)
                            {
                                entry = entry.Next;
                            }
                            else
                            {
                                entry = null;   // Log no more!
                            }
                            break;
                        }
                        else
                        {
                            if (entry.Next != null)
                            {
                                entry = entry.Next;
                                break;
                            }
                            else
                            {
                                Thread.Sleep(100);  // Wait for some action.
                            }
                        }
                    }
                }
            }

            public void CloseFile(bool awaitLogSilence = true)
            {
                if (m_formatter == null) return;
                m_awaitSilence = awaitLogSilence;
                m_stop = true;
                m_thread.Join();

                m_formatter.Dispose();
                m_formatter = null;
                m_parent.Remove(this);
            }

            public void Dispose()
            {
                this.CloseFile(true);
            }
        }

        List<FileCreator> m_fileCreators = new List<FileCreator>();
        public LogFileCreationManager(out IService serviceAccess) :
            base("LogFileCreationManager", out serviceAccess, typeof(ILogger))
        {
        }

        public ILogFileCreator AddLogFileCreator(IOutputFormatter formatter, bool includePast)
        {
            if (formatter == null) { throw new ArgumentNullException(nameof(formatter)); }

            var creator = new FileCreator(this, formatter);
            m_fileCreators.Add(creator);
            creator.Start(includePast);
            return creator;
        }

        protected override void Stop(ServiceManager manager, ITaskContext context)
        {
            var creators = m_fileCreators.ToArray();
            foreach (var creator in creators)
            {
                creator.CloseFile(true);
            }
        }

        private void Remove(FileCreator creator)
        {
            m_fileCreators.Remove(creator);
        }
    }
}
