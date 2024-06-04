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
            panelCustomToolstrips = new Panel();
            toolStrip2 = new ToolStrip();
            toolStrip1 = new ToolStrip();
            toolStripMain = new ToolStrip();
            toolStripMainMenu = new ToolStripDropDownButton();
            toolStripMenuItemFile = new ToolStripMenuItem();
            toolStripMenuItemView = new ToolStripMenuItem();
            viewExecutionLogToolStripMenuItem = new ToolStripMenuItem();
            viewErrorsToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator3 = new ToolStripSeparator();
            viewObjectCommandPromptToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            viewToolbarsToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            toolStripMenuItemExit = new ToolStripMenuItem();
            dockManager = new ActiproSoftware.UI.WinForms.Controls.Docking.DockManager(components);
            autoHideTabStripPanel1 = new ActiproSoftware.UI.WinForms.Controls.Docking.AutoHideTabStripPanel();
            autoHideContainer1 = new ActiproSoftware.UI.WinForms.Controls.Docking.AutoHideContainer();
            autoHideTabStripPanel2 = new ActiproSoftware.UI.WinForms.Controls.Docking.AutoHideTabStripPanel();
            autoHideContainer2 = new ActiproSoftware.UI.WinForms.Controls.Docking.AutoHideContainer();
            autoHideTabStripPanel3 = new ActiproSoftware.UI.WinForms.Controls.Docking.AutoHideTabStripPanel();
            autoHideContainer3 = new ActiproSoftware.UI.WinForms.Controls.Docking.AutoHideContainer();
            autoHideTabStripPanel4 = new ActiproSoftware.UI.WinForms.Controls.Docking.AutoHideTabStripPanel();
            autoHideContainer4 = new ActiproSoftware.UI.WinForms.Controls.Docking.AutoHideContainer();
            propertyGrid1 = new PropertyGrid();
            toolWindowProperties = new ActiproSoftware.UI.WinForms.Controls.Docking.ToolWindow();
            toolWindowContainer1 = new ActiproSoftware.UI.WinForms.Controls.Docking.ToolWindowContainer();
            webView21 = new Microsoft.Web.WebView2.WinForms.WebView2();
            toolWindowHelp = new ActiproSoftware.UI.WinForms.Controls.Docking.ToolWindow();
            toolWindowContainer2 = new ActiproSoftware.UI.WinForms.Controls.Docking.ToolWindowContainer();
            logView = new ListView();
            toolWindowExecutionLog = new ActiproSoftware.UI.WinForms.Controls.Docking.ToolWindow();
            toolWindowContainer3 = new ActiproSoftware.UI.WinForms.Controls.Docking.ToolWindowContainer();
            toolStripComboBoxTool = new ToolStripComboBox();
            toolStripComboBoxToolCommand = new ToolStripComboBox();
            toolStripButtonRunCommand = new ToolStripButton();
            toolStripSeparatorTool = new ToolStripSeparator();
            toolStripSplitButtonRunScript = new ToolStripSplitButton();
            toolStripButtonStopScriptExecution = new ToolStripButton();
            toolStripSeparatorExtraFields = new ToolStripSeparator();
            toolStripTextBoxExeNote = new ToolStripTextBox();
            toolStripSeparatorBeforeShortcuts = new ToolStripSeparator();
            toolStripButtonAddShortcut = new ToolStripButton();
            panelCustomToolstrips.SuspendLayout();
            toolStripMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dockManager).BeginInit();
            toolWindowProperties.SuspendLayout();
            toolWindowContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)webView21).BeginInit();
            toolWindowHelp.SuspendLayout();
            toolWindowContainer2.SuspendLayout();
            toolWindowExecutionLog.SuspendLayout();
            toolWindowContainer3.SuspendLayout();
            SuspendLayout();
            // 
            // statusStrip
            // 
            statusStrip.Location = new Point(0, 539);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(784, 22);
            statusStrip.TabIndex = 3;
            statusStrip.Text = "statusStrip";
            // 
            // panelCustomToolstrips
            // 
            panelCustomToolstrips.BackColor = Color.DarkGray;
            panelCustomToolstrips.Controls.Add(toolStrip2);
            panelCustomToolstrips.Controls.Add(toolStrip1);
            panelCustomToolstrips.Dock = DockStyle.Top;
            panelCustomToolstrips.Location = new Point(0, 25);
            panelCustomToolstrips.Name = "panelCustomToolstrips";
            panelCustomToolstrips.Size = new Size(784, 100);
            panelCustomToolstrips.TabIndex = 4;
            // 
            // toolStrip2
            // 
            toolStrip2.BackColor = Color.Turquoise;
            toolStrip2.Location = new Point(0, 25);
            toolStrip2.Name = "toolStrip2";
            toolStrip2.Size = new Size(784, 25);
            toolStrip2.TabIndex = 1;
            toolStrip2.Text = "toolStrip2";
            // 
            // toolStrip1
            // 
            toolStrip1.BackColor = Color.Tan;
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(784, 25);
            toolStrip1.TabIndex = 0;
            toolStrip1.Text = "toolStrip1";
            // 
            // toolStripMain
            // 
            toolStripMain.BackColor = Color.AntiqueWhite;
            toolStripMain.Items.AddRange(new ToolStripItem[] { toolStripMainMenu, toolStripComboBoxTool, toolStripComboBoxToolCommand, toolStripButtonRunCommand, toolStripSeparatorTool, toolStripSplitButtonRunScript, toolStripButtonStopScriptExecution, toolStripSeparatorExtraFields, toolStripTextBoxExeNote, toolStripSeparatorBeforeShortcuts, toolStripButtonAddShortcut });
            toolStripMain.Location = new Point(0, 0);
            toolStripMain.Name = "toolStripMain";
            toolStripMain.Size = new Size(784, 25);
            toolStripMain.TabIndex = 5;
            toolStripMain.Text = "toolStripMain";
            // 
            // toolStripMainMenu
            // 
            toolStripMainMenu.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripMainMenu.DropDownItems.AddRange(new ToolStripItem[] { toolStripMenuItemFile, toolStripMenuItemView, toolStripSeparator1, toolStripMenuItemExit });
            toolStripMainMenu.Image = (Image)resources.GetObject("toolStripMainMenu.Image");
            toolStripMainMenu.ImageTransparentColor = Color.Magenta;
            toolStripMainMenu.Name = "toolStripMainMenu";
            toolStripMainMenu.Size = new Size(31, 22);
            toolStripMainMenu.Text = "M";
            // 
            // toolStripMenuItemFile
            // 
            toolStripMenuItemFile.Name = "toolStripMenuItemFile";
            toolStripMenuItemFile.Size = new Size(99, 22);
            toolStripMenuItemFile.Text = "&File";
            // 
            // toolStripMenuItemView
            // 
            toolStripMenuItemView.DropDownItems.AddRange(new ToolStripItem[] { viewExecutionLogToolStripMenuItem, viewErrorsToolStripMenuItem, toolStripSeparator3, viewObjectCommandPromptToolStripMenuItem, toolStripSeparator2, viewToolbarsToolStripMenuItem });
            toolStripMenuItemView.Name = "toolStripMenuItemView";
            toolStripMenuItemView.Size = new Size(99, 22);
            toolStripMenuItemView.Text = "&View";
            // 
            // viewExecutionLogToolStripMenuItem
            // 
            viewExecutionLogToolStripMenuItem.Checked = true;
            viewExecutionLogToolStripMenuItem.CheckOnClick = true;
            viewExecutionLogToolStripMenuItem.CheckState = CheckState.Checked;
            viewExecutionLogToolStripMenuItem.Name = "viewExecutionLogToolStripMenuItem";
            viewExecutionLogToolStripMenuItem.Size = new Size(212, 22);
            viewExecutionLogToolStripMenuItem.Text = "Execution &Log";
            // 
            // viewErrorsToolStripMenuItem
            // 
            viewErrorsToolStripMenuItem.Checked = true;
            viewErrorsToolStripMenuItem.CheckOnClick = true;
            viewErrorsToolStripMenuItem.CheckState = CheckState.Checked;
            viewErrorsToolStripMenuItem.Name = "viewErrorsToolStripMenuItem";
            viewErrorsToolStripMenuItem.Size = new Size(212, 22);
            viewErrorsToolStripMenuItem.Text = "&Errors List";
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
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(209, 6);
            // 
            // viewToolbarsToolStripMenuItem
            // 
            viewToolbarsToolStripMenuItem.Name = "viewToolbarsToolStripMenuItem";
            viewToolbarsToolStripMenuItem.Size = new Size(212, 22);
            viewToolbarsToolStripMenuItem.Text = "&Toolbars";
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(96, 6);
            // 
            // toolStripMenuItemExit
            // 
            toolStripMenuItemExit.Name = "toolStripMenuItemExit";
            toolStripMenuItemExit.Size = new Size(99, 22);
            toolStripMenuItemExit.Text = "E&xit";
            // 
            // dockManager
            // 
            dockManager.DocumentMdiStyle = ActiproSoftware.UI.WinForms.Controls.Docking.DocumentMdiStyle.Tabbed;
            dockManager.HostContainerControl = this;
            // 
            // autoHideTabStripPanel1
            // 
            autoHideTabStripPanel1.AllowDrop = true;
            autoHideTabStripPanel1.Dock = DockStyle.Left;
            autoHideTabStripPanel1.DockManager = dockManager;
            autoHideTabStripPanel1.Location = new Point(0, 125);
            autoHideTabStripPanel1.Name = "autoHideTabStripPanel1";
            autoHideTabStripPanel1.Size = new Size(6, 414);
            autoHideTabStripPanel1.TabIndex = 6;
            // 
            // autoHideContainer1
            // 
            autoHideContainer1.AutoHideTabStripPanel = autoHideTabStripPanel1;
            autoHideContainer1.DockManager = dockManager;
            autoHideContainer1.Location = new Point(6, 125);
            autoHideContainer1.Name = "autoHideContainer1";
            autoHideContainer1.Size = new Size(1, 414);
            autoHideContainer1.TabIndex = 7;
            // 
            // autoHideTabStripPanel2
            // 
            autoHideTabStripPanel2.AllowDrop = true;
            autoHideTabStripPanel2.Dock = DockStyle.Top;
            autoHideTabStripPanel2.DockManager = dockManager;
            autoHideTabStripPanel2.Location = new Point(6, 125);
            autoHideTabStripPanel2.Name = "autoHideTabStripPanel2";
            autoHideTabStripPanel2.Size = new Size(772, 6);
            autoHideTabStripPanel2.TabIndex = 8;
            // 
            // autoHideContainer2
            // 
            autoHideContainer2.AutoHideTabStripPanel = autoHideTabStripPanel2;
            autoHideContainer2.DockManager = dockManager;
            autoHideContainer2.Location = new Point(6, 131);
            autoHideContainer2.Name = "autoHideContainer2";
            autoHideContainer2.Size = new Size(772, 1);
            autoHideContainer2.TabIndex = 9;
            // 
            // autoHideTabStripPanel3
            // 
            autoHideTabStripPanel3.AllowDrop = true;
            autoHideTabStripPanel3.Dock = DockStyle.Right;
            autoHideTabStripPanel3.DockManager = dockManager;
            autoHideTabStripPanel3.Location = new Point(778, 125);
            autoHideTabStripPanel3.Name = "autoHideTabStripPanel3";
            autoHideTabStripPanel3.Size = new Size(6, 414);
            autoHideTabStripPanel3.TabIndex = 10;
            // 
            // autoHideContainer3
            // 
            autoHideContainer3.AutoHideTabStripPanel = autoHideTabStripPanel3;
            autoHideContainer3.DockManager = dockManager;
            autoHideContainer3.Location = new Point(777, 125);
            autoHideContainer3.Name = "autoHideContainer3";
            autoHideContainer3.Size = new Size(1, 414);
            autoHideContainer3.TabIndex = 11;
            // 
            // autoHideTabStripPanel4
            // 
            autoHideTabStripPanel4.AllowDrop = true;
            autoHideTabStripPanel4.Dock = DockStyle.Bottom;
            autoHideTabStripPanel4.DockManager = dockManager;
            autoHideTabStripPanel4.Location = new Point(6, 533);
            autoHideTabStripPanel4.Name = "autoHideTabStripPanel4";
            autoHideTabStripPanel4.Size = new Size(772, 6);
            autoHideTabStripPanel4.TabIndex = 12;
            // 
            // autoHideContainer4
            // 
            autoHideContainer4.AutoHideTabStripPanel = autoHideTabStripPanel4;
            autoHideContainer4.DockManager = dockManager;
            autoHideContainer4.Location = new Point(6, 532);
            autoHideContainer4.Name = "autoHideContainer4";
            autoHideContainer4.Size = new Size(772, 1);
            autoHideContainer4.TabIndex = 13;
            // 
            // propertyGrid1
            // 
            propertyGrid1.Dock = DockStyle.Fill;
            propertyGrid1.Location = new Point(0, 0);
            propertyGrid1.Name = "propertyGrid1";
            propertyGrid1.Size = new Size(198, 379);
            propertyGrid1.TabIndex = 14;
            // 
            // toolWindowProperties
            // 
            toolWindowProperties.Controls.Add(propertyGrid1);
            toolWindowProperties.Dock = DockStyle.Fill;
            toolWindowProperties.DockManager = dockManager;
            toolWindowProperties.Key = "propertyGrid";
            toolWindowProperties.Location = new Point(1, 22);
            toolWindowProperties.Name = "toolWindowProperties";
            toolWindowProperties.Size = new Size(198, 379);
            toolWindowProperties.TabIndex = 0;
            toolWindowProperties.Text = "Properties";
            // 
            // toolWindowContainer1
            // 
            toolWindowContainer1.Controls.Add(toolWindowProperties);
            toolWindowContainer1.Dock = DockStyle.Left;
            toolWindowContainer1.DockManager = dockManager;
            toolWindowContainer1.Location = new Point(418, 131);
            toolWindowContainer1.Name = "toolWindowContainer1";
            toolWindowContainer1.Size = new Size(206, 402);
            toolWindowContainer1.TabIndex = 14;
            // 
            // webView21
            // 
            webView21.AllowExternalDrop = true;
            webView21.CreationProperties = null;
            webView21.DefaultBackgroundColor = Color.White;
            webView21.Dock = DockStyle.Fill;
            webView21.Location = new Point(0, 0);
            webView21.Name = "webView21";
            webView21.Size = new Size(198, 379);
            webView21.TabIndex = 15;
            webView21.ZoomFactor = 1D;
            // 
            // toolWindowHelp
            // 
            toolWindowHelp.Controls.Add(webView21);
            toolWindowHelp.Dock = DockStyle.Fill;
            toolWindowHelp.DockManager = dockManager;
            toolWindowHelp.Key = "webView21";
            toolWindowHelp.Location = new Point(1, 22);
            toolWindowHelp.Name = "toolWindowHelp";
            toolWindowHelp.Size = new Size(198, 379);
            toolWindowHelp.TabIndex = 0;
            toolWindowHelp.Text = "Help";
            // 
            // toolWindowContainer2
            // 
            toolWindowContainer2.Controls.Add(toolWindowHelp);
            toolWindowContainer2.Dock = DockStyle.Left;
            toolWindowContainer2.DockManager = dockManager;
            toolWindowContainer2.Location = new Point(212, 131);
            toolWindowContainer2.Name = "toolWindowContainer2";
            toolWindowContainer2.Size = new Size(206, 402);
            toolWindowContainer2.TabIndex = 15;
            // 
            // logView
            // 
            logView.Dock = DockStyle.Fill;
            logView.Location = new Point(0, 0);
            logView.Name = "logView";
            logView.Size = new Size(198, 379);
            logView.TabIndex = 16;
            logView.UseCompatibleStateImageBehavior = false;
            // 
            // toolWindowExecutionLog
            // 
            toolWindowExecutionLog.CanBecomeDocument = ActiproSoftware.UI.WinForms.DefaultableBoolean.True;
            toolWindowExecutionLog.Controls.Add(logView);
            toolWindowExecutionLog.Dock = DockStyle.Fill;
            toolWindowExecutionLog.DockManager = dockManager;
            toolWindowExecutionLog.Key = "Execution Log";
            toolWindowExecutionLog.Location = new Point(1, 22);
            toolWindowExecutionLog.Name = "toolWindowExecutionLog";
            toolWindowExecutionLog.Size = new Size(198, 379);
            toolWindowExecutionLog.TabIndex = 0;
            toolWindowExecutionLog.Text = "Execution Log";
            // 
            // toolWindowContainer3
            // 
            toolWindowContainer3.Controls.Add(toolWindowExecutionLog);
            toolWindowContainer3.Dock = DockStyle.Left;
            toolWindowContainer3.DockManager = dockManager;
            toolWindowContainer3.Location = new Point(6, 131);
            toolWindowContainer3.Name = "toolWindowContainer3";
            toolWindowContainer3.Size = new Size(206, 402);
            toolWindowContainer3.TabIndex = 16;
            // 
            // toolStripComboBoxTool
            // 
            toolStripComboBoxTool.BackColor = SystemColors.Control;
            toolStripComboBoxTool.DropDownStyle = ComboBoxStyle.DropDownList;
            toolStripComboBoxTool.Enabled = false;
            toolStripComboBoxTool.Name = "toolStripComboBoxTool";
            toolStripComboBoxTool.Size = new Size(120, 25);
            // 
            // toolStripComboBoxToolCommand
            // 
            toolStripComboBoxToolCommand.AutoSize = false;
            toolStripComboBoxToolCommand.Enabled = false;
            toolStripComboBoxToolCommand.Name = "toolStripComboBoxToolCommand";
            toolStripComboBoxToolCommand.Size = new Size(250, 23);
            toolStripComboBoxToolCommand.ToolTipText = "Command prompt for the selected tool/object.";
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
            // 
            // toolStripSeparatorTool
            // 
            toolStripSeparatorTool.Name = "toolStripSeparatorTool";
            toolStripSeparatorTool.Size = new Size(6, 25);
            // 
            // toolStripSplitButtonRunScript
            // 
            toolStripSplitButtonRunScript.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripSplitButtonRunScript.Enabled = false;
            toolStripSplitButtonRunScript.Image = (Image)resources.GetObject("toolStripSplitButtonRunScript.Image");
            toolStripSplitButtonRunScript.ImageTransparentColor = Color.Magenta;
            toolStripSplitButtonRunScript.Name = "toolStripSplitButtonRunScript";
            toolStripSplitButtonRunScript.Size = new Size(111, 22);
            toolStripSplitButtonRunScript.Text = "Run File Element";
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
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(784, 561);
            Controls.Add(autoHideContainer1);
            Controls.Add(autoHideContainer2);
            Controls.Add(autoHideContainer3);
            Controls.Add(autoHideContainer4);
            Controls.Add(toolWindowContainer1);
            Controls.Add(toolWindowContainer2);
            Controls.Add(toolWindowContainer3);
            Controls.Add(autoHideTabStripPanel2);
            Controls.Add(autoHideTabStripPanel4);
            Controls.Add(autoHideTabStripPanel3);
            Controls.Add(autoHideTabStripPanel1);
            Controls.Add(panelCustomToolstrips);
            Controls.Add(toolStripMain);
            Controls.Add(statusStrip);
            Name = "MainForm";
            Text = "StepBro Workbench";
            panelCustomToolstrips.ResumeLayout(false);
            panelCustomToolstrips.PerformLayout();
            toolStripMain.ResumeLayout(false);
            toolStripMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dockManager).EndInit();
            toolWindowProperties.ResumeLayout(false);
            toolWindowContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)webView21).EndInit();
            toolWindowHelp.ResumeLayout(false);
            toolWindowContainer2.ResumeLayout(false);
            toolWindowExecutionLog.ResumeLayout(false);
            toolWindowContainer3.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private StatusStrip statusStrip;
        private UI.WinForms.Controls.LogViewer logViewer;
        private UI.WinForms.HelpBrowser helpBrowser;
        private Panel panelCustomToolstrips;
        private ToolStrip toolStripMain;
        private ToolStrip toolStrip2;
        private ToolStrip toolStrip1;
        private Core.Controls.ParsingErrorListView parsingErrorListView;
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
        private ActiproSoftware.UI.WinForms.Controls.Docking.ToolWindowContainer toolWindowContainer1;
        private ActiproSoftware.UI.WinForms.Controls.Docking.ToolWindow toolWindowProperties;
        private PropertyGrid propertyGrid1;
        private ActiproSoftware.UI.WinForms.Controls.Docking.ToolWindowContainer toolWindowContainer2;
        private ActiproSoftware.UI.WinForms.Controls.Docking.ToolWindow toolWindowHelp;
        private Microsoft.Web.WebView2.WinForms.WebView2 webView21;
        private ToolStripMenuItem viewObjectCommandPromptToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator3;
        private ActiproSoftware.UI.WinForms.Controls.Docking.ToolWindowContainer toolWindowContainer3;
        private ActiproSoftware.UI.WinForms.Controls.Docking.ToolWindow toolWindowExecutionLog;
        private ListView logView;
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
    }
}
