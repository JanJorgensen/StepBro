using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Execution;
using StepBro.Core.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace StepBro.PanelCreator.DummyUI
{
    internal class DummyBaseUIElement : IPanelElement, INotifyPropertyChanged
    {
        private static uint g_nextID = 100;
        public uint m_id;
        private string m_parentPropertyname;
        private string m_name;
        private string m_type;
        private IPanelElement m_parent = null;
        private List<IPanelElement> m_childs = new List<IPanelElement>();
        private Dictionary<string, object> m_props = new Dictionary<string, object>();

        public event PropertyChangedEventHandler PropertyChanged;

        public DummyBaseUIElement(IPanelElement parent, string propertyName, string name, string type)
        {
            m_id = g_nextID++;
            m_parent = parent;
            m_parentPropertyname = propertyName;
            m_name = name;
            m_type = type;
        }

        public uint Id { get { return m_id; } }

        public string PropertyName
        {
            get { return m_parentPropertyname; }
            private set { m_parentPropertyname = value; }
        }

        public string ElementName
        {
            get { return m_name; }
        }

        public string ElementType
        {
            get { return m_type; }
        }

        public IPanelElement Parent => throw new NotImplementedException();

        public override string ToString()
        {
            return $"{m_type} {m_name} ({m_parentPropertyname}";
        }

        public IEnumerable<IPanelElement> GetChilds()
        {
            return m_childs;
        }

        public void Setup(PropertyBlock data, bool allowProperties = true)
        {
            foreach (var field in data)
            {
                if (field.BlockEntryType == PropertyBlockEntryType.Value)
                {
                    var valueField = field as PropertyBlockValue;
                    if (field.Name == "Name")
                    {
                        this.PropertyName = valueField.ValueAsString();
                    }
                    else if (allowProperties)
                    {
                        if (valueField.IsStringOrIdentifier)
                        {
                            m_props.Add(field.Name, valueField.ValueAsString());
                            field.IsUsedOrApproved = true;
                        }
                        else
                        {
                            m_props.Add(field.Name, valueField.Value);
                            field.IsUsedOrApproved = true;
                        }
                    }
                }
                else if (field.BlockEntryType == PropertyBlockEntryType.Block)
                {
                    switch (field.Name)
                    {
                        case "Childs":
                        case "Controls":
                            this.Setup(field as PropertyBlock, false);
                            break;
                        default:
                            var child = new DummyBaseUIElement(this, field.Name, field.Name, field.SpecifiedTypeName);
                            m_childs.Add(child);
                            child.Setup(field as PropertyBlock);
                            break;
                    }
                    field.IsUsedOrApproved = true;
                }
                else if (field.BlockEntryType == PropertyBlockEntryType.Array)
                {

                }
            }
        }

        public void SetProperty([Implicit] ICallContext context, string property, object value)
        {
            m_props[property] = value;
        }

        public IPanelElement TryFindChildElement([Implicit] ICallContext context, string name)
        {
            return m_childs.FirstOrDefault(c => c.ElementName == name || c.PropertyName == name);
        }

        public object GetProperty([Implicit] ICallContext context, string property)
        {
            if (m_props.ContainsKey(property)) { return m_props[property]; }
            else
            {
                throw new DynamicPropertyNotFoundException(property);
            }
        }

        public bool TrySetProperty(string property, object value)
        {
            throw new NotImplementedException();
        }

        public object GetValue([Implicit] ICallContext context)
        {
            throw new NotImplementedException();
        }

        public bool SetValue([Implicit] ICallContext context, object value)
        {
            throw new NotImplementedException();
        }
    }
}
