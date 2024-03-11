using StepBro.Core.Api;
using StepBro.Core.Data;
using System;

namespace StepBro.Streams
{
    [Public]
    public abstract class Stream : IDisposable
    {
        private bool m_isDisposed = false;

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

        // ¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤

        //using System;
        //using System.Collections.Generic;
        //using System.Text;

        //namespace Stream.Instances
        //    {
        //        public delegate void DataDelegate(Stream _sender, byte[] _data);
        //        public delegate void DataReceivedHandler(Stream _sender, int _numBytesAvailable);
        //        public delegate void IsOpenChangedDelegate(Stream _sender, bool _isOpen);

        //        public abstract class Stream : ZapToolInstanceBase
        //        {
        //            private static Encoder s_utf8Encoder;
        //            private Encoding m_encoding;
        //            private Encoder m_encoder;
        //            private Decoder m_decoder;
        //            protected TraceLog m_traceLog;

        //            /// <summary>
        //            /// The m_notificationLock can be used by inheriting classes to make sure that the OnDataWritten and OnDataRead messages will be associated with the actual read/write, that is, so the OnDataRead response does not come before the OnDataWritten request.
        //            /// <example>
        //            /// ...
        //            /// Write ( data )
        //            /// {
        //            ///    using ( AcquireNotificationLock("locker/reason") )
        //            ///    {
        //            ///       m_member.Write ( data );
        //            ///       OnDataWritten ( data );
        //            ///    }
        //            /// }
        //            ///
        //            /// Read ( data )
        //            /// {
        //            ///    using ( AcquireNotificationLock("locker/reason") )
        //            ///    {
        //            ///       m_member.Read ( data );
        //            ///       OnDataRead ( data );
        //            ///    }
        //            /// }
        //            /// ...
        //            /// </example>

        //            /// </summary>
        //            private SingleLock m_notificationLock;

        //            protected LockHelper AcquireNotificationLock(string _locker)
        //            {
        //                return new LockHelper(_locker, m_notificationLock);
        //            }

        //            static Stream()
        //            {
        //                s_utf8Encoder = Encoding.UTF8.GetEncoder();
        //            }

        //            protected Stream(IZapTool parent, IZapToolInstance owner, string name, Type _type)
        //               : base(parent, owner, name)
        //            {
        //                m_traceLog = TraceLogger.Instance.GetTraceLog("Stream.Instances.Stream");
        //                m_notificationLock = new SingleLock(String.Format("{0} Notification", name));

        //                ChangeEncoding(Encoding.UTF8);

        //                SetIsInvocable(false);
        //            }

        //            public Encoding Encoding
        //            {
        //                get
        //                {
        //                    return m_encoding;
        //                }
        //                set
        //                {
        //                    if (m_encoding != value)
        //                    {
        //                        this.ChangeEncoding(value);
        //                    }
        //                }
        //            }

        //            public Encoder Encoder
        //            {
        //                get
        //                {
        //                    return m_encoder;
        //                }
        //            }

        //            public Decoder Decoder
        //            {
        //                get
        //                {
        //                    return m_decoder;
        //                }
        //            }

        //            protected virtual void ChangeEncoding(Encoding encoding)
        //            {
        //                using (MethodTrace trace = m_traceLog.CreateMethodTrace("ChangeEncoding", encoding))
        //                {
        //                    m_encoding = encoding;
        //                    m_encoder = m_encoding.GetEncoder();
        //                    m_decoder = m_encoding.GetDecoder();
        //                    trace.Exit();
        //                }
        //            }

        //            [Public]
        //            public abstract int WriteTimeout
        //            {
        //                get;
        //                set;
        //            }

        //            [Public]
        //            public abstract int ReadTimeout
        //            {
        //                get;
        //                set;
        //            }

        //            public virtual void Open(ICallContext context)
        //            {
        //                if (this.IsSimulated)
        //                {
        //                    context.Log("Not opening since the stream is simulated.");

        //                    if (!this.IsOpen)
        //                    {
        //                        context.Log("Simulating that the stream is opened successfully.");

        //                        SetOpenedState(true);
        //                    }
        //                }
        //                else
        //                {
        //                    OpenImplementation(context);
        //                }
        //            }

        //            protected abstract void OpenImplementation(ICallContext context);
        //            public abstract void Close(ICallContext context);

        //            protected virtual void OnOpenedChanged(bool opened)
        //            {
        //                if (this.IsOpenChanged != null)
        //                {
        //                    this.IsOpenChanged(this, opened);
        //                }
        //            }

        //            public event IsOpenChangedDelegate IsOpenChanged;

