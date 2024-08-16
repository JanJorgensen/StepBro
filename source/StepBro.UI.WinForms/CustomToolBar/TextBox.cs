using StepBro.Core.Api;
using StepBro.Core.Execution;
using StepBro.ToolBarCreator;
using StepBro.UI.WinForms.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StepBro.UI.WinForms.ButtonLogic;

namespace StepBro.UI.WinForms.CustomToolBar
{
    internal class TextBox : ToolStripTextBoxWithAutoWidth, IToolBarElement
    {
        private IToolBarElement m_parent;
        private ICoreAccess m_coreAccess = null;
        private Color m_normalBack;

        public TextBox(IToolBarElement parent, ICoreAccess coreAccess, string name) : base()
        {
            Debug.Assert(parent != null);
            m_parent = parent;
            m_coreAccess = coreAccess;
            m_normalBack = this.BackColor;
            this.Margin = new Padding(1, Margin.Top, 1, Margin.Bottom);
            this.Name = name;
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            //if (!m_setupFromLogic)
            //{
            //    m_normalBack = this.BackColor;
            //}
        }

        //protected override void OnTextChanged(EventArgs e)
        //{
        //    base.OnTextChanged(e);

        //    var preferredWidth = this.GetPreferredWidth();
        //    if (m_maxWidth > 0 && this.AutoSize == false && this.Width < preferredWidth)
        //    {
        //        this.SetWidth(preferredWidth);
        //    }
        //}

        #region IToolBarElement

        public uint Id => throw new NotImplementedException();

        public IToolBarElement ParentElement { get { return m_parent; } }

        public string PropertyName => throw new NotImplementedException();

        public string ElementName { get { return this.Name; } }

        public string ElementType { get { return "Button"; } }

        public event PropertyChangedEventHandler PropertyChanged { add { } remove { } }

        public IEnumerable<IToolBarElement> GetChilds()
        {
            yield break;
        }

        public object GetProperty([Implicit] ICallContext context, string property)
        {
            return ToolBar.GetElementPropertyValue(this, context, property);
        }

        public object GetValue([Implicit] ICallContext context)
        {
            throw new NotImplementedException();
        }

        public void SetProperty([Implicit] ICallContext context, string property, object value)
        {
            ToolBar.SetElementPropertyValue(this, context, property, value);
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
