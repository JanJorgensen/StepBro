using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.File;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using static StepBro.Core.Data.PropertyBlockDecoder;

namespace StepBro.Core.General
{
    /// <summary>
    /// Reads a StepBro.cfg file from 
    /// </summary>
    public class FolderConfiguration
    {
        private string m_folder;
        public FolderConfiguration(string folder)
        {
            m_folder = folder;
            this.LibFolders = new List<string>();
        }
        public FolderConfiguration(string folder, bool isSearchRoot, List<string> libFolders)
        {
            m_folder = folder;
            this.IsSearchRoot = isSearchRoot;
            this.LibFolders = (libFolders != null) ? libFolders : new List<string>();
        }
        public string Folder { get { return m_folder; } }
        public bool IsSearchRoot { get; private set; } = false;
        public FolderConfiguration ParentConfiguration { get; internal set; } = null;
        public List<string> LibFolders { get; private set; } = null;
        public List<FolderShortcut> Shortcuts { get; private set; } = new List<FolderShortcut>();

        public IEnumerable<FolderShortcut> ListShortcuts()
        {
            foreach (var shortcut in Shortcuts)
            {
                yield return shortcut;
            }
            if (this.ParentConfiguration != null)
            {
                foreach (var shortcut in this.ParentConfiguration.ListShortcuts())
                {
                    yield return shortcut;
                }
            }
        }

        private static PropertyBlockDecoder.Block<object, FolderConfiguration> m_decoder = null;

        public static FolderConfiguration Create(string folder, List<Tuple<string, int, string>> errors)
        {
            FolderConfiguration config = null;
            var filepath = Path.GetFullPath(Path.Combine(folder, Constants.STEPBRO_FOLDER_CONFIG_FILE));
            if (System.IO.File.Exists(filepath))
            {
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
                config = new FolderConfiguration(folder);       // Create empty file.
            }
            return config;
        }

        public static FolderConfiguration Create(PropertyBlock data, string path, List<Tuple<int, string>> errors)
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

                var config = new FolderConfiguration(path);
                m_decoder.DecodeData(null, data, config, errors);

                foreach (var sc in config.Shortcuts)
                {
                    string error = null;
                    if (!sc.TryResolve(StepBro.Core.ServiceManager.Global.Get<IFolderManager>().ListShortcuts().Concat(config.Shortcuts), path, ref error))
                    {
                        errors.Add(new Tuple<int, string>(-1, $"Error resolving shortcut \"{sc.Name}\". {error}"));
                    }
                }
                return config;
            }
        }
    }
}
