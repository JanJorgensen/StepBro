using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StepBro.Core.Data;

namespace StepBro.Core.Messaging
{
    public abstract class MessageBase
    {

        public abstract ArrayReference<byte> Encode();

        //public abstract 
    }
}
