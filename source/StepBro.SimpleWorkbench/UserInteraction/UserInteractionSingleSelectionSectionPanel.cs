using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StepBro.SimpleWorkbench
{
    public partial class UserInteractionSingleSelectionSectionPanel : UserControl
    {
        private bool m_initialValues = true;
        private int m_panelInitialHeight = 0;
        private int m_groupBoxInitialHeight = 0;
        private int m_groupBoxEmptyHeight = 0;
        private int m_firstOptionHeight = 0;
        private int m_firstOptionLeft = 0;
        private int m_firstOptionTop = 0;
        private int m_optionsVerticalSpacing = 0;

        public UserInteractionSingleSelectionSectionPanel()
        {
            InitializeComponent();
        }

        public EventHandler SelectionChanged;

        public void SetHeader(string text)
        {
            this.SetupIfJustOpened();
            groupBox.Text = String.IsNullOrEmpty(text) ? "Options" : text;
        }

        public void AddOption(string text)
        {
            this.SetupIfJustOpened();

            groupBox.SuspendLayout();
            SuspendLayout();

            var radioButton = new RadioButton();
            radioButton.AutoSize = true;
            radioButton.Location = new Point(m_firstOptionLeft, m_firstOptionTop + (groupBox.Controls.Count * m_optionsVerticalSpacing));
            radioButton.Name = "option" + groupBox.Controls.Count.ToString(); ;
            radioButton.Size = new Size(94, m_firstOptionHeight);
            radioButton.TabIndex = groupBox.Controls.Count;
            radioButton.TabStop = true;
            radioButton.Text = text;
            radioButton.UseVisualStyleBackColor = true;
            radioButton.CheckedChanged += RadioButton_CheckedChanged;
            radioButton.Tag = groupBox.Controls.Count;
            groupBox.Controls.Add(radioButton);
            groupBox.Height = m_groupBoxEmptyHeight + (m_optionsVerticalSpacing * groupBox.Controls.Count);
            this.Height = groupBox.Height + (m_panelInitialHeight - m_groupBoxInitialHeight);

            groupBox.ResumeLayout(false);
            groupBox.PerformLayout();
            ResumeLayout(false);
        }

        private void RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            var button = (RadioButton)sender;
            if (button.Checked)
            {
                this.CurrentSelection = (int)button.Tag;
                this.SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void SetupIfJustOpened()
        {
            if (m_initialValues)
            {
                m_panelInitialHeight = this.Height;
                m_groupBoxInitialHeight = groupBox.Height;
                m_firstOptionHeight = radioButton1.Height;
                m_firstOptionLeft = radioButton1.Left;
                m_firstOptionTop = radioButton1.Top;
                m_optionsVerticalSpacing = radioButton2.Top - radioButton1.Top;
                m_groupBoxEmptyHeight = groupBox.Height - (m_optionsVerticalSpacing * 2);
                groupBox.Controls.Clear();

                m_initialValues = false;
            }
        }

        public int CurrentSelection { get; private set; } = -1;
    }
}
