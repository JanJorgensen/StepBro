﻿using System;
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
        public static readonly string STATION_PROPERTIES_CONFIG_VARIABLE = "config";
        public static readonly string VARIABLE_DEVICE_REFERENCE = "device";
        public static readonly string STEPBRO_FOLDER_CONFIG_FILE = "StepBro.cfg";
        public static readonly string STEPBRO_FOLDER_CONFIG_FILE_ROOT = "RootFolderHere";
        public static readonly string STEPBRO_FOLDER_CONFIG_FILE_LIBS = "libs";
        public static readonly string STEPBRO_FOLDER_CONFIG_FILE_SHORTCUT = "Shortcut";
        public static readonly string FOLDER_CONFIG_FILE_ROOT_FLAG = "RootFolderHere";
        public static readonly string PLUGINS_LIST_FILE = "StepBro Plugins.cfg";
        public static readonly string CURRENT_FILE_FOLDER_SHORTCUT = "this";
        public static readonly string TOP_FILE_FOLDER_SHORTCUT = "TopFolder";
        public static readonly string STEPBRO_OUTPUT_FOLDER_SHORTCUT = "STEPBRO_OUTPUT_FOLDER";
        public static readonly string STEPBRO_FILE_EXTENSION = ".sbs";
        public static readonly string STEPBRO_PERSISTANCE_LOG_FILE_EXTENSION = ".sbl";
        public static readonly string STEPBRO_SCRIPT_EXECUTION_LOG_LOCATION = "Script Execution";
        public static readonly string STEPBRO_DOCCOMMENT_SUMMARY = "Summary";
        public static readonly string STEPBRO_DOCCOMMENT_REMARKS = "Remarks";
        public static readonly string STEPBRO_DOCCOMMENT_PARAM = "Param";
    }
}
