using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Logging;
using System;

namespace StepBro.Streams
{
    [Public]
    public abstract class Stream : IDisposable, ISpecialLoggerSource
    {
        private bool m_isDisposed = false;
        private string m_objectName;
        private bool m_specialLoggerEnabled = false;

        public Stream([ObjectName] string objectName = "<a Stream>")
        {
            m_objectName = objectName;
        }

        public void Dispose()
        {
            if (!m_isDisposed)
            {
                try
                {
                    this.DoDispose();
                }
                finally
                {
                    m_isDisposed = true;
                }
            }
        }

        protected virtual void DoDispose() { }

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
                if (context != null && context.LoggingEnabled) context.Logger.Log($"Open ({this.GetTargetIdentification()})");
                try
                {
                    var result = this.DoOpen(context);

                    if (this.IsOpen != wasOpen)
                    {
                        this.IsOpenChanged?.Invoke(this, EventArgs.Empty);
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
            if (this.IsOpen != wasOpen)
            {
                this.IsOpenChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public abstract void Write([Implicit] StepBro.Core.Execution.ICallContext context, string text);
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

        #region SpecialLogger

        string ISpecialLoggerSource.Name { get { return this.Name; } }

        bool ISpecialLoggerSource.Enabled { get { return m_specialLoggerEnabled; } }

        bool ISpecialLoggerSource.SetEnabled(bool value)
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
