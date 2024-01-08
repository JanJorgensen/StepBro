namespace StepBro.UI.WinForms.PanelElements
{
    partial class ProcedureActivationCheckbox
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
            checkBox = new CheckBox();
            SuspendLayout();
            // 
            // checkBox
            // 
            checkBox.AutoSize = true;
            checkBox.Location = new Point(0, 0);
            checkBox.Name = "checkBox";
            checkBox.Size = new Size(77, 19);
            checkBox.TabIndex = 0;
            checkBox.Text = "checkBox";
            checkBox.UseVisualStyleBackColor = true;
            checkBox.CheckedChanged += checkBox_CheckedChanged;
            // 
            // ProcedureActivationCheckbox
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Controls.Add(checkBox);
            Name = "ProcedureActivationCheckbox";
            Size = new Size(150, 19);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private CheckBox checkBox;
    }
}