        //            private bool m_Opened = false;
        //            [Public]
        //            public bool IsOpen { get { return m_Opened; } }

        //            protected void SetOpenedState(bool isOpen)
        //            {
        //                if (isOpen != m_Opened)
        //                {
        //                    m_Opened = isOpen;
        //                    this.NotifyPropertyChanged("IsOpen");
        //                    this.OnOpenedChanged(m_Opened);
        //                    this.SetIsInvocable(m_Opened);
        //                }
        //            }

        //            [Public]
        //            public abstract int DataAvailable { get; }

        //            /// <summary>
        //            /// If SupportsDataReceived is true, the stream can emit the DataReceived signal when data is available to be read, otherwise the stream only supports manual polling of data.
        //            /// </summary>
        //            [Public]
        //            public abstract bool SupportsDataReceived { get; }

        //            public void Write(ICallContext _context, byte[] _data)
        //            {
        //                this.Write(_context, _data, 0, _data.Length);
        //            }

        //            public void Write(ICallContext _context, Sequanto.Data.DataFlatten _flatData)
        //            {
        //                this.Write(_context, _flatData.GetFlattened(true), 0, _flatData.Size);
        //            }

        //            public abstract void Write(ICallContext _context, byte[] _data, int _offset, int _count);
        //            public abstract int Read(ICallContext context, byte[] buffer, int offset, int count, bool showWaitProgress);

        //            public virtual int ReadByte(ICallContext context, bool showWaitProgress)
        //            {
        //                try
        //                {
        //                    byte[] buffer = new byte[1];
        //                    int bytesRead = this.Read(context, buffer, 0, 1, showWaitProgress);
        //                    if (bytesRead == 0)
        //                    {
        //                        return -1;
        //                    }
        //                    else
        //                    {
        //                        return (int)buffer[0];
        //                    }
        //                }
        //                catch (StreamException e)
        //                {
        //                    if (e.Error != null && e.Error is StreamTimeoutExecutionError)
        //                    {
        //                        return -1;
        //                    }
        //                    throw e;
        //                }
        //            }

        //            public virtual void Write(ICallContext _context, string _data)
        //            {
        //                byte[] encodedData = new byte[_data.Length];
        //                int charsUsed, bytesUsed;
        //                bool completed;
        //                m_encoder.Convert(_data.ToCharArray(), 0, _data.Length, encodedData, 0, encodedData.Length, true, out charsUsed, out bytesUsed, out completed);
        //                this.Write(_context, encodedData);
        //            }

        //            public virtual int Read(ICallContext _context, char[] _buffer, int _offset, int _count, bool _showWaitProgress)
        //            {
        //                char[] c = new char[1];
        //                int current = 0;
        //                byte[] byteToRead = new byte[1];

        //                StatusLevel statusLevel = null;
        //                if (_showWaitProgress)
        //                {
        //                    statusLevel = new StatusLevel(_context);
        //                    _context.ShowStepProgress(_count, String.Format("Reading {0} characters.", _count));
        //                }

        //                int lastProgressUpdate = System.Environment.TickCount;

        //                while (true)
        //                {
        //                    current = this.Read(_context, byteToRead, 0, 1, false);

        //                    if (current > 0)
        //                    {
        //                        // Otherwise we add the read byte to the decoder and checks if a new character is created by that
        //                        if (m_decoder.GetChars(byteToRead, 0, 1, _buffer, _offset, false) > 0)
        //                        {
        //                            _offset++;
        //                            // We would like to only send progress updates once every 100 ms.
        //                            if (_showWaitProgress)
        //                            {
        //                                int now = System.Environment.TickCount;
        //                                if (now - lastProgressUpdate > 100)
        //                                {
        //                                    _context.SetStepProgress(_offset, null);
        //                                    lastProgressUpdate = now;
        //                                }
        //                            }
        //                            if (_offset >= _count)
        //                                break;
        //                        }
        //                    }
        //                }

        //                if (_showWaitProgress)
        //                {
        //                    statusLevel.Dispose();
        //                }
        //                return _offset;
        //            }

        //            /// <summary>
        //            /// Read from the stream until the terminator is read, then return the read string (without the terminator).
        //            /// </summary>
        //            /// <param name="terminator">The string that signifies the end of the desired input, e.g. Environment.NewLine.</param>
        //            /// <returns></returns>
        //            public virtual string ReadUntil(ICallContext _context, string _terminator, bool _showWaitProgress)
        //            {
        //                if (String.IsNullOrEmpty(_terminator))
        //                {
        //                    _context.ReportFailureOrError(new ExecutionError(), "The terminator given is empty");
        //                    return "";
        //                }
        //                StringBuilder buffer = new StringBuilder();
        //                char[] c = new char[1];
        //                int current = 0;
        //                int cTerminator = 0;
        //                byte[] byteToRead = new byte[1];
        //                try
        //                {
        //                    bool found = false;
        //                    int start = System.Environment.TickCount;
        //                    while (!found && (System.Environment.TickCount - start) < 10 * 1000)
        //                    {
        //                        current = this.Read(_context, byteToRead, 0, 1, false);

