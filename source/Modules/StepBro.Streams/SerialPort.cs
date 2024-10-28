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
using static StepBro.Core.Execution.ActionQueue;
using System.Collections.Generic;

namespace StepBro.Streams
{
    public enum Handshake
    {
        None = 0,
        XOnXOff = 1,
        RequestToSend = 2,
        RequestToSendXOnXOff = 3
    }
    
    public enum Parity
    {
        None = 0,
        Odd = 1,
        Even = 2,
        Mark = 3,
        Space = 4
    }
    
    public enum StopBits
    {
        None = 0,
        One = 1,
        Two = 2,
        OnePointFive = 3
    }

    [Public]
    public class SerialPort : Stream, INameable
    {
        public delegate void OpenPortFailureExplorer(ICallContext context, Exception ex);
        private static OpenPortFailureExplorer s_OpenPortFailureExplorer = null;

        private System.IO.Ports.SerialPort m_port;
        private long m_dataReceivedCounter = 0L;
        private IComponentLogging m_componentLogging = null;
        private bool m_reportOverrun = false;

        public SerialPort([ObjectName] string objectName = "<a SerialPort>") : base(objectName)
        {
            m_port = new System.IO.Ports.SerialPort();
            m_port.ErrorReceived += this.Port_ErrorReceived;
            m_port.Encoding = Encoding.Latin1;
            m_port.ReadTimeout = 500;
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
                this.Logger.LogError(e.EventType.ToString());
                //m_componentLogging.LogError(e.EventType.ToString());
            }
        }

        protected override void DoDispose(bool disposing)
        {
            base.DoDispose(disposing);
            m_port.Close();
            m_port.Dispose();
            m_port = null;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
        private static List<Tuple<string, string, string, string>> ListAvailablePortsInternal()
        {
            var list = new List<Tuple<string, string, string, string>>();
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

                    list.Add(new Tuple<string, string, string, string>(portName, caption, manufacturer, deviceIDParts[deviceIDParts.Length - 1]));
                }
            }
            return list;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
        public static System.Collections.Generic.List<string> ListAvailablePorts()
        {
            return ListAvailablePortsInternal().Select(p => p.Item1).ToList();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
        protected override bool DoOpen(StepBro.Core.Execution.ICallContext context)
        {
            try
            {
                m_port.Open();
                if (m_componentLogging == null)
                {
                    m_componentLogging = Core.Main.GetService<IComponentLoggerService>().CreateComponentLogger(this);
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
                    foreach (var port in ListAvailablePortsInternal())
                    {
                        context.Logger.Log($"Available port: {port.Item1}, \"{port.Item2}\" by {port.Item3}. ID: {port.Item4}");
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
                if (this.CommLogging)
                {
                    this.Logger.LogCommSent(text.EscapeString());
                }
                else if (context != null && context.LoggingEnabled)
                {
                    var s = text.EscapeString();
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
                //if (m_specialLogging != null && m_specialLogging.Enabled)
                //{
                //    m_specialLogging.LogSent(text);
                //}
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
            //m_asyncLogger?.LogCommReceived(line);
            //if (m_specialLogging != null && m_specialLogging.Enabled)
            //{
            //    m_specialLogging.LogReceived(line);
            //}
            return line;
        }

        public override void DiscardInBuffer()
        {
            m_port.DiscardInBuffer();
        }
    }
}