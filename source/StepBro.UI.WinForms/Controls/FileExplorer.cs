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
        private object m_currentSelection = null;
        private bool m_isDoubleClick = false;
        private ImageList m_imageList = null;

        public FileExplorer()
        {
            InitializeComponent();
        }

        public object HostTopFileDependancyObject { get; set; } = null;
        public object HostDependancyObject { get; set; } = null;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            m_imageList = new ImageList();
            m_imageList.Images.Add(Properties.Resources.Folder);
            m_imageList.Images.Add(Properties.Resources.AnyFile);
            m_imageList.Images.Add(Properties.Resources.ScriptFile);
            m_imageList.Images.Add(Properties.Resources.TopScriptFile);
            m_imageList.Images.Add(Properties.Resources.Namespace);
            m_imageList.Images.Add(Properties.Resources.TestList);
            m_imageList.Images.Add(Properties.Resources.Procedure);
            m_imageList.Images.Add(Properties.Resources.Variable);
            treeView.ImageList = m_imageList;

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

        public class SelectionEventArgs : EventArgs
        {
            public enum SelectionType { Selected, Unselected, Activated }

            public SelectionEventArgs(object nodeData, SelectionType selection)
            {
                this.NodeData = nodeData;
                this.Selection = selection;
            }

            public object NodeData { get; private set; }
            public SelectionType Selection { get; private set; }
        }

        [Browsable(true)]
        [Description("Notifies changes in the current file node selection.")]
        public event EventHandler<SelectionEventArgs> SelectionChanged;

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

                        if (toolStripButtonShowElements.Checked)
                        {
                            this.AddElementsToFileNode(file as IScriptFile, fileNode);
                        }
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

        private void AddElementsToFileNode(IScriptFile file, TreeNode fileNode)
        {
            if (file is IScriptFile)
            {
                foreach (var element in file.ListElements().Where(e =>
                    e.ElementType == FileElementType.ProcedureDeclaration ||
                    e.ElementType == FileElementType.TestList ||
                    e.ElementType == FileElementType.FileVariable))
                {
                    var image = 0;
                    switch (element.ElementType)
                    {
                        case FileElementType.ProcedureDeclaration: image = 6; break;
                        case FileElementType.TestList: image = 5; break;
                        case FileElementType.FileVariable: image = 7; break;
                        default: break;
                    }
                    var elementNode = new TreeNode
                    {
                        ImageIndex = image,
                        SelectedImageIndex = image,
                        Text = element.Name
                    };
                    elementNode.Tag = element;
                    fileNode.Nodes.Add(elementNode);
                }
            }
        }

        private void treeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            this.SelectionChanged?.Invoke(this, new SelectionEventArgs(e.Node.Tag, SelectionEventArgs.SelectionType.Activated));
        }

        private void TreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node != null)
            {
                m_currentSelection = e.Node.Tag;
                this.SelectionChanged?.Invoke(this, new SelectionEventArgs(m_currentSelection, SelectionEventArgs.SelectionType.Selected));
            }
        }

        private void treeView_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            if (m_currentSelection != null)
            {
                this.SelectionChanged?.Invoke(this, new SelectionEventArgs(m_currentSelection, SelectionEventArgs.SelectionType.Unselected));
                m_currentSelection = null;
            }
        }

        private void toolStripButtonShowFiles_CheckedChanged(object sender, EventArgs e)
        {
            this.UpdateTree();
            toolStripButtonShowElements.Enabled = toolStripButtonShowFiles.Checked;
            toolStripButtonShowFiles.Enabled = toolStripButtonShowElements.Checked;
        }

        private void toolStripButtonShowElements_CheckedChanged(object sender, EventArgs e)
        {
            this.UpdateTree();
            toolStripButtonShowElements.Enabled = toolStripButtonShowFiles.Checked;
            toolStripButtonShowFiles.Enabled = toolStripButtonShowElements.Checked;
        }

        private void treeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (m_isDoubleClick && e.Action == TreeViewAction.Expand)
                e.Cancel = true;
            m_isDoubleClick = false;
        }

        private void treeView_BeforeCollapse(object sender, TreeViewCancelEventArgs e)
        {
            if (m_isDoubleClick && e.Action == TreeViewAction.Collapse)
                e.Cancel = true;
            m_isDoubleClick = false;
        }

        private void treeView_MouseDown(object sender, MouseEventArgs e)
        {
            m_isDoubleClick = e.Clicks > 1;
        }
    }
}
