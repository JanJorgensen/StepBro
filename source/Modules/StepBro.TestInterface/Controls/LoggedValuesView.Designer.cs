namespace StepBro.TestInterface.Controls
{
    partial class LoggedValuesView
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
            this.dataView = new StepBro.Core.Controls.DataView();
            this.SuspendLayout();
            // 
            // dataView
            // 
            this.dataView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataView.Location = new System.Drawing.Point(0, 0);
            this.dataView.Name = "dataView";
            this.dataView.Size = new System.Drawing.Size(501, 387);
            this.dataView.TabIndex = 0;
            // 
            // LoggedValuesView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dataView);
            this.Name = "LoggedValuesView";
            this.Size = new System.Drawing.Size(501, 387);
            this.ResumeLayout(false);

        }

        #endregion

        private StepBro.Core.Controls.DataView dataView;
    }
}
