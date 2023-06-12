using System;

namespace StepBro.Core.Data
{
    public class LogLineData : ILineReaderEntry
    {
        public enum LogType
        {
            /// <summary>
            /// A line without any known category.
            /// </summary>
            Neutral,
            /// <summary>
            /// A line with error information.
            /// </summary>
            Error,
            /// <summary>
            /// A line that is sent from the current unit.
            /// </summary>
            Sent,
            /// <summary>
            /// A received line without any further categorization. 
            /// </summary>
            Received,
            /// <summary>
            /// A received line that marks the end of a block. 
            /// </summary>
            ReceivedEnd,
            /// <summary>
            /// A received line that is part of a larger block, and which is not the last one.
            /// </summary>
            ReceivedPartial,
            /// <summary>
            /// A received line that is part of an error message.
            /// </summary>
            ReceivedError,
            /// <summary>
            /// A received line that didn't come as a result of a direct request.
            /// </summary>
            ReceivedAsync,
            /// <summary>
            /// A received line that is a part of a remote execution trace.
            /// </summary>
            ReceivedTrace,
        }

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
