namespace StepBro.ConsoleSidekick.WinForms
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
            comboBoxConnection = new ComboBox();
            comboBoxCommand = new ComboBox();
            buttonExecute = new Button();
            buttonRunScript = new Button();
            comboBoxPartner = new ComboBox();
            comboBoxFileElement = new ComboBox();
            comboBoxScriptFile = new ComboBox();
            toolTip = new ToolTip(components);
            panelMain = new Panel();
            splitContainer = new SplitContainer();
            panelCommander = new Panel();
            buttonMenu = new Button();
            panelExecution = new Panel();
            timerMasterPull = new System.Windows.Forms.Timer(components);
            buttonScriptMenu = new Button();
            panelMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
            splitContainer.Panel1.SuspendLayout();
            splitContainer.Panel2.SuspendLayout();
            splitContainer.SuspendLayout();
            panelCommander.SuspendLayout();
            panelExecution.SuspendLayout();
            SuspendLayout();
            // 
            // comboBoxConnection
            // 
            comboBoxConnection.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            comboBoxConnection.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxConnection.Enabled = false;
            comboBoxConnection.FormattingEnabled = true;
            comboBoxConnection.Location = new Point(64, 3);
            comboBoxConnection.Name = "comboBoxConnection";
            comboBoxConnection.Size = new Size(355, 23);
            comboBoxConnection.TabIndex = 1;
            toolTip.SetToolTip(comboBoxConnection, "Command target");
            comboBoxConnection.SelectedIndexChanged += comboBoxConnection_SelectedIndexChanged;
            // 
            // comboBoxCommand
            // 
            comboBoxCommand.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            comboBoxCommand.Enabled = false;
            comboBoxCommand.FormattingEnabled = true;
            comboBoxCommand.Location = new Point(3, 32);
            comboBoxCommand.Name = "comboBoxCommand";
            comboBoxCommand.Size = new Size(335, 23);
            comboBoxCommand.TabIndex = 2;
            toolTip.SetToolTip(comboBoxCommand, "Command");
            comboBoxCommand.SelectedIndexChanged += comboBoxCommand_SelectedIndexChanged;
            comboBoxCommand.TextChanged += comboBoxCommand_TextChanged;
            comboBoxCommand.KeyPress += comboBoxCommand_KeyPress;
            // 
            // buttonExecute
            // 
            buttonExecute.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonExecute.Location = new Point(344, 31);
            buttonExecute.Name = "buttonExecute";
            buttonExecute.Size = new Size(75, 23);
            buttonExecute.TabIndex = 3;
            buttonExecute.Text = "Execute";
            toolTip.SetToolTip(buttonExecute, "Execute the selected command");
            buttonExecute.UseVisualStyleBackColor = true;
            buttonExecute.Click += buttonExecute_Click;
            buttonExecute.KeyPress += comboBoxCommand_KeyPress;
            // 
            // buttonRunScript
            // 
            buttonRunScript.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonRunScript.Enabled = false;
            buttonRunScript.Location = new Point(524, 31);
            buttonRunScript.Name = "buttonRunScript";
            buttonRunScript.Size = new Size(75, 23);
            buttonRunScript.TabIndex = 3;
            buttonRunScript.Text = "Run";
            toolTip.SetToolTip(buttonRunScript, "Execute the selected command");
            buttonRunScript.UseVisualStyleBackColor = true;
            buttonRunScript.Click += buttonRunScript_Click;
            // 
            // comboBoxPartner
            // 
            comboBoxPartner.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            comboBoxPartner.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxPartner.Enabled = false;
            comboBoxPartner.FormattingEnabled = true;
            comboBoxPartner.Location = new Point(375, 31);
            comboBoxPartner.Name = "comboBoxPartner";
            comboBoxPartner.Size = new Size(143, 23);
            comboBoxPartner.TabIndex = 2;
            comboBoxPartner.SelectedIndexChanged += comboBoxPartner_SelectedIndexChanged;
            // 
            // comboBoxFileElement
            // 
            comboBoxFileElement.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            comboBoxFileElement.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxFileElement.Enabled = false;
            comboBoxFileElement.FormattingEnabled = true;
            comboBoxFileElement.Location = new Point(3, 32);
            comboBoxFileElement.Name = "comboBoxFileElement";
            comboBoxFileElement.Size = new Size(366, 23);
            comboBoxFileElement.TabIndex = 1;
            toolTip.SetToolTip(comboBoxFileElement, "Selection of the script file element");
            comboBoxFileElement.SelectedIndexChanged += comboBoxFileElement_SelectedIndexChanged;
            // 
            // comboBoxScriptFile
            // 
            comboBoxScriptFile.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            comboBoxScriptFile.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxScriptFile.FormattingEnabled = true;
            comboBoxScriptFile.Location = new Point(64, 3);
            comboBoxScriptFile.Name = "comboBoxScriptFile";
            comboBoxScriptFile.Size = new Size(535, 23);
            comboBoxScriptFile.TabIndex = 0;
            toolTip.SetToolTip(comboBoxScriptFile, "Script file selection");
            comboBoxScriptFile.SelectedIndexChanged += comboBoxScriptFile_SelectedIndexChanged;
            // 
            // panelMain
            // 
            panelMain.BorderStyle = BorderStyle.FixedSingle;
            panelMain.Controls.Add(splitContainer);
            panelMain.Dock = DockStyle.Fill;
            panelMain.Location = new Point(0, 0);
            panelMain.Name = "panelMain";
            panelMain.Size = new Size(1030, 62);
            panelMain.TabIndex = 4;
            // 
            // splitContainer
            // 
            splitContainer.Dock = DockStyle.Fill;
            splitContainer.Location = new Point(0, 0);
            splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            splitContainer.Panel1.Controls.Add(panelCommander);
            splitContainer.Panel1MinSize = 200;
            // 
            // splitContainer.Panel2
            // 
            splitContainer.Panel2.Controls.Add(panelExecution);
            splitContainer.Size = new Size(1028, 60);
            splitContainer.SplitterDistance = 422;
            splitContainer.TabIndex = 6;
            // 
            // panelCommander
            // 
            panelCommander.BackColor = Color.Black;
            panelCommander.Controls.Add(buttonMenu);
            panelCommander.Controls.Add(comboBoxConnection);
            panelCommander.Controls.Add(comboBoxCommand);
            panelCommander.Controls.Add(buttonExecute);
            panelCommander.Dock = DockStyle.Fill;
            panelCommander.Location = new Point(0, 0);
            panelCommander.Name = "panelCommander";
            panelCommander.Size = new Size(422, 60);
            panelCommander.TabIndex = 4;
            // 
            // buttonMenu
            // 
            buttonMenu.Enabled = false;
            buttonMenu.Location = new Point(3, 2);
            buttonMenu.Name = "buttonMenu";
            buttonMenu.Size = new Size(55, 23);
            buttonMenu.TabIndex = 0;
            buttonMenu.Text = "Menu";
            buttonMenu.UseVisualStyleBackColor = true;
            buttonMenu.Click += buttonMenu_Click;
            // 
            // panelExecution
            // 
            panelExecution.BackColor = Color.FromArgb(255, 128, 0);
            panelExecution.Controls.Add(buttonScriptMenu);
            panelExecution.Controls.Add(buttonRunScript);
            panelExecution.Controls.Add(comboBoxScriptFile);
            panelExecution.Controls.Add(comboBoxPartner);
            panelExecution.Controls.Add(comboBoxFileElement);
            panelExecution.Dock = DockStyle.Fill;
            panelExecution.Location = new Point(0, 0);
            panelExecution.Name = "panelExecution";
            panelExecution.Size = new Size(602, 60);
            panelExecution.TabIndex = 5;
            // 
            // timerMasterPull
            // 
            timerMasterPull.Enabled = true;
            timerMasterPull.Tick += timerMasterPull_Tick;
            // 
            // buttonScriptMenu
            // 
            buttonScriptMenu.Enabled = false;
            buttonScriptMenu.Location = new Point(3, 3);
            buttonScriptMenu.Name = "buttonScriptMenu";
            buttonScriptMenu.Size = new Size(55, 23);
            buttonScriptMenu.TabIndex = 4;
            buttonScriptMenu.Text = "Menu";
            buttonScriptMenu.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(1030, 62);
            ControlBox = false;
            Controls.Add(panelMain);
            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new Size(400, 62);
            Name = "MainForm";
            ShowIcon = false;
            SizeGripStyle = SizeGripStyle.Hide;
            StartPosition = FormStartPosition.Manual;
            Text = "StepBro Sidekick";
            panelMain.ResumeLayout(false);
            splitContainer.Panel1.ResumeLayout(false);
            splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
            splitContainer.ResumeLayout(false);
            panelCommander.ResumeLayout(false);
            panelExecution.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private ComboBox comboBoxConnection;
        private ComboBox comboBoxCommand;
        private Button buttonExecute;
        private ToolTip toolTip;
        private ComboBox comboBoxPartner;
        private ComboBox comboBoxFileElement;
        private ComboBox comboBoxScriptFile;
        private Button buttonRunScript;
        private Panel panelMain;
        private Panel panelCommander;
        private SplitContainer splitContainer;
        private Panel panelExecution;
        private Button buttonMenu;
        private System.Windows.Forms.Timer timerMasterPull;
        private Button buttonScriptMenu;
    }
}