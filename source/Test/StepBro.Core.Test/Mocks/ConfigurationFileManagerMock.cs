﻿using System;
using System.Collections.Generic;
using StepBro.Core.Data;
using StepBro.Core.General;
using StepBro.Core.Logging;

namespace StepBro.Core.Test.Mocks
{
    internal class ConfigurationFileManagerMock : ServiceBase<IConfigurationFileManager, ConfigurationFileManagerMock>, IConfigurationFileManager
    {
        public ConfigurationFileManagerMock(out IService serviceAccess) :
            base("ConfigurationFileManager", out serviceAccess, typeof(ILogger))
        {
        }

        public string StationPropertiesFile { get; } = string.Empty;

        public PropertyBlock StationProperties { get; set; } = null;

        public PropertyBlock GetStationProperties()
        {
            return this.StationProperties;
        }

        public FolderConfiguration GetOrReadFolderConfig(string configFile, List<Tuple<string, int, string>> errors)
        {
            throw new NotImplementedException();
        }
    }
}
