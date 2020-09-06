namespace StepBro.Workbench
{
    partial class ErrorsWindow
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
            this.parsingErrorListView = new StepBro.Core.Controls.ParsingErrorListView();
            this.SuspendLayout();
            // 
            // parsingErrorListView
            // 
            this.parsingErrorListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.parsingErrorListView.Location = new System.Drawing.Point(0, 0);
            this.parsingErrorListView.Name = "parsingErrorListView";
            this.parsingErrorListView.Size = new System.Drawing.Size(800, 450);
            this.parsingErrorListView.TabIndex = 0;
            this.parsingErrorListView.DoubleClickedLine += new StepBro.Core.Controls.ParsingErrorListView.DoubleClickLineEventHandler(this.parsingErrorListView_DoubleClickedLine);
            // 
            // ErrorsWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.parsingErrorListView);
            this.Name = "ErrorsWindow";
            this.TabText = "Error List";
            this.Text = "Error List";
            this.ResumeLayout(false);

        }

        #endregion

        private StepBro.Core.Controls.ParsingErrorListView parsingErrorListView;
    }
}