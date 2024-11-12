using System;
using System.Text.Json.Serialization;
using static StepBro.Core.Data.PropertyBlockDecoder;

namespace StepBro.SimpleWorkbench
{
    public class UserDataCurrent
    {
        [JsonIgnore]
        private List<UIElementSetting> m_UIElementSettings = null;

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
        }
        public class ObjectCommandShortcut : Shortcut
        {
            public string Instance { get; set; } = null;
            public string Command { get; set; } = null;
        }

        public class UIElementSetting
        {
            public string ElementName { get; set; }
            public string ValueID { get; set; }
            public string Value { get; set; }
        }

        public int version { get; set; } = 2;
        public Shortcut[] Shortcuts { get; set; } = null;
        public UIElementSetting[] UIElementSettings
        {
            get
            {
                return (m_UIElementSettings != null) ? m_UIElementSettings.ToArray() : null;
            }
            set
            {
                m_UIElementSettings = new List<UIElementSetting>(value);
            }
        }

        public string[] HiddenToolbars { get; set; } = null;
        //public System.Xml.XmlDocument ToolLayout { get; set; } = null;
        //public System.Xml.XmlDocument DocumentLayout { get; set; } = null;

        public void SaveUIElementSetting(string element, string id, string value)
        {
            if (m_UIElementSettings == null)
            {
                m_UIElementSettings = new List<UIElementSetting>();
            }
            var setting = m_UIElementSettings.FirstOrDefault(s => s.ElementName == element && s.ValueID == id);
            if (setting != null)
            {
                setting.Value = value;
            }
            else
            {
                m_UIElementSettings.Add(new UIElementSetting { ElementName = element, ValueID = id, Value = value });
            }
        }

        public void SaveUIElementSetting(string element, string id, string[] value)
        {
            var index = 0;
            foreach (var v in value)
            {
                this.SaveUIElementSetting(element, $"{id}[{index++}]", v);
            }
        }

        public string TryGetUIElementSetting(string element, string id)
        {
            if (m_UIElementSettings != null)
            {
                var setting = m_UIElementSettings.FirstOrDefault(s => s.ElementName == element && s.ValueID == id);
                if (setting != null)
                {
                    return setting.Value;
                }
            }
            return null;
        }

        public List<string> TryGetUIElementSettingList(string element, string id)
        {
            var index = 0;
            var value = this.TryGetUIElementSetting(element, $"{id}[{index++}]");
            if (value != null)
            {
                List<string> list = new List<string>();
                while (value != null)
                {
                    list.Add(value);
                    value = this.TryGetUIElementSetting(element, $"{id}[{index++}]");
                }
                return list;
            }
            else
            {
                return null;
            }
        }
    }

}
