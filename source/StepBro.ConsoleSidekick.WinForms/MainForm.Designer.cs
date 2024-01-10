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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            toolTip = new ToolTip(components);
            timerMasterPull = new System.Windows.Forms.Timer(components);
            toolStripMain = new ToolStrip();
            toolStripComboBoxTool = new ToolStripComboBox();
            toolStripComboBoxToolCommand = new ToolStripComboBox();
            toolStripButtonRunCommand = new ToolStripButton();
            toolStripSeparatorTool = new ToolStripSeparator();
            toolStripSplitButtonRunScript = new ToolStripSplitButton();
            toolStripButtonStopScriptExecution = new ToolStripButton();
            toolStripButtonAddShortcut = new ToolStripButton();
            toolStripSeparatorBeforeShortcuts = new ToolStripSeparator();
            toolStripMain.SuspendLayout();
            SuspendLayout();
            // 
            // timerMasterPull
            // 
            timerMasterPull.Enabled = true;
            timerMasterPull.Interval = 150;
            timerMasterPull.Tick += timerMasterPull_Tick;
            // 
            // toolStripMain
            // 
            toolStripMain.BackColor = Color.DarkOrange;
            toolStripMain.GripStyle = ToolStripGripStyle.Hidden;
            toolStripMain.Items.AddRange(new ToolStripItem[] { toolStripComboBoxTool, toolStripComboBoxToolCommand, toolStripButtonRunCommand, toolStripSeparatorTool, toolStripSplitButtonRunScript, toolStripButtonStopScriptExecution, toolStripButtonAddShortcut, toolStripSeparatorBeforeShortcuts });
            toolStripMain.Location = new Point(0, 0);
            toolStripMain.Name = "toolStripMain";
            toolStripMain.Padding = new Padding(0, 1, 1, 2);
            toolStripMain.Size = new Size(1368, 26);
            toolStripMain.TabIndex = 0;
            // 
            // toolStripComboBoxTool
            // 
            toolStripComboBoxTool.BackColor = SystemColors.Control;
            toolStripComboBoxTool.DropDownStyle = ComboBoxStyle.DropDownList;
            toolStripComboBoxTool.Enabled = false;
            toolStripComboBoxTool.Name = "toolStripComboBoxTool";
            toolStripComboBoxTool.Size = new Size(120, 23);
            toolStripComboBoxTool.SelectedIndexChanged += toolStripComboBoxTool_SelectedIndexChanged;
            // 
            // toolStripComboBoxToolCommand
            // 
            toolStripComboBoxToolCommand.AutoSize = false;
            toolStripComboBoxToolCommand.Enabled = false;
            toolStripComboBoxToolCommand.Name = "toolStripComboBoxToolCommand";
            toolStripComboBoxToolCommand.Size = new Size(250, 23);
            toolStripComboBoxToolCommand.ToolTipText = "Command prompt for the selected tool/object.";
            toolStripComboBoxToolCommand.SelectedIndexChanged += toolStripComboBoxToolCommand_SelectedIndexChanged;
            toolStripComboBoxToolCommand.KeyPress += toolStripComboBoxToolCommand_KeyPress;
            toolStripComboBoxToolCommand.TextChanged += toolStripComboBoxToolCommand_TextChanged;
            // 
            // toolStripButtonRunCommand
            // 
            toolStripButtonRunCommand.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButtonRunCommand.Enabled = false;
            toolStripButtonRunCommand.Image = (Image)resources.GetObject("toolStripButtonRunCommand.Image");
            toolStripButtonRunCommand.ImageTransparentColor = Color.Magenta;
            toolStripButtonRunCommand.Name = "toolStripButtonRunCommand";
            toolStripButtonRunCommand.Size = new Size(23, 20);
            toolStripButtonRunCommand.Text = "P";
            toolStripButtonRunCommand.ToolTipText = "Run Object Command";
            toolStripButtonRunCommand.Click += toolStripButtonRunCommand_Click;
            // 
            // toolStripSeparatorTool
            // 
            toolStripSeparatorTool.Name = "toolStripSeparatorTool";
            toolStripSeparatorTool.Size = new Size(6, 23);
            // 
            // toolStripSplitButtonRunScript
            // 
            toolStripSplitButtonRunScript.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripSplitButtonRunScript.Enabled = false;
            toolStripSplitButtonRunScript.Image = (Image)resources.GetObject("toolStripSplitButtonRunScript.Image");
            toolStripSplitButtonRunScript.ImageTransparentColor = Color.Magenta;
            toolStripSplitButtonRunScript.Name = "toolStripSplitButtonRunScript";
            toolStripSplitButtonRunScript.Size = new Size(111, 20);
            toolStripSplitButtonRunScript.Text = "Run File Element";
            toolStripSplitButtonRunScript.ButtonClick += toolStripSplitButtonRunScript_ButtonClick;
            // 
            // toolStripButtonStopScriptExecution
            // 
            toolStripButtonStopScriptExecution.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButtonStopScriptExecution.Enabled = false;
            toolStripButtonStopScriptExecution.Image = (Image)resources.GetObject("toolStripButtonStopScriptExecution.Image");
            toolStripButtonStopScriptExecution.ImageTransparentColor = Color.Magenta;
            toolStripButtonStopScriptExecution.Name = "toolStripButtonStopScriptExecution";
            toolStripButtonStopScriptExecution.Size = new Size(23, 20);
            toolStripButtonStopScriptExecution.Text = "S";
            toolStripButtonStopScriptExecution.ToolTipText = "Stop the running script execution";
            toolStripButtonStopScriptExecution.Click += toolStripButtonStopScriptExecution_Click;
            // 
            // toolStripButtonAddShortcut
            // 
            toolStripButtonAddShortcut.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButtonAddShortcut.Enabled = false;
            toolStripButtonAddShortcut.Image = (Image)resources.GetObject("toolStripButtonAddShortcut.Image");
            toolStripButtonAddShortcut.ImageTransparentColor = Color.Magenta;
            toolStripButtonAddShortcut.Name = "toolStripButtonAddShortcut";
            toolStripButtonAddShortcut.Size = new Size(23, 20);
            toolStripButtonAddShortcut.Text = "+";
            toolStripButtonAddShortcut.ToolTipText = "Add shortcut button for the selected script execution.";
            toolStripButtonAddShortcut.Click += toolStripButtonAddShortcut_Click;
            // 
            // toolStripSeparatorBeforeShortcuts
            // 
            toolStripSeparatorBeforeShortcuts.Name = "toolStripSeparatorBeforeShortcuts";
            toolStripSeparatorBeforeShortcuts.Size = new Size(6, 23);
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(1368, 30);
            ControlBox = false;
            Controls.Add(toolStripMain);
            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "MainForm";
            ShowIcon = false;
            SizeGripStyle = SizeGripStyle.Hide;
            StartPosition = FormStartPosition.Manual;
            Text = "StepBro Sidekick";
            toolStripMain.ResumeLayout(false);
            toolStripMain.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private ToolTip toolTip;
        private System.Windows.Forms.Timer timerMasterPull;
        private ToolStrip toolStripMain;
        private ToolStripComboBox toolStripComboBoxTool;
        private ToolStripSeparator toolStripSeparatorTool;
        private ToolStripComboBox toolStripComboBoxToolCommand;
        private ToolStripSplitButton toolStripSplitButtonRunScript;
        private ToolStripButton toolStripButtonRunCommand;
        private ToolStripButton toolStripButtonStopScriptExecution;
        private ToolStripButton toolStripButtonAddShortcut;
        private ToolStripSeparator toolStripSeparatorBeforeShortcuts;
    }
}