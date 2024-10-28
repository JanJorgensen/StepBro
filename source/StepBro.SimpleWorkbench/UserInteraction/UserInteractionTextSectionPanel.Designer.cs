namespace StepBro.SimpleWorkbench
{
    partial class UserInteractionTextSectionPanel
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
            flowLayoutPanel = new FlowLayoutPanel();
            labelHeader = new Label();
            labelText = new Label();
            flowLayoutPanel.SuspendLayout();
            SuspendLayout();
            // 
            // flowLayoutPanel
            // 
            flowLayoutPanel.Controls.Add(labelHeader);
            flowLayoutPanel.Controls.Add(labelText);
            flowLayoutPanel.Dock = DockStyle.Fill;
            flowLayoutPanel.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel.Location = new Point(0, 0);
            flowLayoutPanel.Name = "flowLayoutPanel";
            flowLayoutPanel.Size = new Size(289, 148);
            flowLayoutPanel.TabIndex = 0;
            // 
            // labelHeader
            // 
            labelHeader.AutoSize = true;
            labelHeader.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labelHeader.Location = new Point(3, 2);
            labelHeader.Margin = new Padding(3, 2, 3, 3);
            labelHeader.Name = "labelHeader";
            labelHeader.Size = new Size(32, 15);
            labelHeader.TabIndex = 0;
            labelHeader.Text = "label";
            // 
            // labelText
            // 
            labelText.AutoSize = true;
            labelText.Location = new Point(16, 22);
            labelText.Margin = new Padding(16, 2, 3, 0);
            labelText.Name = "labelText";
            labelText.Size = new Size(32, 15);
            labelText.TabIndex = 1;
            labelText.Text = "label";
            // 
            // UserInteractionTextSectionPanel
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(flowLayoutPanel);
            Name = "UserInteractionTextSectionPanel";
            Size = new Size(289, 148);
            flowLayoutPanel.ResumeLayout(false);
            flowLayoutPanel.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private FlowLayoutPanel flowLayoutPanel;
        private Label labelHeader;
        private Label labelText;
    }
}
