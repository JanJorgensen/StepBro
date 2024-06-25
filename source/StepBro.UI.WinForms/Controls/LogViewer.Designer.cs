namespace StepBro.UI.WinForms.Controls
{
    partial class LogViewer
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
            toolStrip1 = new ToolStrip();
            logView = new ChronoListView();
            SuspendLayout();
            // 
            // toolStrip1
            // 
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(636, 25);
            toolStrip1.TabIndex = 0;
            toolStrip1.Text = "toolStrip1";
            // 
            // logView
            // 
            logView.Dock = DockStyle.Fill;
            logView.Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            logView.Location = new Point(0, 25);
            logView.Name = "logView";
            logView.Size = new Size(636, 204);
            logView.TabIndex = 1;
            logView.TailMode = true;
            logView.ZeroTime = new DateTime(2024, 6, 24, 10, 25, 15, 655);
            // 
            // LogViewer
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(logView);
            Controls.Add(toolStrip1);
            Name = "LogViewer";
            Size = new Size(636, 229);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ToolStrip toolStrip1;
        private ChronoListView logView;
    }
}
