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
using System.Windows.Forms;
using static StepBro.UI.WinForms.ProcedureActivationButtonLogic;

namespace StepBro.UI.WinForms.CustomToolBar
{
    public class ProcedureActivationButton : ToolStripMenuItem, IToolBarElement, ProcedureActivationButtonLogic.IProcedureActivationButton
    {
        private IToolBarElement m_parent;
        private ICoreAccess m_coreAccess = null;
        private ProcedureActivationButtonLogic m_logic;
        private Color m_normalBack;
        private string m_text = "";
        private bool m_setupFromLogic = false;

        public ProcedureActivationButton(IToolBarElement parent, ICoreAccess coreAccess, string name) : base()
        {
            Debug.Assert(parent != null);
            m_parent = parent;
            m_coreAccess = coreAccess;
            m_logic = new ProcedureActivationButtonLogic(this, coreAccess);
            m_normalBack = this.BackColor;
            this.Margin = new Padding(1, Margin.Top, 1, Margin.Bottom);
            this.Name = name;
            m_text = name;
            this.Text = "\u23F5 " + m_text;   // Just the default text.
        }

        public string Instance
        {
            get { return (m_logic.StartAction.TargetObject != null) ? m_logic.StartAction.TargetObject : (string)m_parent.TryGetChildProperty("Instance"); }
            set { m_logic.StartAction.TargetObject = value; }
        }
        public string Procedure
        {
            get { return m_logic.StartAction.Name; }
            set { m_logic.StartAction.Name = value; }
        }
        public string Partner
        {
            get { return m_logic.StartAction.Partner; }
            set { m_logic.StartAction.Partner = value; }
        }

        public object[] Arguments
        {
            get { return (m_logic.StartAction.Arguments != null) ? m_logic.StartAction.Arguments.ToArray() : null; }
        }

        public void AddToArguments(object value)
        {
            if (m_logic.StartAction.Arguments == null)
            {
                m_logic.StartAction.Arguments = new List<object>();
            }
            m_logic.StartAction.Arguments.Add(value);
        }

        public void AddToArguments(IEnumerable<object> values)
        {
            if (m_logic.StartAction.Arguments == null)
            {
                m_logic.StartAction.Arguments = new List<object>();
            }
            m_logic.StartAction.Arguments.AddRange(values);
        }

        public void SetStoppable()
        {
            m_logic.SetStoppable();
        }
        public void SetStopOnButtonRelease()
        {
            m_logic.SetStopOnButtonRelease();
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            if (!m_setupFromLogic)
            {
                m_normalBack = this.BackColor;
            }
        }

        #region IProcedureActivationButton

        void IProcedureActivationButton.CommandHandler(ButtonCommand command)
        {
            m_setupFromLogic = true;
            switch (command)
            {
                case ButtonCommand.ModeCheckOnClick:
                    this.CheckOnClick = true;
                    break;
                case ButtonCommand.Enable:
                    this.Enabled = true;
                    break;
                case ButtonCommand.Disable:
                    this.Enabled = false;
                    break;
                case ButtonCommand.SetChecked:
                    this.Checked = true;
                    break;
                case ButtonCommand.SetUnchecked:
                    this.Checked = false;
                    break;
                case ButtonCommand.ShowActive:
                    this.BackColor = Color.Orange;
                    break;
                case ButtonCommand.ShowNormal:
                    this.BackColor = m_normalBack;
                    this.Text = "\u23F5 " + m_text;
                    break;
                case ButtonCommand.ShowAwaitingExecutionEnd:
                    this.BackColor = Color.Red;
                    this.Text = "\u231B " + m_text;
                    break;
                case ButtonCommand.ShowPlaySymbol:
                    this.Text = "\u23F5 " + m_text;
                    break;
                case ButtonCommand.ShowStopSymbol:
                    this.Text = "\u23F9 " + m_text;
                    break;
                case ButtonCommand.ShowWaitSymbol:
                    this.Text = "\u231B " + m_text;
                    break;
                default: break;
            }
            m_setupFromLogic = false;
        }

        void IProcedureActivationButton.BeginInvoke(Action action)
        {
            this.Parent.BeginInvoke(action);
        }

        #endregion

        #region Mouse and Keyboard

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            m_logic.ButtonClicked();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                m_logic.ButtonPushed();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == MouseButtons.Left)
            {
                m_logic.ButtonReleased();
            }
        }

        protected override void OnCheckedChanged(EventArgs e)
        {
            base.OnCheckedChanged(e);
            m_logic.CheckedChanged(this.Checked);
        }

        #endregion

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
