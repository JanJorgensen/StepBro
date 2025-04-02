namespace StepBro.SimpleWorkbench
{
    partial class UserInteractionHostPanel
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            userInteractionPanel = new UserInteractionPanel();
            panelBackground = new Panel();
            SuspendLayout();
            // 
            // userInteractionPanel
            // 
            userInteractionPanel.Location = new Point(32, 64);
            userInteractionPanel.Name = "userInteractionPanel";
            userInteractionPanel.Size = new Size(339, 288);
            userInteractionPanel.TabIndex = 0;
            // 
            // panelBackground
            // 
            panelBackground.BackColor = Color.Wheat;
            panelBackground.Dock = DockStyle.Fill;
            panelBackground.Location = new Point(0, 0);
            panelBackground.Name = "panelBackground";
            panelBackground.Size = new Size(419, 393);
            panelBackground.TabIndex = 1;
            // 
            // UserInteractionHostPanel
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(userInteractionPanel);
            Controls.Add(panelBackground);
            Name = "UserInteractionHostPanel";
            Size = new Size(419, 393);
            ResumeLayout(false);
        }

        #endregion

        private UserInteractionPanel userInteractionPanel;
        private Panel panelBackground;
    }
}
