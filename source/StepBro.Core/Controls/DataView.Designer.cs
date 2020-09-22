namespace StepBro.Core.Controls
{
    partial class DataView
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DataView));
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.buttonAutoUpdate = new System.Windows.Forms.ToolStripButton();
            this.updateTimer = new System.Windows.Forms.Timer(this.components);
            this.dataViewControl = new StepBro.Core.Controls.DataViewControl();
            this.toolStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataViewControl)).BeginInit();
            this.SuspendLayout();
            // 
            // toolStrip
            // 
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonAutoUpdate});
            this.toolStrip.Location = new System.Drawing.Point(0, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new System.Drawing.Size(512, 25);
            this.toolStrip.TabIndex = 0;
            this.toolStrip.Text = "toolStrip";
            // 
            // buttonAutoUpdate
            // 
            this.buttonAutoUpdate.CheckOnClick = true;
            this.buttonAutoUpdate.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.buttonAutoUpdate.Image = ((System.Drawing.Image)(resources.GetObject("buttonAutoUpdate.Image")));
            this.buttonAutoUpdate.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonAutoUpdate.Name = "buttonAutoUpdate";
            this.buttonAutoUpdate.Size = new System.Drawing.Size(78, 22);
            this.buttonAutoUpdate.Text = "Auto Update";
            // 
            // updateTimer
            // 
            this.updateTimer.Enabled = true;
            this.updateTimer.Interval = 500;
            this.updateTimer.Tick += new System.EventHandler(this.updateTimer_Tick);
            // 
            // dataViewControl
            // 
            this.dataViewControl.AutoCompleteBracketsList = new char[] {
        '(',
        ')',
        '{',
        '}',
        '[',
        ']',
        '\"',
        '\"',
        '\'',
        '\''};
            this.dataViewControl.AutoScrollMinSize = new System.Drawing.Size(2, 14);
            this.dataViewControl.BackBrush = null;
            this.dataViewControl.CharHeight = 14;
            this.dataViewControl.CharWidth = 8;
            this.dataViewControl.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.dataViewControl.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.dataViewControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataViewControl.Font = new System.Drawing.Font("Courier New", 9.75F);
            this.dataViewControl.IsReplaceMode = false;
            this.dataViewControl.Location = new System.Drawing.Point(0, 25);
            this.dataViewControl.MaxUpdateTime = System.TimeSpan.Parse("00:00:00.2000000");
            this.dataViewControl.Name = "dataViewControl";
            this.dataViewControl.Paddings = new System.Windows.Forms.Padding(0);
            this.dataViewControl.ReadOnly = true;
            this.dataViewControl.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            this.dataViewControl.ServiceColors = ((FastColoredTextBoxNS.ServiceColors)(resources.GetObject("dataViewControl.ServiceColors")));
            this.dataViewControl.ShowLineNumbers = false;
            this.dataViewControl.Size = new System.Drawing.Size(512, 440);
            this.dataViewControl.TabIndex = 1;
            this.dataViewControl.Zoom = 100;
            // 
            // DataView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dataViewControl);
            this.Controls.Add(this.toolStrip);
            this.Name = "DataView";
            this.Size = new System.Drawing.Size(512, 465);
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataViewControl)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton buttonAutoUpdate;
        private System.Windows.Forms.Timer updateTimer;
        private DataViewControl dataViewControl;
    }
}
