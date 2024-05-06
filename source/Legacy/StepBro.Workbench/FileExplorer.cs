using StepBro.Core.General;
using StepBro.Core.ScriptData;
using System;
using System.Windows.Forms;

namespace StepBro.Workbench
{
    public partial class FileExplorer : ToolWindow
    {
        private ILoadedFilesManager m_fileManager;

        public FileExplorer()
        {
            this.InitializeComponent();

            if (StepBro.Core.Main.IsInitialized)
            {
                m_fileManager = StepBro.Core.Main.GetService<ILoadedFilesManager>();
                m_fileManager.FileLoaded += this.FileManager_FileLoaded;
                m_fileManager.FileClosed += this.FileManager_FileClosed;
                refreshTimer.Stop();
                refreshTimer.Start();
            }
        }

        private void FileManager_FileLoaded(object sender, LoadedFileEventArgs args)
        {
            this.RestartTimer();
        }

        private void FileManager_FileClosed(object sender, LoadedFileEventArgs args)
        {
            this.RestartTimer();
        }

        private void RestartTimer()
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() =>
                {
                    refreshTimer.Stop();
                    refreshTimer.Start();
                }));
            }
            else
            {
                refreshTimer.Stop();
                refreshTimer.Start();
            }
        }

        private void refreshTimer_Tick(object sender, EventArgs e)
        {
            refreshTimer.Stop();
            this.UpdateTree();
        }

        private void UpdateTree()
        {
            System.Diagnostics.Debug.WriteLine("UpdateTree");
            treeView.BeginUpdate();
            try
            {
                treeView.Nodes.Clear();
                var loadedFilesNode = new TreeNode
                {
                    ImageIndex = 1,
                    SelectedImageIndex = 1,
                    Text = "Loaded Files"
                };
                treeView.Nodes.Add(loadedFilesNode);
                foreach (var file in m_fileManager.ListFiles<ILoadedFile>())
                {
                    TreeNode fileNode;
                    if (file is IScriptFile)
                    {
                        fileNode = new TreeNode
                        {
                            ImageIndex = 4,
                            SelectedImageIndex = 4,
                            Text = file.FileName,
                            ToolTipText = file.FilePath
                        };
                        loadedFilesNode.Nodes.Add(fileNode);
                    }
                    else
                    {
                        fileNode = new TreeNode
                        {
                            ImageIndex = 5,
                            SelectedImageIndex = 5,
                            Text = file.FileName,
                            ToolTipText = file.FilePath
                        };
                        loadedFilesNode.Nodes.Add(fileNode);
                    }
                    fileNode.Tag = file;
                }
                loadedFilesNode.Expand();
            }
            finally
            {
                treeView.EndUpdate();
            }
        }
        protected override void OnRightToLeftLayoutChanged(EventArgs e)
        {
            treeView.RightToLeftLayout = this.RightToLeftLayout;
        }

        private void treeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            MainForm.Instance.ShowFileEditor(e.Node.Tag as ILoadedFile, -1, -1, -1);
        }

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }
    }
}