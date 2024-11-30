using StepBro.UI.WinForms.CustomToolBar;

namespace StepBro.SimpleWorkbench
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
            statusStrip = new StatusStrip();
            toolStripStatusLabelApplicationTaskState = new ToolStripStatusLabel();
            toolStripDropDownButtonTestActions = new ToolStripDropDownButton();
            toolStripStatusLabelExecutionResult = new ToolStripStatusLabel();
            panelCustomToolstrips = new ToolBarHost();
            toolStripMain = new ToolStrip();
            toolStripMainMenu = new ToolStripDropDownButton();
            toolStripMenuItemFile = new ToolStripMenuItem();
            toolStripMenuItemFileOpen = new ToolStripMenuItem();
            toolStripMenuItemFileSave = new ToolStripMenuItem();
            toolStripSeparator4 = new ToolStripSeparator();
            toolStripMenuItemSaveLogActivate = new ToolStripMenuItem();
            toolStripMenuItemSaveLogLongOnly = new ToolStripMenuItem();
            toolStripMenuItemSaveLogNow = new ToolStripMenuItem();
            toolStripSeparator5 = new ToolStripSeparator();
            toolStripMenuItemFileTestReport = new ToolStripMenuItem();
            toolStripMenuItemCreateDocumentationFiles = new ToolStripMenuItem();
            toolStripMenuItemCreateProjectOverview = new ToolStripMenuItem();
            toolStripMenuItemCreateDocForSelectedFile = new ToolStripMenuItem();
            toolStripMenuItemView = new ToolStripMenuItem();
            viewExecutionLogToolStripMenuItem = new ToolStripMenuItem();
            viewErrorsToolStripMenuItem = new ToolStripMenuItem();
            viewPropertiesToolStripMenuItem = new ToolStripMenuItem();
            viewReportOverviewToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator3 = new ToolStripSeparator();
            viewObjectCommandPromptToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItemExeNoteInput = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            viewSelectionDocToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator6 = new ToolStripSeparator();
            viewToolbarsToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItemToolbarsDummyEntry = new ToolStripMenuItem();
            shortcutsToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItemDeleteShortcut = new ToolStripMenuItem();
            toolStripMenuItemDeleteAllShortcuts = new ToolStripMenuItem();
            helpToolStripMenuItem = new ToolStripMenuItem();
            viewDocumentationBrowserToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            toolStripMenuItemExit = new ToolStripMenuItem();
            toolStripDropDownButtonTool = new ToolStripDropDownButton();
            toolStripSeparatorToolSelection = new ToolStripSeparator();
            toolStripMenuItemNoToolsYet = new ToolStripMenuItem();
            toolStripComboBoxTool = new ToolStripComboBox();
            toolStripComboBoxToolCommand = new ToolStripComboBox();
            toolStripButtonRunCommand = new ToolStripButton();
            toolStripSeparatorTool = new ToolStripSeparator();
            toolStripSplitButtonRunScript = new ToolStripSplitButton();
            toolStripMenuItemRunByNamespace = new ToolStripMenuItem();
            toolStripSeparatorRunBeforeHistory = new ToolStripSeparator();
            toolStripSeparatorRunAfterHistory = new ToolStripSeparator();
            toolStripTextBoxRunSearch = new ToolStripTextBox();
            toolStripButtonStopScriptExecution = new ToolStripButton();
            toolStripSeparatorExtraFields = new ToolStripSeparator();
            toolStripTextBoxExeNote = new ToolStripTextBox();
            toolStripSeparatorBeforeShortcuts = new ToolStripSeparator();
            toolStripButtonAddShortcut = new ToolStripButton();
            dockManager = new ActiproSoftware.UI.WinForms.Controls.Docking.DockManager(components);
            autoHideTabStripPanel1 = new ActiproSoftware.UI.WinForms.Controls.Docking.AutoHideTabStripPanel();
            autoHideContainer1 = new ActiproSoftware.UI.WinForms.Controls.Docking.AutoHideContainer();
            autoHideTabStripPanel2 = new ActiproSoftware.UI.WinForms.Controls.Docking.AutoHideTabStripPanel();
            autoHideContainer2 = new ActiproSoftware.UI.WinForms.Controls.Docking.AutoHideContainer();
            autoHideTabStripPanel3 = new ActiproSoftware.UI.WinForms.Controls.Docking.AutoHideTabStripPanel();
            autoHideContainer3 = new ActiproSoftware.UI.WinForms.Controls.Docking.AutoHideContainer();
            autoHideTabStripPanel4 = new ActiproSoftware.UI.WinForms.Controls.Docking.AutoHideTabStripPanel();
            autoHideContainer4 = new ActiproSoftware.UI.WinForms.Controls.Docking.AutoHideContainer();
            statusStrip.SuspendLayout();
            toolStripMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dockManager).BeginInit();
            SuspendLayout();
            // 
            // statusStrip
            // 
            statusStrip.Items.AddRange(new ToolStripItem[] { toolStripStatusLabelApplicationTaskState, toolStripDropDownButtonTestActions, toolStripStatusLabelExecutionResult });
            statusStrip.Location = new Point(0, 505);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(1028, 22);
            statusStrip.TabIndex = 3;
            statusStrip.Text = "statusStrip";
            // 
            // toolStripStatusLabelApplicationTaskState
            // 
            toolStripStatusLabelApplicationTaskState.AutoSize = false;
            toolStripStatusLabelApplicationTaskState.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripStatusLabelApplicationTaskState.Name = "toolStripStatusLabelApplicationTaskState";
            toolStripStatusLabelApplicationTaskState.Size = new Size(150, 17);
            toolStripStatusLabelApplicationTaskState.Text = "Working...";
            toolStripStatusLabelApplicationTaskState.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // toolStripDropDownButtonTestActions
            // 
            toolStripDropDownButtonTestActions.BackColor = Color.Khaki;
            toolStripDropDownButtonTestActions.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripDropDownButtonTestActions.Image = (Image)resources.GetObject("toolStripDropDownButtonTestActions.Image");
            toolStripDropDownButtonTestActions.ImageTransparentColor = Color.Magenta;
            toolStripDropDownButtonTestActions.Name = "toolStripDropDownButtonTestActions";
            toolStripDropDownButtonTestActions.Size = new Size(83, 20);
            toolStripDropDownButtonTestActions.Text = "Test Actions";
            toolStripDropDownButtonTestActions.Visible = false;
            // 
            // toolStripStatusLabelExecutionResult
            // 
            toolStripStatusLabelExecutionResult.AutoSize = false;
            toolStripStatusLabelExecutionResult.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripStatusLabelExecutionResult.Name = "toolStripStatusLabelExecutionResult";
            toolStripStatusLabelExecutionResult.Size = new Size(863, 17);
            toolStripStatusLabelExecutionResult.Spring = true;
            toolStripStatusLabelExecutionResult.TextAlign = ContentAlignment.MiddleLeft;
            toolStripStatusLabelExecutionResult.Click += toolStripStatusLabelExecutionResult_Click;
            // 
            // panelCustomToolstrips
            // 
            panelCustomToolstrips.BackColor = Color.DarkGray;
            panelCustomToolstrips.Dock = DockStyle.Top;
            panelCustomToolstrips.Location = new Point(0, 25);
            panelCustomToolstrips.Name = "panelCustomToolstrips";
            panelCustomToolstrips.Size = new Size(1028, 28);
            panelCustomToolstrips.TabIndex = 4;
            panelCustomToolstrips.Visible = false;
            // 
            // toolStripMain
            // 
            toolStripMain.Items.AddRange(new ToolStripItem[] { toolStripMainMenu, toolStripDropDownButtonTool, toolStripComboBoxTool, toolStripComboBoxToolCommand, toolStripButtonRunCommand, toolStripSeparatorTool, toolStripSplitButtonRunScript, toolStripButtonStopScriptExecution, toolStripSeparatorExtraFields, toolStripTextBoxExeNote, toolStripSeparatorBeforeShortcuts, toolStripButtonAddShortcut });
            toolStripMain.Location = new Point(0, 0);
            toolStripMain.Name = "toolStripMain";
            toolStripMain.Size = new Size(1028, 25);
            toolStripMain.TabIndex = 5;
            toolStripMain.Text = "toolStripMain";
            // 
            // toolStripMainMenu
            // 
            toolStripMainMenu.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripMainMenu.DropDownItems.AddRange(new ToolStripItem[] { toolStripMenuItemFile, toolStripMenuItemView, shortcutsToolStripMenuItem, helpToolStripMenuItem, toolStripSeparator1, toolStripMenuItemExit });
            toolStripMainMenu.Image = (Image)resources.GetObject("toolStripMainMenu.Image");
            toolStripMainMenu.ImageTransparentColor = Color.Magenta;
            toolStripMainMenu.Name = "toolStripMainMenu";
            toolStripMainMenu.Size = new Size(31, 22);
            toolStripMainMenu.Text = "☰";
            // 
            // toolStripMenuItemFile
            // 
            toolStripMenuItemFile.DropDownItems.AddRange(new ToolStripItem[] { toolStripMenuItemFileOpen, toolStripMenuItemFileSave, toolStripSeparator4, toolStripMenuItemSaveLogActivate, toolStripMenuItemSaveLogLongOnly, toolStripMenuItemSaveLogNow, toolStripSeparator5, toolStripMenuItemFileTestReport, toolStripMenuItemCreateDocumentationFiles });
            toolStripMenuItemFile.Name = "toolStripMenuItemFile";
            toolStripMenuItemFile.Size = new Size(179, 22);
            toolStripMenuItemFile.Text = "&File";
            toolStripMenuItemFile.DropDownOpening += toolStripMenuItemFile_DropDownOpening;
            // 
            // toolStripMenuItemFileOpen
            // 
            toolStripMenuItemFileOpen.Enabled = false;
            toolStripMenuItemFileOpen.Name = "toolStripMenuItemFileOpen";
            toolStripMenuItemFileOpen.ShortcutKeys = Keys.Control | Keys.O;
            toolStripMenuItemFileOpen.Size = new Size(220, 22);
            toolStripMenuItemFileOpen.Text = "Open...";
            toolStripMenuItemFileOpen.Click += toolStripMenuItemFileOpen_Click;
            // 
            // toolStripMenuItemFileSave
            // 
            toolStripMenuItemFileSave.Name = "toolStripMenuItemFileSave";
            toolStripMenuItemFileSave.ShortcutKeys = Keys.Control | Keys.S;
            toolStripMenuItemFileSave.Size = new Size(220, 22);
            toolStripMenuItemFileSave.Text = "Save";
            toolStripMenuItemFileSave.Click += toolStripMenuItemFileSave_Click;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new Size(217, 6);
            // 
            // toolStripMenuItemSaveLogActivate
            // 
            toolStripMenuItemSaveLogActivate.CheckOnClick = true;
            toolStripMenuItemSaveLogActivate.Enabled = false;
            toolStripMenuItemSaveLogActivate.Name = "toolStripMenuItemSaveLogActivate";
            toolStripMenuItemSaveLogActivate.Size = new Size(220, 22);
            toolStripMenuItemSaveLogActivate.Text = "Save Log";
            // 
            // toolStripMenuItemSaveLogLongOnly
            // 
            toolStripMenuItemSaveLogLongOnly.CheckOnClick = true;
            toolStripMenuItemSaveLogLongOnly.Enabled = false;
            toolStripMenuItemSaveLogLongOnly.Name = "toolStripMenuItemSaveLogLongOnly";
            toolStripMenuItemSaveLogLongOnly.Size = new Size(220, 22);
            toolStripMenuItemSaveLogLongOnly.Text = "Save Only Long Executions";
            // 
            // toolStripMenuItemSaveLogNow
            // 
            toolStripMenuItemSaveLogNow.Name = "toolStripMenuItemSaveLogNow";
            toolStripMenuItemSaveLogNow.Size = new Size(220, 22);
            toolStripMenuItemSaveLogNow.Text = "Save Log Now...";
            toolStripMenuItemSaveLogNow.Click += toolStripMenuItemSaveLogNow_Click;
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new Size(217, 6);
            // 
            // toolStripMenuItemFileTestReport
            // 
            toolStripMenuItemFileTestReport.Enabled = false;
            toolStripMenuItemFileTestReport.Name = "toolStripMenuItemFileTestReport";
            toolStripMenuItemFileTestReport.Size = new Size(220, 22);
            toolStripMenuItemFileTestReport.Text = "Test Report";
            // 
            // toolStripMenuItemCreateDocumentationFiles
            // 
            toolStripMenuItemCreateDocumentationFiles.DropDownItems.AddRange(new ToolStripItem[] { toolStripMenuItemCreateProjectOverview, toolStripMenuItemCreateDocForSelectedFile });
            toolStripMenuItemCreateDocumentationFiles.Name = "toolStripMenuItemCreateDocumentationFiles";
            toolStripMenuItemCreateDocumentationFiles.Size = new Size(220, 22);
            toolStripMenuItemCreateDocumentationFiles.Text = "Create Documentation Files";
            toolStripMenuItemCreateDocumentationFiles.DropDownOpening += toolStripMenuItemCreateDocumentationFiles_DropDownOpening;
            // 
            // toolStripMenuItemCreateProjectOverview
            // 
            toolStripMenuItemCreateProjectOverview.Name = "toolStripMenuItemCreateProjectOverview";
            toolStripMenuItemCreateProjectOverview.Size = new Size(148, 22);
            toolStripMenuItemCreateProjectOverview.Text = "Project Files...";
            toolStripMenuItemCreateProjectOverview.Click += toolStripMenuItemCreateProjectOverview_Click;
            // 
            // toolStripMenuItemCreateDocForSelectedFile
            // 
            toolStripMenuItemCreateDocForSelectedFile.Name = "toolStripMenuItemCreateDocForSelectedFile";
            toolStripMenuItemCreateDocForSelectedFile.Size = new Size(148, 22);
            toolStripMenuItemCreateDocForSelectedFile.Text = "Selected File...";
            toolStripMenuItemCreateDocForSelectedFile.Click += toolStripMenuItemCreateDocForSelectedFile_Click;
            // 
            // toolStripMenuItemView
            // 
            toolStripMenuItemView.DropDownItems.AddRange(new ToolStripItem[] { viewExecutionLogToolStripMenuItem, viewErrorsToolStripMenuItem, viewPropertiesToolStripMenuItem, viewReportOverviewToolStripMenuItem, toolStripSeparator3, viewObjectCommandPromptToolStripMenuItem, toolStripMenuItemExeNoteInput, toolStripSeparator2, viewSelectionDocToolStripMenuItem, toolStripSeparator6, viewToolbarsToolStripMenuItem });
            toolStripMenuItemView.Name = "toolStripMenuItemView";
            toolStripMenuItemView.Size = new Size(179, 22);
            toolStripMenuItemView.Text = "&View";
            toolStripMenuItemView.DropDownOpened += toolStripMenuItemView_DropDownOpened;
            // 
            // viewExecutionLogToolStripMenuItem
            // 
            viewExecutionLogToolStripMenuItem.Name = "viewExecutionLogToolStripMenuItem";
            viewExecutionLogToolStripMenuItem.Size = new Size(212, 22);
            viewExecutionLogToolStripMenuItem.Text = "Execution &Log";
            viewExecutionLogToolStripMenuItem.Click += viewExecutionLogToolStripMenuItem_Click;
            // 
            // viewErrorsToolStripMenuItem
            // 
            viewErrorsToolStripMenuItem.Name = "viewErrorsToolStripMenuItem";
            viewErrorsToolStripMenuItem.Size = new Size(212, 22);
            viewErrorsToolStripMenuItem.Text = "&Errors List";
            viewErrorsToolStripMenuItem.Click += viewErrorsToolStripMenuItem_Click;
            // 
            // viewPropertiesToolStripMenuItem
            // 
            viewPropertiesToolStripMenuItem.Name = "viewPropertiesToolStripMenuItem";
            viewPropertiesToolStripMenuItem.Size = new Size(212, 22);
            viewPropertiesToolStripMenuItem.Text = "Properties";
            viewPropertiesToolStripMenuItem.Click += viewPropertiesToolStripMenuItem_Click;
            // 
            // viewReportOverviewToolStripMenuItem
            // 
            viewReportOverviewToolStripMenuItem.Name = "viewReportOverviewToolStripMenuItem";
            viewReportOverviewToolStripMenuItem.Size = new Size(212, 22);
            viewReportOverviewToolStripMenuItem.Text = "Report Overview";
            viewReportOverviewToolStripMenuItem.Click += viewReportOverviewToolStripMenuItem_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(209, 6);
            // 
            // viewObjectCommandPromptToolStripMenuItem
            // 
            viewObjectCommandPromptToolStripMenuItem.Checked = true;
            viewObjectCommandPromptToolStripMenuItem.CheckOnClick = true;
            viewObjectCommandPromptToolStripMenuItem.CheckState = CheckState.Checked;
            viewObjectCommandPromptToolStripMenuItem.Name = "viewObjectCommandPromptToolStripMenuItem";
            viewObjectCommandPromptToolStripMenuItem.Size = new Size(212, 22);
            viewObjectCommandPromptToolStripMenuItem.Text = "Object Command Prompt";
            viewObjectCommandPromptToolStripMenuItem.Click += viewObjectCommandPromptToolStripMenuItem_Click;
            // 
            // toolStripMenuItemExeNoteInput
            // 
            toolStripMenuItemExeNoteInput.CheckOnClick = true;
            toolStripMenuItemExeNoteInput.Name = "toolStripMenuItemExeNoteInput";
            toolStripMenuItemExeNoteInput.Size = new Size(212, 22);
            toolStripMenuItemExeNoteInput.Text = "Execution Note Input";
            toolStripMenuItemExeNoteInput.CheckedChanged += toolStripMenuItemExeNoteInput_CheckedChanged;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(209, 6);
            // 
            // viewSelectionDocToolStripMenuItem
            // 
            viewSelectionDocToolStripMenuItem.Name = "viewSelectionDocToolStripMenuItem";
            viewSelectionDocToolStripMenuItem.Size = new Size(212, 22);
            viewSelectionDocToolStripMenuItem.Text = "Selection Documentation";
            viewSelectionDocToolStripMenuItem.Click += viewSelectionDocToolStripMenuItem_Click;
            // 
            // toolStripSeparator6
            // 
            toolStripSeparator6.Name = "toolStripSeparator6";
            toolStripSeparator6.Size = new Size(209, 6);
            // 
            // viewToolbarsToolStripMenuItem
            // 
            viewToolbarsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { toolStripMenuItemToolbarsDummyEntry });
            viewToolbarsToolStripMenuItem.Name = "viewToolbarsToolStripMenuItem";
            viewToolbarsToolStripMenuItem.Size = new Size(212, 22);
            viewToolbarsToolStripMenuItem.Text = "&Toolbars";
            viewToolbarsToolStripMenuItem.DropDownOpening += viewToolbarsToolStripMenuItem_DropDownOpening;
            // 
            // toolStripMenuItemToolbarsDummyEntry
            // 
            toolStripMenuItemToolbarsDummyEntry.Name = "toolStripMenuItemToolbarsDummyEntry";
            toolStripMenuItemToolbarsDummyEntry.Size = new Size(203, 22);
            toolStripMenuItemToolbarsDummyEntry.Text = "dummy - will be deleted";
            // 
            // shortcutsToolStripMenuItem
            // 
            shortcutsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { toolStripMenuItemDeleteShortcut, toolStripMenuItemDeleteAllShortcuts });
            shortcutsToolStripMenuItem.Name = "shortcutsToolStripMenuItem";
            shortcutsToolStripMenuItem.Size = new Size(179, 22);
            shortcutsToolStripMenuItem.Text = "Execution Shortcuts";
            // 
            // toolStripMenuItemDeleteShortcut
            // 
            toolStripMenuItemDeleteShortcut.CheckOnClick = true;
            toolStripMenuItemDeleteShortcut.Name = "toolStripMenuItemDeleteShortcut";
            toolStripMenuItemDeleteShortcut.Size = new Size(174, 22);
            toolStripMenuItemDeleteShortcut.Text = "Delete shortcut";
            // 
            // toolStripMenuItemDeleteAllShortcuts
            // 
            toolStripMenuItemDeleteAllShortcuts.Name = "toolStripMenuItemDeleteAllShortcuts";
            toolStripMenuItemDeleteAllShortcuts.Size = new Size(174, 22);
            toolStripMenuItemDeleteAllShortcuts.Text = "Delete all shortcuts";
            toolStripMenuItemDeleteAllShortcuts.Click += toolStripMenuItemDeleteAllShortcuts_Click;
            // 
            // helpToolStripMenuItem
            // 
            helpToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { viewDocumentationBrowserToolStripMenuItem });
            helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            helpToolStripMenuItem.Size = new Size(179, 22);
            helpToolStripMenuItem.Text = "&Help";
            helpToolStripMenuItem.Visible = false;
            // 
            // viewDocumentationBrowserToolStripMenuItem
            // 
            viewDocumentationBrowserToolStripMenuItem.CheckOnClick = true;
            viewDocumentationBrowserToolStripMenuItem.Name = "viewDocumentationBrowserToolStripMenuItem";
            viewDocumentationBrowserToolStripMenuItem.Size = new Size(230, 22);
            viewDocumentationBrowserToolStripMenuItem.Text = "View Documentation Browser";
            viewDocumentationBrowserToolStripMenuItem.Click += viewDocumentationBrowserToolStripMenuItem_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(176, 6);
            // 
            // toolStripMenuItemExit
            // 
            toolStripMenuItemExit.Name = "toolStripMenuItemExit";
            toolStripMenuItemExit.Size = new Size(179, 22);
            toolStripMenuItemExit.Text = "E&xit";
            toolStripMenuItemExit.Click += toolStripMenuItemExit_Click;
            // 
            // toolStripDropDownButtonTool
            // 
            toolStripDropDownButtonTool.BackColor = SystemColors.Control;
            toolStripDropDownButtonTool.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripDropDownButtonTool.DropDownItems.AddRange(new ToolStripItem[] { toolStripSeparatorToolSelection, toolStripMenuItemNoToolsYet });
            toolStripDropDownButtonTool.Image = (Image)resources.GetObject("toolStripDropDownButtonTool.Image");
            toolStripDropDownButtonTool.ImageTransparentColor = Color.Magenta;
            toolStripDropDownButtonTool.Name = "toolStripDropDownButtonTool";
            toolStripDropDownButtonTool.Padding = new Padding(2, 0, 0, 0);
            toolStripDropDownButtonTool.Size = new Size(43, 22);
            toolStripDropDownButtonTool.Text = "tool";
            toolStripDropDownButtonTool.DropDownOpening += toolStripDropDownButtonTool_DropDownOpening;
            // 
            // toolStripSeparatorToolSelection
            // 
            toolStripSeparatorToolSelection.BackColor = Color.Gold;
            toolStripSeparatorToolSelection.Margin = new Padding(0, 3, 0, 3);
            toolStripSeparatorToolSelection.Name = "toolStripSeparatorToolSelection";
            toolStripSeparatorToolSelection.Size = new Size(203, 6);
            toolStripSeparatorToolSelection.Visible = false;
            // 
            // toolStripMenuItemNoToolsYet
            // 
            toolStripMenuItemNoToolsYet.Enabled = false;
            toolStripMenuItemNoToolsYet.Name = "toolStripMenuItemNoToolsYet";
            toolStripMenuItemNoToolsYet.Size = new Size(206, 22);
            toolStripMenuItemNoToolsYet.Text = "No tools to choose from.";
            // 
            // toolStripComboBoxTool
            // 
            toolStripComboBoxTool.BackColor = SystemColors.Control;
            toolStripComboBoxTool.DropDownStyle = ComboBoxStyle.DropDownList;
            toolStripComboBoxTool.Enabled = false;
            toolStripComboBoxTool.Name = "toolStripComboBoxTool";
            toolStripComboBoxTool.Size = new Size(120, 25);
            toolStripComboBoxTool.Visible = false;
            toolStripComboBoxTool.SelectedIndexChanged += toolStripComboBoxTool_SelectedIndexChanged;
            // 
            // toolStripComboBoxToolCommand
            // 
            toolStripComboBoxToolCommand.AutoSize = false;
            toolStripComboBoxToolCommand.BackColor = SystemColors.Window;
            toolStripComboBoxToolCommand.Enabled = false;
            toolStripComboBoxToolCommand.Name = "toolStripComboBoxToolCommand";
            toolStripComboBoxToolCommand.Size = new Size(250, 23);
            toolStripComboBoxToolCommand.ToolTipText = "Command prompt for the selected tool/object.";
            toolStripComboBoxToolCommand.Visible = false;
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
            toolStripButtonRunCommand.Size = new Size(23, 22);
            toolStripButtonRunCommand.Text = "P";
            toolStripButtonRunCommand.ToolTipText = "Run Object Command";
            toolStripButtonRunCommand.Click += toolStripButtonRunCommand_Click;
            // 
            // toolStripSeparatorTool
            // 
            toolStripSeparatorTool.Name = "toolStripSeparatorTool";
            toolStripSeparatorTool.Size = new Size(6, 25);
            // 
            // toolStripSplitButtonRunScript
            // 
            toolStripSplitButtonRunScript.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripSplitButtonRunScript.DropDownItems.AddRange(new ToolStripItem[] { toolStripMenuItemRunByNamespace, toolStripSeparatorRunBeforeHistory, toolStripSeparatorRunAfterHistory, toolStripTextBoxRunSearch });
            toolStripSplitButtonRunScript.Image = (Image)resources.GetObject("toolStripSplitButtonRunScript.Image");
            toolStripSplitButtonRunScript.ImageTransparentColor = Color.Magenta;
            toolStripSplitButtonRunScript.Name = "toolStripSplitButtonRunScript";
            toolStripSplitButtonRunScript.Size = new Size(111, 22);
            toolStripSplitButtonRunScript.Text = "Run File Element";
            toolStripSplitButtonRunScript.ButtonClick += toolStripSplitButtonRunScript_ButtonClick;
            toolStripSplitButtonRunScript.DropDownOpening += toolStripSplitButtonRunScript_DropDownOpening;
            // 
            // toolStripMenuItemRunByNamespace
            // 
            toolStripMenuItemRunByNamespace.Name = "toolStripMenuItemRunByNamespace";
            toolStripMenuItemRunByNamespace.Size = new Size(180, 22);
            toolStripMenuItemRunByNamespace.Text = "By Namespace";
            // 
            // toolStripSeparatorRunBeforeHistory
            // 
            toolStripSeparatorRunBeforeHistory.Name = "toolStripSeparatorRunBeforeHistory";
            toolStripSeparatorRunBeforeHistory.Size = new Size(177, 6);
            // 
            // toolStripSeparatorRunAfterHistory
            // 
            toolStripSeparatorRunAfterHistory.Name = "toolStripSeparatorRunAfterHistory";
            toolStripSeparatorRunAfterHistory.Size = new Size(177, 6);
            toolStripSeparatorRunAfterHistory.Visible = false;
            // 
            // toolStripTextBoxRunSearch
            // 
            toolStripTextBoxRunSearch.BackColor = Color.LemonChiffon;
            toolStripTextBoxRunSearch.Name = "toolStripTextBoxRunSearch";
            toolStripTextBoxRunSearch.Size = new Size(100, 23);
            toolStripTextBoxRunSearch.ToolTipText = "Search for script file element.";
            toolStripTextBoxRunSearch.KeyPress += toolStripTextBoxRunSearch_KeyPress;
            toolStripTextBoxRunSearch.TextChanged += toolStripTextBoxRunSearch_TextChanged;
            // 
            // toolStripButtonStopScriptExecution
            // 
            toolStripButtonStopScriptExecution.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButtonStopScriptExecution.Enabled = false;
            toolStripButtonStopScriptExecution.Image = (Image)resources.GetObject("toolStripButtonStopScriptExecution.Image");
            toolStripButtonStopScriptExecution.ImageTransparentColor = Color.Magenta;
            toolStripButtonStopScriptExecution.Name = "toolStripButtonStopScriptExecution";
            toolStripButtonStopScriptExecution.Size = new Size(23, 22);
            toolStripButtonStopScriptExecution.Text = "S";
            toolStripButtonStopScriptExecution.ToolTipText = "Stop the running script execution";
            toolStripButtonStopScriptExecution.Click += toolStripButtonStopScriptExecution_Click;
            // 
            // toolStripSeparatorExtraFields
            // 
            toolStripSeparatorExtraFields.Name = "toolStripSeparatorExtraFields";
            toolStripSeparatorExtraFields.Size = new Size(6, 25);
            toolStripSeparatorExtraFields.Visible = false;
            // 
            // toolStripTextBoxExeNote
            // 
            toolStripTextBoxExeNote.Name = "toolStripTextBoxExeNote";
            toolStripTextBoxExeNote.Size = new Size(100, 25);
            toolStripTextBoxExeNote.ToolTipText = "Execution Note";
            toolStripTextBoxExeNote.Visible = false;
            // 
            // toolStripSeparatorBeforeShortcuts
            // 
            toolStripSeparatorBeforeShortcuts.Name = "toolStripSeparatorBeforeShortcuts";
            toolStripSeparatorBeforeShortcuts.Size = new Size(6, 25);
            // 
            // toolStripButtonAddShortcut
            // 
            toolStripButtonAddShortcut.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButtonAddShortcut.Enabled = false;
            toolStripButtonAddShortcut.Image = (Image)resources.GetObject("toolStripButtonAddShortcut.Image");
            toolStripButtonAddShortcut.ImageTransparentColor = Color.Magenta;
            toolStripButtonAddShortcut.Name = "toolStripButtonAddShortcut";
            toolStripButtonAddShortcut.Size = new Size(23, 22);
            toolStripButtonAddShortcut.Text = "+";
            toolStripButtonAddShortcut.ToolTipText = "Add shortcut button for the selected script execution.";
            toolStripButtonAddShortcut.Click += toolStripButtonAddShortcut_Click;
            // 
            // dockManager
            // 
            dockManager.DocumentMdiStyle = ActiproSoftware.UI.WinForms.Controls.Docking.DocumentMdiStyle.Tabbed;
            dockManager.HostContainerControl = this;
            dockManager.TabbedMdiMaximumTabExtent = 250;
            dockManager.WindowClosed += dockManager_WindowClosed;
            dockManager.WindowClosing += dockManager_WindowClosing;
            // 
            // autoHideTabStripPanel1
            // 
            autoHideTabStripPanel1.AllowDrop = true;
            autoHideTabStripPanel1.Dock = DockStyle.Left;
            autoHideTabStripPanel1.DockManager = dockManager;
            autoHideTabStripPanel1.Location = new Point(0, 53);
            autoHideTabStripPanel1.Name = "autoHideTabStripPanel1";
            autoHideTabStripPanel1.Size = new Size(6, 452);
            autoHideTabStripPanel1.TabIndex = 6;
            // 
            // autoHideContainer1
            // 
            autoHideContainer1.AutoHideTabStripPanel = autoHideTabStripPanel1;
            autoHideContainer1.DockManager = dockManager;
            autoHideContainer1.Location = new Point(6, 53);
            autoHideContainer1.Name = "autoHideContainer1";
            autoHideContainer1.Size = new Size(1, 452);
            autoHideContainer1.TabIndex = 7;
            // 
            // autoHideTabStripPanel2
            // 
            autoHideTabStripPanel2.AllowDrop = true;
            autoHideTabStripPanel2.Dock = DockStyle.Top;
            autoHideTabStripPanel2.DockManager = dockManager;
            autoHideTabStripPanel2.Location = new Point(6, 53);
            autoHideTabStripPanel2.Name = "autoHideTabStripPanel2";
            autoHideTabStripPanel2.Size = new Size(1016, 6);
            autoHideTabStripPanel2.TabIndex = 8;
            // 
            // autoHideContainer2
            // 
            autoHideContainer2.AutoHideTabStripPanel = autoHideTabStripPanel2;
            autoHideContainer2.DockManager = dockManager;
            autoHideContainer2.Location = new Point(6, 59);
            autoHideContainer2.Name = "autoHideContainer2";
            autoHideContainer2.Size = new Size(1016, 36);
            autoHideContainer2.TabIndex = 9;
            // 
            // autoHideTabStripPanel3
            // 
            autoHideTabStripPanel3.AllowDrop = true;
            autoHideTabStripPanel3.Dock = DockStyle.Right;
            autoHideTabStripPanel3.DockManager = dockManager;
            autoHideTabStripPanel3.Location = new Point(1022, 53);
            autoHideTabStripPanel3.Name = "autoHideTabStripPanel3";
            autoHideTabStripPanel3.Size = new Size(6, 452);
            autoHideTabStripPanel3.TabIndex = 10;
            // 
            // autoHideContainer3
            // 
            autoHideContainer3.AutoHideTabStripPanel = autoHideTabStripPanel3;
            autoHideContainer3.DockManager = dockManager;
            autoHideContainer3.Location = new Point(1021, 53);
            autoHideContainer3.Name = "autoHideContainer3";
            autoHideContainer3.Size = new Size(1, 452);
            autoHideContainer3.TabIndex = 11;
            // 
            // autoHideTabStripPanel4
            // 
            autoHideTabStripPanel4.AllowDrop = true;
            autoHideTabStripPanel4.Dock = DockStyle.Bottom;
            autoHideTabStripPanel4.DockManager = dockManager;
            autoHideTabStripPanel4.Location = new Point(6, 499);
            autoHideTabStripPanel4.Name = "autoHideTabStripPanel4";
            autoHideTabStripPanel4.Size = new Size(1016, 6);
            autoHideTabStripPanel4.TabIndex = 12;
            // 
            // autoHideContainer4
            // 
            autoHideContainer4.AutoHideTabStripPanel = autoHideTabStripPanel4;
            autoHideContainer4.DockManager = dockManager;
            autoHideContainer4.Location = new Point(6, 498);
            autoHideContainer4.Name = "autoHideContainer4";
            autoHideContainer4.Size = new Size(1016, 1);
            autoHideContainer4.TabIndex = 13;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(1028, 527);
            Controls.Add(autoHideContainer1);
            Controls.Add(autoHideContainer2);
            Controls.Add(autoHideContainer3);
            Controls.Add(autoHideContainer4);
            Controls.Add(autoHideTabStripPanel2);
            Controls.Add(autoHideTabStripPanel4);
            Controls.Add(autoHideTabStripPanel3);
            Controls.Add(autoHideTabStripPanel1);
            Controls.Add(panelCustomToolstrips);
            Controls.Add(toolStripMain);
            Controls.Add(statusStrip);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "MainForm";
            Text = "StepBro Workbench";
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            toolStripMain.ResumeLayout(false);
            toolStripMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dockManager).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private StatusStrip statusStrip;
        private ToolBarHost panelCustomToolstrips;
        private ToolStrip toolStripMain;
        private ToolStripDropDownButton toolStripMainMenu;
        private ToolStripMenuItem toolStripMenuItemFile;
        private ToolStripMenuItem toolStripMenuItemView;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem toolStripMenuItemExit;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem viewToolbarsToolStripMenuItem;
        private ToolStripMenuItem viewExecutionLogToolStripMenuItem;
        private ToolStripMenuItem viewErrorsToolStripMenuItem;
        private ActiproSoftware.UI.WinForms.Controls.Docking.DockManager dockManager;
        private ActiproSoftware.UI.WinForms.Controls.Docking.AutoHideContainer autoHideContainer1;
        private ActiproSoftware.UI.WinForms.Controls.Docking.AutoHideTabStripPanel autoHideTabStripPanel1;
        private ActiproSoftware.UI.WinForms.Controls.Docking.AutoHideContainer autoHideContainer2;
        private ActiproSoftware.UI.WinForms.Controls.Docking.AutoHideTabStripPanel autoHideTabStripPanel2;
        private ActiproSoftware.UI.WinForms.Controls.Docking.AutoHideContainer autoHideContainer3;
        private ActiproSoftware.UI.WinForms.Controls.Docking.AutoHideTabStripPanel autoHideTabStripPanel3;
        private ActiproSoftware.UI.WinForms.Controls.Docking.AutoHideContainer autoHideContainer4;
        private ActiproSoftware.UI.WinForms.Controls.Docking.AutoHideTabStripPanel autoHideTabStripPanel4;
        private ToolStripMenuItem viewObjectCommandPromptToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripComboBox toolStripComboBoxTool;
        private ToolStripComboBox toolStripComboBoxToolCommand;
        private ToolStripButton toolStripButtonRunCommand;
        private ToolStripSeparator toolStripSeparatorTool;
        private ToolStripSplitButton toolStripSplitButtonRunScript;
        private ToolStripButton toolStripButtonStopScriptExecution;
        private ToolStripSeparator toolStripSeparatorExtraFields;
        private ToolStripTextBox toolStripTextBoxExeNote;
        private ToolStripSeparator toolStripSeparatorBeforeShortcuts;
        private ToolStripButton toolStripButtonAddShortcut;
        private ToolStripMenuItem viewPropertiesToolStripMenuItem;
        private ToolStripMenuItem helpToolStripMenuItem;
        private ToolStripMenuItem viewDocumentationBrowserToolStripMenuItem;
        private ToolStripDropDownButton toolStripDropDownButtonTestActions;
        private ToolStripStatusLabel toolStripStatusLabelApplicationTaskState;
        private ToolStripMenuItem toolStripMenuItemRunByNamespace;
        private ToolStripSeparator toolStripSeparatorRunBeforeHistory;
        private ToolStripSeparator toolStripSeparatorRunAfterHistory;
        private ToolStripTextBox toolStripTextBoxRunSearch;
        private ToolStripMenuItem shortcutsToolStripMenuItem;
        private ToolStripMenuItem toolStripMenuItemDeleteShortcut;
        private ToolStripMenuItem toolStripMenuItemDeleteAllShortcuts;
        private ToolStripMenuItem toolStripMenuItemExeNoteInput;
        private ToolStripMenuItem toolStripMenuItemToolbarsDummyEntry;
        private ToolStripStatusLabel toolStripStatusLabelExecutionResult;
        private ToolStripMenuItem toolStripMenuItemFileOpen;
        private ToolStripMenuItem toolStripMenuItemFileSave;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripMenuItem toolStripMenuItemSaveLogActivate;
        private ToolStripMenuItem toolStripMenuItemFileTestReport;
        private ToolStripMenuItem toolStripMenuItemSaveLogLongOnly;
        private ToolStripMenuItem toolStripMenuItemSaveLogNow;
        private ToolStripSeparator toolStripSeparator5;
        private ToolStripMenuItem viewReportOverviewToolStripMenuItem;
        private ToolStripMenuItem viewSelectionDocToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator6;
        private ToolStripMenuItem toolStripMenuItemCreateDocumentationFiles;
        private ToolStripMenuItem toolStripMenuItemCreateProjectOverview;
        private ToolStripMenuItem toolStripMenuItemCreateDocForSelectedFile;
        private ToolStripDropDownButton toolStripDropDownButtonTool;
        private ToolStripSeparator toolStripSeparatorToolSelection;
        private ToolStripMenuItem toolStripMenuItemNoToolsYet;
    }
}
