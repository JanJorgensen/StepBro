using Microsoft.Win32;
using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Execution;
using StepBro.Core.Logging;
using System;
using System.Linq;
using System.Text;
using System.Management;
using System.Security.AccessControl;

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
    public class SerialPort : Stream, INameable
    {
        public delegate void OpenPortFailureExplorer(ICallContext context, Exception ex);
        private static OpenPortFailureExplorer s_OpenPortFailureExplorer = null;

        private System.IO.Ports.SerialPort m_port;
        private long m_dataReceivedCounter = 0L;
        //private ILogger m_asyncLogger = null;
        private ISpecialLogging m_specialLogging = null;
        private bool m_reportOverrun = false;

        public SerialPort([ObjectName] string objectName = "<a SerialPort>") : base(objectName)
        {
            m_port = new System.IO.Ports.SerialPort();
            m_port.ErrorReceived += this.Port_ErrorReceived;
            m_port.Encoding = Encoding.Latin1;
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

        public override int ReadTimeout { get { return m_port.ReadTimeout; } set { m_port.ReadTimeout = value; } }


        protected override string GetTargetIdentification()
        {
            return String.IsNullOrEmpty(this.PortName) ? "Serial Port" : this.PortName;
        }

        protected override void SetEncoding(System.Text.Encoding encoding) { m_port.Encoding = encoding; }
        protected override System.Text.Encoding GetEncoding() { return m_port.Encoding; }

        private void Port_ErrorReceived(object sender, System.IO.Ports.SerialErrorReceivedEventArgs e)
        {
            if (m_reportOverrun || e.EventType != System.IO.Ports.SerialError.Overrun)
            {
                //m_asyncLogger.LogError(e.EventType.ToString());
                m_specialLogging.LogError(e.EventType.ToString());
            }
        }

        protected override void DoDispose()
        {
            m_port.Close();
            m_port.Dispose();
            m_port = null;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
        protected override bool DoOpen(StepBro.Core.Execution.ICallContext context)
        {
            try
            {
                m_port.Open();
                if (m_specialLogging == null)
                {
                    //m_asyncLogger = Core.Main.GetService<ILogger>().LogEntering(this.Name, null);
                    m_specialLogging = Core.Main.GetService<ISpecialLoggerService>().CreateSpecialLogger(this);
                }
            }
            catch (Exception ex)
            {
                if (s_OpenPortFailureExplorer != null)
                {
                    s_OpenPortFailureExplorer(context, ex);
                }
                if (context != null && context.LoggingEnabled)
                {
                    using (ManagementClass i_Entity = new ManagementClass("Win32_PnPEntity"))
                    {
                        foreach (ManagementObject i_Inst in i_Entity.GetInstances())
                        {
                            // Solution found at: https://stackoverflow.com/questions/2837985/getting-serial-port-information

                            Object classID = i_Inst.GetPropertyValue("ClassGuid");
                            if (classID == null || classID.ToString().ToUpper() != "{4D36E978-E325-11CE-BFC1-08002BE10318}")
                                continue; // Skip all devices except device class "PORTS"

                            string caption = i_Inst.GetPropertyValue("Caption").ToString();
                            string manufacturer = i_Inst.GetPropertyValue("Manufacturer").ToString();
                            string deviceID = i_Inst.GetPropertyValue("PnpDeviceID").ToString();
                            string regPath = "HKEY_LOCAL_MACHINE\\System\\CurrentControlSet\\Enum\\" + deviceID + "\\Device Parameters";
                            string portName = Registry.GetValue(regPath, "PortName", "").ToString();

                            int i = caption.IndexOf(" (COM");
                            if (i > 0) // remove COM port from description
                                caption = caption.Substring(0, i);
                            var deviceIDParts = deviceID.Split(new char[] { '\\' });

                            context.Logger.Log($"Available port: {portName}, \"{caption}\" by {manufacturer}. ID: {deviceIDParts[deviceIDParts.Length - 1]}");
                        }
                    }

                    // Cross platform solution.
                    //var available = System.IO.Ports.SerialPort.GetPortNames();
                    //if (available != null && available.Length > 0)
                    //{
                    //    context.Logger.Log("Available port(s): " + String.Join(", ", available.Select(s => "'" + s + "'")));
                    //}
                }
                throw;
            }
            try
            {
                m_port.DiscardInBuffer();
                m_reportOverrun = true;
            }
            catch (Exception ex)
            {
                context.ReportError("Error discarding in-buffer.", exception: ex);
            }
            return m_port.IsOpen;
        }

        protected override void DoClose(StepBro.Core.Execution.ICallContext context)
        {
            if (m_port != null && m_port.IsOpen)
            {
                m_port.Close();
                m_reportOverrun = false;
            }
        }

        public override bool IsOpen { get { return m_port != null && m_port.IsOpen; } }

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
                    context.Logger.Log(s);
                }
                m_port.Write(text);
            }
            else
            {
                if (context != null) context.ReportError("Write, but port is not open.");
            }
        }

        public override string ReadLineDirect()
        {
            var line = m_port.ReadLine();
            if (m_specialLogging != null && m_specialLogging.Enabled)
            {
                m_specialLogging.LogSent(line);
            }
            return line;
        }

        public override void DiscardInBuffer()
        {
            m_port.DiscardInBuffer();
        }
    }
}