        //                        if (current > 0)
        //                        {
        //                            try
        //                            {
        //                                // Otherwise we add the read byte to the decoder and checks if a new character is created by that
        //                                if (m_decoder.GetChars(byteToRead, 0, 1, c, 0, false) > 0)
        //                                {
        //                                    buffer.Append(c[0]);
        //                                    if (_terminator[cTerminator] == c[0])
        //                                    {
        //                                        cTerminator++;
        //                                        if (cTerminator == _terminator.Length)
        //                                        {
        //                                            found = true;
        //                                            break;
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        cTerminator = 0;
        //                                    }
        //                                }
        //                            }
        //                            catch (DecoderFallbackException)
        //                            {
        //                                m_decoder.Reset();
        //                            }
        //                        }
        //                    }
        //                    if (!found)
        //                    {
        //                        throw new StreamException(this, new StreamTimeoutExecutionError("Timeout waiting for '{0}' terminator.", _terminator));
        //                    }
        //                }
        //                catch (StreamException)
        //                {
        //                    if (_context != null)
        //                    {
        //                        _context.Log("Received error before terminator was read, read output so far follows:");
        //                        _context.Log(buffer.ToString());
        //                    }
        //                    throw;
        //                }
        //                if (_terminator.Length == cTerminator)
        //                {
        //                    buffer.Remove(buffer.Length - _terminator.Length, _terminator.Length);
        //                    return buffer.ToString();
        //                }
        //                else
        //                {
        //                    if (_context != null)
        //                    {
        //                        _context.Log("End of stream reached after reading {0} characters without finding the terminator.", buffer.Length);
        //                    }
        //                    throw new StreamException(this, new EndOfStreamExecutionError());
        //                }
        //            }

        //            public virtual int ReadChar(ICallContext _context, bool _showWaitProgress)
        //            {
        //                char[] c = new char[1];
        //                int current;
        //                byte[] byteToRead = new byte[1];

        //                current = this.Read(_context, byteToRead, 0, 1, _showWaitProgress);
        //                if (current > 0)
        //                {
        //                    // Otherwise we add the read byte to the decoder and checks if a new character is created by that
        //                    if (m_decoder.GetChars(byteToRead, 0, 1, c, 0, false) > 0)
        //                    {
        //                        return c[0];
        //                    }
        //                }
        //                return -1;
        //            }

        //            public virtual void ClearReadErrors(ICallContext _context) { }
        //            public virtual void FlushReadBuffer(ICallContext _context) { }
        //            public virtual void FlushWriteBuffer(ICallContext _context) { }

        //            public virtual event DataDelegate DataRead;
        //            public virtual event DataDelegate DataWritten;

        //            private event DataReceivedHandler m_dataReceived;
        //            public virtual event DataReceivedHandler DataReceived
        //            {
        //                add
        //                {
        //                    if (this.SupportsDataReceived)
        //                    {
        //                        m_dataReceived += value;
        //                    }
        //                    else
        //                    {
        //                        throw new StreamException(this, new OperationNotSupportedExecutionError(), "The stream does not support the DataReceivid signal.");
        //                    }
        //                }
        //                remove
        //                {
        //                    if (this.SupportsDataReceived)
        //                    {
        //                        m_dataReceived -= value;
        //                    }
        //                    else
        //                    {
        //                        throw new StreamException(this, new OperationNotSupportedExecutionError(), "The stream does not support the DataReceivid signal.");
        //                    }
        //                }
        //            }

        //            protected virtual void OnDataReceived(int _numBytesAvailable)
        //            {
        //                try
        //                {
        //                    if (m_dataReceived != null)
        //                    {
        //                        m_dataReceived(this, _numBytesAvailable);
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    Base.Core.ReportUnexpectedException(ex, "Stream.OnDataReceived(int)");
        //                }
        //            }

        //            protected virtual void OnDataReceived(string _data)
        //            {
        //                try
        //                {
        //                    if (m_dataReceived != null)
        //                    {
        //                        char[] data = _data.ToCharArray();
        //                        m_dataReceived(this, s_utf8Encoder.GetByteCount(data, 0, data.Length, true));
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    Base.Core.ReportUnexpectedException(ex, "Stream.OnDataReceived(string)");
        //                }
        //            }

