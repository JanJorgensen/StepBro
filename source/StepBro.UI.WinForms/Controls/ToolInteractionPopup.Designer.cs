namespace StepBro.UI.WinForms.Controls
{
    partial class ToolInteractionPopup
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
            panel = new Panel();
            toolInteractionView = new ToolInteractionView();
            panel.SuspendLayout();
            SuspendLayout();
            // 
            // panel
            // 
            panel.BorderStyle = BorderStyle.FixedSingle;
            panel.Controls.Add(toolInteractionView);
            panel.Dock = DockStyle.Fill;
            panel.Location = new Point(0, 0);
            panel.Name = "panel";
            panel.Size = new Size(239, 460);
            panel.TabIndex = 0;
            // 
            // toolInteractionView
            // 
            toolInteractionView.Dock = DockStyle.Fill;
            toolInteractionView.Location = new Point(0, 0);
            toolInteractionView.Name = "toolInteractionView";
            toolInteractionView.Size = new Size(237, 458);
            toolInteractionView.TabIndex = 0;
            toolInteractionView.TextCommandToolSelected += toolInteractionView_TextCommandToolSelected;
            // 
            // ToolInteractionPopup
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(239, 460);
            Controls.Add(panel);
            FormBorderStyle = FormBorderStyle.None;
            Name = "ToolInteractionPopup";
            StartPosition = FormStartPosition.Manual;
            Text = "ToolInteractionPopup";
            panel.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel panel;
        private ToolInteractionView toolInteractionView;
    }
}