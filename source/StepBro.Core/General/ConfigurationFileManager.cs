using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.File;
using StepBro.Core.Logging;
using StepBro.Core.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StepBro.Core.Data.PropertyBlockDecoder;
using static StepBro.Core.General.ProjectUserData;

namespace StepBro.Core.General
{
    public interface IConfigurationFileManager
    {
        string StationPropertiesFile { get; }
        PropertyBlock GetStationProperties();
        FolderConfiguration ReadFolderConfig(string folder, List<Tuple<string, int, string>> errors);
        FolderConfiguration GetFolderConfig(string folder);
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

        public FolderConfiguration ReadFolderConfig(string folder, List<Tuple<string, int, string>> errors)
        {
            var config = m_folderConfigs.FirstOrDefault(c => c.Folder == folder);
            if (config == null)     // If not already loaded, load it now.
            {
                var filepath = Path.GetFullPath(Path.Combine(folder, Constants.STEPBRO_FOLDER_CONFIG_FILE));
                if (System.IO.File.Exists(filepath))
                {
                    System.Diagnostics.Debug.WriteLine("OPENING CONFIG FILE IN " + folder);
                    PropertyBlock props;
                    try
                    {
                        props = filepath.GetPropertyBlockFromFile();
                    }
                    catch
                    {
                        errors.Add(new Tuple<string, int, string>(filepath, -1, $"Error reading configuration file."));
                        return null;
                    }
                    var fileErrors = new List<Tuple<int, string>>();
                    config = FolderConfiguration.Create(props, folder, fileErrors);
                    foreach (var error in fileErrors) errors.Add(new Tuple<string, int, string>(filepath, error.Item1, error.Item2));
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("NO CONFIG FILE IN " + folder);
                    config = new FolderConfiguration(folder);       // Create empty file.
                }
                m_folderConfigs.Add(config);

                var root = Path.GetDirectoryName(folder);
                if (!config.IsSearchRoot && root != folder)
                {
                    config.ParentConfiguration = ReadFolderConfig(root, errors);
                }
            }
            return config;
        }

        public FolderConfiguration GetFolderConfig(string folder)
        {
            return null;
        }
    }
}
