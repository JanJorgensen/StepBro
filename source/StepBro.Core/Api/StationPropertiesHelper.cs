using StepBro.Core.Data;
using StepBro.Core.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Api
{
    public static class StationPropertiesHelper
    {
        public static PropertyBlock TryGetStationPropertiesDeviceData(ServiceManager services = null)
        {
            if (services == null)
            {
                services = ServiceManager.Global;
            }
            if (services == null)
            {
                throw new NullReferenceException("No ServiceManager instance");
            }

            var configManager = services.Get<IConfigurationFileManager>();
            if (configManager == null)
            {
                throw new NullReferenceException("No configuration file manager instance");
            }

            return configManager.GetStationProperties();
        }

        public static PropertyBlock TryGetDeviceFromStationProperties(this PropertyBlock stationProperties, string deviceName)
        {
            var devices = stationProperties.TryGetElement(Constants.STATION_PROPERTIES_DEVICE_GROUP) as PropertyBlock;
            if (devices == null) return null;
            var found = devices.TryGetElement(deviceName) as PropertyBlock;
            if (found == null)
            {
                foreach (var entry in devices)
                {
                    var device = entry as PropertyBlock;
                    if (device == null) continue;
                    var aliases = device.TryGetElement(Constants.STATION_PROPERTIES_DEVICE_ALIAS_ENTRY) as PropertyBlockArray;
                    if (aliases == null) continue;
                    if (aliases.Any(
                        ae =>
                            (ae.BlockEntryType == PropertyBlockEntryType.Value) &&
                            (ae as PropertyBlockValue).Value is string &&
                            String.Equals(deviceName, ((ae as PropertyBlockValue).Value as string), StringComparison.InvariantCultureIgnoreCase)))
                    {
                        found = device;
                        break;
                    }
                }
            }
            if (found != null)
            {
                var foundFiltered = new PropertyBlock(-1);
                var type = found.TryGetElement(Constants.STATION_PROPERTIES_DEVICE_TYPE_ENTRY) as PropertyBlockValue;
                if (type != null)
                {
                    foundFiltered.Tag = type.Value as string;
                }
            }
            return found;
        }

        /// <summary>
        /// Creates a clone of a PropertyBlock, where the general device entries used in the station properties file has been removed.
        /// </summary>
        /// <remarks>This function will typically be used on a device configuration entry read from the station properties file.</remarks>
        /// <param name="props">The device properties.</param>
        /// <returns>Filtered clone of the specified properties.</returns>
        public static PropertyBlock CloneWithoutGeneralDeviceConfigEntries(this PropertyBlock props)
        {
            var result = new PropertyBlock(props.Line, props.Name);
            string[] generalEntries = new string[] { Constants.STATION_PROPERTIES_DEVICE_TYPE_ENTRY, Constants.STATION_PROPERTIES_DEVICE_ALIAS_ENTRY };
            foreach (var entry in props)
            {
                if (entry.BlockEntryType == PropertyBlockEntryType.Value &&
                    generalEntries.Count(g => String.Equals(entry.Name, g, StringComparison.InvariantCultureIgnoreCase)) > 0)
                {
                    continue;
                }
                result.Add(entry.Clone());
            }
            return props;
        }

        public static PropertyBlock MergeStationPropertiesWithLocalProperties(this PropertyBlock stationProperties, PropertyBlock localProperties)
        {
            return stationProperties.CloneWithoutGeneralDeviceConfigEntries().Merge(localProperties);
        }
    }
}
