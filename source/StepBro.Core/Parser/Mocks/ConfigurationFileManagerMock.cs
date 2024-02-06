using StepBro.Core.Data;
using StepBro.Core.General;
using StepBro.Core.Logging;
using System;

namespace StepBro.Core.Parser.Mocks
{
    internal class ConfigurationFileManagerMock : ServiceBase<IConfigurationFileManager, ConfigurationFileManagerMock>, IConfigurationFileManager
    {
        public ConfigurationFileManagerMock(out IService serviceAccess) :
            base("ConfigurationFileManager", out serviceAccess, typeof(ILogger))
        {
        }

        public PropertyBlock GetStationProperties()
        {
            throw new NotImplementedException();
        }

        public FolderConfiguration ReadFolderConfig(string configFile)
        {
            throw new NotImplementedException();
        }
    }
}
