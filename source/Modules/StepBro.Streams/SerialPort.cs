using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Execution;
using StepBro.Core.Logging;
using System;

namespace StepBro.Streams
{
    //
    // Summary:
    //     Specifies the control protocol used in establishing a serial port communication
    //     for a System.IO.Ports.SerialPort object.
    public enum Handshake
    {
        //
        // Summary:
        //     No control is used for the handshake.
        None = 0,
        //
        // Summary:
        //     The XON/XOFF software control protocol is used. The XOFF control is sent to stop
        //     the transmission of data. The XON control is sent to resume the transmission.
        //     These software controls are used instead of Request to Send (RTS) and Clear to
        //     Send (CTS) hardware controls.
        XOnXOff = 1,
        //
        // Summary:
        //     Request-to-Send (RTS) hardware flow control is used. RTS signals that data is
        //     available for transmission. If the input buffer becomes full, the RTS line will
        //     be set to false. The RTS line will be set to true when more room becomes available
        //     in the input buffer.
        RequestToSend = 2,
        //
        // Summary:
        //     Both the Request-to-Send (RTS) hardware control and the XON/XOFF software controls
        //     are used.
        RequestToSendXOnXOff = 3
    }
    //
    // Summary:
    //     Specifies the parity bit for a System.IO.Ports.SerialPort object.
    public enum Parity
    {
        //
        // Summary:
        //     No parity check occurs.
        None = 0,
        //
        // Summary:
        //     Sets the parity bit so that the count of bits set is an odd number.
        Odd = 1,
        //
        // Summary:
        //     Sets the parity bit so that the count of bits set is an even number.
        Even = 2,
        //
        // Summary:
        //     Leaves the parity bit set to 1.
        Mark = 3,
        //
        // Summary:
        //     Leaves the parity bit set to 0.
        Space = 4
    }
    //
    // Summary:
    //     Specifies the number of stop bits used on the System.IO.Ports.SerialPort object.
    public enum StopBits
    {
        //
        // Summary:
        //     No stop bits are used. This value is not supported by the System.IO.Ports.SerialPort.StopBits
        //     property.
        None = 0,
        //
        // Summary:
        //     One stop bit is used.
        One = 1,
        //
        // Summary:
        //     Two stop bits are used.
        Two = 2,
        //
        // Summary:
        //     1.5 stop bits are used.
        OnePointFive = 3
    }

    [Public]
    public class SerialPort : Stream
    {
        private System.IO.Ports.SerialPort m_port;
        private long m_dataReceivedCounter = 0L;
        private ArrayFifo<byte> m_binaryFifo = null;
        private ArrayFifo<char> m_textualFifo = null;

        public SerialPort()
        {
            m_port = new System.IO.Ports.SerialPort();
            m_port.ErrorReceived += this.Port_ErrorReceived;
            m_port.DataReceived += this.Port_DataReceived;
        }

        internal System.IO.Ports.SerialPort Port { get { return m_port; } }
        public long DataReceivedCounter { get { return m_dataReceivedCounter; } }
        public string PortName { get { return m_port.PortName; } set { m_port.PortName = value; } }
        public long BaudRate { get { return (long)m_port.BaudRate; } set { m_port.BaudRate = (int)value; } }
        public long DataBits { get { return (long)m_port.DataBits; } set { m_port.DataBits = (int)value; } }
        public Handshake Handshake { get { return (Handshake)m_port.Handshake; } set { m_port.Handshake = (System.IO.Ports.Handshake)value; } }
        public Parity Parity { get { return (Parity)m_port.Parity; } set { m_port.Parity = (System.IO.Ports.Parity)value; } }
        public StopBits StopBits { get { return (StopBits)m_port.StopBits; } set { m_port.StopBits = (System.IO.Ports.StopBits)value; } }

        public override string NewLine { get { return m_port.NewLine; } set { m_port.NewLine = value; } }

        private void Port_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            m_dataReceivedCounter++;
            if (m_binaryFifo != null)
            {
                m_binaryFifo.Add((byte[] buffer, int offset, int count) =>
                {
                    return this.TickReceiveCounter(m_port.Read(buffer, offset, count));
                }, m_port.BytesToRead);
            }
            else if (m_textualFifo != null)
            {
                m_textualFifo.Add((char[] buffer, int offset, int count) =>
                {
                    return this.TickReceiveCounter(m_port.Read(buffer, offset, count));
                }, m_port.BytesToRead);
            }
        }

        private void Port_ErrorReceived(object sender, System.IO.Ports.SerialErrorReceivedEventArgs e)
        {
            Core.Main.GetService<IMainLogger>().Logger.RootLogger.LogError(this.GetType().Name, e.EventType.ToString());
        }

        protected override void DoDispose()
        {
            m_port.Close();
            m_port.Dispose();
            m_port = null;
        }

        protected override IReadBuffer<byte> CreateBinaryReadBuffer(int size)
        {
            m_binaryFifo = new ArrayFifo<byte>(size);
            return m_binaryFifo;
        }

        protected override IReadBuffer<char> CreateTextualReadBuffer(int size)
        {
            m_textualFifo = new ArrayFifo<char>(size);
            return m_textualFifo;
        }

        protected override bool DoOpen(StepBro.Core.Execution.ICallContext context)
        {
            m_port.Open();
            try
            {
                m_port.DiscardInBuffer();
            }
            catch { }
            return m_port.IsOpen;
        }

