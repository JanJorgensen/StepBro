namespace StepBro.Workbench
{
    partial class ObjectPanelDockWindow
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
            this.labelErrorMessage = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labelErrorMessage
            // 
            this.labelErrorMessage.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.labelErrorMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelErrorMessage.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelErrorMessage.ForeColor = System.Drawing.Color.Tomato;
            this.labelErrorMessage.Location = new System.Drawing.Point(0, 0);
            this.labelErrorMessage.Name = "labelErrorMessage";
            this.labelErrorMessage.Size = new System.Drawing.Size(800, 450);
            this.labelErrorMessage.TabIndex = 0;
            this.labelErrorMessage.Text = "Panel not loaded";
            this.labelErrorMessage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ObjectPanelDockWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.labelErrorMessage);
            this.Name = "ObjectPanelDockWindow";
            this.Text = "ObjectPanelDockWindow";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelErrorMessage;
    }
}