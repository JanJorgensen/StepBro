﻿using StepBro.Core.Data;
using StepBro.Core.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StepBro.Core.Api
{
    public static class StationPropertiesHelper
    {
        public static PropertyBlock TryGetStationProperties(ServiceManager services = null)
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
                            (ae as PropertyBlockValue).IsStringOrIdentifier &&
                            String.Equals(deviceName, ((ae as PropertyBlockValue).ValueAsString()), StringComparison.InvariantCultureIgnoreCase)))
                    {
                        found = device;
                        break;
                    }
                }
            }
            //if (found != null)
            //{
            //    var foundFiltered = found.Clone();
            //    var type = found.TryGetElement(Constants.STATION_PROPERTIES_DEVICE_TYPE_ENTRY) as PropertyBlockValue;
            //    if (type != null)
            //    {
            //        foundFiltered.Tag = type.Value as string;
            //    }
            //    found = foundFiltered;
            //}
            if (found == null)
            {
                return null;
            }
            return CloneWithoutGeneralDeviceConfigEntries(found);
        }

        public static IEnumerable<PropertyBlockEntry> GetConfigValuesFromStationProperties(this PropertyBlock stationProperties)
        {
            return stationProperties.Where(
                e =>
                    e.HasTypeSpecified &&
                        (e.SpecifiedTypeName.Equals(Constants.STATION_PROPERTIES_CONFIG_VARIABLE, StringComparison.InvariantCultureIgnoreCase) ||
                        e.SpecifiedTypeName.StartsWith(Constants.STATION_PROPERTIES_CONFIG_VARIABLE + " ", StringComparison.InvariantCultureIgnoreCase)));
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
                if (generalEntries.Count(g => String.Equals(entry.Name, g, StringComparison.InvariantCultureIgnoreCase)) > 0)
                {
                    continue;
                }
                result.Add(entry.Clone());
            }
            return result;
        }

        public static PropertyBlock MergeStationPropertiesWithLocalProperties(this PropertyBlock stationProperties, PropertyBlock localProperties)
        {
            var localWithoutDeviceReference = localProperties.Clone(c => !String.Equals(c.Name, Constants.VARIABLE_DEVICE_REFERENCE, StringComparison.InvariantCultureIgnoreCase));
            return stationProperties.CloneWithoutGeneralDeviceConfigEntries().Merge(localWithoutDeviceReference);
        }
    }
}
