using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Execution;
using StepBro.Core.Logging;
using StepBro.Core.ScriptData;
using StepBro.Core.Tasks;
using StepBro.Streams;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static StepBro.Core.Data.LogLineData;

namespace StepBro.Streams
{
    public class TextLineCommunicator :
        AvailabilityBase,
        INameable,
        INamedObject,
        INotifyPropertyChanged,
        ITextCommandInput
    {
        protected readonly object m_sync = new object();
        private string m_name = null;
        protected Stream m_stream = null;
        private Task m_receiverTask = null;
        private bool m_stopReceiver = false;
        private ConcurrentQueue<TimestampedString> m_receiveQueue = null;

        public TextLineCommunicator([ObjectName] string objectName = "<a TextLineCommunicator>")
        {
            m_name = objectName;
            this.OnNameChanged();
        }

        protected override void DoDispose(bool disposing)
        {
            this.Close(null);
            this.Stream = null;
        }

        [ObjectName]
        public string Name
        {
            get { return m_name; }
            set
            {
                if (String.IsNullOrWhiteSpace(value)) throw new ArgumentException();
                if (m_name != null) throw new InvalidOperationException("The object is already named.");
                m_name = value;
                this.OnNameChanged();
            }
        }

        protected virtual void OnNameChanged()
        {
        }

        string INamedObject.ShortName { get { return m_name; } }
        string INamedObject.FullName { get { return m_name; } }

        public string DisplayName { get { return (m_name != null) ? m_name : this.GetType().Name; } }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string name)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion

        #region Properties

        public Stream Stream
        {
            get { return m_stream; }
            set
            {
                if (!Object.ReferenceEquals(value, m_stream))
                {
                    if (m_stream != null)
                    {
                        m_stream.IsOpenChanged -= m_stream_IsOpenChanged;
                        m_stream = null;
                    }
                    if (value != null)
                    {
                        m_stream = value;
                        m_stream.Encoding = Encoding.Latin1;
                        m_stream.NewLine = "\r";
                        m_stream.IsOpenChanged += m_stream_IsOpenChanged;
                        m_stream.ReadTimeout = 500;
                    }
                }
            }
        }

        public bool UseReceiverQueue
        {
            get
            {
                return (m_receiveQueue != null);
            }
            set
            {
                if (value)
                {
                    if (m_receiveQueue == null)
                    {
                        m_receiveQueue = new ConcurrentQueue<TimestampedString>();
                    }
                }
                else
                {
                    if (m_receiveQueue != null)
                    {
                        lock (m_sync)
                        {
                            var q = m_receiveQueue;
                            m_receiveQueue = null;
                            q.Clear();
                        }
                    }
                }
            }
        }

        #endregion

        #region IConnection

        [Obsolete]
        public bool IsConnected
        {
            get { return this.IsOpen; }
        }
        public bool IsOpen
        {
            get { return m_stream != null && m_stream.IsOpen && !m_receiverTask.IsFaulted; }
        }

        [Obsolete]
        public bool Connect([Implicit] ICallContext context)
        {
            return this.Open(context);
        }

        public bool Open([Implicit] ICallContext context)
        {
            if (m_stream.IsOpen || m_stream.Open(context))
            {
                if (m_receiverTask == null)
                {
                    m_stopReceiver = false;
                    m_receiverTask = new Task(
                        ReceiverTask,
                        TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach);
                    context.TaskManager.RegisterTask(m_receiverTask);
                    m_receiverTask.Start();
                }
                return true;
            }
            else return false;
        }

        [Obsolete]
        public bool Disconnect([Implicit] ICallContext context)
        {
            return this.Close(context);
        }

        public bool Close([Implicit] ICallContext context)
        {
            if (m_receiverTask != null && !m_receiverTask.IsCompleted)
            {
                m_stopReceiver = true;
                m_receiverTask.Wait();      // Important, to avoid having two running tasks if connecting again soon. 
            }
            if (m_receiveQueue != null)
            {
                m_receiveQueue.Clear();
            }
            if (m_stream != null)
            {
                m_stream.Close(context);
            }
            m_receiverTask = null;
            return true;
        }

        protected virtual void OnClose()
        {
        }

        public void SendDirect([Implicit] ICallContext context, string text)
        {
            if (!m_stream.IsOpen)
            {
                context.Logger.LogError("Stream is not opened.");
                return;
            }
            this.PreSend(context, text);
            this.DoSendDirect(text);
        }

        protected virtual void PreSend(ICallContext context, string text)
        {

        }

        bool ITextCommandInput.AcceptingCommands()
        {
            return this.IsStillValid && this.IsOpen;
        }

        void ITextCommandInput.ExecuteCommand(string command)
        {
            if (!m_stream.IsOpen)
            {
                return;
            }
            EnqueueCommand(command);
        }

        protected virtual void EnqueueCommand(string command)
        {
            this.DoSendDirect(command);
        }

        #endregion

        private void m_stream_IsOpenChanged(object sender, EventArgs e)
        {
            if (!m_stream.IsOpen)
            {
                this.NotifyPropertyChanged(nameof(IsOpen));
            }
        }

        private void ReceiverTask()
        {
            while (!m_stopReceiver)
            {
                this.OnReceiverThreadLoop();

                string line = null;
                try
                {
                    line = m_stream.ReadLineDirect();
                    if (line != null && line.Length > 0)
                    {
                        this.OnLineReceived(line);
                    }
                }
                catch { }
            }
        }

        protected virtual void OnLineReceived(string line)
        {
            this.OnTextLineReceived(line);  // Default is to just forward everything as a textual data line.
        }

        protected virtual void OnTextLineReceived(string line)
        {
            m_receiveQueue?.Enqueue(new TimestampedString(DateTime.UtcNow, line));
        }

        protected virtual void OnReceiverThreadLoop()
        {
        }

        public bool TryDequeue(out TimestampedString received)
        {
            if (m_receiveQueue != null)
            {
                return m_receiveQueue.TryDequeue(out received);
            }
            else
            {
                received = null;
                return false;
            }
        }

        public string TryDequeue()
        {
            if (m_receiveQueue != null)
            {
                TimestampedString received;
                if ( m_receiveQueue.TryDequeue(out received))
                {
                    return received.Data;
                }
            }
            return null;
        }

        protected void DoSendDirect(string text)
        {
            // The following line can be enabled if debugging the communication.
            //DebugLogEntry.Register(new DebugLogEntryString("Send: " + text));
            m_stream.Write(null, text + m_stream.NewLine);
        }
    }
}
