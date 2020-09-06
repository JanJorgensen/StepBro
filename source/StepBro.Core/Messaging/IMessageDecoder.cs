using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Messaging
{
    public interface IMessageDecoder
    {
        TMsg TryDecodeMessage<TMsg>(MessageDataDecoder decoder) where TMsg : MessageBase;
    }
}
