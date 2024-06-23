using StepBro.Core.Api;
using StepBro.Core.Execution;
using StepBro.ToolBarCreator;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.UI.WinForms.CustomToolBar
{
    public class Selector : ToolStripComboBox, IToolBarElement
    {
        private IToolBarElement m_parent;
        private ICoreAccess m_coreAccess = null;
        private string m_name;

        public Selector(IToolBarElement parent, ICoreAccess coreAccess, string name) : base()
        {
            m_parent = parent;
            m_coreAccess = coreAccess;
            m_name = name;
        }

        #region IToolBarElement

        public uint Id => throw new NotImplementedException();

        public IToolBarElement ParentElement { get { return m_parent; } }

        public string PropertyName => throw new NotImplementedException();

        public string ElementName
        {
            get { return m_name; }
        }

        public string ElementType
        {
            get { return "Selector"; }
        }

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
            return null;
        }

        #endregion
    }
}
