using System.Collections.Generic;

namespace StepBro.Core.Devices
{
    public interface IDriver
    {
        string Name { get; }
        string Description { get; }
        string Version { get; }

        /// <summary>
        /// Scans for connected and discoverable devices.
        /// </summary>
        /// <returns>Returns null if succeess, or an error message if failed.</returns>
        string ScanForDevices();
        IEnumerable<IDevice> ListDevices(bool discoveredOnly = true);
        IDevice GetDevice(string name);
    }
}
