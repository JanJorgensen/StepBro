using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StepBro.Core.Api;

namespace StepBro.CAN
{
    [Public]
    public enum MessageType
    {
        Standard,
        Extended,
        FD,
        Status,
        Special
    }

    [Public]
    public interface IMessage
    {
        MessageType Type { get; }
        uint ID { get; set; }
        byte[] Data { get; set; }
        DateTime Timestamp { get; }
    }
}
