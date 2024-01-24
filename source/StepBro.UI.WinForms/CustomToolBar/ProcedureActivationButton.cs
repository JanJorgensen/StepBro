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
using static StepBro.UI.WinForms.ProcedureActivationButtonLogic;

namespace StepBro.UI.WinForms.CustomToolBar
{
    public class ProcedureActivationButton : ToolStripMenuItem, IToolBarElement, IToolBarElementSetup, ProcedureActivationButtonLogic.IProcedureActivationButton
    {
        private ICoreAccess m_coreAccess = null;
        private ProcedureActivationButtonLogic m_logic;
        private Color m_normalBack;

        public ProcedureActivationButton(ICoreAccess coreAccess) : base()
        {
            m_coreAccess = coreAccess;
            m_logic = new ProcedureActivationButtonLogic(this, coreAccess);
            m_normalBack = this.BackColor;
        }

        #region IToolBarElementSetup

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public ICoreAccess Core { get { return m_coreAccess; } }

        public void Setup(PropertyBlock definition)
        {
            this.Name = definition.Name;
            this.Text = definition.Name;   // Just the default text.
            foreach (var element in definition)
            {
                if (!m_logic.Setup(element))
                {
                    if (element.BlockEntryType == PropertyBlockEntryType.Value)
                    {
                        var valueField = element as PropertyBlockValue;
                        if (valueField.Name.Equals("Text", StringComparison.InvariantCultureIgnoreCase))
                        {
                            this.Text = valueField.ValueAsString();
                        }
                        else if (valueField.Name.Equals("Color", StringComparison.InvariantCultureIgnoreCase))
                        {
                            try
                            {
                                Color color = (Color)(typeof(Color).GetProperty(valueField.ValueAsString()).GetValue(null));
                                this.BackColor = color;
                            }
                            finally { }
                        }
                    }
                    else if (element.BlockEntryType == PropertyBlockEntryType.Flag)
                    {
                        var flagField = element as PropertyBlockFlag;
                        //if (flagField.Name == nameof(StretchChilds))
                        //{
                        //    StretchChilds = true;
                        //    SizeToChilds = false;
                        //}
                        //else if (flagField.Name == nameof(SizeToChilds))
                        //{
                        //    SizeToChilds = true;
                        //    StretchChilds = false;
                        //}
                    }
                    else if (element.BlockEntryType == PropertyBlockEntryType.Block)
                    {
                        //var type = element.SpecifiedTypeName;
                        //if (type != null)
                        //{
                        //    var elementBlock = element as PropertyBlock;
                        //    if (type == nameof(Menu))
                        //    {
                        //        var menu = new Menu(m_coreAccess);
                        //        this.DropDownItems.Add(menu);
                        //        menu.Setup(element.Name, elementBlock);
                        //    }
                        //}
                    }
                }
            }
        }

        #endregion

        #region IProcedureActivationButton

        void IProcedureActivationButton.CommandHandler(ButtonCommand command)
        {
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
                    break;
                case ButtonCommand.ShowNormal:
                    this.BackColor = m_normalBack;
                    break;
                case ButtonCommand.ShowAwaitingExecutionEnd:
                    this.BackColor = Color.Red;
                    break;
            }
        }

        void IProcedureActivationButton.BeginInvoke(Action action)
        {
            this.Parent.BeginInvoke(action);
        }

        #endregion

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
