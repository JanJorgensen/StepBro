using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static StepBro.Core.Host.UserInteraction;

namespace StepBro.SimpleWorkbench
{
    public partial class UserInteractionPanel : UserControl
    {
        UserInteraction m_interactionData = null;

        public UserInteractionPanel()
        {
            InitializeComponent();
        }

        public void Setup(UserInteraction interactionData)
        {
            m_interactionData = interactionData;

            if (!String.IsNullOrEmpty(m_interactionData.HeaderText))
            {
                labelHeader.Text = m_interactionData.HeaderText;
            }

            buttonOK.Visible = m_interactionData.ShowOK;
            buttonYes.Visible = m_interactionData.ShowYesNo;
            buttonNo.Visible = m_interactionData.ShowYesNo;
            buttonCancel.Visible = m_interactionData.ShowCancel;

            flowLayoutPanelSections.Controls.Clear();
            int sectionsHeight = 0;
            foreach (var section in m_interactionData.ListSections())
            {
                if (section is SectionTextBlock textData)
                {
                    var sectionPanel = new UserInteractionTextSectionPanel();
                    sectionPanel.SetHeader(textData.Header);
                    sectionPanel.SetText(textData.Text);
                    flowLayoutPanelSections.Controls.Add(sectionPanel);
                    sectionsHeight += (sectionPanel.Height + sectionPanel.Margin.Top + sectionPanel.Margin.Bottom);
                }
                else if (section is SectionSingleSelection selection)
                {
                    var sectionPanel = new UserInteractionSingleSelectionSectionPanel();
                    sectionPanel.Tag = selection.Tag;
                    sectionPanel.SetHeader(selection.Header);
                    foreach (var option in selection.Options)
                    {
                        sectionPanel.AddOption(option);
                    }
                    sectionPanel.SelectionChanged += SingleSelectionSectionPanel_SelectionChanged;
                    flowLayoutPanelSections.Controls.Add(sectionPanel);
                    sectionsHeight += (sectionPanel.Height + sectionPanel.Margin.Top + sectionPanel.Margin.Bottom);
                }
            }

            var sectionPanelOptimalHeight = sectionsHeight;
            if (!flowLayoutPanelSections.MinimumSize.IsEmpty && flowLayoutPanelSections.MinimumSize.Height > sectionPanelOptimalHeight) sectionPanelOptimalHeight = flowLayoutPanelSections.MinimumSize.Height;
            if (!flowLayoutPanelSections.MaximumSize.IsEmpty && flowLayoutPanelSections.MaximumSize.Height < sectionPanelOptimalHeight) sectionPanelOptimalHeight = flowLayoutPanelSections.MaximumSize.Height;
            this.Size = new Size(this.Size.Width, this.Size.Height + (sectionPanelOptimalHeight - flowLayoutPanelSections.Height));
            //this.Height += (sectionPanelOptimalHeight - flowLayoutPanelSections.Height);
        }

        private void SingleSelectionSectionPanel_SelectionChanged(object sender, EventArgs e)
        {
            var sectionPanel = sender as UserInteractionSingleSelectionSectionPanel;
            m_interactionData.NotifySelection((string)sectionPanel.Tag, sectionPanel.CurrentSelection);
        }


        private void buttonOK_Click(object sender, EventArgs e)
        {
            m_interactionData.NotifyClose(Core.Host.UserResponse.OK);
        }

        private void buttonYes_Click(object sender, EventArgs e)
        {
            m_interactionData.NotifyClose(Core.Host.UserResponse.Yes);
        }

        private void buttonNo_Click(object sender, EventArgs e)
        {
            m_interactionData.NotifyClose(Core.Host.UserResponse.No);
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            m_interactionData.NotifyClose(Core.Host.UserResponse.Cancel);
        }
    }
}
