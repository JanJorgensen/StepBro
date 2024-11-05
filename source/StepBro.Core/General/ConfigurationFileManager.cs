using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.File;
using StepBro.Core.Logging;
using StepBro.Core.Tasks;
using StepBro.ToolBarCreator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StepBro.Core.Data.PropertyBlockDecoder;

namespace StepBro.Core.General
{
    public interface IConfigurationFileManager
    {
        PropertyBlock GetStationProperties();
        FolderConfiguration ReadFolderConfig(ILogger logger, string configFile);
        FolderConfiguration ReadFolderConfig(string configFile, List<Tuple<int, string>> errors);
        void ResetFolderConfigurations();
    }

    internal class ConfigurationFileManager : ServiceBase<IConfigurationFileManager, ConfigurationFileManager>, IConfigurationFileManager
    {
        private PropertyBlock m_stationProperties = null;
        private List<NamedData<FolderConfiguration>> m_folderConfigs = new List<NamedData<FolderConfiguration>>();

        public ConfigurationFileManager(out IService serviceAccess) :
            base("ConfigurationFileManager", out serviceAccess, typeof(ILogger))
        {
        }

        protected override void Start(ServiceManager manager, ITaskContext context)
        {
            string stationPropFile = System.Environment.GetEnvironmentVariable(Constants.STEPBRO_STATION_PROPERTIES);
            if (!String.IsNullOrEmpty(stationPropFile))
            {
                System.Diagnostics.Debug.WriteLine($"Station properties file: ({stationPropFile}).");
                if (System.IO.File.Exists(stationPropFile))
                {
                    try
                    {
                        m_stationProperties = stationPropFile.GetPropertyBlockFromFile();
#if DEBUG
                        context.Logger.LogSystem($"Successfully read the station properties file.");
#else
                        context.Logger.LogSystem($"Station properties file: {stationPropFile}.");
#endif
                    }
                    catch
                    {
                        context.Logger.LogError($"Error reading station properties file ({stationPropFile}).");
                    }
                }
                else
                {
                    context.Logger.LogError($"Station properties file ({stationPropFile}) was not found.");
                }
            }
            else
            {
                context.Logger.LogSystem($"Environment variable \"{Constants.STEPBRO_STATION_PROPERTIES}\" for station properties file reference is not created.");
            }
        }

        public PropertyBlock GetStationProperties()
        {
            return m_stationProperties;
        }

        public FolderConfiguration ReadFolderConfig(ILogger logger, string configFile)
        {
            var errors = new List<Tuple<int, string>>();
            var folderConfig = this.ReadFolderConfig(configFile, errors);
            foreach (var e in errors)
            {
                if (e.Item1 <= 0) logger.LogError($"Config file '{configFile}': {e.Item2}");
                else logger.LogError($"Config file '{configFile}' line {e.Item1}: {e.Item2}");
            }
            return folderConfig;
        }

        public FolderConfiguration ReadFolderConfig(string configFile, List<Tuple<int, string>> errors)
        {
            var config = m_folderConfigs.FirstOrDefault(c => c.Name == configFile);
            if (config.IsEmpty())
            {
                config = new NamedData<FolderConfiguration>(configFile, null);
                PropertyBlock props;
                try
                {
                    props = configFile.GetPropertyBlockFromFile();
                }
                catch
                {
                    errors.Add(new Tuple<int, string>(-1, $"Error reading configuration file."));
                    return null;
                }
                var configData = FolderConfiguration.Create(props, System.IO.Path.GetDirectoryName(configFile), errors);
                config = new NamedData<FolderConfiguration>(configFile, configData);
                m_folderConfigs.Add(config);
            }
            return config.Value;
        }

        public void ResetFolderConfigurations()
        {
            m_folderConfigs.Clear();
        }
    }

    public class FolderConfiguration
    {
        private string m_folder;
        public FolderConfiguration(string folder)
        {
            m_folder = folder;
        }
        public FolderConfiguration(string folder, bool isSearchRoot, List<string> libFolders)
        {
            m_folder = folder;
            this.IsSearchRoot = isSearchRoot;
            this.LibFolders = libFolders;
        }
        public string Folder { get { return m_folder; } }
        public bool IsSearchRoot { get; private set; } = false;
        public List<string> LibFolders { get; private set; } = null;
        public List<FolderShortcut> Shortcuts { get; private set; } = null;

        private static PropertyBlockDecoder.Block<object, FolderConfiguration> m_decoder = null;

        public static FolderConfiguration Create(PropertyBlock data, string currentPath, List<Tuple<int, string>> errors)
        {
            if (data == null) return null;
            else
            {
                if (m_decoder == null)
                {
                    m_decoder = new Block<object, FolderConfiguration>
                        (
                            nameof(FolderConfiguration),
                            Doc(""),
                            new Flag<FolderConfiguration>(
                                Constants.STEPBRO_FOLDER_CONFIG_FILE_ROOT, 
                                Doc(""),
                                (c, f) =>
                                {
                                    string name = f.Name;
                                    if (f.HasTypeSpecified)
                                    {
                                        return "The flag cannot have a name; only a type.";
                                    }
                                    c.IsSearchRoot = true;
                                    return null;    // No errors
                                }),
                            new ArrayString<FolderConfiguration>(
                                Constants.STEPBRO_FOLDER_CONFIG_FILE_LIBS, 
                                Usage.Setting,
                                Doc(""),
                                (c, a) =>
                                {
                                    c.LibFolders = a;
                                    return null;    // No errors
                                }),
                            new ValueString<FolderConfiguration>(
                                Constants.STEPBRO_FOLDER_CONFIG_FILE_SHORTCUT, 
                                Usage.Element,
                                Doc(""),
                                (c, v) =>
                                {
                                    var text = v.ValueAsString();
                                    if (!v.HasTypeSpecified)
                                    {
                                        return "The shortcut is missing a name.";
                                    }

                                    if (c.Shortcuts == null)
                                    {
                                        c.Shortcuts = new List<FolderShortcut>();
                                    }
                                    var shortcut = new FolderShortcut(FolderShortcutOrigin.Configuration, v.Name, text);
                                    c.Shortcuts.Add(shortcut);
                                    return null;    // No errors
                                })
                        );
                }

                var config = new FolderConfiguration(currentPath);
                m_decoder.DecodeData(data, config, errors);
                return config;
            }
        }
    }
}
