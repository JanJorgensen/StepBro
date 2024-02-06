using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Logging;
using StepBro.Core.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.General
{
    public interface IConfigurationFileManager
    {
        PropertyBlock GetStationProperties();
        FolderConfiguration ReadFolderConfig(string configFile);
    }

    internal class ConfigurationFileManager : ServiceBase<IConfigurationFileManager, ConfigurationFileManager>, IConfigurationFileManager
    {
        private PropertyBlock m_stationProperties = null;

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

        public FolderConfiguration ReadFolderConfig(string configFile)
        {
            PropertyBlock props = null;
            try
            {
                props = configFile.GetPropertyBlockFromFile();
                //#if DEBUG
                //                context.Logger.LogSystem($"Successfully read the station properties file.");
                //#else
                //                        context.Logger.LogSystem($"Station properties file: {stationPropFile}.");
                //#endif
            }
            catch
            {
                //context.Logger.LogError($"Error reading station properties file ({stationPropFile}).");
                return null;
            }
            return FolderConfiguration.Create(props);
        }
    }

    public class FolderConfiguration
    {
        public FolderConfiguration(bool isSearchRoot, List<string> libFolders)
        {
            this.IsSearchRoot = isSearchRoot;
            this.LibFolders = libFolders;
        }
        public bool IsSearchRoot { get; private set; }
        public List<string> LibFolders { get; private set; }

        public static FolderConfiguration Create(PropertyBlock data)
        {
            if (data == null) return null;
            else
            {
                bool isRoot = false;
                List<string> folders = new List<string>();

                foreach (PropertyBlockEntry entry in data)
                {
                    if (entry.BlockEntryType == PropertyBlockEntryType.Flag)
                    {
                        if (entry.Name == Constants.STEPBRO_FOLDER_CONFIG_FILE_ROOT)
                        {
                            isRoot = true;
                            continue;
                        }
                    }
                    else if (entry.BlockEntryType == PropertyBlockEntryType.Array)
                    {
                        if (entry.Name == Constants.STEPBRO_FOLDER_CONFIG_FILE_LIBS)
                        {
                            var asArray = entry as PropertyBlockArray;
                            if (asArray.IsStringArray())
                            {
                                folders = asArray.AsStringArray();
                                continue;
                            }
                        }
                    }
                }
                return new FolderConfiguration(isRoot, folders);
            }
        }
    }
}