        protected override void DoClose(StepBro.Core.Execution.ICallContext context)
        {
            if (m_port.IsOpen)
            {
                m_port.Close();
            }
        }

        public override bool IsOpen { get { return m_port.IsOpen; } }

        public override void Write([Implicit] StepBro.Core.Execution.ICallContext context, string text)
        {
            if (m_port.IsOpen)
            {
                if (context != null && context.LoggingEnabled)
                {
                    var s = text.Trim(' ', '\r', '\n', '\t').EscapeString();
                    if (s.Length > 120)
                    {
                        s = "Write \"" + s.Substring(0, 120) + "\"...";
                    }
                    else
                    {
                        s = "Write \"" + s + "\"";
                    }
                    context.Logger.Log("SerialPort", s);
                }
                m_port.Write(text);
            }
            else
            {
                if (context != null) context.ReportError(description: "Write, but port is not open.");
            }
        }

        public override string ReadLine([Implicit] ICallContext context, TimeSpan timeout)
        {
            if (m_port.IsOpen)
            {
                var entry = DateTime.Now;
                var timeleft = timeout;

                if (m_textualFifo == null)
                {
                    while (m_port.BytesToRead == 0 && timeleft >= TimeSpan.Zero)
                    {
                        System.Threading.Thread.Sleep(20);
                        if (timeout != TimeSpan.Zero)
                        {
                            timeleft = timeout - (DateTime.Now - entry);
                        }
                    }
                    if (m_port.BytesToRead > 0)
                    {
                        var line = m_port.ReadLine();
                        if (context != null && context.LoggingEnabled)
                        {
                            context.Logger.Log("SerialPort", "ReadLine : " + StringUtils.ObjectToString(line));
                        }
                        return line;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    var newLineLen = m_port.NewLine.Length;
                    int newLineCharsSeen = 0;
                    var knownCount = 0;
                    while (timeleft >= TimeSpan.Zero)
                    {
                        if (m_textualFifo.AwaitNewData(knownCount, timeleft))
                        {
                            while (knownCount < m_textualFifo.Count)
                            {
                                if (m_textualFifo[knownCount] == m_port.NewLine[newLineCharsSeen])
                                {
                                    newLineCharsSeen++;
                                    if (newLineCharsSeen == newLineLen)
                                    {
                                        var line = new string(m_textualFifo.Get(0, knownCount + (1 - newLineLen), knownCount + 1));
                                        context.Logger.Log("SerialPort", "ReadLine : " + StringUtils.ObjectToString(line));
                                        return line;
                                    }
                                }
                                else
                                {
                                    newLineCharsSeen = 0;
                                }
                                knownCount++;
                            }
                            if (timeout != TimeSpan.Zero)
                            {
                                timeleft = timeout - (DateTime.Now - entry);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (context != null) context.ReportError(description: "No lines read.");
                    return null;
                }
            }
            else
            {
                if (context != null) context.ReportError(description: "ReadLine, but port is not open.");
            }
            return null;
        }


        //    public override int Read(ICallContext _context, byte[] _buffer, int _offset, int _count, bool showWaitProgress)
        //    {
        //        EnsureOpenAndNoErrors();

        //        try
        //        {
        //            TimeoutMethodDelegate<int> method = new TimeoutMethodDelegate<int>(_context, m_serialPort.ReadTimeout, String.Format("Reading {0} bytes", _count), showWaitProgress, new TimeoutMethodDelegate<int>.Delegate(delegate ()
        //            {
        //                using (AcquireNotificationLock("SerialPort.Read"))
        //                {
        //                    int ret = m_serialPort.Read(_buffer, _offset, _count);
        //                    OnDataRead(_buffer, _offset, ret);
        //                    return ret;
        //                }
        //            }));
        //            return method.Run();
        //        }
        //        catch (TimeoutException)
        //        {
        //            throw new StreamException(this, new StreamTimeoutExecutionError());
        //        }
        //        catch (InvalidOperationException e)
        //        {
        //            this.SetState("Error");
        //            m_LastEventState.SetState(e.ToString(), true);
        //            throw new StreamException(this, new StreamNotOpenExecutionError());
        //        }
        //    }

        //    public override void ClearReadErrors(ICallContext _context)
        //    {
        //        if (m_hasReceiveError)
        //        {
        //            m_hasReceiveError = false;
        //            if (m_serialPort.IsOpen)
        //            {
        //                m_serialPort.DiscardInBuffer();
        //                m_serialPort.DiscardOutBuffer();
        //            }
        //        }
        //        m_LastEventState.Enabled = false;
        //    }

        //    public override void FlushReadBuffer(ICallContext _context)
        //    {
        //        EnsureOpenAndNoErrors();

        //        try
        //        {
        //            m_serialPort.DiscardInBuffer();
        //        }
        //        catch (InvalidOperationException e)
        //        {
        //            m_LastEventState.SetState(e.ToString(), true);
        //            throw new StreamException(this, new StreamNotOpenExecutionError());
        //        }
        //    }

        //    public override void FlushWriteBuffer(ICallContext _context)
        //    {
        //        EnsureOpenAndNoErrors();

        //        try
        //        {
        //            m_serialPort.DiscardOutBuffer();
        //        }
        //        catch (InvalidOperationException e)
        //        {
        //            m_LastEventState.SetState(e.ToString(), true);
        //            throw new StreamException(this, new StreamNotOpenExecutionError());
        //        }
        //    }
    }
}