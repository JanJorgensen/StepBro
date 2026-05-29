using Microsoft.VisualBasic;
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
    public partial class UserInteractionHostPanel : UserControl
    {
        UserInteraction m_interaction = null;

        public UserInteractionHostPanel()
        {
            InitializeComponent();
        }

        public void Setup(UserInteraction interaction)
        {
            m_interaction = interaction;
            m_interaction.OnOpen += Interaction_OnOpen;
            m_interaction.OnClose += Interaction_OnClose;
        }

        private void Interaction_OnOpen(object sender, EventArgs e)
        {
            this.Invoke(this.ShowPanel);
        }

        private void Interaction_OnClose(object sender, EventArgs e)
        {
            m_interaction.OnOpen -= Interaction_OnOpen;
            m_interaction.OnClose -= Interaction_OnClose;
            this.Invoke(this.HidePanel);
        }

        private void ShowPanel()
        {
            SuspendLayout();
            Controls.Clear();
            userInteractionPanel = new UserInteractionPanel();
            userInteractionPanel.Location = new Point(32, 64);      // TODO
            userInteractionPanel.Name = "userInteractionPanel";
            userInteractionPanel.Size = new Size(450, 288);         // TODO
            userInteractionPanel.TabIndex = 0;
            Controls.Add(userInteractionPanel);
            Controls.Add(panelBackground);
            ResumeLayout(false);

            userInteractionPanel.Setup(m_interaction);
            userInteractionPanel.Location = new Point(Math.Max(0, (this.Width - userInteractionPanel.Width) / 2), Math.Max(0, (this.Height - userInteractionPanel.Height) / 2));
        }

        private void HidePanel()
        {
            SuspendLayout();
            foreach (Control control in Controls)
            {
                control.Dispose();
            }
            Controls.Clear();
            Controls.Add(panelBackground);
            ResumeLayout(false);
        }
    }
}
