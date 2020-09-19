using System;

namespace StepBro.Core.Data
{
    public class LogLineData
    {
        public enum LogType
        {
            Neutral,
            Sent,
            ReceivedEnd,
            ReceivedPartial,
            ReceivedError,
            ReceivedAsync,
            ReceivedTrace
        }

        public LogLineData Previous { get; private set; }
        public LogLineData Next { get; private set; }
        public LogType Type { get; private set; }
        public uint ID { get; private set; }
        public DateTime Timestamp { get; private set; }
        public string LineText { get; private set; }

        public LogLineData(LogLineData previous, LogType type, uint id, string text)
        {
            if (previous != null) previous.Next = this;
            this.Previous = previous;
            this.Type = type;
            this.ID = id;
            this.Timestamp = DateTime.Now;
            LineText = text;
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
}
