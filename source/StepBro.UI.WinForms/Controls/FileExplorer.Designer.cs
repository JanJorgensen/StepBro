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
            imageList = new ImageList(components);
            refreshTimer = new System.Windows.Forms.Timer(components);
            SuspendLayout();
            // 
            // treeView
            // 
            treeView.Dock = DockStyle.Fill;
            treeView.ImageIndex = 0;
            treeView.ImageList = imageList;
            treeView.Indent = 19;
            treeView.Location = new Point(0, 0);
            treeView.Name = "treeView";
            treeView.SelectedImageIndex = 0;
            treeView.Size = new Size(150, 150);
            treeView.TabIndex = 0;
            treeView.BeforeSelect += treeView_BeforeSelect;
            treeView.AfterSelect += TreeView_AfterSelect;
            treeView.NodeMouseDoubleClick += treeView_NodeMouseDoubleClick;
            // 
            // imageList
            // 
            imageList.ColorDepth = ColorDepth.Depth32Bit;
            imageList.ImageStream = (ImageListStreamer)resources.GetObject("imageList.ImageStream");
            imageList.TransparentColor = Color.Transparent;
            imageList.Images.SetKeyName(0, "Folder.png");
            imageList.Images.SetKeyName(1, "AnyFile.png");
            imageList.Images.SetKeyName(2, "ScriptFile.png");
            imageList.Images.SetKeyName(3, "TopScriptFile.png");
            // 
            // refreshTimer
            // 
            refreshTimer.Enabled = true;
            refreshTimer.Interval = 1000;
            refreshTimer.Tick += refreshTimer_Tick;
            // 
            // FileExplorer
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(treeView);
            Name = "FileExplorer";
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TreeView treeView;
        private ImageList imageList;
        private System.Windows.Forms.Timer refreshTimer;
    }
}
