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
        public static readonly string STEPBRO_FOLDER_CONFIG_FILE = "StepBro.cfg";
        public static readonly string STEPBRO_FOLDER_CONFIG_FILE_ROOT = "RootFolderHere";
        public static readonly string STEPBRO_FOLDER_CONFIG_FILE_LIBS = "libs";
        public static readonly string FOLDER_CONFIG_FILE_ROOT_FLAG = "RootFolderHere";
        public static readonly string PLUGINS_LIST_FILE = "StepBro Plugins.cfg";
        public static readonly string CURRENT_FILE_FOLDER_SHORTCUT = "this";
        public static readonly string TOP_FILE_FOLDER_SHORTCUT = "TopFolder";
    }
}
