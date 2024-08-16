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
using static StepBro.UI.WinForms.ButtonLogic;

namespace StepBro.UI.WinForms.CustomToolBar
{
    public class Button : ToolStripMenuItem, IToolBarElement, ButtonLogic.IButton, IResizeable
    {
        private IToolBarElement m_parent;
        private ICoreAccess m_coreAccess = null;
        private ButtonLogic m_logic;
        private Color m_normalBack;
        private string m_text = "";
        private bool m_setupFromLogic = false;
        private int m_maxWidth = -1;
        private string m_widthGroup = null;
        private string m_changedText = null;
        private System.Windows.Forms.Timer m_timer = null;
        private object m_lock = new object();

        public Button(IToolBarElement parent, ICoreAccess coreAccess, string name) : base()
        {
            Debug.Assert(parent != null);
            m_parent = parent;
            m_coreAccess = coreAccess;
            m_logic = new ButtonLogic(this, coreAccess);
            m_normalBack = this.BackColor;
            this.Margin = new Padding(1, Margin.Top, 1, Margin.Bottom);
            this.Name = name;
            m_text = name;
            base.Text = "\u23F5 " + m_text;   // Just the default text.

            m_timer = new System.Windows.Forms.Timer();
            m_timer.Tick += UpdateTimer_Tick;
            m_timer.Interval = 400;
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            lock (m_lock)
            {
                if (m_changedText != null)
                {
                    this.SetText(m_changedText);
                    m_changedText = null;
                }
                else
                {
                    m_timer.Stop();
                }
            }
        }

        public string Instance
        {
            get { return (m_logic.StartAction.TargetObject != null) ? m_logic.StartAction.TargetObject : (string)m_parent.TryGetChildProperty("Instance"); }
            set { m_logic.StartAction.TargetObject = value; m_logic.UpdateMode(); }
        }
        public string Procedure
        {
            get { return m_logic.StartAction.FileElementName; }
            set { m_logic.StartAction.FileElementName = value; m_logic.UpdateMode(); }
        }
        public string Partner
        {
            get { return m_logic.StartAction.Partner; }
            set { m_logic.StartAction.Partner = value; m_logic.UpdateMode(); }
        }
        public string ObjectCommand
        {
            get { return m_logic.StartAction.ObjectCommand; }
            set { m_logic.StartAction.ObjectCommand = value; m_logic.UpdateMode(); }
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
            m_logic.UpdateMode();
        }

        public void AddToArguments(IEnumerable<object> values)
        {
            if (m_logic.StartAction.Arguments == null)
            {
                m_logic.StartAction.Arguments = new List<object>();
            }
            m_logic.StartAction.Arguments.AddRange(values);
            m_logic.UpdateMode();
        }

        public void SetStoppable()
        {
            m_logic.SetStoppable();
        }
        public void SetStopOnButtonRelease()
        {
            m_logic.SetStopOnButtonRelease();
        }

        public void SetCheckOnClick()
        {
            m_logic.SetCheckOnClick();
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            if (!m_setupFromLogic)
            {
                m_normalBack = this.BackColor;
            }
        }

        public new string Text
        {
            get { return base.Text; }
            set
            {
                lock (m_lock)
                {
                    if (m_changedText == null)
                    {
                        if (m_timer.Enabled)
                        {
                            m_changedText = value;  // Let the running timer handle the change.
                        }
                        else
                        {
                            this.SetText(value);
                            m_timer.Start();        // Start the timer, to postpone the next change time.
                        }
                    }
                    else
                    {
                        m_changedText = value;  // Let the running timer handle the change.
                    }
                }
            }
        }

        private void SetText(string text)
        {
            base.Text = text;
        }

        public new int Width
        {
            get { return base.Width; }
            set
            {
                this.SetWidth(value);
            }
        }

        #region IResizeable members

        public int MaxWidth
        {
            get { return m_maxWidth; }
            set
            {
                m_maxWidth = value;
                //this.SetWidth(value);   // Temporary!! To be deleted.
            }
        }

        public void SetWidth(int width)
        {
            this.AutoSize = false;
            this.Size = new Size(width, this.Height);
        }

        public int GetPreferredWidth()
        {
            return this.GetPreferredSize(new Size(m_maxWidth, this.Height)).Width;
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);

            var preferredWidth = this.GetPreferredWidth();
            if (m_maxWidth > 0 && this.AutoSize == false && this.Width < preferredWidth)
            {
                this.SetWidth(preferredWidth);
            }
        }

        public string WidthGroup { get { return m_widthGroup; } set { m_widthGroup = value; } }

        #endregion

        #region IProcedureActivationButton

        void IButton.CommandHandler(ButtonCommand command)
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

        void IButton.BeginInvoke(Action action)
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
