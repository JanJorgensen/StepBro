using StepBro.Core.General;
using StepBro.Core.ScriptData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StepBro.UI.WinForms.Controls
{
    public partial class FileExplorer : UserControl
    {
        // ABOUT THE WARNING: https://stackoverflow.com/questions/77565011/how-to-fix-warning-msb3825-for-a-localizable-winform-containing-a-listview-with

        private ILoadedFilesManager m_fileManager;
        private Font m_bold = null;
        private ILoadedFile m_currentSelection = null;

        public FileExplorer()
        {
            InitializeComponent();
        }

        public object HostTopFileDependancyObject { get; set; } = null;
        public object HostDependancyObject { get; set; } = null;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            m_bold = new Font(this.Font, FontStyle.Bold);
            if (StepBro.Core.Main.IsInitialized)
            {
                m_fileManager = StepBro.Core.Main.GetService<ILoadedFilesManager>();
                m_fileManager.FileLoaded += this.FileManager_FileLoaded;
                m_fileManager.FileClosed += this.FileManager_FileClosed;
                refreshTimer.Stop();
                refreshTimer.Start();
            }
        }

        public class FileSelectionEventArgs : EventArgs
        {
            public enum SelectionType { Selected, Unselected, Activated }

            public FileSelectionEventArgs(ILoadedFile file, SelectionType selection)
            {
                this.File = file;
                this.Selection = selection;
            }

            public ILoadedFile File { get; private set; }
            public SelectionType Selection { get; private set; }
        }

        [Browsable(true)]
        [Description("Notifies changes in the current file node selection.")]
        public event EventHandler<FileSelectionEventArgs> FileSelectionChanged;

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
                    ImageIndex = 0,
                    SelectedImageIndex = 0,
                    Text = "Loaded Files"
                };
                treeView.Nodes.Add(loadedFilesNode);
                foreach (var file in m_fileManager.ListFiles<ILoadedFile>())
                {
                    TreeNode fileNode;
                    if (file is IScriptFile)
                    {
                        var isTopFile = this.HostTopFileDependancyObject != null && file.IsDependantOf(this.HostTopFileDependancyObject);
                        fileNode = new TreeNode
                        {
                            ImageIndex = isTopFile ? 3 : 2,
                            SelectedImageIndex = isTopFile ? 3 : 2,
                            Text = file.FileName,
                            ToolTipText = file.FilePath
                        };
                        if (this.HostDependancyObject != null && file.IsDependantOf(this.HostDependancyObject))
                        {
                            fileNode.NodeFont = m_bold;
                        }
                        loadedFilesNode.Nodes.Add(fileNode);
                    }
                    else
                    {
                        fileNode = new TreeNode
                        {
                            ImageIndex = 1,
                            SelectedImageIndex = 1,
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

        private void treeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            this.FileSelectionChanged?.Invoke(this, new FileSelectionEventArgs(m_currentSelection, FileSelectionEventArgs.SelectionType.Activated));
        }

        private void TreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node != null && e.Node.Tag != null && e.Node.Tag is ILoadedFile)
            {
                m_currentSelection = e.Node.Tag as ILoadedFile;
                this.FileSelectionChanged?.Invoke(this, new FileSelectionEventArgs(m_currentSelection, FileSelectionEventArgs.SelectionType.Selected));
            }
        }

        private void treeView_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            if (m_currentSelection != null)
            {
                this.FileSelectionChanged?.Invoke(this, new FileSelectionEventArgs(m_currentSelection, FileSelectionEventArgs.SelectionType.Unselected));
                m_currentSelection = null;
            }
        }
    }
}
