using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Logging;
using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO;
using System.ComponentModel.Design;

namespace StepBro.Streams
{
    [Public]
    public abstract class Stream : AvailabilityBase, IComponentLoggerSource, INotifyPropertyChanged, ITextCommandInput
    {
        public delegate bool LineReceivedHandler(string line);

        protected readonly object m_sync = new object();
        private string m_objectName;
        private ILogger m_asyncLogger = null;
        private bool m_commLogging = true;
        private bool m_specialLoggerEnabled = false;
        private Task m_lineReceiverTask = null;
        private bool m_stopReceiver = false;
        private ConcurrentQueue<TimestampedString> m_lineReceiveQueue = null;
        private LineReceivedHandler m_lineReceiver = null;

        public Stream([ObjectName] string objectName = "<a Stream>")
        {
            m_objectName = objectName;
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string name)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion

        [ObjectName]
        public string Name
        {
            get { return m_objectName; }
            set
            {
                if (String.IsNullOrWhiteSpace(value)) throw new ArgumentException();
                if (m_objectName != null) throw new InvalidOperationException("The object is already named.");
                m_objectName = value;
            }
        }

        public virtual string NewLine
        {
            get; set;
        } = "\n";

        /// <summary>
        /// Indicates whether the stream contains a finite amount of data (e.g. a file).
        /// </summary>
        public virtual bool IsFiniteStream { get { return false; } }

        public System.Text.Encoding Encoding
        {
            get { return this.GetEncoding(); }
            set { this.SetEncoding(value); }
        }

        public virtual int ReadTimeout { get { return 10000; } set { } }

        protected ILogger Logger { get { return m_asyncLogger; } }

        public bool CommLogging
        {
            get { return m_commLogging; }
            set { m_commLogging = value; }
        }

        public bool UseTextLineMode
        {
            get
            {
                // 'Line Mode' is enabled is either a line receiver has been set up, or an line receive queue is created.
                return (m_lineReceiver != null || m_lineReceiveQueue != null);
            }
            set
            {
                if (value != this.UseTextLineMode)
                {
                    if (value)
                    {
                        if (m_lineReceiver == null)
                        {
                            this.SetupLineReceiveQueue();
                        }
                        if (this.IsOpen)
                        {
                            this.StartTextLineReceiverTask();
                        }
                    }
                    else
                    {
                        this.StopTextLineReceiverTask();
                        this.SetupLineReceiver(null);       // First this,
                        this.DeleteLineReceiveQueue();      // .. then this.
                    }
                }
            }
        }

        private void SetupLineReceiveQueue()
        {
            if (m_lineReceiveQueue == null)
            {
                m_lineReceiveQueue = new ConcurrentQueue<TimestampedString>();
            }
        }

        private void DeleteLineReceiveQueue()
        {
            if (m_lineReceiveQueue != null)
            {
                lock (m_sync)
                {
                    var q = m_lineReceiveQueue;
                    m_lineReceiveQueue = null;
                    q.Clear();
                }
            }
        }

        public void SetupLineReceiver(LineReceivedHandler receiver)
        {
            if (receiver != null)
            {
                m_lineReceiver = receiver;
                this.DeleteLineReceiveQueue();      // The queue is not used, then.
            }
            else
            {
                this.SetupLineReceiveQueue();
                m_lineReceiver = null;
            }
        }

        protected abstract string GetTargetIdentification();
        protected abstract void SetEncoding(System.Text.Encoding encoding);
        protected abstract System.Text.Encoding GetEncoding();

        protected abstract bool DoOpen(StepBro.Core.Execution.ICallContext context);
        protected abstract void DoClose(StepBro.Core.Execution.ICallContext context);
        public abstract bool IsOpen { get; }

        public event EventHandler IsOpenChanged;

        public bool Open([Implicit] StepBro.Core.Execution.ICallContext context)
        {
            var wasOpen = this.IsOpen;
            if (wasOpen)
            {
                if (context != null && context.LoggingEnabled) context.Logger.Log("Open (but already open)");
                // TODO: Report error and let script handle it.
                return true;
            }
            else
            {
                if (m_asyncLogger == null)
                {
                    m_asyncLogger = ((ILoggerScope)Core.Main.GetService<ILogger>()).LogEntering(LogEntry.Type.Component, this.Name, null, null);
                }
                if (context != null && context.LoggingEnabled) context.Logger.Log($"Open ({this.GetTargetIdentification()})");
                try
                {
                    var result = this.DoOpen(context);

                    if (this.IsOpen != wasOpen)
                    {
                        this.IsOpenChanged?.Invoke(this, EventArgs.Empty);
                    }

                    if (this.UseTextLineMode)
                    {
                        this.StartTextLineReceiverTask();
                    }

                    return result;
                }
                catch (Exception ex)
                {
                    if (context != null)
                    {
                        context.ReportError($"Could not open stream; {ex.Message}");
                    }
                    return false;
                }
            }
        }

