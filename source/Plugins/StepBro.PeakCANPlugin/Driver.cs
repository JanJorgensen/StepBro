using Peak.Can.Basic;
using StepBro.Core.Api;
using StepBro.Core.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace StepBro.CAN
{
    using TPCANHandle = System.UInt16;

    [Public]
    public class PCAN : IDriver
    {
        private static List<PCANAdapter> g_adapters;

        private PCAN() { }

        static PCAN()
        {
            Driver = new PCAN();
            g_adapters = new List<PCANAdapter>();
            var ids = new string[] {
                "NONEBUS",
                "ISABUS1", "ISABUS2", "ISABUS3", "ISABUS4", "ISABUS5", "ISABUS6", "ISABUS7", "ISABUS8",
                "DNGBUS1",
                "PCIBUS1", "PCIBUS2", "PCIBUS3", "PCIBUS4", "PCIBUS5", "PCIBUS6", "PCIBUS7", "PCIBUS8", "PCIBUS9", "PCIBUS10", "PCIBUS11", "PCIBUS12", "PCIBUS13", "PCIBUS14", "PCIBUS15", "PCIBUS16",
                "USBBUS1", "USBBUS2", "USBBUS3", "USBBUS4", "USBBUS5", "USBBUS6", "USBBUS7", "USBBUS8", "USBBUS9", "USBBUS10", "USBBUS11", "USBBUS12", "USBBUS13", "USBBUS14", "USBBUS15", "USBBUS16",
                "PCCBUS1", "PCCBUS2",
                "LANBUS1", "LANBUS2", "LANBUS3", "LANBUS4", "LANBUS5", "LANBUS6", "LANBUS7", "LANBUS8", "LANBUS9", "LANBUS10", "LANBUS11", "LANBUS12", "LANBUS13", "LANBUS14", "LANBUS15", "LANBUS16" };
            foreach (var s in ids)
            {
                g_adapters.Add(new PCANAdapter(Driver, s, AdapterIdentificationToHandle(s)));
            }
        }

        [Public]
        public static PCAN Driver { get; private set; }

        [Public]
        public IAdapter GetAdapter([Implicit] ICallContext context, string identification = "")
        {
            if (context != null) context.Logger.Log("GetAdapter", identification);
            if (String.IsNullOrEmpty(identification)) return this.GetAdapter(context, "USBBUS1");
            foreach (var a in g_adapters)
            {
                if (a.Identification.Equals(identification, StringComparison.InvariantCultureIgnoreCase))
                {
                    return a;
                }
            }
            return null;
        }

        [Public]
        public IEnumerable<IAdapter> ListAdapters()
        {
            foreach (var a in g_adapters)
            {
                yield return a;
            }
        }

        internal static TPCANHandle AdapterIdentificationToHandle(string identification)
        {
            switch (identification)
            {
                case "NONEBUS": return PCANBasic.PCAN_NONEBUS;
                case "PCAN_ISABUS1": return PCANBasic.PCAN_ISABUS1;
                case "PCAN_ISABUS2": return PCANBasic.PCAN_ISABUS2;
                case "PCAN_ISABUS3": return PCANBasic.PCAN_ISABUS3;
                case "PCAN_ISABUS4": return PCANBasic.PCAN_ISABUS4;
                case "ISABUS5": return PCANBasic.PCAN_ISABUS5;
                case "ISABUS6": return PCANBasic.PCAN_ISABUS6;
                case "ISABUS7": return PCANBasic.PCAN_ISABUS7;
                case "ISABUS8": return PCANBasic.PCAN_ISABUS8;
                case "DNGBUS1": return PCANBasic.PCAN_DNGBUS1;
                case "PCIBUS1": return PCANBasic.PCAN_PCIBUS1;
                case "PCIBUS2": return PCANBasic.PCAN_PCIBUS2;
                case "PCIBUS3": return PCANBasic.PCAN_PCIBUS3;
                case "PCIBUS4": return PCANBasic.PCAN_PCIBUS4;
                case "PCIBUS5": return PCANBasic.PCAN_PCIBUS5;
                case "PCIBUS6": return PCANBasic.PCAN_PCIBUS6;
                case "PCIBUS7": return PCANBasic.PCAN_PCIBUS7;
                case "PCIBUS8": return PCANBasic.PCAN_PCIBUS8;
                case "PCIBUS9": return PCANBasic.PCAN_PCIBUS9;
                case "PCIBUS10": return PCANBasic.PCAN_PCIBUS10;
                case "PCIBUS11": return PCANBasic.PCAN_PCIBUS11;
                case "PCIBUS12": return PCANBasic.PCAN_PCIBUS12;
                case "PCIBUS13": return PCANBasic.PCAN_PCIBUS13;
                case "PCIBUS14": return PCANBasic.PCAN_PCIBUS14;
                case "PCIBUS15": return PCANBasic.PCAN_PCIBUS15;
                case "PCIBUS16": return PCANBasic.PCAN_PCIBUS16;
                case "USBBUS1": return PCANBasic.PCAN_USBBUS1;
                case "USBBUS2": return PCANBasic.PCAN_USBBUS2;
                case "USBBUS3": return PCANBasic.PCAN_USBBUS3;
                case "USBBUS4": return PCANBasic.PCAN_USBBUS4;
                case "USBBUS5": return PCANBasic.PCAN_USBBUS5;
                case "USBBUS6": return PCANBasic.PCAN_USBBUS6;
                case "USBBUS7": return PCANBasic.PCAN_USBBUS7;
                case "USBBUS8": return PCANBasic.PCAN_USBBUS8;
                case "USBBUS9": return PCANBasic.PCAN_USBBUS9;
                case "USBBUS10": return PCANBasic.PCAN_USBBUS10;
                case "USBBUS11": return PCANBasic.PCAN_USBBUS11;
                case "USBBUS12": return PCANBasic.PCAN_USBBUS12;
                case "USBBUS13": return PCANBasic.PCAN_USBBUS13;
                case "USBBUS14": return PCANBasic.PCAN_USBBUS14;
                case "USBBUS15": return PCANBasic.PCAN_USBBUS15;
                case "USBBUS16": return PCANBasic.PCAN_USBBUS16;
                case "PCCBUS1": return PCANBasic.PCAN_PCCBUS1;
                case "PCCBUS2": return PCANBasic.PCAN_PCCBUS2;
                case "LANBUS1": return PCANBasic.PCAN_LANBUS1;
                case "LANBUS2": return PCANBasic.PCAN_LANBUS2;
                case "LANBUS3": return PCANBasic.PCAN_LANBUS3;
                case "LANBUS4": return PCANBasic.PCAN_LANBUS4;
                case "LANBUS5": return PCANBasic.PCAN_LANBUS5;
                case "LANBUS6": return PCANBasic.PCAN_LANBUS6;
                case "LANBUS7": return PCANBasic.PCAN_LANBUS7;
                case "LANBUS8": return PCANBasic.PCAN_LANBUS8;
                case "LANBUS9": return PCANBasic.PCAN_LANBUS9;
                case "LANBUS10": return PCANBasic.PCAN_LANBUS10;
                case "LANBUS11": return PCANBasic.PCAN_LANBUS11;
                case "LANBUS12": return PCANBasic.PCAN_LANBUS12;
                case "LANBUS13": return PCANBasic.PCAN_LANBUS13;
                case "LANBUS14": return PCANBasic.PCAN_LANBUS14;
                case "LANBUS15": return PCANBasic.PCAN_LANBUS15;
                case "LANBUS16": return PCANBasic.PCAN_LANBUS16;
                default:
                    break;
            }
            return PCANBasic.PCAN_NONEBUS;
        }
    }

    internal class PCANAdapter : IAdapter
    {
        private readonly PCAN m_parent;
        private readonly string m_identification;
        private readonly TPCANHandle m_handle;
        private readonly PCANChannel m_onlyChannel;

        internal PCANAdapter(PCAN parent, string identification, TPCANHandle handle)
        {
            m_parent = parent;
            m_identification = identification;
            m_handle = handle;
            m_onlyChannel = new PCANChannel(this);
        }

        internal TPCANHandle Handle { get { return m_handle; } }

        public string DeviceType
        {
            get
            {
                return m_identification;
            }
        }

        public string Identification
        {
            get
            {
                return m_identification;
            }
        }

        public IDriver Driver
        {
            get
            {
                return m_parent;
            }
        }

        public int Channels
        {
            get
            {
                return 1;
            }
        }

        public IChannel GetChannel([Implicit] ICallContext context, int index)
        {
            if (context != null)
            {
                context.Logger.Log("GetChannel", index.ToString());
            }
            if (index != 0) throw new ArgumentOutOfRangeException("index");

            return m_onlyChannel;
        }
    }

    internal class PCANChannel : IChannel
    {
        private readonly PCANAdapter m_parent;
        private readonly TPCANHandle m_handle;   // Only one channel per adapter, thus also saving handle here.
        private ChannelMode m_mode;
        private Baudrate m_baudrate;
        private bool m_open = false;
        private string m_errorStatus = "";
        private string m_lastOperationStatus = "";
        private Thread m_receiver = null;
        private List<ReceiveQueue> m_receivers = null;
        private readonly ReceiveQueue m_noQueueReceived = new ReceiveQueue("default");

        internal PCANChannel(PCANAdapter adapter)
        {
            m_parent = adapter;
            m_mode = ChannelMode.Extended;
            m_handle = adapter.Handle;
            m_receivers = new List<ReceiveQueue>();
            m_receivers.Add(m_noQueueReceived);
        }

        private TPCANStatus UpdateStatusFromOperation(TPCANStatus status)
        {
            if (status == TPCANStatus.PCAN_ERROR_OK)
            {
                m_errorStatus = "";
            }
            else
            {
                m_errorStatus = status.ToString();
            }
            status &= ~TPCANStatus.PCAN_ERROR_ANYBUSERR;
            m_lastOperationStatus = status.ToString();
            return status;
        }

        public IAdapter Adapter
        {
            get
            {
                return m_parent;
            }
        }

        public void Setup([Implicit] ICallContext context, Baudrate baudrate, ChannelMode mode)
        {
            if (context != null && context.LoggingEnabled)
            {
                context.Logger.Log("Setup", $"( {baudrate}, {mode} )");
            }
            m_baudrate = baudrate;
            m_mode = mode;
            if (m_receiver == null)
            {
                m_receiver = new Thread(new ThreadStart(this.ReceiveThreadHandler));
                m_receiver.Start();
                //Core.Main.ServiceManager.Get<>
            }
        }

        private void ReceiveThreadHandler()
        {
            TPCANMsg msg;
            TPCANTimestamp timestamp;
            TPCANStatus result;
            while (m_open)
            {
                var status = PCANBasic.GetStatus(m_handle);
                if ((status & TPCANStatus.PCAN_ERROR_QRCVEMPTY) == 0) // If NOT empty
                {
                    result = PCANBasic.Read(m_handle, out msg, out timestamp);
                    if (this.UpdateStatusFromOperation(result) == TPCANStatus.PCAN_ERROR_OK)
                    {
                        this.PutReceivedInQueue(new PCANMessage(msg, timestamp));
                    }
                }
                else
                {
                    Thread.Sleep(5);
                }
            }
            m_receiver = null;
        }

        private void PutReceivedInQueue(PCANMessage message)
        {
            foreach (var rq in m_receivers)
            {
                if (rq.TryAddToQueue(message)) return;
            }
        }

        public bool IsOpen
        {
            get
            {
                return m_open;
            }
        }

        public string ErrorStatus
        {
            get
            {
                return m_errorStatus;
            }
        }

        public string LastOperationStatus
        {
            get
            {
                return m_lastOperationStatus;
            }
        }

        public bool Open([Implicit] ICallContext context)
        {
            if (!m_open)
            {
                TPCANStatus result = PCANBasic.Initialize(m_handle, ToPCANBaudrate(m_baudrate), TPCANType.PCAN_TYPE_ISA, 0, 0);
                if (this.UpdateStatusFromOperation(result) == TPCANStatus.PCAN_ERROR_OK)
                {
                    if (context != null && context.LoggingEnabled)
                    {
                        context.Logger.Log("Open", "Opened successfully");
                    }
                    m_open = true;
                    return true;
                }
                else
                {
                    if (context != null) context.Logger.LogError("PCAN", "Open failed");
                }
            }
            return false;
        }

        public bool Close([Implicit] ICallContext context)
        {
            if (m_open)
            {
                m_open = false;
                while (m_receiver != null) System.Threading.Thread.Sleep(100);
                TPCANStatus result = PCANBasic.Uninitialize(m_handle);
                if (this.UpdateStatusFromOperation(result) == TPCANStatus.PCAN_ERROR_OK)
                {
                    return true;
                }
            }
            return false;
        }

        public IMessage CreateMessage(MessageType type, uint id, byte[] data)
        {
            TPCANMessageType t = TPCANMessageType.PCAN_MESSAGE_STANDARD;
            switch (type)
            {
                case MessageType.Standard:
                    t = TPCANMessageType.PCAN_MESSAGE_STANDARD;
                    break;
                case MessageType.Extended:
                    t = TPCANMessageType.PCAN_MESSAGE_EXTENDED;
                    break;
                case MessageType.Status:
                    t = TPCANMessageType.PCAN_MESSAGE_STATUS;
                    break;
                case MessageType.Special:
                    throw new NotSupportedException("It is not possible to create a CAN message with the \"Special\" type.");
            }
            TPCANMsg msg = new TPCANMsg();
            msg.MSGTYPE = t;
            msg.ID = id;
            msg.DATA = (data != null) ? data : new byte[] { };
            msg.LEN = (data != null) ? (byte)data.Length : (byte)0;
            return new PCANMessage(msg);
        }

        public void Flush()
        {
            if (m_open)
            {
                PCANBasic.Reset(m_handle);
            }
        }

        public IMessage GetReceived([Implicit] ICallContext context)
        {
            return m_noQueueReceived.GetNext(context);
        }

        public void ResetErrors()
        {
            this.Flush();
        }

        public bool Send(IMessage message)
        {
            if (this.IsOpen)
            {
                //TPCANStatus status = PCANBasic.GetStatus(m_handle);
                //while ((status & TPCANStatus.PCAN_ERROR_XMTFULL) != 0)
                //{
                //    if ((status & ~TPCANStatus.PCAN_ERROR_ANYBUSERR) != 0)
                //    {
                //        m_errorStatus = status.ToString();
                //        return false;
                //    }
                //    status = PCANBasic.GetStatus(m_handle);
                //}
                PCANMessage pmsg = (PCANMessage)message;
                var msg = pmsg.NativeMessage;
                TPCANStatus result = PCANBasic.Write(m_handle, ref msg);
                pmsg.NativeMessage = msg;   // Save possibly updated message data.

                if (this.UpdateStatusFromOperation(result) == TPCANStatus.PCAN_ERROR_OK)
                {
                    return true;
                }
            }
            return false;
        }

        public IMessage Send(uint id, byte[] data)
        {
            IMessage msg = this.CreateMessage(id, data);
            if (this.Send(msg)) return msg;
            else return null;
        }

        public IMessage Send(MessageType type, uint id, byte[] data)
        {
            IMessage msg = this.CreateMessage(type, id, data);
            if (this.Send(msg)) return msg;
            else return null;
        }

        public IMessage CreateMessage(uint id, byte[] data)
        {
            MessageType type;
            if (m_mode == ChannelMode.Standard) type = MessageType.Standard;
            else type = MessageType.Extended;
            return this.CreateMessage(type, id, data);
        }


        internal static TPCANBaudrate ToPCANBaudrate(Baudrate baudrate)
        {
            switch (baudrate)
            {
                case Baudrate.BR5K: return TPCANBaudrate.PCAN_BAUD_5K;
                case Baudrate.BR10K: return TPCANBaudrate.PCAN_BAUD_10K;
                case Baudrate.BR20K: return TPCANBaudrate.PCAN_BAUD_20K;
                case Baudrate.BR33K: return TPCANBaudrate.PCAN_BAUD_33K;
                case Baudrate.BR47K: return TPCANBaudrate.PCAN_BAUD_47K;
                case Baudrate.BR50K: return TPCANBaudrate.PCAN_BAUD_50K;
                case Baudrate.BR83K: return TPCANBaudrate.PCAN_BAUD_83K;
                case Baudrate.BR95K: return TPCANBaudrate.PCAN_BAUD_95K;
                case Baudrate.BR100K: return TPCANBaudrate.PCAN_BAUD_100K;
                case Baudrate.BR125K: return TPCANBaudrate.PCAN_BAUD_125K;
                case Baudrate.BR250K: return TPCANBaudrate.PCAN_BAUD_250K;
                case Baudrate.BR500K: return TPCANBaudrate.PCAN_BAUD_500K;
                case Baudrate.BR800K: return TPCANBaudrate.PCAN_BAUD_800K;
                case Baudrate.BR1000K: return TPCANBaudrate.PCAN_BAUD_1M;
                default:
                    return TPCANBaudrate.PCAN_BAUD_100K;
            }
        }

        internal static Baudrate ToInterfaceBaudrate(TPCANBaudrate baudrate)
        {
            switch (baudrate)
            {
                case TPCANBaudrate.PCAN_BAUD_1M: return Baudrate.BR1000K;
                case TPCANBaudrate.PCAN_BAUD_800K: return Baudrate.BR800K;
                case TPCANBaudrate.PCAN_BAUD_500K: return Baudrate.BR500K;
                case TPCANBaudrate.PCAN_BAUD_250K: return Baudrate.BR250K;
                case TPCANBaudrate.PCAN_BAUD_125K: return Baudrate.BR125K;
                case TPCANBaudrate.PCAN_BAUD_100K: return Baudrate.BR100K;
                case TPCANBaudrate.PCAN_BAUD_95K: return Baudrate.BR95K;
                case TPCANBaudrate.PCAN_BAUD_83K: return Baudrate.BR83K;
                case TPCANBaudrate.PCAN_BAUD_50K: return Baudrate.BR50K;
                case TPCANBaudrate.PCAN_BAUD_47K: return Baudrate.BR47K;
                case TPCANBaudrate.PCAN_BAUD_33K: return Baudrate.BR33K;
                case TPCANBaudrate.PCAN_BAUD_20K: return Baudrate.BR20K;
                case TPCANBaudrate.PCAN_BAUD_10K: return Baudrate.BR10K;
                case TPCANBaudrate.PCAN_BAUD_5K: return Baudrate.BR5K;
                default:
                    return Baudrate.Unsupported;
            }
        }

        public ReceiveQueue CreateReceiveQueue([Implicit] ICallContext context, string name, Predicate<IMessage> filter)
        {
            if (m_receivers.FirstOrDefault(r => !String.IsNullOrEmpty(r.Name) && String.Equals(r.Name, name, StringComparison.InvariantCulture)) != null)
            {
                if (context != null)
                {
                    context.ReportError(description: "There are already a receive queue named \"" + name + "\".");
                }
                return null;
            }
            else
            {
                if (context != null && context.LoggingEnabled)
                {
                    context.Logger.Log("PCANChannel.CreateReceiveQueue", "Registered receive queue named \"" + name + "\".");
                }
                var q = new ReceiveQueue(name, filter);
                m_receivers.Insert(m_receivers.Count - 1, q);
                return q;
            }
        }
    }

    internal class PCANMessage : IMessage
    {
        private static long TICKSPERMICRO = TimeSpan.TicksPerMillisecond / 1000L;
        private static long TICKSPERMILLI = TimeSpan.TicksPerMillisecond;
        private TPCANMsg m_msg;
        private TimeSpan m_relativeTime;

        public PCANMessage(TPCANMsg msg)
        {
            m_msg = msg;
            this.SetData(msg.DATA, msg.LEN);
            m_relativeTime = TimeSpan.Zero;
        }

        public PCANMessage(TPCANMsg msg, TPCANTimestamp timestamp)
        {
            m_msg = msg;
            this.SetData(msg.DATA, msg.LEN);
            m_relativeTime = new TimeSpan(timestamp.micros * TICKSPERMICRO + (timestamp.millis + 0x100000000L * timestamp.millis_overflow) * TICKSPERMILLI);
        }

        public TPCANMsg NativeMessage
        {
            get { return m_msg; }
            internal set
            {
                m_msg = value;
                this.SetData(value.DATA, value.LEN);
            }
        }

        private void SetData(byte[] data, int length)
        {
            if (data == null || data.Length != 8)
            {
                int len = (data != null) ? data.Length : 0;
                var bytes = new byte[8];
                for (int i = 0; i < length; i++)
                {
                    bytes[i] = data[i];
                }
                m_msg.DATA = bytes;
                m_msg.LEN = (byte)length;
            }
            else
            {
                m_msg.DATA = data;
                m_msg.LEN = (byte)8;
            }
        }

        public byte[] Data
        {
            get
            {
                if (m_msg.LEN < 8)
                {
                    int len = (int)m_msg.LEN;
                    var ret = new byte[len];
                    Array.Copy(m_msg.DATA, ret, len);
                    return ret;
                }
                else
                {
                    return m_msg.DATA;
                }
            }

            set
            {
                this.SetData(value, (value != null) ? value.Length : 0);
            }
        }

        public uint ID
        {
            get
            {
                return m_msg.ID;
            }

            set
            {
                m_msg.ID = value;
            }
        }

        public DateTime Timestamp
        {
            get
            {
                return DateTime.MinValue;
            }
        }

        public MessageType Type
        {
            get
            {
                switch (m_msg.MSGTYPE)
                {
                    case TPCANMessageType.PCAN_MESSAGE_STANDARD:
                        return MessageType.Standard;
                    case TPCANMessageType.PCAN_MESSAGE_EXTENDED:
                        return MessageType.Extended;
                    case TPCANMessageType.PCAN_MESSAGE_STATUS:
                        return MessageType.Status;

                    case TPCANMessageType.PCAN_MESSAGE_RTR:
                    case TPCANMessageType.PCAN_MESSAGE_FD:
                    case TPCANMessageType.PCAN_MESSAGE_BRS:
                    case TPCANMessageType.PCAN_MESSAGE_ESI:
                    default:
                        return MessageType.Special;
                }
            }
        }
    }
}
