using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Execution;
using StepBro.Core.Logging;
using StepBro.ToolBarCreator;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.UI.WinForms.CustomToolBar
{
    internal class ToolStripMenuSubMenu : ToolStripMenuItem, IMenu, IToolBarElement
    {
        private IToolBarElement m_parent;
        private ICoreAccess m_coreAccess = null;
        private Dictionary<string, object> m_commonChildFields = null;

        public ToolStripMenuSubMenu(IToolBarElement parent, ICoreAccess coreAccess, string name) : base()
        {
            m_parent = parent;
            m_coreAccess = coreAccess;
            this.Name = name;
            this.Text = name;
        }

        internal void SetChildProperty(string name, object value)
        {
            if (m_commonChildFields == null)
            {
                m_commonChildFields = new Dictionary<string, object>();
            }
            m_commonChildFields[name] = value;
        }

        public void SetTitle(string title)
        {
            this.Text = title;
        }


        #region IToolBarElement

        public uint Id => throw new NotImplementedException();

        public IToolBarElement ParentElement { get { return m_parent; } }

        public string PropertyName => throw new NotImplementedException();

        public string ElementName => throw new NotImplementedException();

        public string ElementType => throw new NotImplementedException();

        public event PropertyChangedEventHandler PropertyChanged { add { } remove { } }

        public IEnumerable<IToolBarElement> GetChilds()
        {
            throw new NotImplementedException();
        }

        public object GetProperty([Implicit] ICallContext context, string property)
        {
            throw new NotImplementedException();
        }

        public object GetValue([Implicit] ICallContext context)
        {
            throw new NotImplementedException();
        }

        public void SetProperty([Implicit] ICallContext context, string property, object value)
        {
            throw new NotImplementedException();
        }

        public bool SetValue([Implicit] ICallContext context, object value)
        {
            throw new NotImplementedException();
        }

        public IToolBarElement TryFindChildElement([Implicit] ICallContext context, string name)
        {
            throw new NotImplementedException();
        }

        public object TryGetChildProperty(string name)
        {
            if (m_commonChildFields != null && m_commonChildFields.ContainsKey(name))
            {
                return m_commonChildFields[name];
            }
            return m_parent.TryGetChildProperty(name);
        }

        #endregion
    }
}
