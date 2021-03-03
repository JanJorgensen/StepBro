using ActiproSoftware.Windows;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using StepBro.Core.Data;
using StepBro.Core.Utils;
using StepBro.Core.Logging;
using System.ComponentModel;
using System.Windows;
using System.IO;

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
                this.Timestamp = entry.Timestamp.ToSecondsTimestamp(zero);
                string type;
                switch (entry.EntryType)
                {
                    case LogEntry.Type.Detail:
                        type = "detail";
                        break;
                    case LogEntry.Type.Async:
                        type = "Async";
                        break;
                    case LogEntry.Type.TaskEntry:
                        type = "TaskEntry";
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
                this.LeftMargin = new Thickness(20.0 * entry.IndentLevel, 0, 0, 0);
                //this.Indent = entry.IndentLevel * 20;
            }

            public string Timestamp { get; set; }
            public string Type { get; set; }
            //public int Indent { get; set; }
            public Thickness LeftMargin { get; set; }
            public string Message { get; set; }
        }

        private static TimeSpan Time2ms = TimeSpan.FromTicks(TimeSpan.TicksPerMillisecond * 2);
        private static TimeSpan Time100ms = TimeSpan.FromTicks(TimeSpan.TicksPerMillisecond * 100);
        private static TimeSpan Time1s = TimeSpan.FromTicks(TimeSpan.TicksPerMillisecond * 1000);
        private static TimeSpan Time10s = TimeSpan.FromTicks(TimeSpan.TicksPerMillisecond * 10000);
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
                                    break;
                                }
                            }
                            LogEntries.Add(new LogEntryViewModel(next, m_zeroTime));
                            m_lastHandled = next;
                        }
                        this.EntriesAdded?.Invoke(this, EventArgs.Empty);
                    }, null);
                }
            }
        }

        public event EventHandler EntriesAdded;

        public void ClearLog()
        {

        }

        public void SaveAsClearText(string filepath)
        {

        }

        public void SaveAsHtml(string filepath)
        {
            LogEntry first = m_firstSeen;
            LogEntry last = m_lastHandled;

            using (StreamWriter file = File.CreateText(filepath))
            {
                WriteHtmlHeader(file);

                LogEntry entry = first;
                DateTime previous = DateTime.MinValue;
                var zero = entry.Timestamp;
                while (entry != null)
                {
                    if (WriteHtmlEntry(file, zero, previous, entry))
                    {
                        previous = entry.Timestamp;
                    }

                    if (Object.Equals(entry, last)) break;  // Stop if this was the last entry (ignore any entries added after that).
                    entry = entry.Next;
                }
            }
        }

        public void WriteHtmlHeader(StreamWriter file)
        {
            file.WriteLine("<style type=\"text/css\">");
            file.WriteLine("#line { width:10160px; margin:0 auto; }");
            file.WriteLine("#xq1 { float:left; width:80px; }");
            file.WriteLine("#xq2 { float:left; width:80px; }");
            file.WriteLine("#xq3_0 { float:left; width:10000px; }");
            file.WriteLine("#xq3_i1 { float:left; width:30px; }");
            file.WriteLine("#xq3_1 { float:left; width:9970px; }");
            file.WriteLine("#xq3_i2 { float:left; width:60px; }");
            file.WriteLine("#xq3_2 { float:left; width:9940px; }");
            file.WriteLine("#xq3_i3 { float:left; width:90px; }");
            file.WriteLine("#xq3_3 { float:left; width:9910px; }");
            file.WriteLine("#xq3_i4 { float:left; width:120px; }");
            file.WriteLine("#xq3_4 { float:left; width:9880px; }");
            file.WriteLine("#xq3_i5 { float:left; width:150px; }");
            file.WriteLine("#xq3_5 { float:left; width:9850px; }");
            file.WriteLine("</style>");
        }

        public bool WriteHtmlEntry(StreamWriter file, DateTime zero, DateTime previous, LogEntry entry)
        {
            var t = entry.Timestamp;
            var timestamp = t.ToSecondsTimestamp(zero);
            string type = "-";
            string timestampExtra = "";
            string typeExtra = "";
            string messageExtra = "";
            switch (entry.EntryType)
            {
                case LogEntry.Type.Normal:
                    break;
                case LogEntry.Type.Pre:
                    break;
                case LogEntry.Type.Post:
                    break;
                case LogEntry.Type.TaskEntry:
                    type = "Task";
                    break;
                case LogEntry.Type.Detail:
                    messageExtra = " style=\"color:#B6B6B4\"";
                    break;
                case LogEntry.Type.Async:
                    messageExtra = " style=\"color:#C45AEC\"";
                    break;
                case LogEntry.Type.Error:
                    type = "ERROR";
                    typeExtra = messageExtra = " style=\"color: red;\"";
                    break;
                case LogEntry.Type.UserAction:
                    type = "USER";
                    typeExtra = messageExtra = " style=\"color: blue;\"";
                    break;
                case LogEntry.Type.System:
                    type = "SYSTEM";
                    typeExtra = messageExtra = " style=\"color: blue;\"";
                    break;
                default:
                    break;
            }

            string message = entry.Text;
            if (entry.Location != null)
            {
                if (entry.Text != null)
                {
                    message = entry.Location + " - " + entry.Text;
                }
                else
                {
                    message = entry.Location;
                }
            }

            if (String.IsNullOrEmpty(message)) return false;

            var dt = t - previous;

            if (dt <= Time2ms) timestampExtra = " style=\"color:#E5E4E2\"";
            else if (dt < Time100ms) timestampExtra = " style=\"color:#B6B6B4\"";
            else if (dt > Time10s) timestampExtra = " style=\"color:red;\"";
            else if (dt > Time1s) timestampExtra = " style=\"color:orange;\"";

            Microsoft.AspNetCore.Html.HtmlString ht = new Microsoft.AspNetCore.Html.HtmlString(type);
            Microsoft.AspNetCore.Html.HtmlString hm = new Microsoft.AspNetCore.Html.HtmlString(message);
            type = ht.ToString();
            message = hm.ToString();
            string indent = (entry.IndentLevel > 0) ? $"<div id=\"xq3_i{entry.IndentLevel}\">-</div>" : "";
            file.WriteLine($"<div id=\"line\"><div id=\"xq1\"{timestampExtra}>{timestamp}</div><div id=\"xq2\"{typeExtra}>{type}</div>{indent}<div id=\"xq3_{entry.IndentLevel}\"{messageExtra}>{message}</div></div>");

            return true;
        }
    }
}