        public void Close([Implicit] StepBro.Core.Execution.ICallContext context)
        {
            if (context != null && context.LoggingEnabled) context.Logger.Log("Close");
            var wasOpen = this.IsOpen;
            this.DoClose(context);

            this.StopTextLineReceiverTask();
            if (m_lineReceiveQueue != null)
            {
                m_lineReceiveQueue.Clear();
            }

            if (this.IsOpen != wasOpen)
            {
                this.IsOpenChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void StartTextLineReceiverTask()
        {
            if (m_lineReceiverTask == null)
            {
                m_stopReceiver = false;
                m_lineReceiverTask = new Task(
                    LineReceiverTask,
                    TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach);
                var taskManager = StepBro.Core.Main.GetService<StepBro.Core.Tasks.TaskManager>();
                taskManager.RegisterTask(m_lineReceiverTask);
                m_lineReceiverTask.Start();
            }
        }

        private void StopTextLineReceiverTask()
        {
            if (m_lineReceiverTask != null)
            {
                if (m_lineReceiverTask != null && !m_lineReceiverTask.IsCompleted)
                {
                    m_stopReceiver = true;
                    m_lineReceiverTask.Wait();      // Important, to avoid having two running tasks if connecting again soon. 
                }
                m_lineReceiverTask = null;
            }
        }

        public event EventHandler OnLineReceiverTaskLoop;

        private void LineReceiverTask()
        {
            while (!m_stopReceiver)
            {
                this.OnLineReceiverTaskLoop?.Invoke(this, EventArgs.Empty);

                string line = null;
                try
                {
                    line = this.ReadLineDirect();
                    if (line != null && line.Length > 0)
                    {
                        if (m_commLogging && m_asyncLogger != null)
                        {
                            m_asyncLogger.LogCommReceived(line);
                        }
                        if (m_lineReceiver == null || !m_lineReceiver(line))
                        {
                            lock (m_sync)
                            {
                                if (m_lineReceiveQueue != null)
                                {
                                    m_lineReceiveQueue.Enqueue(new TimestampedString(DateTime.UtcNow, line));
                                }
                            }
                        }
                    }
                }
                catch { }
            }
        }

        public abstract void Write([Implicit] StepBro.Core.Execution.ICallContext context, string text);

        public void WriteLine([Implicit] StepBro.Core.Execution.ICallContext context, string text)
        {
            if (m_commLogging)
            {
                if (context != null)
                {
                    if (context.LoggingEnabled)
                    {
                        context.Logger.LogCommSent(text);
                    }
                }
                else if (m_asyncLogger != null)
                {
                    m_asyncLogger.LogCommSent(text);
                }
            }
            this.Write(context, text + this.NewLine);
        }

        public abstract string ReadLineDirect();

        public string ReadLine([Implicit] StepBro.Core.Execution.ICallContext context, TimeSpan timeout)
        {
            if (this.IsOpen)
            {
                var entry = DateTime.Now;
                while (timeout != TimeSpan.MaxValue && (DateTime.Now - entry) < timeout && (context == null || context.StopRequested()))
                {
                    try
                    {
                        var line = this.ReadLineDirect();
                        if (context != null && context.LoggingEnabled)
                        {
                            context.Logger.Log("ReadLine : " + StringUtils.ObjectToString(line));
                        }
                        if (this.CommLogging && m_asyncLogger != null)
                        {
                            m_asyncLogger.LogCommReceived(line);
                        }
                        return line;
                    }
                    catch { }
                }
            }
            else
            {
                if (context != null) context.ReportError("ReadLine, but stream is not open.");
            }
            return null;
        }

        public abstract void DiscardInBuffer();

        public bool TryDequeue(out TimestampedString received)
        {
            if (m_lineReceiveQueue != null)
            {
                return m_lineReceiveQueue.TryDequeue(out received);
            }
            else
            {
                received = null;
                return false;
            }
        }

        public string TryDequeue()
        {
            if (m_lineReceiveQueue != null)
            {
                TimestampedString received;
                if (m_lineReceiveQueue.TryDequeue(out received))
                {
                    return received.Data;
                }
            }
            return null;
        }

        bool ITextCommandInput.AcceptingCommands()
        {
            return this.IsStillValid && this.IsOpen;
        }

        void ITextCommandInput.ExecuteCommand(string command)
        {
            if (!this.IsOpen)
            {
                return;
            }
            this.Write(null, command + this.NewLine);
        }

        #region SpecialLogger

        string IComponentLoggerSource.Name { get { return this.Name; } }

        bool IComponentLoggerSource.Enabled { get { return m_specialLoggerEnabled; } }

        bool IComponentLoggerSource.SetEnabled(bool value)
        {
            if (value != m_specialLoggerEnabled)
            {
                m_specialLoggerEnabled = value;
                return true;
            }
            else return true;
        }

        #endregion
    }
}
