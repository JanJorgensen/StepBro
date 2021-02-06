using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ActiproSoftware.Windows;
using StepBro.Core.Data;
using StepBro.Core.Utils;

namespace StepBro.UI.Controls
{
    internal class SimpleLogViewerViewModel : ObservableObjectBase
    {
        public class LogEntryViewModel
        {
            LogLineData m_entry;
            public LogEntryViewModel(LogLineData entry, DateTime zero)
            {
                m_entry = entry;
                this.Timestamp = entry.Timestamp.ToMinutesTimestamp(zero);
                string type;
                switch (entry.Type)
                {
                    case LogLineData.LogType.Sent:
                        type = "Sent";
                        break;
                    case LogLineData.LogType.ReceivedEnd:
                        type = "Received";
                        break;
                    case LogLineData.LogType.ReceivedPartial:
                        type = "ReceivedPartial";
                        break;
                    case LogLineData.LogType.ReceivedError:
                        type = "Error";
                        break;
                    case LogLineData.LogType.ReceivedAsync:
                        type = "Async";
                        break;
                    case LogLineData.LogType.ReceivedTrace:
                        type = "Trace";
                        break;
                    case LogLineData.LogType.Neutral:
                    default:
                        type = "";
                        break;
                }
                this.Type = type;
            }

            public string Timestamp { get; set; }
            public string Type { get; set; }
            public string Message { get; set; }
        }

        private DateTime m_zeroTime;
        private ILogLineSource m_source = null;
        private LogLineData m_firstSeen = null;
        private LogLineData m_lastHandled = null;
        private LogLineData m_received = null;
        private object m_sync = new object();
        private bool m_updating = false;
        private readonly SynchronizationContext m_syncContext;

        public ObservableCollection<LogEntryViewModel> LogEntries;

        public SimpleLogViewerViewModel()
        {
            m_zeroTime = DateTime.Now;
            m_syncContext = SynchronizationContext.Current;
        }

        public void SetSource(ILogLineSource source)
        {
            if (m_source != null)
            {
                m_source.LinesAdded -= source_LinesAdded;
            }
            m_source = source;
            if (m_source != null)
            {
                m_source.LinesAdded += source_LinesAdded;
            }
        }

        private void source_LinesAdded(object sender, LogLineEventArgs args)
        {
            lock(m_sync)
            {
                if (!m_updating)
                {
                    m_updating = true;
                    m_received = args.Line;
                    m_syncContext.Post(o =>
                    {
                        if (m_firstSeen == null)
                        {
                            m_firstSeen = m_received;
                            m_lastHandled = m_firstSeen;
                            LogEntries.Add(new LogEntryViewModel(m_lastHandled, m_zeroTime));
                        }
                        LogLineData next;
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
                            LogEntries.Add(new LogEntryViewModel(m_lastHandled, m_zeroTime));
                            m_lastHandled = next;
                        }
                    }, null);
                }
            }
        }
    }
}
