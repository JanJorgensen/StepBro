namespace StepBro.UI.WinForms.Dialogs
{
    partial class DialogSaveExecutionLog
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
            groupBoxOptions = new GroupBox();
            radioButtonSaveLastExecution = new RadioButton();
            radioButtonSelectedInLogView = new RadioButton();
            radioButtonShownInLogView = new RadioButton();
            radioButtonAfterClear = new RadioButton();
            radioButtonEntireLog = new RadioButton();
            buttonSave = new Button();
            buttonCancel = new Button();
            label1 = new Label();
            linkLabelTargetFolder = new LinkLabel();
            label2 = new Label();
            groupBoxOptions.SuspendLayout();
            SuspendLayout();
            // 
            // groupBoxOptions
            // 
            groupBoxOptions.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBoxOptions.Controls.Add(radioButtonSaveLastExecution);
            groupBoxOptions.Controls.Add(radioButtonSelectedInLogView);
            groupBoxOptions.Controls.Add(radioButtonShownInLogView);
            groupBoxOptions.Controls.Add(radioButtonAfterClear);
            groupBoxOptions.Controls.Add(radioButtonEntireLog);
            groupBoxOptions.Location = new Point(12, 12);
            groupBoxOptions.Name = "groupBoxOptions";
            groupBoxOptions.Size = new Size(465, 155);
            groupBoxOptions.TabIndex = 0;
            groupBoxOptions.TabStop = false;
            groupBoxOptions.Text = "Options";
            // 
            // radioButtonSaveLastExecution
            // 
            radioButtonSaveLastExecution.AutoSize = true;
            radioButtonSaveLastExecution.Checked = true;
            radioButtonSaveLastExecution.Location = new Point(6, 22);
            radioButtonSaveLastExecution.Name = "radioButtonSaveLastExecution";
            radioButtonSaveLastExecution.Size = new Size(168, 19);
            radioButtonSaveLastExecution.TabIndex = 0;
            radioButtonSaveLastExecution.TabStop = true;
            radioButtonSaveLastExecution.Text = "Save current/last execution";
            radioButtonSaveLastExecution.UseVisualStyleBackColor = true;
            radioButtonSaveLastExecution.Click += radioButtonOption_Click;
            // 
            // radioButtonSelectedInLogView
            // 
            radioButtonSelectedInLogView.AutoSize = true;
            radioButtonSelectedInLogView.Enabled = false;
            radioButtonSelectedInLogView.Location = new Point(6, 122);
            radioButtonSelectedInLogView.Name = "radioButtonSelectedInLogView";
            radioButtonSelectedInLogView.Size = new Size(294, 19);
            radioButtonSelectedInLogView.TabIndex = 4;
            radioButtonSelectedInLogView.TabStop = true;
            radioButtonSelectedInLogView.Text = "Save only selected entries in the execution log view";
            radioButtonSelectedInLogView.UseVisualStyleBackColor = true;
            radioButtonSelectedInLogView.Click += radioButtonOption_Click;
            // 
            // radioButtonShownInLogView
            // 
            radioButtonShownInLogView.AutoSize = true;
            radioButtonShownInLogView.Enabled = false;
            radioButtonShownInLogView.Location = new Point(6, 97);
            radioButtonShownInLogView.Name = "radioButtonShownInLogView";
            radioButtonShownInLogView.Size = new Size(259, 19);
            radioButtonShownInLogView.TabIndex = 3;
            radioButtonShownInLogView.TabStop = true;
            radioButtonShownInLogView.Text = "Save what's shown in the execution log view";
            radioButtonShownInLogView.UseVisualStyleBackColor = true;
            radioButtonShownInLogView.Click += radioButtonOption_Click;
            // 
            // radioButtonAfterClear
            // 
            radioButtonAfterClear.AutoSize = true;
            radioButtonAfterClear.Location = new Point(6, 72);
            radioButtonAfterClear.Name = "radioButtonAfterClear";
            radioButtonAfterClear.Size = new Size(143, 19);
            radioButtonAfterClear.TabIndex = 2;
            radioButtonAfterClear.TabStop = true;
            radioButtonAfterClear.Text = "Save all since last clear";
            radioButtonAfterClear.UseVisualStyleBackColor = true;
            radioButtonAfterClear.Click += radioButtonOption_Click;
            // 
            // radioButtonEntireLog
            // 
            radioButtonEntireLog.AutoSize = true;
            radioButtonEntireLog.Location = new Point(6, 47);
            radioButtonEntireLog.Name = "radioButtonEntireLog";
            radioButtonEntireLog.Size = new Size(236, 19);
            radioButtonEntireLog.TabIndex = 1;
            radioButtonEntireLog.TabStop = true;
            radioButtonEntireLog.Text = "Save entire execution log for the session";
            radioButtonEntireLog.UseVisualStyleBackColor = true;
            radioButtonEntireLog.Click += radioButtonOption_Click;
            // 
            // buttonSave
            // 
            buttonSave.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonSave.DialogResult = DialogResult.OK;
            buttonSave.Location = new Point(321, 236);
            buttonSave.Name = "buttonSave";
            buttonSave.Size = new Size(75, 23);
            buttonSave.TabIndex = 2;
            buttonSave.Text = "Save";
            buttonSave.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonCancel.Location = new Point(402, 236);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(75, 23);
            buttonCancel.TabIndex = 3;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label1.AutoSize = true;
            label1.Location = new Point(12, 188);
            label1.Name = "label1";
            label1.Size = new Size(78, 15);
            label1.TabIndex = 3;
            label1.Text = "Target Folder:";
            // 
            // linkLabelTargetFolder
            // 
            linkLabelTargetFolder.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            linkLabelTargetFolder.AutoSize = true;
            linkLabelTargetFolder.Location = new Point(40, 207);
            linkLabelTargetFolder.Name = "linkLabelTargetFolder";
            linkLabelTargetFolder.Size = new Size(74, 15);
            linkLabelTargetFolder.TabIndex = 1;
            linkLabelTargetFolder.TabStop = true;
            linkLabelTargetFolder.Text = "<the folder>";
            linkLabelTargetFolder.LinkClicked += linkLabelTargetFolder_LinkClicked;
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label2.AutoSize = true;
            label2.Location = new Point(12, 247);
            label2.Name = "label2";
            label2.Size = new Size(177, 15);
            label2.TabIndex = 5;
            label2.Text = "TODO: Add different file formats";
            // 
            // DialogSaveExecutionLog
            // 
            AcceptButton = buttonSave;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            CancelButton = buttonCancel;
            ClientSize = new Size(489, 271);
            Controls.Add(label2);
            Controls.Add(linkLabelTargetFolder);
            Controls.Add(label1);
            Controls.Add(buttonCancel);
            Controls.Add(buttonSave);
            Controls.Add(groupBoxOptions);
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "DialogSaveExecutionLog";
            SizeGripStyle = SizeGripStyle.Hide;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Save Execution Log";
            groupBoxOptions.ResumeLayout(false);
            groupBoxOptions.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private GroupBox groupBoxOptions;
        private RadioButton radioButtonShownInLogView;
        private RadioButton radioButtonAfterClear;
        private RadioButton radioButtonEntireLog;
        private RadioButton radioButtonSelectedInLogView;
        private Button buttonSave;
        private Button buttonCancel;
        private Label label1;
        private LinkLabel linkLabelTargetFolder;
        private RadioButton radioButtonSaveLastExecution;
        private Label label2;
    }
}