        //            protected virtual void OnDataRead(byte[] _data)
        //            {
        //                try
        //                {
        //                    if (DataRead != null)
        //                    {
        //                        DataRead(this, _data);
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    Base.Core.ReportUnexpectedException(ex, "Stream.OnDataWritten(byte[])");
        //                }
        //            }

        //            protected virtual void OnDataWritten(byte[] _data)
        //            {
        //                try
        //                {
        //                    if (DataWritten != null)
        //                    {
        //                        DataWritten(this, _data);
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    Base.Core.ReportUnexpectedException(ex, "Stream.OnDataWritten(_data)");
        //                }
        //            }

        //            /// <summary>
        //            /// Wrapper for OnDataWritten such that the DataWritten byte array is only created if we really have to (someone has subscribed to the event).
        //            /// </summary>
        //            /// <param name="_data"></param>
        //            /// <param name="_offset"></param>
        //            /// <param name="_count"></param>
        //            protected void OnDataWritten(byte[] _data, int _offset, int _count)
        //            {
        //                try
        //                {
        //                    if (DataWritten != null)
        //                    {
        //                        if (_offset != 0 || _count != _data.Length)
        //                        {
        //                            byte[] data = new byte[_count];
        //                            Array.Copy(_data, _offset, data, 0, _count);
        //                            OnDataWritten(data);
        //                        }
        //                        else
        //                        {
        //                            OnDataWritten(_data);
        //                        }
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    Base.Core.ReportUnexpectedException(ex, "Stream.OnDataWritten(byte[], int, int)");
        //                }
        //            }

        //            /// <summary>
        //            /// Wrapper for OnDataWritten(byte[]) to write data in UTF-8 to a byte stream.
        //            /// </summary>
        //            /// <param name="_data"></param>
        //            protected void OnDataWritten(string _data)
        //            {
        //                try
        //                {
        //                    if (DataWritten != null)
        //                    {
        //                        char[] data = _data.ToCharArray();
        //                        byte[] output = new byte[s_utf8Encoder.GetByteCount(data, 0, data.Length, true)];
        //                        s_utf8Encoder.GetBytes(data, 0, data.Length, output, 0, true);
        //                        OnDataWritten(output);
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    Base.Core.ReportUnexpectedException(ex, "Stream.OnDataWritten(string)");
        //                }
        //            }

        //            /// <summary>
        //            /// Wrapper for OnDataRead(byte[]).
        //            /// </summary>
        //            /// <param name="_data"></param>
        //            /// <param name="_offset"></param>
        //            /// <param name="_count"></param>
        //            protected void OnDataRead(byte[] _data, int _offset, int _count)
        //            {
        //                try
        //                {
        //                    if (DataRead != null)
        //                    {
        //                        if (_offset != 0 || _count != _data.Length)
        //                        {
        //                            byte[] data = new byte[_count];
        //                            Array.Copy(_data, _offset, data, 0, _count);
        //                            OnDataRead(data);
        //                        }
        //                        else
        //                        {
        //                            OnDataRead(_data);
        //                        }
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    Base.Core.ReportUnexpectedException(ex, "Stream.OnDataRead(byte[], int, int)");
        //                }
        //            }

        //            /// <summary>
        //            /// Wrapper for OnDataRead(byte[]) which converts the characters to utf-8 bytes.
        //            /// </summary>
        //            /// <param name="_data"></param>
        //            /// <param name="_offset"></param>
        //            /// <param name="_count"></param>
        //            protected void OnDataRead(char[] _data, int _offset, int _count)
        //            {
        //                try
        //                {
        //                    if (DataRead != null)
        //                    {
        //                        byte[] output = new byte[s_utf8Encoder.GetByteCount(_data, _offset, _count, true)];
        //                        s_utf8Encoder.GetBytes(_data, _offset, _count, output, 0, true);
        //                        OnDataRead(output);
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    Base.Core.ReportUnexpectedException(ex, "Stream.OnDataRead(char[], int, int)");
        //                }
        //            }

        //            /// <summary>
        //            /// Wrapper for OnDataRead(byte[]) which converts the string to utf-8 bytes.
        //            /// </summary>
        //            /// <param name="_data"></param>
        //            protected void OnDataRead(string _data)
        //            {
        //                if (DataRead != null)
        //                {
        //                    char[] data = _data.ToCharArray();
        //                    OnDataRead(data, 0, data.Length);
        //                }
        //            }

        //            protected override void DestroyInstance()
        //            {
        //                m_notificationLock.Dispose();
        //            }
        //        }
        //    }






    }
}
