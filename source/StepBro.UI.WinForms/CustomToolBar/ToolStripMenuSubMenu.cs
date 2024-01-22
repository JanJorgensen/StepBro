using StepBro.Core.Api;
using StepBro.Core.Data;
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
    internal class ToolStripMenuSubMenu : ToolStripMenuItem, IToolBarElement, IToolBarElementSetup
    {
        private ICoreAccess m_coreAccess = null;
        private MenuLogic m_menuLogic = null;

        public ToolStripMenuSubMenu(ICoreAccess coreAccess) : base()
        {
            m_coreAccess = coreAccess;
            m_menuLogic = new MenuLogic(this, coreAccess);
        }

        #region IToolBarElementSetup

        public void Clear()
        {
            foreach (IToolBarElementSetup item in this.DropDownItems)
            {
                item.Clear();
            }
            this.DropDownItems.Clear();
        }

        public ICoreAccess Core { get { return m_coreAccess; } }

        public void Setup(PropertyBlock definition)
        {
            m_menuLogic.Setup(definition);
        }

        #endregion

        #region IToolBarElement

        public uint Id => throw new NotImplementedException();

        public IToolBarElement ParentElement => throw new NotImplementedException();

        public string PropertyName => throw new NotImplementedException();

        public string ElementName => throw new NotImplementedException();

        public string ElementType => throw new NotImplementedException();

        public event PropertyChangedEventHandler PropertyChanged;

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

        #endregion
    }
}
