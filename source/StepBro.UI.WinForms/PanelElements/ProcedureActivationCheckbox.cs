using StepBro.Core.Data;
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
    public partial class ProcedureActivationCheckbox : PanelElementBase
    {
        private ProcedureInfo m_changeProcedure;
        private ProcedureInfo m_setProcedure;
        private ProcedureInfo m_resetProcedure;
        private ProcedureInfo m_enabledCheckProcedure;

        public ProcedureActivationCheckbox()
        {
            InitializeComponent();
        }

        protected override bool Setup(PanelElementBase parent, PropertyBlock definition)
        {
            checkBox.Text = definition.Name;
            base.Setup(parent, definition);
            foreach (var field in definition)
            {
                if (field.BlockEntryType == PropertyBlockEntryType.Value)
                {
                    var valueField = field as PropertyBlockValue;
                    if (valueField.Name == "Text")
                    {
                        checkBox.Text = valueField.ValueAsString();
                    }
                    else if (valueField.Name == "Activate")
                    {
                        if (m_setProcedure == null)
                        {
                            m_setProcedure = new ProcedureInfo();
                        }
                        m_setProcedure.Name = valueField.ValueAsString();
                    }
                    else if (valueField.Name == "Deactivate")
                    {
                        if (m_resetProcedure == null)
                        {
                            m_resetProcedure = new ProcedureInfo();
                        }
                        m_resetProcedure.Name = valueField.ValueAsString();
                    }
                }
                else if (field.BlockEntryType == PropertyBlockEntryType.Flag)
                {
                    var flagField = field as PropertyBlockFlag;
                    //if (flagField.Name == "Stoppable")
                    //{
                    //    m_mode = Mode.ClickToStop;
                    //    button.Hide();
                    //    checkBox.Show();
                    //}
                    //else if (flagField.Name == "StopOnButtonRelease")
                    //{
                    //    m_mode = Mode.RunWhilePushed;
                    //}
                }
            }
            return true;
        }

        private void checkBox_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
