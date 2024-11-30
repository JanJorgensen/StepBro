using StepBro.Core.Data;
using StepBro.Core.ScriptData;
using StepBro.Core.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StepBro.Core.General
{
    public class UserDataProject : ServiceBase<UserDataProject, UserDataProject>
    {
        [JsonDerivedType(typeof(ProcedureShortcut), typeDiscriminator: "procedure")]
        [JsonDerivedType(typeof(ObjectCommandShortcut), typeDiscriminator: "command")]
        public class Shortcut
        {
            public string Text { get; set; }
        }
        public class ProcedureShortcut : Shortcut
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
            public string Type { get; set; }
            public string Value { get; set; }

            public void SetValue(object value)
            {
                if (value == null)
                {
                    this.Type = null;
                    this.Value = null;
                }
                else
                {
                    this.Value = StringUtils.ObjectToString(value, true);
                    this.Type = value.GetType().StepBroTypeName();
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
                            m_data = data;
                            if (m_data.HiddenToolbars != null)
                            {
                                this.SaveElementSettingString(ELEMENT_TOOLBARS, ELEMENT_TOOLBARS_HIDDEN, m_data.HiddenToolbars);
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

        public void SaveElementSettingString(string element, string id, string value)
        {
            if (m_data.ElementSettings == null)
            {
                m_data.ElementSettings = new List<ElementSetting>();
            }
            var setting = m_data.ElementSettings.FirstOrDefault(s => s.Element == element && s.ID == id);
            if (setting != null)
            {
                setting.Value = value;
            }
            else
            {
                m_data.ElementSettings.Add(new ElementSetting { Element = element, ID = id, Value = value });
            }
        }

        public void SaveElementSettingString(string element, string id, string[] value)
        {
            if (value == null || value.Length == 0)
            {
                // TODI: Remove existing.
            }
            else
            {
                // TODO: Overwrite if already existing, remove tail if fewer than existing and insert just fter if more than existing.
                var index = 0;
                foreach (var v in value)
                {
                    this.SaveElementSettingString(element, $"{id}[{index++}]", v);
                }
            }
        }

        public void SaveElementSettingValue(string element, string id, object value)
        {
            string stringValue = null;
            if (value != null)
            {
                var t = value.GetType();
                if (t.IsArray)
                {
                    return;
                }
                if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>))
                {

                }
            }
            SaveElementSettingString (element, id, stringValue);
        }

        public string TryGetElementSetting(string element, string id)
        {
            if (m_data.ElementSettings != null)
            {
                var setting = m_data.ElementSettings.FirstOrDefault(s => s.Element == element && s.ID == id);
                if (setting != null)
                {
                    return setting.Value;
                }
            }
            return null;
        }

        public List<string> TryGetElementSettingList(string element, string id)
        {
            var index = 0;
            List<string> list = null;
            while (true)
            {
                var value = this.TryGetElementSetting(element, $"{id}[{index++}]");
                if (value != null)
                {
                    if (list == null)
                    {
                        list = new List<string>();
                    }
                    list.Add(value);
                }
                else break;
            }
            return list;
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
                        JsonSerializer.Serialize(createStream, m_userFilePath, options);
                    }
                }
            }
        }
    }
}
