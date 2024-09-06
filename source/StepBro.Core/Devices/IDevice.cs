using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Devices
{
    //public enum DeviceConnectType
    //{
    //    Discoverable,
    //    Createable
    //}

    public interface IDevice
    {
        IDriver Driver { get; }
        string Identification { get; }
        string DeviceType { get; }

        bool IsDiscovered { get; }
        /// <summary>
        /// Whether a connection has been established to the device.
        /// </summary>
        /// <remarks>
        /// When a device is connected, it usually knows about the connection. 
        /// The device might still need to be 'opened', to be reserved and to enter an 'active' state. 
        /// </remarks>
        bool IsConnected { get; }
        bool IsCreated { get; }

        //bool HasSubChannels { get; }
    }
}
