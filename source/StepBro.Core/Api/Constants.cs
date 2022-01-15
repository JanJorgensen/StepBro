using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Api
{
    public static class Constants
    {
        /// <summary>
        /// Name of environment variable specifying the path to the station properties file.
        /// </summary>
        public static readonly string STEPBRO_STATION_PROPERTIES = "STEPBRO_STATION_PROPERTIES";
        public static readonly string STATION_PROPERTIES_DEVICE_GROUP = "devices";
        public static readonly string STATION_PROPERTIES_DEVICE_ALIAS_ENTRY = "aliases";
        public static readonly string STATION_PROPERTIES_DEVICE_TYPE_ENTRY = "devicetype";
        public static readonly string VARIABLE_DEVICE_REFERENCE = "device";
    }
}
