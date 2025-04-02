namespace StepBro.UI.WinForms.Controls
{
    partial class LogViewer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LogViewer));
            toolStrip1 = new ToolStrip();
            toolStripButtonClear = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            toolStripDropDownButtonDisplayLevels = new ToolStripDropDownButton();
            toolStripMenuItemLevels2 = new ToolStripMenuItem();
            toolStripMenuItemLevels3 = new ToolStripMenuItem();
            toolStripMenuItemLevels4 = new ToolStripMenuItem();
            toolStripMenuItemLevels5 = new ToolStripMenuItem();
            toolStripMenuItemLevels6 = new ToolStripMenuItem();
            toolStripMenuItemLevelsAll = new ToolStripMenuItem();
            toolStripButtonFollowHead = new ToolStripButton();
            toolStripDropDownButtonLoggers = new ToolStripDropDownButton();
            toolStripMenuItemSpecialLoggersNoneAvailable = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            toolStripDropDownButtonSkipSelection = new ToolStripDropDownButton();
            toolStripMenuItemSkipSearchMatches = new ToolStripMenuItem();
            toolStripMenuItemSkipError = new ToolStripMenuItem();
            toolStripMenuItemSkipScriptExecutionStart = new ToolStripMenuItem();
            toolStripMenuItemSkipMeasurement = new ToolStripMenuItem();
            toolStripSeparator3 = new ToolStripSeparator();
            toolStripMenuItemSkipWrapAround = new ToolStripMenuItem();
            toolStripButtonSkipPrevious = new ToolStripButton();
            toolStripButtonSkipNext = new ToolStripButton();
            toolStripSeparator4 = new ToolStripSeparator();
            toolStripTextBoxQuickSearch = new ToolStripTextBox();
            toolStripButtonClearSearch = new ToolStripButton();
            toolStripDropDownButtonQuickSearchOptions = new ToolStripDropDownButton();
            toolStripMenuItemQuickSearchMarkMatching = new ToolStripMenuItem();
            toolStripMenuItemQuickSearchFilter = new ToolStripMenuItem();
            logView = new ChronoListView();
            toolStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // toolStrip1
            // 
            toolStrip1.Items.AddRange(new ToolStripItem[] { toolStripButtonClear, toolStripSeparator1, toolStripDropDownButtonDisplayLevels, toolStripButtonFollowHead, toolStripDropDownButtonLoggers, toolStripSeparator2, toolStripDropDownButtonSkipSelection, toolStripButtonSkipPrevious, toolStripButtonSkipNext, toolStripSeparator4, toolStripTextBoxQuickSearch, toolStripButtonClearSearch, toolStripDropDownButtonQuickSearchOptions });
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(636, 25);
            toolStrip1.TabIndex = 0;
            toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButtonClear
            // 
            toolStripButtonClear.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButtonClear.Image = (Image)resources.GetObject("toolStripButtonClear.Image");
            toolStripButtonClear.ImageTransparentColor = Color.Magenta;
            toolStripButtonClear.Name = "toolStripButtonClear";
            toolStripButtonClear.Size = new Size(38, 22);
            toolStripButtonClear.Text = "Clear";
            toolStripButtonClear.ToolTipText = "Hide what's in the execution log up till now.";
            toolStripButtonClear.Click += toolStripButtonClear_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 25);
            // 
            // toolStripDropDownButtonDisplayLevels
            // 
            toolStripDropDownButtonDisplayLevels.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripDropDownButtonDisplayLevels.DropDownItems.AddRange(new ToolStripItem[] { toolStripMenuItemLevels2, toolStripMenuItemLevels3, toolStripMenuItemLevels4, toolStripMenuItemLevels5, toolStripMenuItemLevels6, toolStripMenuItemLevelsAll });
            toolStripDropDownButtonDisplayLevels.ImageTransparentColor = Color.Magenta;
            toolStripDropDownButtonDisplayLevels.Name = "toolStripDropDownButtonDisplayLevels";
            toolStripDropDownButtonDisplayLevels.Size = new Size(26, 22);
            toolStripDropDownButtonDisplayLevels.Text = "8";
            toolStripDropDownButtonDisplayLevels.ToolTipText = "Select the number of log scope levels to show.";
            // 
            // toolStripMenuItemLevels2
            // 
            toolStripMenuItemLevels2.CheckOnClick = true;
            toolStripMenuItemLevels2.Name = "toolStripMenuItemLevels2";
            toolStripMenuItemLevels2.Size = new Size(120, 22);
            toolStripMenuItemLevels2.Text = "2 levels";
            toolStripMenuItemLevels2.CheckedChanged += toolStripMenuItemDisplayLevel_CheckedChanged;
            // 
            // toolStripMenuItemLevels3
            // 
            toolStripMenuItemLevels3.CheckOnClick = true;
            toolStripMenuItemLevels3.Name = "toolStripMenuItemLevels3";
            toolStripMenuItemLevels3.Size = new Size(120, 22);
            toolStripMenuItemLevels3.Text = "3 levels";
            toolStripMenuItemLevels3.CheckedChanged += toolStripMenuItemDisplayLevel_CheckedChanged;
            // 
            // toolStripMenuItemLevels4
            // 
            toolStripMenuItemLevels4.CheckOnClick = true;
            toolStripMenuItemLevels4.Name = "toolStripMenuItemLevels4";
            toolStripMenuItemLevels4.Size = new Size(120, 22);
            toolStripMenuItemLevels4.Text = "4 levels";
            toolStripMenuItemLevels4.CheckedChanged += toolStripMenuItemDisplayLevel_CheckedChanged;
            // 
            // toolStripMenuItemLevels5
            // 
            toolStripMenuItemLevels5.CheckOnClick = true;
            toolStripMenuItemLevels5.Name = "toolStripMenuItemLevels5";
            toolStripMenuItemLevels5.Size = new Size(120, 22);
            toolStripMenuItemLevels5.Text = "5 levels";
            toolStripMenuItemLevels5.CheckedChanged += toolStripMenuItemDisplayLevel_CheckedChanged;
            // 
            // toolStripMenuItemLevels6
            // 
            toolStripMenuItemLevels6.CheckOnClick = true;
            toolStripMenuItemLevels6.Name = "toolStripMenuItemLevels6";
            toolStripMenuItemLevels6.Size = new Size(120, 22);
            toolStripMenuItemLevels6.Text = "6 levels";
            toolStripMenuItemLevels6.CheckedChanged += toolStripMenuItemDisplayLevel_CheckedChanged;
            // 
            // toolStripMenuItemLevelsAll
            // 
            toolStripMenuItemLevelsAll.Checked = true;
            toolStripMenuItemLevelsAll.CheckOnClick = true;
            toolStripMenuItemLevelsAll.CheckState = CheckState.Checked;
            toolStripMenuItemLevelsAll.Name = "toolStripMenuItemLevelsAll";
            toolStripMenuItemLevelsAll.Size = new Size(120, 22);
            toolStripMenuItemLevelsAll.Text = "All levels";
            toolStripMenuItemLevelsAll.CheckedChanged += toolStripMenuItemDisplayLevel_CheckedChanged;
            // 
            // toolStripButtonFollowHead
            // 
            toolStripButtonFollowHead.Alignment = ToolStripItemAlignment.Right;
            toolStripButtonFollowHead.AutoToolTip = false;
            toolStripButtonFollowHead.CheckOnClick = true;
            toolStripButtonFollowHead.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButtonFollowHead.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            toolStripButtonFollowHead.ImageTransparentColor = Color.Magenta;
            toolStripButtonFollowHead.Name = "toolStripButtonFollowHead";
            toolStripButtonFollowHead.Size = new Size(23, 22);
            toolStripButtonFollowHead.Text = "V";
            toolStripButtonFollowHead.ToolTipText = "Jump to and follow the end.";
            toolStripButtonFollowHead.CheckedChanged += toolStripButtonFollowHead_CheckedChanged;
            // 
            // toolStripDropDownButtonLoggers
            // 
            toolStripDropDownButtonLoggers.Alignment = ToolStripItemAlignment.Right;
            toolStripDropDownButtonLoggers.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripDropDownButtonLoggers.DropDownItems.AddRange(new ToolStripItem[] { toolStripMenuItemSpecialLoggersNoneAvailable });
            toolStripDropDownButtonLoggers.Image = (Image)resources.GetObject("toolStripDropDownButtonLoggers.Image");
            toolStripDropDownButtonLoggers.ImageTransparentColor = Color.Magenta;
            toolStripDropDownButtonLoggers.Name = "toolStripDropDownButtonLoggers";
            toolStripDropDownButtonLoggers.Size = new Size(62, 22);
            toolStripDropDownButtonLoggers.Text = "Loggers";
            toolStripDropDownButtonLoggers.DropDownOpening += toolStripDropDownButtonLoggers_DropDownOpening;
            // 
            // toolStripMenuItemSpecialLoggersNoneAvailable
            // 
            toolStripMenuItemSpecialLoggersNoneAvailable.CheckOnClick = true;
            toolStripMenuItemSpecialLoggersNoneAvailable.Enabled = false;
            toolStripMenuItemSpecialLoggersNoneAvailable.Name = "toolStripMenuItemSpecialLoggersNoneAvailable";
            toolStripMenuItemSpecialLoggersNoneAvailable.Size = new Size(166, 22);
            toolStripMenuItemSpecialLoggersNoneAvailable.Text = "<none available>";
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(6, 25);
            // 
            // toolStripDropDownButtonSkipSelection
            // 
            toolStripDropDownButtonSkipSelection.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripDropDownButtonSkipSelection.DropDownItems.AddRange(new ToolStripItem[] { toolStripMenuItemSkipSearchMatches, toolStripMenuItemSkipError, toolStripMenuItemSkipScriptExecutionStart, toolStripMenuItemSkipMeasurement, toolStripSeparator3, toolStripMenuItemSkipWrapAround });
            toolStripDropDownButtonSkipSelection.Image = (Image)resources.GetObject("toolStripDropDownButtonSkipSelection.Image");
            toolStripDropDownButtonSkipSelection.ImageTransparentColor = Color.Magenta;
            toolStripDropDownButtonSkipSelection.Name = "toolStripDropDownButtonSkipSelection";
            toolStripDropDownButtonSkipSelection.Size = new Size(45, 22);
            toolStripDropDownButtonSkipSelection.Text = "Error";
            toolStripDropDownButtonSkipSelection.ToolTipText = "Selected options for the 'Skip' operations.";
            // 
            // toolStripMenuItemSkipSearchMatches
            // 
            toolStripMenuItemSkipSearchMatches.Name = "toolStripMenuItemSkipSearchMatches";
            toolStripMenuItemSkipSearchMatches.Size = new Size(186, 22);
            toolStripMenuItemSkipSearchMatches.Text = "Search Matches";
            toolStripMenuItemSkipSearchMatches.Click += toolStripMenuItemSkipSearchMatches_Click;
            // 
            // toolStripMenuItemSkipError
            // 
            toolStripMenuItemSkipError.Name = "toolStripMenuItemSkipError";
            toolStripMenuItemSkipError.Size = new Size(186, 22);
            toolStripMenuItemSkipError.Text = "Fail or Error";
            toolStripMenuItemSkipError.Click += toolStripMenuItemSkipError_Click;
            // 
            // toolStripMenuItemSkipScriptExecutionStart
            // 
            toolStripMenuItemSkipScriptExecutionStart.Name = "toolStripMenuItemSkipScriptExecutionStart";
            toolStripMenuItemSkipScriptExecutionStart.Size = new Size(186, 22);
            toolStripMenuItemSkipScriptExecutionStart.Text = "Script Execution Start";
            toolStripMenuItemSkipScriptExecutionStart.Click += toolStripMenuItemSkipScriptExecutionStart_Click;
            // 
            // toolStripMenuItemSkipMeasurement
            // 
            toolStripMenuItemSkipMeasurement.Name = "toolStripMenuItemSkipMeasurement";
            toolStripMenuItemSkipMeasurement.Size = new Size(186, 22);
            toolStripMenuItemSkipMeasurement.Text = "Measurement";
            toolStripMenuItemSkipMeasurement.Visible = false;
            toolStripMenuItemSkipMeasurement.Click += toolStripMenuItemSkipMeasurement_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(183, 6);
            // 
            // toolStripMenuItemSkipWrapAround
            // 
            toolStripMenuItemSkipWrapAround.CheckOnClick = true;
            toolStripMenuItemSkipWrapAround.Name = "toolStripMenuItemSkipWrapAround";
            toolStripMenuItemSkipWrapAround.Size = new Size(186, 22);
            toolStripMenuItemSkipWrapAround.Text = "Wrap Around";
            toolStripMenuItemSkipWrapAround.CheckedChanged += toolStripMenuItemSkipWrapAround_CheckedChanged;
            // 
            // toolStripButtonSkipPrevious
            // 
            toolStripButtonSkipPrevious.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButtonSkipPrevious.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            toolStripButtonSkipPrevious.Image = (Image)resources.GetObject("toolStripButtonSkipPrevious.Image");
            toolStripButtonSkipPrevious.ImageTransparentColor = Color.Magenta;
            toolStripButtonSkipPrevious.Name = "toolStripButtonSkipPrevious";
            toolStripButtonSkipPrevious.Size = new Size(23, 22);
            toolStripButtonSkipPrevious.Text = "P";
            toolStripButtonSkipPrevious.ToolTipText = "Skip to previous occurance";
            toolStripButtonSkipPrevious.Click += toolStripButtonSkipPrevious_Click;
            // 
            // toolStripButtonSkipNext
            // 
            toolStripButtonSkipNext.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButtonSkipNext.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            toolStripButtonSkipNext.Image = (Image)resources.GetObject("toolStripButtonSkipNext.Image");
            toolStripButtonSkipNext.ImageTransparentColor = Color.Magenta;
            toolStripButtonSkipNext.Name = "toolStripButtonSkipNext";
            toolStripButtonSkipNext.Size = new Size(23, 22);
            toolStripButtonSkipNext.Text = "N";
            toolStripButtonSkipNext.ToolTipText = "Skip to next occurance";
            toolStripButtonSkipNext.Click += toolStripButtonSkipNext_Click;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new Size(6, 25);
            // 
            // toolStripTextBoxQuickSearch
            // 
            toolStripTextBoxQuickSearch.Name = "toolStripTextBoxQuickSearch";
            toolStripTextBoxQuickSearch.Size = new Size(100, 25);
            toolStripTextBoxQuickSearch.TextChanged += toolStripTextBoxQuickSearch_TextChanged;
            // 
            // toolStripButtonClearSearch
            // 
            toolStripButtonClearSearch.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButtonClearSearch.Image = (Image)resources.GetObject("toolStripButtonClearSearch.Image");
            toolStripButtonClearSearch.ImageTransparentColor = Color.Magenta;
            toolStripButtonClearSearch.Name = "toolStripButtonClearSearch";
            toolStripButtonClearSearch.Size = new Size(23, 22);
            toolStripButtonClearSearch.Text = "X";
            toolStripButtonClearSearch.ToolTipText = "Clear search";
            toolStripButtonClearSearch.Visible = false;
            toolStripButtonClearSearch.Click += toolStripButtonClearSearch_Click;
            // 
            // toolStripDropDownButtonQuickSearchOptions
            // 
            toolStripDropDownButtonQuickSearchOptions.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripDropDownButtonQuickSearchOptions.DropDownItems.AddRange(new ToolStripItem[] { toolStripMenuItemQuickSearchMarkMatching, toolStripMenuItemQuickSearchFilter });
            toolStripDropDownButtonQuickSearchOptions.Image = (Image)resources.GetObject("toolStripDropDownButtonQuickSearchOptions.Image");
            toolStripDropDownButtonQuickSearchOptions.ImageTransparentColor = Color.Magenta;
            toolStripDropDownButtonQuickSearchOptions.Name = "toolStripDropDownButtonQuickSearchOptions";
            toolStripDropDownButtonQuickSearchOptions.Size = new Size(55, 22);
            toolStripDropDownButtonQuickSearchOptions.Text = "Search";
            // 
            // toolStripMenuItemQuickSearchMarkMatching
            // 
            toolStripMenuItemQuickSearchMarkMatching.Checked = true;
            toolStripMenuItemQuickSearchMarkMatching.CheckOnClick = true;
            toolStripMenuItemQuickSearchMarkMatching.CheckState = CheckState.Checked;
            toolStripMenuItemQuickSearchMarkMatching.Name = "toolStripMenuItemQuickSearchMarkMatching";
            toolStripMenuItemQuickSearchMarkMatching.Size = new Size(217, 22);
            toolStripMenuItemQuickSearchMarkMatching.Text = "Mark matches";
            toolStripMenuItemQuickSearchMarkMatching.CheckedChanged += toolStripMenuItemQuickSearchMarkMatching_CheckedChanged;
            // 
            // toolStripMenuItemQuickSearchFilter
            // 
            toolStripMenuItemQuickSearchFilter.CheckOnClick = true;
            toolStripMenuItemQuickSearchFilter.Name = "toolStripMenuItemQuickSearchFilter";
            toolStripMenuItemQuickSearchFilter.Size = new Size(217, 22);
            toolStripMenuItemQuickSearchFilter.Text = "Hide non-matching entries";
            toolStripMenuItemQuickSearchFilter.CheckedChanged += toolStripMenuItemQuickSearchFilter_CheckedChanged;
            // 
            // logView
            // 
            logView.Dock = DockStyle.Fill;
            logView.Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            logView.HeadMode = true;
            logView.Location = new Point(0, 25);
            logView.Name = "logView";
            logView.Size = new Size(636, 204);
            logView.TabIndex = 1;
            logView.ZeroTime = new DateTime(2024, 6, 24, 10, 25, 15, 655);
            logView.HeadModeChanged += logView_HeadModeChanged;
            // 
            // LogViewer
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(logView);
            Controls.Add(toolStrip1);
            Name = "LogViewer";
            Size = new Size(636, 229);
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ToolStrip toolStrip1;
        private ChronoListView logView;
        private ToolStripDropDownButton toolStripDropDownButtonDisplayLevels;
        private ToolStripMenuItem toolStripMenuItemLevels2;
        private ToolStripMenuItem toolStripMenuItemLevels3;
        private ToolStripMenuItem toolStripMenuItemLevels4;
        private ToolStripMenuItem toolStripMenuItemLevels5;
        private ToolStripMenuItem toolStripMenuItemLevels6;
        private ToolStripMenuItem toolStripMenuItemLevelsAll;
        private ToolStripButton toolStripButtonFollowHead;
        private ToolStripButton toolStripButtonClear;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripDropDownButton toolStripDropDownButtonLoggers;
        private ToolStripMenuItem toolStripMenuItemSpecialLoggersNoneAvailable;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripButton toolStripButtonSkipPrevious;
        private ToolStripButton toolStripButtonSkipNext;
        private ToolStripDropDownButton toolStripDropDownButtonSkipSelection;
        private ToolStripMenuItem toolStripMenuItemSkipError;
        private ToolStripMenuItem toolStripMenuItemSkipScriptExecutionStart;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripMenuItem toolStripMenuItemSkipWrapAround;
        private ToolStripMenuItem toolStripMenuItemSkipMeasurement;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripTextBox toolStripTextBoxQuickSearch;
        private ToolStripDropDownButton toolStripDropDownButtonQuickSearchOptions;
        private ToolStripMenuItem toolStripMenuItemQuickSearchFilter;
        private ToolStripMenuItem toolStripMenuItemSkipSearchMatches;
        private ToolStripButton toolStripButtonClearSearch;
        private ToolStripMenuItem toolStripMenuItemQuickSearchMarkMatching;
    }
}
