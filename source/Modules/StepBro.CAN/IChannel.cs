using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StepBro.Core.Api;
using StepBro.Core.Execution;

namespace StepBro.CAN
{
    [Public]
    public enum ChannelMode
    {
        Standard,
        Extended,
        FD,
        Mixed
    }

    [Public]
    public interface IChannel : IDisposable
    {
        IAdapter Adapter { get; }
        void Setup([Implicit] ICallContext context, Baudrate baudrate, ChannelMode mode);
        //CANUSB.CANUSB_ACCEPTANCE_CODE_ALL,
        //CANUSB.CANUSB_ACCEPTANCE_MASK_ALL,
        //CANUSB.CANUSB_FLAG_TIMESTAMP);

        bool IsOpen { get; }
        bool Open([Implicit] ICallContext context);
        bool Close([Implicit] ICallContext context);
        void Flush();
        void ResetErrors();
        string ErrorStatus { get; }
        string LastOperationStatus { get; }
        bool Send(IMessage message);
        IMessage Send(uint id, byte[] data);
        IMessage Send(MessageType type, uint id, byte[] data);
        IMessage GetReceived([Implicit] ICallContext context);
        IMessage CreateMessage(uint id, byte[] data);
        IMessage CreateMessage(MessageType type, uint id, byte[] data);
        ReceiveQueue CreateReceiveQueue([Implicit] ICallContext context, string name, Predicate<IMessage> filter);
        ReceivedStatus CreateStatusReceiver([Implicit] ICallContext context, string name, Predicate<IMessage> filter);
        MessageTransmitter SetupPeriodicTransmit(string name, MessageType type, uint ID, byte[] data, TimeSpan time, bool startNow);
    }
}
