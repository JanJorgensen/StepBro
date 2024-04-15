namespace StepBro.ExecutionHelper
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            textBoxTestCounter = new TextBox();
            timerMasterPull = new System.Windows.Forms.Timer(components);
            SuspendLayout();
            // 
            // textBoxTestCounter
            // 
            textBoxTestCounter.Location = new Point(12, 12);
            textBoxTestCounter.Name = "textBoxTestCounter";
            textBoxTestCounter.Size = new Size(150, 31);
            textBoxTestCounter.TabIndex = 0;
            // 
            // timerMasterPull
            // 
            timerMasterPull.Enabled = true;
            timerMasterPull.Tick += timerMasterPull_Tick;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(144F, 144F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(800, 450);
            Controls.Add(textBoxTestCounter);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = "MainForm";
            Text = "StepBro Execution Helper";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox textBoxTestCounter;
        private System.Windows.Forms.Timer timerMasterPull;
    }
}
