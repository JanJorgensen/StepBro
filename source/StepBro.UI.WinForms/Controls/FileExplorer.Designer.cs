namespace StepBro.UI.WinForms.Controls
{
    partial class FileExplorer
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FileExplorer));
            treeView = new TreeView();
            refreshTimer = new System.Windows.Forms.Timer(components);
            toolStrip = new ToolStrip();
            toolStripButtonShowFiles = new ToolStripButton();
            toolStripButtonShowElements = new ToolStripButton();
            toolStripTextBoxSearch = new ToolStripTextBox();
            toolStrip.SuspendLayout();
            SuspendLayout();
            // 
            // treeView
            // 
            treeView.Dock = DockStyle.Fill;
            treeView.FullRowSelect = true;
            treeView.Indent = 19;
            treeView.Location = new Point(0, 25);
            treeView.Name = "treeView";
            treeView.Size = new Size(428, 217);
            treeView.TabIndex = 0;
            treeView.BeforeCollapse += treeView_BeforeCollapse;
            treeView.BeforeExpand += treeView_BeforeExpand;
            treeView.BeforeSelect += treeView_BeforeSelect;
            treeView.AfterSelect += TreeView_AfterSelect;
            treeView.NodeMouseDoubleClick += treeView_NodeMouseDoubleClick;
            treeView.MouseDown += treeView_MouseDown;
            // 
            // refreshTimer
            // 
            refreshTimer.Enabled = true;
            refreshTimer.Interval = 1000;
            refreshTimer.Tick += refreshTimer_Tick;
            // 
            // toolStrip
            // 
            toolStrip.Items.AddRange(new ToolStripItem[] { toolStripButtonShowFiles, toolStripButtonShowElements, toolStripTextBoxSearch });
            toolStrip.Location = new Point(0, 0);
            toolStrip.Name = "toolStrip";
            toolStrip.Size = new Size(428, 25);
            toolStrip.TabIndex = 1;
            toolStrip.Text = "toolStrip";
            // 
            // toolStripButtonShowFiles
            // 
            toolStripButtonShowFiles.Checked = true;
            toolStripButtonShowFiles.CheckOnClick = true;
            toolStripButtonShowFiles.CheckState = CheckState.Checked;
            toolStripButtonShowFiles.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButtonShowFiles.Enabled = false;
            toolStripButtonShowFiles.Image = (Image)resources.GetObject("toolStripButtonShowFiles.Image");
            toolStripButtonShowFiles.ImageTransparentColor = Color.Magenta;
            toolStripButtonShowFiles.Name = "toolStripButtonShowFiles";
            toolStripButtonShowFiles.Size = new Size(34, 22);
            toolStripButtonShowFiles.Text = "Files";
            toolStripButtonShowFiles.ToolTipText = "Show general files view";
            toolStripButtonShowFiles.CheckedChanged += toolStripButtonShowFiles_CheckedChanged;
            // 
            // toolStripButtonShowElements
            // 
            toolStripButtonShowElements.CheckOnClick = true;
            toolStripButtonShowElements.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButtonShowElements.Image = (Image)resources.GetObject("toolStripButtonShowElements.Image");
            toolStripButtonShowElements.ImageTransparentColor = Color.Magenta;
            toolStripButtonShowElements.Name = "toolStripButtonShowElements";
            toolStripButtonShowElements.Size = new Size(59, 22);
            toolStripButtonShowElements.Text = "Elements";
            toolStripButtonShowElements.CheckedChanged += toolStripButtonShowElements_CheckedChanged;
            // 
            // toolStripTextBoxSearch
            // 
            toolStripTextBoxSearch.Alignment = ToolStripItemAlignment.Right;
            toolStripTextBoxSearch.Name = "toolStripTextBoxSearch";
            toolStripTextBoxSearch.Size = new Size(100, 25);
            toolStripTextBoxSearch.Visible = false;
            // 
            // FileExplorer
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(treeView);
            Controls.Add(toolStrip);
            Name = "FileExplorer";
            Size = new Size(428, 242);
            toolStrip.ResumeLayout(false);
            toolStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TreeView treeView;
        private System.Windows.Forms.Timer refreshTimer;
        private ToolStrip toolStrip;
        private ToolStripButton toolStripButtonShowFiles;
        private ToolStripButton toolStripButtonShowElements;
        private ToolStripTextBox toolStripTextBoxSearch;
    }
}
