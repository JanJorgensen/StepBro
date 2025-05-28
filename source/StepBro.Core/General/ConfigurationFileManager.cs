using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Logging;
using StepBro.Core.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StepBro.Core.General
{
    public interface IConfigurationFileManager
    {
        string StationPropertiesFile { get; }
        PropertyBlock GetStationProperties();
        FolderConfiguration GetOrReadFolderConfig(string folder, List<Tuple<string, int, string>> errors);
    }

    internal class ConfigurationFileManager : ServiceBase<IConfigurationFileManager, ConfigurationFileManager>, IConfigurationFileManager
    {
        private string m_stationPropertiesFileName = null;
        private PropertyBlock m_stationProperties = null;
        private List<FolderConfiguration> m_folderConfigs = new List<FolderConfiguration>();
        private ITextFileSystem m_fileSystem = null;

        public ConfigurationFileManager(out IService serviceAccess) :
            base("ConfigurationFileManager", out serviceAccess, typeof(ILogger))
        {
            this.AddOptionalDependency(typeof(ITextFileSystem));
        }

        protected override void Start(ServiceManager manager, ITaskContext context)
        {
            m_fileSystem = manager.Get<ITextFileSystem>();
            m_stationPropertiesFileName = System.Environment.GetEnvironmentVariable(Constants.STEPBRO_STATION_PROPERTIES);
            if (!String.IsNullOrEmpty(m_stationPropertiesFileName))
            {
                System.Diagnostics.Debug.WriteLine($"Station properties file: ({m_stationPropertiesFileName}).");
                if (m_fileSystem.FileExists(m_stationPropertiesFileName))
                {
                    try
                    {
                        m_stationProperties = m_stationPropertiesFileName.GetPropertyBlockFromFile();
#if DEBUG
                        context.Logger.LogSystem($"Successfully read the station properties file.");
#else
                        context.Logger.LogSystem($"Station properties file: {m_stationPropertiesFileName}.");
#endif
                    }
                    catch
                    {
                        context.Logger.LogError($"Error reading station properties file ({m_stationPropertiesFileName}).");
                    }
                }
                else
                {
                    context.Logger.LogError($"Station properties file ({m_stationPropertiesFileName}) was not found.");
                }
            }
            else
            {
                context.Logger.LogSystem($"Environment variable \"{Constants.STEPBRO_STATION_PROPERTIES}\" for station properties file reference is not created.");
            }
        }

        public string StationPropertiesFile { get => m_stationPropertiesFileName; }

        public PropertyBlock GetStationProperties()
        {
            return m_stationProperties;
        }

        public FolderConfiguration GetOrReadFolderConfig(string folder, List<Tuple<string, int, string>> errors)
        {
            if (String.IsNullOrEmpty(folder)) throw new ArgumentException("Argument \"folder\" is null or empty.");
            var config = m_folderConfigs.FirstOrDefault(c => c.Folder == folder);
            if (config == null)     // If not already loaded, load it now.
            {
                config = FolderConfiguration.Create(folder, errors);
                m_folderConfigs.Add(config);

                var root = Path.GetDirectoryName(folder);
                if (root != null && !config.IsSearchRoot && root != folder)
                {
                    config.ParentConfiguration = GetOrReadFolderConfig(root, errors);
                }
            }
            return config;
        }
    }
}
