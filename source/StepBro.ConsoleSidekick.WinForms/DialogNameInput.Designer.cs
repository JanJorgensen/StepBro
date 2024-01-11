namespace StepBro.ConsoleSidekick.WinForms
{
    partial class DialogNameInput
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
            textBoxString = new TextBox();
            buttonOK = new Button();
            buttonCancel = new Button();
            labelDescription = new Label();
            SuspendLayout();
            // 
            // textBoxString
            // 
            textBoxString.Location = new Point(12, 130);
            textBoxString.Name = "textBoxString";
            textBoxString.Size = new Size(477, 23);
            textBoxString.TabIndex = 0;
            // 
            // buttonOK
            // 
            buttonOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonOK.DialogResult = DialogResult.OK;
            buttonOK.Location = new Point(333, 165);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new Size(75, 23);
            buttonOK.TabIndex = 1;
            buttonOK.Text = "OK";
            buttonOK.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonCancel.Location = new Point(414, 165);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(75, 23);
            buttonCancel.TabIndex = 2;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            // 
            // labelDescription
            // 
            labelDescription.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            labelDescription.Location = new Point(12, 9);
            labelDescription.Name = "labelDescription";
            labelDescription.Size = new Size(477, 118);
            labelDescription.TabIndex = 3;
            labelDescription.Text = "description";
            // 
            // DialogNameInput
            // 
            AcceptButton = buttonOK;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            CancelButton = buttonCancel;
            ClientSize = new Size(501, 200);
            Controls.Add(labelDescription);
            Controls.Add(buttonCancel);
            Controls.Add(buttonOK);
            Controls.Add(textBoxString);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "DialogNameInput";
            SizeGripStyle = SizeGripStyle.Hide;
            Text = "DialogNameInput";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private TextBox textBoxString;
        private Button buttonOK;
        private Button buttonCancel;
        private Label labelDescription;
    }
}