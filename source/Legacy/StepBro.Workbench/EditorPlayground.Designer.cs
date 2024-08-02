namespace StepBro.Workbench
{
    partial class EditorPlayground
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditorPlayground));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemClear = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemAdd50Lines = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemAction1 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemAction2 = new System.Windows.Forms.ToolStripMenuItem();
            this.fctb = new FastColoredTextBoxNS.FastColoredTextBox();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fctb)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.menuItemAction1,
            this.menuItemAction2});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(472, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemClear,
            this.menuItemAdd50Lines});
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(59, 20);
            this.toolStripMenuItem1.Text = "General";
            // 
            // menuItemClear
            // 
            this.menuItemClear.Name = "menuItemClear";
            this.menuItemClear.Size = new System.Drawing.Size(180, 22);
            this.menuItemClear.Text = "Clear";
            this.menuItemClear.Click += new System.EventHandler(this.menuItemClear_Click);
            // 
            // menuItemAdd50Lines
            // 
            this.menuItemAdd50Lines.Name = "menuItemAdd50Lines";
            this.menuItemAdd50Lines.Size = new System.Drawing.Size(180, 22);
            this.menuItemAdd50Lines.Text = "Add 50 Lines";
            this.menuItemAdd50Lines.Click += new System.EventHandler(this.menuItemAdd50Lines_Click);
            // 
            // menuItemAction1
            // 
            this.menuItemAction1.Name = "menuItemAction1";
            this.menuItemAction1.Size = new System.Drawing.Size(63, 20);
            this.menuItemAction1.Text = "Action 1";
            this.menuItemAction1.Click += new System.EventHandler(this.menuItemAction1_Click);
            // 
            // menuItemAction2
            // 
            this.menuItemAction2.Name = "menuItemAction2";
            this.menuItemAction2.Size = new System.Drawing.Size(63, 20);
            this.menuItemAction2.Text = "Action 2";
            this.menuItemAction2.Click += new System.EventHandler(this.menuItemAction2_Click);
            // 
            // fctb
            // 
            this.fctb.AutoCompleteBracketsList = new char[] {
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
            this.fctb.AutoIndent = false;
            this.fctb.AutoIndentChars = false;
            this.fctb.AutoIndentExistingLines = false;
            this.fctb.AutoScrollMinSize = new System.Drawing.Size(27, 14);
            this.fctb.BackBrush = null;
            this.fctb.CharHeight = 14;
            this.fctb.CharWidth = 8;
            this.fctb.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.fctb.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.fctb.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fctb.IsReplaceMode = false;
            this.fctb.Location = new System.Drawing.Point(0, 24);
            this.fctb.Name = "fctb";
            this.fctb.Paddings = new System.Windows.Forms.Padding(0);
            this.fctb.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            this.fctb.ServiceColors = ((FastColoredTextBoxNS.ServiceColors)(resources.GetObject("fctb.ServiceColors")));
            this.fctb.Size = new System.Drawing.Size(472, 259);
            this.fctb.TabIndex = 1;
            this.fctb.Zoom = 100;
            this.fctb.LineInserted += new System.EventHandler<FastColoredTextBoxNS.LineInsertedEventArgs>(this.fctb_LineInserted);
            this.fctb.LineRemoved += new System.EventHandler<FastColoredTextBoxNS.LineRemovedEventArgs>(this.fctb_LineRemoved);
            this.fctb.Click += new System.EventHandler(this.fctb_Click);
            // 
            // EditorPlayground
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.fctb);
            this.Controls.Add(this.menuStrip1);
            this.Name = "EditorPlayground";
            this.Size = new System.Drawing.Size(472, 283);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fctb)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem menuItemClear;
        private System.Windows.Forms.ToolStripMenuItem menuItemAdd50Lines;
        private System.Windows.Forms.ToolStripMenuItem menuItemAction1;
        private System.Windows.Forms.ToolStripMenuItem menuItemAction2;
        private FastColoredTextBoxNS.FastColoredTextBox fctb;
    }
}
