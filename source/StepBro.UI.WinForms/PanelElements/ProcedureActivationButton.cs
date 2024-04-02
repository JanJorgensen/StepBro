using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Tasks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StepBro.UI.WinForms.PanelElements
{
    public partial class ProcedureActivationButton : PanelElementBase
    {
        private enum Mode { ActivateOnClick = 0, ClickToStop, RunWhilePushed }
        private ProcedureActivationInfo m_startProcedure;
        //private ProcedureActivationInfo m_stopProcedure;
        //private ProcedureActivationInfo m_enabledCheckProcedure;
        private Mode m_mode = Mode.ActivateOnClick;
        private IExecutionAccess m_execution = null;
        private Color m_buttonColorNormal = Color.White;

        public ProcedureActivationButton()
        {
            InitializeComponent();
        }

        protected override bool Setup(PanelElementBase parent, PropertyBlock definition)
        {
            button.Text = definition.Name;
            checkBox.Text = definition.Name;
            base.Setup(parent, definition);
            if (this.SetForeColor != Color.Empty)
            {
                button.ForeColor = this.SetForeColor;
            }
            if (this.SetBackColor != Color.Empty)
            {
                button.BackColor = this.SetBackColor;
            }
            foreach (var field in definition)
            {
                if (field.BlockEntryType == PropertyBlockEntryType.Value)
                {
                    var valueField = field as PropertyBlockValue;
                    if (valueField.Name == "Text")
                    {
                        button.Text = valueField.ValueAsString();
                        checkBox.Text = valueField.ValueAsString();
                    }
                    else if (valueField.Name == "Procedure")
                    {
                        if (m_startProcedure == null)
                        {
                            m_startProcedure = new ProcedureActivationInfo();
                        }
                        m_startProcedure.Name = valueField.ValueAsString();
                    }
                    else if (valueField.Name == "Model" || valueField.Name == "Partner")
                    {
                        if (m_startProcedure == null)
                        {
                            m_startProcedure = new ProcedureActivationInfo();
                        }
                        m_startProcedure.Partner = valueField.ValueAsString();
                    }
                }
                else if (field.BlockEntryType == PropertyBlockEntryType.Flag)
                {
                    var flagField = field as PropertyBlockFlag;
                    if (flagField.Name == "Stoppable")
                    {
                        m_mode = Mode.ClickToStop;
                        button.Hide();
                        checkBox.Show();
                    }
                    else if (flagField.Name == "StopOnButtonRelease")
                    {
                        m_mode = Mode.RunWhilePushed;
                    }
                }
            }
            return true;
        }

        private void StartProcedure()
        {
            if (m_mode == Mode.ActivateOnClick)
            {
                button.Enabled = false;
            }
            else if (m_mode == Mode.ClickToStop)
            {
                m_buttonColorNormal = checkBox.BackColor;
                checkBox.BackColor = Color.Orange;
            }
            m_execution = this.CoreAccess.StartExecution(m_startProcedure.Name, m_startProcedure.Partner, null, null);
            m_execution.CurrentStateChanged += Execution_CurrentStateChanged;
            System.Diagnostics.Debug.WriteLine("StartProcedure End");
        }

        private void StopProcedure()
        {
            if (m_execution != null && !m_execution.State.HasEnded())
            {
                m_execution.RequestStopExecution();
                if (m_mode == Mode.ClickToStop)
                {
                    checkBox.BackColor = Color.Red;
                    checkBox.Enabled = false;
                }
            }
        }

        private void Execution_CurrentStateChanged(object sender, EventArgs e)
        {
            this.BeginInvoke(this.HandleExecutionStateChange);
        }

        private void HandleExecutionStateChange()
        {
            if (m_execution != null)
            {
                System.Diagnostics.Debug.WriteLine("Execution_CurrentStateChanged; in GUI");
                if (m_execution.State.HasEnded())
                {
                    m_execution.CurrentStateChanged -= Execution_CurrentStateChanged;
                    m_execution = null;
                    button.Enabled = true;
                    checkBox.Checked = false;
                    checkBox.Enabled = true;
                    if (m_mode == Mode.ClickToStop)
                    {
                        checkBox.BackColor = m_buttonColorNormal;
                    }
                }
            }
        }

        private void OnActivateButton()
        {
            if (m_mode == Mode.RunWhilePushed)
            {
                this.StartProcedure();
            }
        }

        private void OnReleaseButton()
        {
            if (m_mode == Mode.RunWhilePushed)
            {
                this.StopProcedure();
            }
        }

        private void checkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox.Checked)
            {
                this.StartProcedure();
            }
            else
            {
                this.StopProcedure();
            }
        }

        private void button_Click(object sender, EventArgs e)
        {
            if (m_mode == Mode.ActivateOnClick)
            {
                this.StartProcedure();
            }
        }

        private void button_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.OnActivateButton();
            }
        }

        private void button_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.OnReleaseButton();
            }
        }

        private void button_KeyDown(object sender, KeyEventArgs e)
        {
            System.Diagnostics.Trace.WriteLine("KeyDown " + e.Modifiers.ToString());
            if (e.KeyCode == Keys.Space && e.Alt == false && e.Control == false && e.Shift == false)
            {
                this.OnActivateButton();
            }
        }

        private void button_KeyUp(object sender, KeyEventArgs e)
        {
            System.Diagnostics.Trace.WriteLine("KeyUp " + e.Modifiers.ToString());
            if (e.KeyCode == Keys.Space && e.Alt == false && e.Control == false && e.Shift == false)
            {
                this.OnReleaseButton();
            }
        }
    }
}
