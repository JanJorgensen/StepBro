using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Execution;
using StepBro.Core.Logging;
using StepBro.ToolBarCreator;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StepBro.UI.WinForms.ProcedureActivationButtonLogic;

namespace StepBro.UI.WinForms.CustomToolBar
{
    public class ObjectCommandButton : ToolStripMenuItem, IToolBarElement
    {
        private IToolBarElement m_parent;
        private ICoreAccess m_coreAccess = null;
        private string m_object = null;
        private string m_command = null;

        public ObjectCommandButton(IToolBarElement parent, ICoreAccess coreAccess, string name) : base()
        {
            Debug.Assert(parent != null);
            m_parent = parent;
            m_coreAccess = coreAccess;
            this.Margin = new Padding(1, Margin.Top, 1, Margin.Bottom);
            this.Name = name;
            this.Text = name;   // Just the default text.
        }

        public string ObjectInstance
        {
            get { return (m_object != null) ? m_object : (string)m_parent.TryGetChildProperty("Instance"); }
            set { m_object = value; }
        }
        public string ObjectCommand
        {
            get { return m_command; }
            set { m_command = value; }
        }

        protected override void OnClick(EventArgs e)
        {
            m_coreAccess.ExecuteObjectCommand(this.ObjectInstance, m_command);
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
            return null;
        }

        #endregion
    }
}
