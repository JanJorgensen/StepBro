using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StepBro.Core.Api;

namespace StepBro.CAN
{
    [Public]
    public enum CANBaudrate
    {
        Unsupported,
        BR5K,
        BR10K,
        BR20K,
        BR33K,
        BR47K,
        BR50K,
        BR83K,
        BR95K,
        BR100K,
        BR125K,
        BR250K,
        BR500K,
        BR800K,
        BR1000K
    }
}
