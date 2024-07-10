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
            toolStripDropDownButtonDisplayLevels = new ToolStripDropDownButton();
            toolStripMenuItemLevels2 = new ToolStripMenuItem();
            toolStripMenuItemLevels3 = new ToolStripMenuItem();
            toolStripMenuItemLevels4 = new ToolStripMenuItem();
            toolStripMenuItemLevels5 = new ToolStripMenuItem();
            toolStripMenuItemLevels6 = new ToolStripMenuItem();
            toolStripMenuItemLevelsAll = new ToolStripMenuItem();
            toolStripButtonFollowHead = new ToolStripButton();
            logView = new ChronoListView();
            toolStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // toolStrip1
            // 
            toolStrip1.Items.AddRange(new ToolStripItem[] { toolStripDropDownButtonDisplayLevels, toolStripButtonFollowHead });
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(636, 25);
            toolStrip1.TabIndex = 0;
            toolStrip1.Text = "toolStrip1";
            // 
            // toolStripDropDownButtonDisplayLevels
            // 
            toolStripDropDownButtonDisplayLevels.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripDropDownButtonDisplayLevels.DropDownItems.AddRange(new ToolStripItem[] { toolStripMenuItemLevels2, toolStripMenuItemLevels3, toolStripMenuItemLevels4, toolStripMenuItemLevels5, toolStripMenuItemLevels6, toolStripMenuItemLevelsAll });
            toolStripDropDownButtonDisplayLevels.Image = (Image)resources.GetObject("toolStripDropDownButtonDisplayLevels.Image");
            toolStripDropDownButtonDisplayLevels.ImageTransparentColor = Color.Magenta;
            toolStripDropDownButtonDisplayLevels.Name = "toolStripDropDownButtonDisplayLevels";
            toolStripDropDownButtonDisplayLevels.Size = new Size(26, 22);
            toolStripDropDownButtonDisplayLevels.Text = "8";
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
            toolStripButtonFollowHead.Image = (Image)resources.GetObject("toolStripButtonFollowHead.Image");
            toolStripButtonFollowHead.ImageTransparentColor = Color.Magenta;
            toolStripButtonFollowHead.Name = "toolStripButtonFollowHead";
            toolStripButtonFollowHead.Size = new Size(23, 22);
            toolStripButtonFollowHead.Text = "V";
            toolStripButtonFollowHead.ToolTipText = "Jump to and follow the end.";
            toolStripButtonFollowHead.CheckedChanged += toolStripButtonFollowHead_CheckedChanged;
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
    }
}
