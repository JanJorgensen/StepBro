namespace StepBro.TestInterface.Controls
{
    partial class CommandTerminal_WinForms
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CommandTerminal_WinForms));
            this.panelBottom = new System.Windows.Forms.Panel();
            this.checkBoxShowCommandButtons = new System.Windows.Forms.CheckBox();
            this.textBoxDelayTime = new System.Windows.Forms.TextBox();
            this.labelDelayTime = new System.Windows.Forms.Label();
            this.comboBoxDelayOption = new System.Windows.Forms.ComboBox();
            this.labelDelayOption = new System.Windows.Forms.Label();
            this.textBoxRepeatCount = new System.Windows.Forms.TextBox();
            this.labelRepeatCount = new System.Windows.Forms.Label();
            this.checkBoxRepeat = new System.Windows.Forms.CheckBox();
            this.buttonSendCommand = new System.Windows.Forms.Button();
            this.checkBoxSend = new System.Windows.Forms.CheckBox();
            this.textBoxCommand = new System.Windows.Forms.TextBox();
            this.buttonSendPrevious = new System.Windows.Forms.Button();
            this.checkBoxSendPrevious = new System.Windows.Forms.CheckBox();
            this.comboBoxPreviousCommands = new System.Windows.Forms.ComboBox();
            this.splitContainerLogWindows = new System.Windows.Forms.SplitContainer();
            this.simpleLogViewFull = new StepBro.Core.Controls.SimpleLogView();
            this.simpleLogViewCommands = new StepBro.Core.Controls.SimpleLogView();
            this.panelCommandButtonInput = new System.Windows.Forms.Panel();
            this.textBoxCommandButtonText = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.toolStripButtonAddCommand = new System.Windows.Forms.ToolStripButton();
            this.toolStripQuickCommands = new System.Windows.Forms.ToolStrip();
            this.timer = new System.Windows.Forms.Timer(this.components);
            this.panelBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerLogWindows)).BeginInit();
            this.splitContainerLogWindows.Panel1.SuspendLayout();
            this.splitContainerLogWindows.Panel2.SuspendLayout();
            this.splitContainerLogWindows.SuspendLayout();
            this.panelCommandButtonInput.SuspendLayout();
            this.toolStripQuickCommands.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.checkBoxShowCommandButtons);
            this.panelBottom.Controls.Add(this.textBoxDelayTime);
            this.panelBottom.Controls.Add(this.labelDelayTime);
            this.panelBottom.Controls.Add(this.comboBoxDelayOption);
            this.panelBottom.Controls.Add(this.labelDelayOption);
            this.panelBottom.Controls.Add(this.textBoxRepeatCount);
            this.panelBottom.Controls.Add(this.labelRepeatCount);
            this.panelBottom.Controls.Add(this.checkBoxRepeat);
            this.panelBottom.Controls.Add(this.buttonSendCommand);
            this.panelBottom.Controls.Add(this.checkBoxSend);
            this.panelBottom.Controls.Add(this.textBoxCommand);
            this.panelBottom.Controls.Add(this.buttonSendPrevious);
            this.panelBottom.Controls.Add(this.checkBoxSendPrevious);
            this.panelBottom.Controls.Add(this.comboBoxPreviousCommands);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 329);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(728, 88);
            this.panelBottom.TabIndex = 0;
            // 
            // checkBoxShowCommandButtons
            // 
            this.checkBoxShowCommandButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxShowCommandButtons.AutoSize = true;
            this.checkBoxShowCommandButtons.Location = new System.Drawing.Point(583, 66);
            this.checkBoxShowCommandButtons.Name = "checkBoxShowCommandButtons";
            this.checkBoxShowCommandButtons.Size = new System.Drawing.Size(142, 17);
            this.checkBoxShowCommandButtons.TabIndex = 9;
            this.checkBoxShowCommandButtons.Text = "Show Command Buttons";
            this.checkBoxShowCommandButtons.UseVisualStyleBackColor = true;
            this.checkBoxShowCommandButtons.CheckedChanged += new System.EventHandler(this.checkBoxShowCommandButtons_CheckedChanged);
            // 
            // textBoxDelayTime
            // 
            this.textBoxDelayTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBoxDelayTime.Enabled = false;
            this.textBoxDelayTime.Location = new System.Drawing.Point(364, 64);
            this.textBoxDelayTime.Name = "textBoxDelayTime";
            this.textBoxDelayTime.Size = new System.Drawing.Size(53, 20);
            this.textBoxDelayTime.TabIndex = 8;
            // 
            // labelDelayTime
            // 
            this.labelDelayTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelDelayTime.AutoSize = true;
            this.labelDelayTime.Enabled = false;
            this.labelDelayTime.Location = new System.Drawing.Point(328, 67);
            this.labelDelayTime.Name = "labelDelayTime";
            this.labelDelayTime.Size = new System.Drawing.Size(30, 13);
            this.labelDelayTime.TabIndex = 7;
            this.labelDelayTime.Text = "Time";
            // 
            // comboBoxDelayOption
            // 
            this.comboBoxDelayOption.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.comboBoxDelayOption.Enabled = false;
            this.comboBoxDelayOption.FormattingEnabled = true;
            this.comboBoxDelayOption.Items.AddRange(new object[] {
            "Await response",
            "After response",
            "Periodic"});
            this.comboBoxDelayOption.Location = new System.Drawing.Point(215, 64);
            this.comboBoxDelayOption.Name = "comboBoxDelayOption";
            this.comboBoxDelayOption.Size = new System.Drawing.Size(103, 21);
            this.comboBoxDelayOption.TabIndex = 6;
            // 
            // labelDelayOption
            // 
            this.labelDelayOption.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelDelayOption.AutoSize = true;
            this.labelDelayOption.Enabled = false;
            this.labelDelayOption.Location = new System.Drawing.Point(175, 67);
            this.labelDelayOption.Name = "labelDelayOption";
            this.labelDelayOption.Size = new System.Drawing.Size(34, 13);
            this.labelDelayOption.TabIndex = 5;
            this.labelDelayOption.Text = "Delay";
            // 
            // textBoxRepeatCount
            // 
            this.textBoxRepeatCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBoxRepeatCount.Enabled = false;
            this.textBoxRepeatCount.Location = new System.Drawing.Point(111, 64);
            this.textBoxRepeatCount.Name = "textBoxRepeatCount";
            this.textBoxRepeatCount.Size = new System.Drawing.Size(54, 20);
            this.textBoxRepeatCount.TabIndex = 4;
            this.textBoxRepeatCount.Text = "<infinite>";
            // 
            // labelRepeatCount
            // 
            this.labelRepeatCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelRepeatCount.AutoSize = true;
            this.labelRepeatCount.Enabled = false;
            this.labelRepeatCount.Location = new System.Drawing.Point(70, 67);
            this.labelRepeatCount.Name = "labelRepeatCount";
            this.labelRepeatCount.Size = new System.Drawing.Size(35, 13);
            this.labelRepeatCount.TabIndex = 3;
            this.labelRepeatCount.Text = "Count";
            // 
            // checkBoxRepeat
            // 
            this.checkBoxRepeat.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxRepeat.AutoSize = true;
            this.checkBoxRepeat.Enabled = false;
            this.checkBoxRepeat.Location = new System.Drawing.Point(3, 66);
            this.checkBoxRepeat.Name = "checkBoxRepeat";
            this.checkBoxRepeat.Size = new System.Drawing.Size(61, 17);
            this.checkBoxRepeat.TabIndex = 2;
            this.checkBoxRepeat.Text = "Repeat";
            this.checkBoxRepeat.UseVisualStyleBackColor = true;
            // 
            // buttonSendCommand
            // 
            this.buttonSendCommand.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSendCommand.Location = new System.Drawing.Point(625, 34);
            this.buttonSendCommand.Name = "buttonSendCommand";
            this.buttonSendCommand.Size = new System.Drawing.Size(100, 24);
            this.buttonSendCommand.TabIndex = 1;
            this.buttonSendCommand.Text = "Send";
            this.buttonSendCommand.UseVisualStyleBackColor = true;
            this.buttonSendCommand.Click += new System.EventHandler(this.buttonSendCommand_Click);
            // 
            // checkBoxSend
            // 
            this.checkBoxSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxSend.Appearance = System.Windows.Forms.Appearance.Button;
            this.checkBoxSend.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.checkBoxSend.Location = new System.Drawing.Point(625, 34);
            this.checkBoxSend.Name = "checkBoxSend";
            this.checkBoxSend.Size = new System.Drawing.Size(100, 24);
            this.checkBoxSend.TabIndex = 2;
            this.checkBoxSend.Text = "Send";
            this.checkBoxSend.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.checkBoxSend.UseVisualStyleBackColor = true;
            this.checkBoxSend.Visible = false;
            // 
            // textBoxCommand
            // 
            this.textBoxCommand.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxCommand.Location = new System.Drawing.Point(3, 37);
            this.textBoxCommand.Name = "textBoxCommand";
            this.textBoxCommand.Size = new System.Drawing.Size(616, 20);
            this.textBoxCommand.TabIndex = 0;
            // 
            // buttonSendPrevious
            // 
            this.buttonSendPrevious.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSendPrevious.Location = new System.Drawing.Point(625, 5);
            this.buttonSendPrevious.Name = "buttonSendPrevious";
            this.buttonSendPrevious.Size = new System.Drawing.Size(100, 24);
            this.buttonSendPrevious.TabIndex = 11;
            this.buttonSendPrevious.Text = "Send Previous";
            this.buttonSendPrevious.UseVisualStyleBackColor = true;
            this.buttonSendPrevious.Click += new System.EventHandler(this.buttonSendPrevious_Click);
            // 
            // checkBoxSendPrevious
            // 
            this.checkBoxSendPrevious.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxSendPrevious.Appearance = System.Windows.Forms.Appearance.Button;
            this.checkBoxSendPrevious.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.checkBoxSendPrevious.Location = new System.Drawing.Point(625, 5);
            this.checkBoxSendPrevious.Name = "checkBoxSendPrevious";
            this.checkBoxSendPrevious.Size = new System.Drawing.Size(100, 24);
            this.checkBoxSendPrevious.TabIndex = 2;
            this.checkBoxSendPrevious.Text = "Send Previous";
            this.checkBoxSendPrevious.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.checkBoxSendPrevious.UseVisualStyleBackColor = true;
            this.checkBoxSendPrevious.Visible = false;
            // 
            // comboBoxPreviousCommands
            // 
            this.comboBoxPreviousCommands.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxPreviousCommands.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxPreviousCommands.FormattingEnabled = true;
            this.comboBoxPreviousCommands.Location = new System.Drawing.Point(3, 8);
            this.comboBoxPreviousCommands.Name = "comboBoxPreviousCommands";
            this.comboBoxPreviousCommands.Size = new System.Drawing.Size(616, 21);
            this.comboBoxPreviousCommands.TabIndex = 10;
            this.comboBoxPreviousCommands.SelectedIndexChanged += new System.EventHandler(this.comboBoxPreviousCommands_SelectedIndexChanged);
            // 
            // splitContainerLogWindows
            // 
            this.splitContainerLogWindows.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerLogWindows.Location = new System.Drawing.Point(0, 0);
            this.splitContainerLogWindows.Name = "splitContainerLogWindows";
            // 
            // splitContainerLogWindows.Panel1
            // 
            this.splitContainerLogWindows.Panel1.Controls.Add(this.simpleLogViewFull);
            // 
            // splitContainerLogWindows.Panel2
            // 
            this.splitContainerLogWindows.Panel2.Controls.Add(this.simpleLogViewCommands);
            this.splitContainerLogWindows.Size = new System.Drawing.Size(728, 329);
            this.splitContainerLogWindows.SplitterDistance = 363;
            this.splitContainerLogWindows.TabIndex = 2;
            // 
            // simpleLogViewFull
            // 
            this.simpleLogViewFull.Dock = System.Windows.Forms.DockStyle.Fill;
            this.simpleLogViewFull.Location = new System.Drawing.Point(0, 0);
            this.simpleLogViewFull.Name = "simpleLogViewFull";
            this.simpleLogViewFull.Size = new System.Drawing.Size(363, 329);
            this.simpleLogViewFull.TabIndex = 0;
            // 
            // simpleLogViewCommands
            // 
            this.simpleLogViewCommands.Dock = System.Windows.Forms.DockStyle.Fill;
            this.simpleLogViewCommands.Location = new System.Drawing.Point(0, 0);
            this.simpleLogViewCommands.Name = "simpleLogViewCommands";
            this.simpleLogViewCommands.Size = new System.Drawing.Size(361, 329);
            this.simpleLogViewCommands.TabIndex = 0;
            // 
            // panelCommandButtonInput
            // 
            this.panelCommandButtonInput.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.panelCommandButtonInput.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelCommandButtonInput.Controls.Add(this.textBoxCommandButtonText);
            this.panelCommandButtonInput.Controls.Add(this.label1);
            this.panelCommandButtonInput.Location = new System.Drawing.Point(256, 261);
            this.panelCommandButtonInput.Name = "panelCommandButtonInput";
            this.panelCommandButtonInput.Size = new System.Drawing.Size(248, 59);
            this.panelCommandButtonInput.TabIndex = 0;
            this.panelCommandButtonInput.Visible = false;
            // 
            // textBoxCommandButtonText
            // 
            this.textBoxCommandButtonText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxCommandButtonText.Location = new System.Drawing.Point(3, 27);
            this.textBoxCommandButtonText.Name = "textBoxCommandButtonText";
            this.textBoxCommandButtonText.Size = new System.Drawing.Size(240, 20);
            this.textBoxCommandButtonText.TabIndex = 1;
            this.textBoxCommandButtonText.Leave += new System.EventHandler(this.textBoxCommandButtonText_Leave);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(112, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Command Button Text";
            // 
            // toolStripButtonAddCommand
            // 
            this.toolStripButtonAddCommand.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButtonAddCommand.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButtonAddCommand.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonAddCommand.Image")));
            this.toolStripButtonAddCommand.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonAddCommand.Name = "toolStripButtonAddCommand";
            this.toolStripButtonAddCommand.Size = new System.Drawing.Size(33, 22);
            this.toolStripButtonAddCommand.Text = "Add";
            this.toolStripButtonAddCommand.ToolTipText = "Add the last executed command to a new command button.";
            this.toolStripButtonAddCommand.Click += new System.EventHandler(this.toolStripButtonAddCommand_Click);
            // 
            // toolStripQuickCommands
            // 
            this.toolStripQuickCommands.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.toolStripQuickCommands.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonAddCommand});
            this.toolStripQuickCommands.Location = new System.Drawing.Point(0, 417);
            this.toolStripQuickCommands.Name = "toolStripQuickCommands";
            this.toolStripQuickCommands.Size = new System.Drawing.Size(728, 25);
            this.toolStripQuickCommands.TabIndex = 1;
            this.toolStripQuickCommands.Text = "toolStrip1";
            this.toolStripQuickCommands.Visible = false;
            // 
            // timer
            // 
            this.timer.Interval = 250;
            this.timer.Tick += new System.EventHandler(this.timer_Tick);
            // 
            // CommandTerminal_WinForms
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.panelCommandButtonInput);
            this.Controls.Add(this.splitContainerLogWindows);
            this.Controls.Add(this.panelBottom);
            this.Controls.Add(this.toolStripQuickCommands);
            this.Name = "CommandTerminal_WinForms";
            this.Size = new System.Drawing.Size(728, 442);
            this.Load += new System.EventHandler(this.CommandTerminal_Load);
            this.panelBottom.ResumeLayout(false);
            this.panelBottom.PerformLayout();
            this.splitContainerLogWindows.Panel1.ResumeLayout(false);
            this.splitContainerLogWindows.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerLogWindows)).EndInit();
            this.splitContainerLogWindows.ResumeLayout(false);
            this.panelCommandButtonInput.ResumeLayout(false);
            this.panelCommandButtonInput.PerformLayout();
            this.toolStripQuickCommands.ResumeLayout(false);
            this.toolStripQuickCommands.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.ComboBox comboBoxDelayOption;
        private System.Windows.Forms.Label labelDelayOption;
        private System.Windows.Forms.TextBox textBoxRepeatCount;
        private System.Windows.Forms.Label labelRepeatCount;
        private System.Windows.Forms.CheckBox checkBoxRepeat;
        private System.Windows.Forms.Button buttonSendPrevious;
        private System.Windows.Forms.Button buttonSendCommand;
        private System.Windows.Forms.ComboBox comboBoxPreviousCommands;
        private System.Windows.Forms.TextBox textBoxCommand;
        private System.Windows.Forms.TextBox textBoxDelayTime;
        private System.Windows.Forms.Label labelDelayTime;
        private System.Windows.Forms.CheckBox checkBoxSend;
        private System.Windows.Forms.CheckBox checkBoxSendPrevious;
        private System.Windows.Forms.CheckBox checkBoxShowCommandButtons;
        private System.Windows.Forms.SplitContainer splitContainerLogWindows;
        private Core.Controls.SimpleLogView simpleLogViewFull;
        private Core.Controls.SimpleLogView simpleLogViewCommands;
        private System.Windows.Forms.Panel panelCommandButtonInput;
        private System.Windows.Forms.TextBox textBoxCommandButtonText;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolStripButton toolStripButtonAddCommand;
        private System.Windows.Forms.ToolStrip toolStripQuickCommands;
        private System.Windows.Forms.Timer timer;
    }
}
