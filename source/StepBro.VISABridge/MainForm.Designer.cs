namespace StepBro.VISABridge
{
    partial class MainForm
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
            this.buttonOpenSession = new System.Windows.Forms.Button();
            this.buttonCloseSession = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxCommand = new System.Windows.Forms.TextBox();
            this.buttonQuery = new System.Windows.Forms.Button();
            this.buttonWrite = new System.Windows.Forms.Button();
            this.buttonRead = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.richTextBoxHistory = new System.Windows.Forms.RichTextBox();
            this.checkBoxAutomated = new System.Windows.Forms.CheckBox();
            this.buttonClearHistory = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonOpenSession
            // 
            this.buttonOpenSession.Location = new System.Drawing.Point(12, 12);
            this.buttonOpenSession.Name = "buttonOpenSession";
            this.buttonOpenSession.Size = new System.Drawing.Size(123, 23);
            this.buttonOpenSession.TabIndex = 0;
            this.buttonOpenSession.Text = "Open Session";
            this.buttonOpenSession.UseVisualStyleBackColor = true;
            this.buttonOpenSession.Click += new System.EventHandler(this.buttonOpenSession_Click);
            // 
            // buttonCloseSession
            // 
            this.buttonCloseSession.Location = new System.Drawing.Point(141, 12);
            this.buttonCloseSession.Name = "buttonCloseSession";
            this.buttonCloseSession.Size = new System.Drawing.Size(123, 23);
            this.buttonCloseSession.TabIndex = 0;
            this.buttonCloseSession.Text = "Close Session";
            this.buttonCloseSession.UseVisualStyleBackColor = true;
            this.buttonCloseSession.Click += new System.EventHandler(this.buttonCloseSession_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 51);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Command:";
            // 
            // textBoxCommand
            // 
            this.textBoxCommand.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxCommand.Location = new System.Drawing.Point(12, 67);
            this.textBoxCommand.Name = "textBoxCommand";
            this.textBoxCommand.Size = new System.Drawing.Size(340, 20);
            this.textBoxCommand.TabIndex = 2;
            // 
            // buttonQuery
            // 
            this.buttonQuery.Location = new System.Drawing.Point(12, 93);
            this.buttonQuery.Name = "buttonQuery";
            this.buttonQuery.Size = new System.Drawing.Size(75, 23);
            this.buttonQuery.TabIndex = 3;
            this.buttonQuery.Text = "Query";
            this.buttonQuery.UseVisualStyleBackColor = true;
            this.buttonQuery.Click += new System.EventHandler(this.buttonQuery_Click);
            // 
            // buttonWrite
            // 
            this.buttonWrite.Location = new System.Drawing.Point(93, 93);
            this.buttonWrite.Name = "buttonWrite";
            this.buttonWrite.Size = new System.Drawing.Size(75, 23);
            this.buttonWrite.TabIndex = 4;
            this.buttonWrite.Text = "Write";
            this.buttonWrite.UseVisualStyleBackColor = true;
            this.buttonWrite.Click += new System.EventHandler(this.buttonWrite_Click);
            // 
            // buttonRead
            // 
            this.buttonRead.Location = new System.Drawing.Point(174, 93);
            this.buttonRead.Name = "buttonRead";
            this.buttonRead.Size = new System.Drawing.Size(75, 23);
            this.buttonRead.TabIndex = 5;
            this.buttonRead.Text = "Read";
            this.buttonRead.UseVisualStyleBackColor = true;
            this.buttonRead.Click += new System.EventHandler(this.buttonRead_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 131);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(39, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "History";
            // 
            // richTextBoxHistory
            // 
            this.richTextBoxHistory.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBoxHistory.HideSelection = false;
            this.richTextBoxHistory.Location = new System.Drawing.Point(12, 147);
            this.richTextBoxHistory.Name = "richTextBoxHistory";
            this.richTextBoxHistory.ReadOnly = true;
            this.richTextBoxHistory.Size = new System.Drawing.Size(340, 117);
            this.richTextBoxHistory.TabIndex = 7;
            this.richTextBoxHistory.Text = "";
            // 
            // checkBoxAutomated
            // 
            this.checkBoxAutomated.AutoSize = true;
            this.checkBoxAutomated.Enabled = false;
            this.checkBoxAutomated.Location = new System.Drawing.Point(270, 16);
            this.checkBoxAutomated.Name = "checkBoxAutomated";
            this.checkBoxAutomated.Size = new System.Drawing.Size(77, 17);
            this.checkBoxAutomated.TabIndex = 8;
            this.checkBoxAutomated.Text = "Automated";
            this.checkBoxAutomated.UseVisualStyleBackColor = true;
            // 
            // buttonClearHistory
            // 
            this.buttonClearHistory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClearHistory.Location = new System.Drawing.Point(277, 118);
            this.buttonClearHistory.Name = "buttonClearHistory";
            this.buttonClearHistory.Size = new System.Drawing.Size(75, 23);
            this.buttonClearHistory.TabIndex = 9;
            this.buttonClearHistory.Text = "Clear";
            this.buttonClearHistory.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(364, 276);
            this.Controls.Add(this.buttonClearHistory);
            this.Controls.Add(this.checkBoxAutomated);
            this.Controls.Add(this.richTextBoxHistory);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.buttonRead);
            this.Controls.Add(this.buttonWrite);
            this.Controls.Add(this.buttonQuery);
            this.Controls.Add(this.textBoxCommand);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonCloseSession);
            this.Controls.Add(this.buttonOpenSession);
            this.MinimumSize = new System.Drawing.Size(380, 315);
            this.Name = "MainForm";
            this.Text = "StepBro VISA Bridge";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonOpenSession;
        private System.Windows.Forms.Button buttonCloseSession;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxCommand;
        private System.Windows.Forms.Button buttonQuery;
        private System.Windows.Forms.Button buttonWrite;
        private System.Windows.Forms.Button buttonRead;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RichTextBox richTextBoxHistory;
        private System.Windows.Forms.CheckBox checkBoxAutomated;
        private System.Windows.Forms.Button buttonClearHistory;
    }
}

