using ActiproSoftware.Windows;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using StepBro.Core.Data;
using StepBro.Core.Utils;
using StepBro.Core.Logging;
using System.ComponentModel;

namespace StepBro.UI.Controls
{
    public class LogViewerViewModel : ObservableObjectBase, ILogSink
    {
        public class LogEntryViewModel
        {
            LogEntry m_entry;
            public LogEntryViewModel(LogEntry entry, DateTime zero)
            {
                m_entry = entry;
                this.Timestamp = entry.Timestamp.ToMinutesTimestamp(zero);
                string type;
                switch (entry.EntryType)
                {
                    case LogEntry.Type.Normal:
                        type = "";
                        break;
                    case LogEntry.Type.Pre:
                        type = "Pre";
                        break;
                    case LogEntry.Type.Post:
                        type = "Post";
                        break;
                    case LogEntry.Type.TaskEntry:
                        type = "TaskEntry";
                        break;
                    case LogEntry.Type.Detail:
                        type = "Detail";
                        break;
                    case LogEntry.Type.Error:
                        type = "Error";
                        break;
                    case LogEntry.Type.UserAction:
                        type = "UserAction";
                        break;
                    case LogEntry.Type.System:
                        type = "System";
                        break;
                    default:
                        type = "";
                        break;
                }
                this.Type = type;
                if (entry.Location != null)
                {
                    if (entry.Text != null)
                    {
                        this.Message = entry.Location + " - " + entry.Text;
                    }
                    else
                    {
                        this.Message = entry.Location;
                    }
                }
                else
                {
                    this.Message = entry.Text;
                }
            }

            public string Timestamp { get; set; }
            public string Type { get; set; }
            public string Message { get; set; }
        }

        private DateTime m_zeroTime;
        private LogEntry m_firstSeen = null;
        private LogEntry m_lastHandled = null;
        private LogEntry m_received = null;
        private object m_sync = new object();
        private bool m_updating = false;
        private ObservableCollection<LogEntryViewModel> m_entries;
        private readonly SynchronizationContext m_syncContext;

        public ObservableCollection<LogEntryViewModel> LogEntries { get { return m_entries; } }

        public LogViewerViewModel()
        {
            if (DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject())) return;

            m_entries = new ObservableCollection<LogEntryViewModel>();
            m_zeroTime = DateTime.Now;
            m_syncContext = SynchronizationContext.Current;
            var logSinkManager = StepBro.Core.Main.GetService<ILogSinkManager>();
            logSinkManager.Add(this);
        }


        public void Start(LogEntry entry)
        {
            if (m_firstSeen == null)
            {
                this.Add(entry);
            }
        }

        public void Stop()
        {
        }

        public void Add(LogEntry entry)
        {
            lock (m_sync)
            {
                if (!m_updating)
                {
                    m_updating = true;
                    m_received = entry;
                    m_syncContext.Post(o =>
                    {
                        if (m_firstSeen == null)
                        {
                            m_firstSeen = m_received;
                            m_lastHandled = m_firstSeen;
                            LogEntries.Add(new LogEntryViewModel(m_lastHandled, m_zeroTime));
                        }
                        LogEntry next;
                        while (true)
                        {
                            lock (m_sync)
                            {
                                next = m_lastHandled.Next;
                                if (next == null)
                                {
                                    m_updating = false;
                                    return;
                                }
                            }
                            LogEntries.Add(new LogEntryViewModel(next, m_zeroTime));
                            m_lastHandled = next;
                        }
                    }, null);
                }
            }
        }
    }
}
