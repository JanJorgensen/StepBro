using StepBro.Core.Data;
using StepBro.Core.General;
using StepBro.Core.Logging;
using System;
using System.Collections.Generic;

namespace StepBro.Core.Parser.Mocks
{
    internal class ConfigurationFileManagerMock : ServiceBase<IConfigurationFileManager, ConfigurationFileManagerMock>, IConfigurationFileManager
    {
        public ConfigurationFileManagerMock(out IService serviceAccess) :
            base("ConfigurationFileManager", out serviceAccess, typeof(ILogger))
        {
        }

        public string StationPropertiesFile { get { return null; } }

        public PropertyBlock GetStationProperties()
        {
            throw new NotImplementedException();
        }

        public FolderConfiguration ReadFolderConfig(string configFile, List<Tuple<string, int, string>> errors)
        {
            return null; // Not implemented.
        }

        public FolderConfiguration GetFolderConfig(string folder)
        {
            return null;
        }

        public void ResetFolderConfigurations() { }
    }
}
