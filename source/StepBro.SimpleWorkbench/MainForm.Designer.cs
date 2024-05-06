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
            splitContainerTopButtom = new SplitContainer();
            splitContainerLeftRest = new SplitContainer();
            tabControlTopLeft = new TabControl();
            tabPage1 = new TabPage();
            tabPage2 = new TabPage();
            splitContainerMainRight = new SplitContainer();
            logViewer = new UI.WinForms.Controls.LogViewer();
            splitContainerBottomLeftRight = new SplitContainer();
            helpBrowser = new UI.WinForms.HelpBrowser();
            statusStrip = new StatusStrip();
            panelCustomToolstrips = new Panel();
            toolStrip2 = new ToolStrip();
            toolStrip1 = new ToolStrip();
            toolStripMain = new ToolStrip();
            parsingErrorListView = new Core.Controls.ParsingErrorListView();
            ((System.ComponentModel.ISupportInitialize)splitContainerTopButtom).BeginInit();
            splitContainerTopButtom.Panel1.SuspendLayout();
            splitContainerTopButtom.Panel2.SuspendLayout();
            splitContainerTopButtom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainerLeftRest).BeginInit();
            splitContainerLeftRest.Panel1.SuspendLayout();
            splitContainerLeftRest.Panel2.SuspendLayout();
            splitContainerLeftRest.SuspendLayout();
            tabControlTopLeft.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainerMainRight).BeginInit();
            splitContainerMainRight.Panel2.SuspendLayout();
            splitContainerMainRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainerBottomLeftRight).BeginInit();
            splitContainerBottomLeftRight.Panel1.SuspendLayout();
            splitContainerBottomLeftRight.Panel2.SuspendLayout();
            splitContainerBottomLeftRight.SuspendLayout();
            panelCustomToolstrips.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainerTopButtom
            // 
            splitContainerTopButtom.Dock = DockStyle.Fill;
            splitContainerTopButtom.Location = new Point(0, 125);
            splitContainerTopButtom.Name = "splitContainerTopButtom";
            splitContainerTopButtom.Orientation = Orientation.Horizontal;
            // 
            // splitContainerTopButtom.Panel1
            // 
            splitContainerTopButtom.Panel1.Controls.Add(splitContainerLeftRest);
            // 
            // splitContainerTopButtom.Panel2
            // 
            splitContainerTopButtom.Panel2.Controls.Add(splitContainerBottomLeftRight);
            splitContainerTopButtom.Size = new Size(1375, 632);
            splitContainerTopButtom.SplitterDistance = 457;
            splitContainerTopButtom.SplitterWidth = 5;
            splitContainerTopButtom.TabIndex = 2;
            // 
            // splitContainerLeftRest
            // 
            splitContainerLeftRest.Dock = DockStyle.Fill;
            splitContainerLeftRest.Location = new Point(0, 0);
            splitContainerLeftRest.Name = "splitContainerLeftRest";
            // 
            // splitContainerLeftRest.Panel1
            // 
            splitContainerLeftRest.Panel1.Controls.Add(tabControlTopLeft);
            // 
            // splitContainerLeftRest.Panel2
            // 
            splitContainerLeftRest.Panel2.Controls.Add(splitContainerMainRight);
            splitContainerLeftRest.Size = new Size(1375, 457);
            splitContainerLeftRest.SplitterDistance = 352;
            splitContainerLeftRest.SplitterWidth = 5;
            splitContainerLeftRest.TabIndex = 0;
            // 
            // tabControlTopLeft
            // 
            tabControlTopLeft.Controls.Add(tabPage1);
            tabControlTopLeft.Controls.Add(tabPage2);
            tabControlTopLeft.Dock = DockStyle.Fill;
            tabControlTopLeft.Location = new Point(0, 0);
            tabControlTopLeft.Name = "tabControlTopLeft";
            tabControlTopLeft.SelectedIndex = 0;
            tabControlTopLeft.Size = new Size(352, 457);
            tabControlTopLeft.TabIndex = 0;
            // 
            // tabPage1
            // 
            tabPage1.Location = new Point(4, 24);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(344, 429);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "tabPage1";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(344, 430);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "tabPage2";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // splitContainerMainRight
            // 
            splitContainerMainRight.Dock = DockStyle.Fill;
            splitContainerMainRight.Location = new Point(0, 0);
            splitContainerMainRight.Name = "splitContainerMainRight";
            // 
            // splitContainerMainRight.Panel2
            // 
            splitContainerMainRight.Panel2.Controls.Add(logViewer);
            splitContainerMainRight.Size = new Size(1018, 457);
            splitContainerMainRight.SplitterDistance = 588;
            splitContainerMainRight.SplitterWidth = 5;
            splitContainerMainRight.TabIndex = 0;
            // 
            // logViewer
            // 
            logViewer.Dock = DockStyle.Fill;
            logViewer.Location = new Point(0, 0);
            logViewer.Name = "logViewer";
            logViewer.Size = new Size(425, 457);
            logViewer.TabIndex = 0;
            // 
            // splitContainerBottomLeftRight
            // 
            splitContainerBottomLeftRight.Dock = DockStyle.Fill;
            splitContainerBottomLeftRight.Location = new Point(0, 0);
            splitContainerBottomLeftRight.Name = "splitContainerBottomLeftRight";
            // 
            // splitContainerBottomLeftRight.Panel1
            // 
            splitContainerBottomLeftRight.Panel1.Controls.Add(helpBrowser);
            // 
            // splitContainerBottomLeftRight.Panel2
            // 
            splitContainerBottomLeftRight.Panel2.Controls.Add(parsingErrorListView);
            splitContainerBottomLeftRight.Size = new Size(1375, 170);
            splitContainerBottomLeftRight.SplitterDistance = 725;
            splitContainerBottomLeftRight.SplitterWidth = 5;
            splitContainerBottomLeftRight.TabIndex = 0;
            // 
            // helpBrowser
            // 
            helpBrowser.Dock = DockStyle.Fill;
            helpBrowser.Location = new Point(0, 0);
            helpBrowser.Name = "helpBrowser";
            helpBrowser.Size = new Size(725, 170);
            helpBrowser.TabIndex = 0;
            helpBrowser.Load += helpBrowser_Load;
            // 
            // statusStrip
            // 
            statusStrip.Location = new Point(0, 757);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(1375, 22);
            statusStrip.TabIndex = 3;
            statusStrip.Text = "statusStrip";
            // 
            // panelCustomToolstrips
            // 
            panelCustomToolstrips.Controls.Add(toolStrip2);
            panelCustomToolstrips.Controls.Add(toolStrip1);
            panelCustomToolstrips.Dock = DockStyle.Top;
            panelCustomToolstrips.Location = new Point(0, 25);
            panelCustomToolstrips.Name = "panelCustomToolstrips";
            panelCustomToolstrips.Size = new Size(1375, 100);
            panelCustomToolstrips.TabIndex = 4;
            // 
            // toolStrip2
            // 
            toolStrip2.BackColor = Color.Turquoise;
            toolStrip2.Location = new Point(0, 25);
            toolStrip2.Name = "toolStrip2";
            toolStrip2.Size = new Size(1375, 25);
            toolStrip2.TabIndex = 1;
            toolStrip2.Text = "toolStrip2";
            // 
            // toolStrip1
            // 
            toolStrip1.BackColor = Color.Tan;
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(1375, 25);
            toolStrip1.TabIndex = 0;
            toolStrip1.Text = "toolStrip1";
            // 
            // toolStripMain
            // 
            toolStripMain.BackColor = Color.Khaki;
            toolStripMain.Location = new Point(0, 0);
            toolStripMain.Name = "toolStripMain";
            toolStripMain.Size = new Size(1375, 25);
            toolStripMain.TabIndex = 5;
            toolStripMain.Text = "toolStripMain";
            // 
            // parsingErrorListView
            // 
            parsingErrorListView.Dock = DockStyle.Fill;
            parsingErrorListView.Location = new Point(0, 0);
            parsingErrorListView.Name = "parsingErrorListView";
            parsingErrorListView.Size = new Size(645, 170);
            parsingErrorListView.TabIndex = 0;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(1375, 779);
            Controls.Add(splitContainerTopButtom);
            Controls.Add(panelCustomToolstrips);
            Controls.Add(toolStripMain);
            Controls.Add(statusStrip);
            Name = "MainForm";
            Text = "StepBro Workbench";
            splitContainerTopButtom.Panel1.ResumeLayout(false);
            splitContainerTopButtom.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerTopButtom).EndInit();
            splitContainerTopButtom.ResumeLayout(false);
            splitContainerLeftRest.Panel1.ResumeLayout(false);
            splitContainerLeftRest.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerLeftRest).EndInit();
            splitContainerLeftRest.ResumeLayout(false);
            tabControlTopLeft.ResumeLayout(false);
            splitContainerMainRight.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerMainRight).EndInit();
            splitContainerMainRight.ResumeLayout(false);
            splitContainerBottomLeftRight.Panel1.ResumeLayout(false);
            splitContainerBottomLeftRight.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerBottomLeftRight).EndInit();
            splitContainerBottomLeftRight.ResumeLayout(false);
            panelCustomToolstrips.ResumeLayout(false);
            panelCustomToolstrips.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private SplitContainer splitContainerTopButtom;
        private SplitContainer splitContainerBottomLeftRight;
        private SplitContainer splitContainerLeftRest;
        private SplitContainer splitContainerMainRight;
        private StatusStrip statusStrip;
        private TabControl tabControlTopLeft;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private UI.WinForms.Controls.LogViewer logViewer;
        private UI.WinForms.HelpBrowser helpBrowser;
        private Panel panelCustomToolstrips;
        private ToolStrip toolStripMain;
        private ToolStrip toolStrip2;
        private ToolStrip toolStrip1;
        private Core.Controls.ParsingErrorListView parsingErrorListView;
    }
}
