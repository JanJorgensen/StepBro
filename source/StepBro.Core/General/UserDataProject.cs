using StepBro.Core.Data;
using StepBro.Core.ScriptData;
using StepBro.Core.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StepBro.Core.General
{
    public class UserDataProject : ServiceBase<UserDataProject, UserDataProject>
    {
        [JsonDerivedType(typeof(ScriptElementShortcut), typeDiscriminator: "scriptelement")]
        [JsonDerivedType(typeof(ProcedureShortcut), typeDiscriminator: "procedure")]
        [JsonDerivedType(typeof(ObjectCommandShortcut), typeDiscriminator: "command")]
        public class Shortcut
        {
            public string Text { get; set; }
        }
        public class ScriptElementShortcut : Shortcut
        {
            public string Element { get; set; } = null;
            public string Partner { get; set; } = null;
            public string Instance { get; set; } = null;
            public string[] Arguments { get; set; } = null;
        }
        public class ProcedureShortcut : Shortcut       // TBD - obsolete
        {
            public string Element { get; set; } = null;
            public string Partner { get; set; } = null;
            public string Instance { get; set; } = null;
            public string[] Arguments { get; set; } = null;
        }
        public class ObjectCommandShortcut : Shortcut
        {
            public string Instance { get; set; } = null;
            public string Command { get; set; } = null;
        }

        public class ElementSetting
        {
            public string Element { get; set; }
            public string ID { get; set; }
            public string Value { get; set; }
            public string[] Values { get; set; }

            bool HasValue() {  return this.Value != null || (this.Values != null && this.Values.Length > 0); }

            public void SetValue(object value)
            {
                if (value == null)
                {
                    this.Value = null;
                }
                else
                {
                    if (value is not string && value is IEnumerable)
                    {
                        this.Value = null;
                        var values = ((IEnumerable)value).Cast<object>().Select(v => StringUtils.ObjectToString(v, true, true)).ToArray();
                        if (values.Length > 0)
                        {
                            this.Values = values;
                        }
                    }
                    else
                    {
                        this.Value = StringUtils.ObjectToString(value, true, true);
                        this.Values = null;
                    }
                }
            }
        }

        private class Data
        {
            public int version { get; set; } = 2;
            public List<Shortcut> Shortcuts { get; set; } = null;
            public List<ElementSetting> ElementSettings { get; set; } = null;

            public string[] HiddenToolbars { get; set; } = null;
        }

        public const string ELEMENT_TOOLBARS = "Toolbars";
        public const string ELEMENT_TOOLBARS_HIDDEN = "Hidden";

        private Data m_data = new Data();   // NOTE: A new object will be created when loading from file.
        private ILoadedFilesManager m_loadedFilesManager = null;
        private IScriptFile m_topFile = null;
        private string m_userFilePath = null;
        private bool m_dataRead = false;

        internal UserDataProject(out IService serviceAccess) :
            base("UserDataProject", out serviceAccess, typeof(ILoadedFilesManager), typeof(IConfigurationFileManager))
        {
        }

        protected override void Start(ServiceManager manager, ITaskContext context)
        {
            m_loadedFilesManager = manager.Get<ILoadedFilesManager>();
            m_loadedFilesManager.FileLoaded += LoadedFilesManager_FileLoaded;
        }

        protected override void Stop(ServiceManager manager, ITaskContext context)
        {
            this.Save();
        }

        public bool FileRead { get => m_dataRead; }

        private void LoadedFilesManager_FileLoaded(object sender, LoadedFileEventArgs args)
        {
            if (m_topFile == null)
            {
                m_topFile = m_loadedFilesManager.TopScriptFile;
                if (m_topFile != null)
                {
                    m_userFilePath = Path.Combine(Path.GetDirectoryName(m_topFile.FilePath), Path.GetFileNameWithoutExtension(m_topFile.FilePath)) + ".user.json";

                    if (System.IO.File.Exists(m_userFilePath))
                    {
                        var data = JsonSerializer.Deserialize<Data>(System.IO.File.ReadAllText(m_userFilePath));
                        if (data != null)
                        {
                            if (data.Shortcuts != null && data.Shortcuts.Count > 0)
                            {
                                for (int i = 0; i < data.Shortcuts.Count; i++)
                                {
                                    if (data.Shortcuts[i] is ProcedureShortcut ps)
                                    {
                                        data.Shortcuts[i] = new ScriptElementShortcut { Element = ps.Element, Instance = ps.Instance, Partner = ps.Partner, Text = ps.Text };   // Convert to element of the future.
                                    }
                                }
                            }
                            m_dataRead = true;
                            m_data = data;
                            if (m_data.HiddenToolbars != null)
                            {
                                this.SaveElementSettingValue(ELEMENT_TOOLBARS, ELEMENT_TOOLBARS_HIDDEN, m_data.HiddenToolbars);
                                m_data.HiddenToolbars = null;
                            }
                        }
                    }
                }
            }
        }

        #region Shortcuts

        public bool AnyShortcuts() { return m_data.Shortcuts != null && m_data.Shortcuts.Any(); }

        public IEnumerable<Shortcut> ListShortcuts()
        {
            return m_data.Shortcuts;
        }

        public void SaveShortcuts(IEnumerable<Shortcut> shortcuts)
        {
            m_data.Shortcuts = new List<Shortcut>(shortcuts);
            if (m_data.Shortcuts.Count == 0)
            {
                m_data.Shortcuts = null;
            }
        }

        #endregion

        #region Element Settings

        public void SaveElementSettingValue(string element, string id, object value)
        {
            if (m_data.ElementSettings == null)
            {
                m_data.ElementSettings = new List<ElementSetting>();
            }
            var setting = m_data.ElementSettings.FirstOrDefault(s => s.Element == element && s.ID == id);
            if (setting != null)
            {
                setting.SetValue(value);
            }
            else
            {
                setting = new ElementSetting
                {
                    Element = element,
                    ID = id
                };
                setting.SetValue(value);
                m_data.ElementSettings.Add(setting);
            }
        }

        public object TryGetElementSetting(string element, string id)
        {
            if (m_data.ElementSettings != null)
            {
                var setting = m_data.ElementSettings.FirstOrDefault(s => s.Element == element && s.ID == id);
                if (setting != null)
                {
                    return (setting.Value != null) ? setting.Value : setting.Values;
                }
            }
            return null;
        }

        #endregion

        public void Save()
        {
            if (m_topFile != null)
            {
                if ((m_data.Shortcuts != null && m_data.Shortcuts.Count > 0) ||
                    (m_data.ElementSettings != null && m_data.ElementSettings.Count > 0))
                {
                    JsonSerializerOptions options = new JsonSerializerOptions();
                    options.WriteIndented = true;
                    options.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;

                    using (FileStream createStream = System.IO.File.Create(m_userFilePath))
                    {
                        JsonSerializer.Serialize(createStream, m_data, options);
                    }
                }
            }
        }
    }
}
