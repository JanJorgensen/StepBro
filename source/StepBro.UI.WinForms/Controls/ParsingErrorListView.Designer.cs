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
            listView = new ListView();
            columnHeaderSeverity = new ColumnHeader();
            columnHeaderDescription = new ColumnHeader();
            columnHeaderFile = new ColumnHeader();
            columnHeaderLine = new ColumnHeader();
            columnHeaderColumn = new ColumnHeader();
            refreshTimer = new System.Windows.Forms.Timer(components);
            SuspendLayout();
            // 
            // listView
            // 
            listView.Columns.AddRange(new ColumnHeader[] { columnHeaderSeverity, columnHeaderDescription, columnHeaderFile, columnHeaderLine, columnHeaderColumn });
            listView.Dock = DockStyle.Fill;
            listView.FullRowSelect = true;
            listView.Location = new Point(0, 0);
            listView.Name = "listView";
            listView.Size = new Size(770, 185);
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
            // ParsingErrorListView
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Controls.Add(listView);
            Name = "ParsingErrorListView";
            Size = new Size(770, 185);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.ListView listView;
        private System.Windows.Forms.ColumnHeader columnHeaderSeverity;
        private System.Windows.Forms.ColumnHeader columnHeaderDescription;
        private System.Windows.Forms.ColumnHeader columnHeaderFile;
        private System.Windows.Forms.ColumnHeader columnHeaderLine;
        private System.Windows.Forms.Timer refreshTimer;
        private System.Windows.Forms.ColumnHeader columnHeaderColumn;
    }
}
