namespace StepBro.ConsoleSidekick.WinForms
{
    partial class PanelsDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            toolStripContainer = new ToolStripContainer();
            tabControlPanels = new TabControl();
            toolStripContainer.ContentPanel.SuspendLayout();
            toolStripContainer.SuspendLayout();
            SuspendLayout();
            // 
            // toolStripContainer
            // 
            // 
            // toolStripContainer.ContentPanel
            // 
            toolStripContainer.ContentPanel.Controls.Add(tabControlPanels);
            toolStripContainer.ContentPanel.Size = new Size(624, 288);
            toolStripContainer.Dock = DockStyle.Fill;
            toolStripContainer.Location = new Point(0, 0);
            toolStripContainer.Name = "toolStripContainer";
            toolStripContainer.Size = new Size(624, 288);
            toolStripContainer.TabIndex = 0;
            toolStripContainer.Text = "toolStripContainer";
            // 
            // tabControlPanels
            // 
            tabControlPanels.Dock = DockStyle.Fill;
            tabControlPanels.Location = new Point(0, 0);
            tabControlPanels.Name = "tabControlPanels";
            tabControlPanels.SelectedIndex = 0;
            tabControlPanels.Size = new Size(624, 288);
            tabControlPanels.TabIndex = 0;
            // 
            // PanelsDialog
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(624, 288);
            Controls.Add(toolStripContainer);
            Name = "PanelsDialog";
            Text = "StepBro Panels";
            toolStripContainer.ContentPanel.ResumeLayout(false);
            toolStripContainer.ResumeLayout(false);
            toolStripContainer.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private ToolStripContainer toolStripContainer;
        private TabControl tabControlPanels;
    }
}