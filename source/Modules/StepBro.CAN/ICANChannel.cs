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
    public enum CANChannelMode
    {
        Standard,
        Extended,
        FD,
        Mixed
    }

    [Public]
    public interface ICANChannel
    {
        ICANAdapter Adapter { get; }
        void Setup([Implicit] ICallContext context, CANBaudrate baudrate, CANChannelMode mode, TimeSpan transmitTimeout);
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
        bool Send(ICANMessage message);
        ICANMessage Send(uint id, byte[] data);
        ICANMessage Send(CANMessageType type, uint id, byte[] data);
        ICANMessage GetReceived();
        ICANMessage CreateMessage(uint id, byte[] data);
        ICANMessage CreateMessage(CANMessageType type, uint id, byte[] data);
    }
}
