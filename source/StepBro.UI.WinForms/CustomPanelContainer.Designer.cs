namespace StepBro.UI.WinForms
{
    partial class CustomPanelContainer
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
            treeViewPanelNodes = new TreeView();
            panelParent = new Panel();
            panelTreeViewParent = new Panel();
            panelTreeViewValueParent = new Panel();
            panelTreeViewParent.SuspendLayout();
            SuspendLayout();
            // 
            // treeViewPanelNodes
            // 
            treeViewPanelNodes.Dock = DockStyle.Fill;
            treeViewPanelNodes.Location = new Point(0, 0);
            treeViewPanelNodes.Name = "treeViewPanelNodes";
            treeViewPanelNodes.ShowNodeToolTips = true;
            treeViewPanelNodes.Size = new Size(200, 336);
            treeViewPanelNodes.TabIndex = 0;
            treeViewPanelNodes.Visible = false;
            // 
            // panelParent
            // 
            panelParent.BorderStyle = BorderStyle.FixedSingle;
            panelParent.Dock = DockStyle.Fill;
            panelParent.Location = new Point(200, 0);
            panelParent.Name = "panelParent";
            panelParent.Size = new Size(336, 366);
            panelParent.TabIndex = 1;
            // 
            // panelTreeViewParent
            // 
            panelTreeViewParent.Controls.Add(treeViewPanelNodes);
            panelTreeViewParent.Controls.Add(panelTreeViewValueParent);
            panelTreeViewParent.Dock = DockStyle.Left;
            panelTreeViewParent.Location = new Point(0, 0);
            panelTreeViewParent.Name = "panelTreeViewParent";
            panelTreeViewParent.Size = new Size(200, 366);
            panelTreeViewParent.TabIndex = 2;
            panelTreeViewParent.Visible = false;
            // 
            // panelTreeViewValueParent
            // 
            panelTreeViewValueParent.BorderStyle = BorderStyle.FixedSingle;
            panelTreeViewValueParent.Dock = DockStyle.Bottom;
            panelTreeViewValueParent.Location = new Point(0, 336);
            panelTreeViewValueParent.Name = "panelTreeViewValueParent";
            panelTreeViewValueParent.Size = new Size(200, 30);
            panelTreeViewValueParent.TabIndex = 3;
            panelTreeViewValueParent.Visible = false;
            // 
            // CustomPanelContainer
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            Controls.Add(panelParent);
            Controls.Add(panelTreeViewParent);
            Name = "CustomPanelContainer";
            Size = new Size(536, 366);
            panelTreeViewParent.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TreeView treeViewPanelNodes;
        private Panel panelParent;
        private Panel panelTreeViewParent;
        private Panel panelTreeViewValueParent;
    }
}