using System;

namespace StepBro.Core.Data
{
    public class LogLineData : ILineReaderEntry
    {
        public enum LogType
        {
            Neutral,
            Sent,
            ReceivedEnd,
            ReceivedPartial,
            ReceivedError,
            ReceivedAsync,
            ReceivedTrace,
        }

        public LogLineData Previous { get; private set; }
        public LogLineData Next { get; private set; }
        public LogType Type { get; private set; }
        public uint ID { get; private set; }
        public DateTime Timestamp { get; private set; }
        public string Text { get; private set; }
        string ILineReaderEntry.Text
        {
            get
            {
                return this.Text.Substring(1);
            }
        }

        public LogLineData(LogLineData previous, LogType type, uint id, string text)
        {
            this.Previous = previous;
            this.Type = type;
            this.ID = id;
            this.Timestamp = DateTime.Now;
            Text = text;
            if (previous != null) previous.Next = this;
        }
    }

    public class LogLineEventArgs : EventArgs
    {
        public LogLineData Line { get; private set; }
        public LogLineEventArgs(LogLineData line)
        {
            this.Line = line;
        }
    }

    public delegate void LogLineAddEventHandler(object sender, LogLineEventArgs args);

    public interface ILogLineParent
    {
        LogLineData FirstEntry { get; }
        event LogLineAddEventHandler LinesAdded;
    }
}
