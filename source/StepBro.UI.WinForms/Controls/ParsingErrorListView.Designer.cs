namespace StepBro.Core.Controls
{
    partial class ParsingErrorListView
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ParsingErrorListView));
            listView = new ListView();
            columnHeaderSeverity = new ColumnHeader();
            columnHeaderDescription = new ColumnHeader();
            columnHeaderFile = new ColumnHeader();
            columnHeaderLine = new ColumnHeader();
            columnHeaderColumn = new ColumnHeader();
            refreshTimer = new System.Windows.Forms.Timer(components);
            toolStrip = new ToolStrip();
            toolStripButtonParseFiles = new ToolStripButton();
            toolStripButtonAutoParseFiles = new ToolStripButton();
            toolStrip.SuspendLayout();
            SuspendLayout();
            // 
            // listView
            // 
            listView.Columns.AddRange(new ColumnHeader[] { columnHeaderSeverity, columnHeaderDescription, columnHeaderFile, columnHeaderLine, columnHeaderColumn });
            listView.Dock = DockStyle.Fill;
            listView.FullRowSelect = true;
            listView.Location = new Point(0, 25);
            listView.Name = "listView";
            listView.Size = new Size(770, 160);
            listView.TabIndex = 0;
            listView.UseCompatibleStateImageBehavior = false;
            listView.View = View.Details;
            listView.SelectedIndexChanged += listView_SelectedIndexChanged;
            listView.DoubleClick += listView_DoubleClick;
            // 
            // columnHeaderSeverity
            // 
            columnHeaderSeverity.Text = "";
            columnHeaderSeverity.Width = 20;
            // 
            // columnHeaderDescription
            // 
            columnHeaderDescription.Text = "Description";
            columnHeaderDescription.Width = 400;
            // 
            // columnHeaderFile
            // 
            columnHeaderFile.Text = "File";
            columnHeaderFile.Width = 180;
            // 
            // columnHeaderLine
            // 
            columnHeaderLine.Text = "Line";
            columnHeaderLine.Width = 50;
            // 
            // columnHeaderColumn
            // 
            columnHeaderColumn.Text = "Col";
            columnHeaderColumn.Width = 50;
            // 
            // refreshTimer
            // 
            refreshTimer.Interval = 200;
            refreshTimer.Tick += refreshTimer_Tick;
            // 
            // toolStrip
            // 
            toolStrip.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip.Items.AddRange(new ToolStripItem[] { toolStripButtonParseFiles, toolStripButtonAutoParseFiles });
            toolStrip.Location = new Point(0, 0);
            toolStrip.Name = "toolStrip";
            toolStrip.Size = new Size(770, 25);
            toolStrip.TabIndex = 1;
            toolStrip.Text = "toolStrip";
            // 
            // toolStripButtonParseFiles
            // 
            toolStripButtonParseFiles.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButtonParseFiles.Image = (Image)resources.GetObject("toolStripButtonParseFiles.Image");
            toolStripButtonParseFiles.ImageTransparentColor = Color.Magenta;
            toolStripButtonParseFiles.Name = "toolStripButtonParseFiles";
            toolStripButtonParseFiles.Size = new Size(65, 22);
            toolStripButtonParseFiles.Text = "Parse Files";
            toolStripButtonParseFiles.Click += toolStripButtonParseFiles_Click;
            // 
            // toolStripButtonAutoParseFiles
            // 
            toolStripButtonAutoParseFiles.CheckOnClick = true;
            toolStripButtonAutoParseFiles.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButtonAutoParseFiles.Image = (Image)resources.GetObject("toolStripButtonAutoParseFiles.Image");
            toolStripButtonAutoParseFiles.ImageTransparentColor = Color.Magenta;
            toolStripButtonAutoParseFiles.Name = "toolStripButtonAutoParseFiles";
            toolStripButtonAutoParseFiles.Size = new Size(94, 22);
            toolStripButtonAutoParseFiles.Text = "Auto Parse Files";
            toolStripButtonAutoParseFiles.CheckedChanged += toolStripButtonAutoParseFiles_CheckedChanged;
            // 
            // ParsingErrorListView
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Controls.Add(listView);
            Controls.Add(toolStrip);
            Name = "ParsingErrorListView";
            Size = new Size(770, 185);
            toolStrip.ResumeLayout(false);
            toolStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ListView listView;
        private System.Windows.Forms.ColumnHeader columnHeaderSeverity;
        private System.Windows.Forms.ColumnHeader columnHeaderDescription;
        private System.Windows.Forms.ColumnHeader columnHeaderFile;
        private System.Windows.Forms.ColumnHeader columnHeaderLine;
        private System.Windows.Forms.Timer refreshTimer;
        private System.Windows.Forms.ColumnHeader columnHeaderColumn;
        private ToolStrip toolStrip;
        private ToolStripButton toolStripButtonParseFiles;
        private ToolStripButton toolStripButtonAutoParseFiles;
    }
}
