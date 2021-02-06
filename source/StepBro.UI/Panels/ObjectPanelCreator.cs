using StepBro.Core.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace StepBro.UI.Panels
{
    public abstract class ObjectPanelInfo
    {
        public string Name { get; private set; }
        public abstract string TypeIdentification { get; }
        public string Description { get; private set; }
        public bool IsObjectPanel { get; private set; }
        public bool AllowMultipleInstances { get; private set; }


        protected abstract UserControl CreatePanelView();

        internal ObjectPanelInfo(string name, string description, bool isObjectPanel, bool allowMultpile)
        {
            this.Name = name;
            this.Description = description;
            this.IsObjectPanel = isObjectPanel;
            this.AllowMultipleInstances = allowMultpile;
        }

        public virtual bool IsCompatibleWithType(Type type)
        {
            return false;
        }

        public virtual bool IsCompatibleWithObject(object @object)
        {
            if (@object == null) return false;
            else
            {
                return this.IsCompatibleWithType(@object.GetType());
            }
        }
    }

    public class ObjectPanelInfo<TPanel, TObject> : ObjectPanelInfo where TPanel : UserControl
    {
        public ObjectPanelInfo(string name, string description, bool allowMultpile) :
            base(name, description, true, allowMultpile)
        {
        }

        public override string TypeIdentification { get { return typeof(TPanel).FullName; } }

        public override bool IsCompatibleWithType(Type type)
        {
            return type.IsAssignableFrom(typeof(TObject));
        }

        protected override UserControl CreatePanelView()
        {
            var panel = System.Activator.CreateInstance<TPanel>();
            return (UserControl)panel;
        }
    }

    public class ObjectPanelInfo<TPanel> : ObjectPanelInfo where TPanel : UserControl
    {
        public ObjectPanelInfo(string name, string description, bool allowMultpile) :
            base(name, description, false, allowMultpile)
        {
        }
        public override string TypeIdentification { get { return typeof(TPanel).FullName; } }

        protected override UserControl CreatePanelView()
        {
            var panel = System.Activator.CreateInstance<TPanel>();
            return (UserControl)panel;
        }
    }

    public abstract class ObjectPanelCreator
    {
        private List<ObjectPanelInfo> m_list = null;

        public IEnumerable<ObjectPanelInfo> ListPanels()
        {
            return m_list;
        }

        public ObjectPanelCreator() { }

        public void UpdatePanelsList()
        {
            if (m_list != null) throw new InvalidOperationException();
            m_list = this.CreatePanelList().ToList();
        }

        protected abstract IEnumerable<ObjectPanelInfo> CreatePanelList();
    }
}
