namespace StepBro.UI.WinForms
{
    partial class DialogAddShortcut
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
            groupBoxShortcutSelection = new GroupBox();
            labelCommandDetails = new Label();
            radioButtonObjectCommand = new RadioButton();
            labelProcedureDetails = new Label();
            radioButtonProcedureExecution = new RadioButton();
            labelButtonText = new Label();
            groupBoxShortcutSelection.SuspendLayout();
            SuspendLayout();
            // 
            // textBoxString
            // 
            textBoxString.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            textBoxString.Location = new Point(12, 178);
            textBoxString.Name = "textBoxString";
            textBoxString.Size = new Size(416, 23);
            textBoxString.TabIndex = 2;
            // 
            // buttonOK
            // 
            buttonOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonOK.DialogResult = DialogResult.OK;
            buttonOK.Location = new Point(272, 213);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new Size(75, 23);
            buttonOK.TabIndex = 3;
            buttonOK.Text = "OK";
            buttonOK.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonCancel.Location = new Point(353, 213);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(75, 23);
            buttonCancel.TabIndex = 4;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            // 
            // groupBoxShortcutSelection
            // 
            groupBoxShortcutSelection.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBoxShortcutSelection.Controls.Add(labelCommandDetails);
            groupBoxShortcutSelection.Controls.Add(radioButtonObjectCommand);
            groupBoxShortcutSelection.Controls.Add(labelProcedureDetails);
            groupBoxShortcutSelection.Controls.Add(radioButtonProcedureExecution);
            groupBoxShortcutSelection.Location = new Point(12, 12);
            groupBoxShortcutSelection.Name = "groupBoxShortcutSelection";
            groupBoxShortcutSelection.Size = new Size(416, 136);
            groupBoxShortcutSelection.TabIndex = 0;
            groupBoxShortcutSelection.TabStop = false;
            groupBoxShortcutSelection.Text = "Shortcut selection";
            // 
            // labelCommandDetails
            // 
            labelCommandDetails.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            labelCommandDetails.Location = new Point(28, 92);
            labelCommandDetails.Name = "labelCommandDetails";
            labelCommandDetails.Size = new Size(382, 23);
            labelCommandDetails.TabIndex = 3;
            labelCommandDetails.Text = "command";
            // 
            // radioButtonObjectCommand
            // 
            radioButtonObjectCommand.AutoSize = true;
            radioButtonObjectCommand.Location = new Point(6, 70);
            radioButtonObjectCommand.Name = "radioButtonObjectCommand";
            radioButtonObjectCommand.Size = new Size(175, 19);
            radioButtonObjectCommand.TabIndex = 2;
            radioButtonObjectCommand.Text = "Object Command Execution";
            radioButtonObjectCommand.UseVisualStyleBackColor = true;
            radioButtonObjectCommand.CheckedChanged += radioButtonObjectCommand_CheckedChanged;
            // 
            // labelProcedureDetails
            // 
            labelProcedureDetails.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            labelProcedureDetails.Location = new Point(28, 44);
            labelProcedureDetails.Name = "labelProcedureDetails";
            labelProcedureDetails.Size = new Size(382, 23);
            labelProcedureDetails.TabIndex = 1;
            labelProcedureDetails.Text = "procedure";
            // 
            // radioButtonProcedureExecution
            // 
            radioButtonProcedureExecution.AutoSize = true;
            radioButtonProcedureExecution.Checked = true;
            radioButtonProcedureExecution.Location = new Point(6, 22);
            radioButtonProcedureExecution.Name = "radioButtonProcedureExecution";
            radioButtonProcedureExecution.Size = new Size(134, 19);
            radioButtonProcedureExecution.TabIndex = 0;
            radioButtonProcedureExecution.TabStop = true;
            radioButtonProcedureExecution.Text = "Procedure Execution";
            radioButtonProcedureExecution.UseVisualStyleBackColor = true;
            radioButtonProcedureExecution.CheckedChanged += radioButtonProcedureExecution_CheckedChanged;
            // 
            // labelButtonText
            // 
            labelButtonText.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            labelButtonText.AutoSize = true;
            labelButtonText.Location = new Point(12, 160);
            labelButtonText.Name = "labelButtonText";
            labelButtonText.Size = new Size(67, 15);
            labelButtonText.TabIndex = 1;
            labelButtonText.Text = "Button Text";
            // 
            // DialogAddShortcut
            // 
            AcceptButton = buttonOK;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            CancelButton = buttonCancel;
            ClientSize = new Size(440, 248);
            Controls.Add(labelButtonText);
            Controls.Add(groupBoxShortcutSelection);
            Controls.Add(buttonCancel);
            Controls.Add(buttonOK);
            Controls.Add(textBoxString);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "DialogAddShortcut";
            SizeGripStyle = SizeGripStyle.Hide;
            Text = "DialogNameInput";
            groupBoxShortcutSelection.ResumeLayout(false);
            groupBoxShortcutSelection.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private TextBox textBoxString;
        private Button buttonOK;
        private Button buttonCancel;
        private GroupBox groupBoxShortcutSelection;
        private RadioButton radioButtonProcedureExecution;
        private RadioButton radioButtonObjectCommand;
        private Label labelProcedureDetails;
        private Label labelCommandDetails;
        private Label labelButtonText;
    }
}