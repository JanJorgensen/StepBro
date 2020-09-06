using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StepBro.Core.Api;

namespace StepBro.CAN
{
    [Public]
    public enum CANMessageType
    {
        Standard,
        Extended,
        FD,
        Status,
        Special
    }

    [Public]
    public interface ICANMessage
    {
        CANMessageType Type { get; }
        uint ID { get; set; }
        byte[] Data { get; set; }
        DateTime Timestamp { get; }
    }
}
