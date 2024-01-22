namespace StepBro.UI.WinForms.PanelElements
{
    partial class ProcedureActivationButton
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
            button = new Button();
            checkBox = new CheckBox();
            SuspendLayout();
            // 
            // button
            // 
            button.Dock = DockStyle.Fill;
            button.FlatStyle = FlatStyle.Flat;
            button.Location = new Point(0, 0);
            button.Margin = new Padding(2);
            button.Name = "button";
            button.Size = new Size(164, 25);
            button.TabIndex = 0;
            button.Text = "button";
            button.UseVisualStyleBackColor = true;
            button.Click += button_Click;
            button.KeyDown += button_KeyDown;
            button.KeyUp += button_KeyUp;
            button.MouseDown += button_MouseDown;
            button.MouseUp += button_MouseUp;
            // 
            // checkBox
            // 
            checkBox.Appearance = Appearance.Button;
            checkBox.Dock = DockStyle.Fill;
            checkBox.FlatStyle = FlatStyle.Flat;
            checkBox.Location = new Point(0, 0);
            checkBox.Name = "checkBox";
            checkBox.Size = new Size(164, 25);
            checkBox.TabIndex = 1;
            checkBox.Text = "checkBox";
            checkBox.TextAlign = ContentAlignment.MiddleCenter;
            checkBox.UseVisualStyleBackColor = true;
            checkBox.Visible = false;
            checkBox.CheckedChanged += checkBox_CheckedChanged;
            // 
            // ProcedureActivationButton
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Controls.Add(button);
            Controls.Add(checkBox);
            Margin = new Padding(2);
            Name = "ProcedureActivationButton";
            Size = new Size(164, 25);
            ResumeLayout(false);
        }

        #endregion

        private Button button;
        private CheckBox checkBox;
    }
}
