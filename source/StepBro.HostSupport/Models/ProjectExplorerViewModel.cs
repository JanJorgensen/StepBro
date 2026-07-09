using CommunityToolkit.Mvvm.ComponentModel;
using StepBro.Core.General;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace StepBro.HostSupport.Models
{
    public enum TreeNodeType { Category, FileFolder, File, ScriptFileProcedure, ScriptFileVariable, ScriptFileTestList, ScriptFileElement }

    public partial class TreeNode : ObservableObject
    {
        public TreeNode(TreeNodeType nodeType, string name, object data)
        {
            this.NodeType = nodeType;
            m_name = name;
            m_dataContext = data;
        }

        public TreeNodeType NodeType { get; }

        [ObservableProperty]
        private string m_name;

        [ObservableProperty]
        private object m_dataContext;

        public ObservableCollection<TreeNode> ChildNodes { get; } = new();
    }

    public class TreeFileNode : TreeNode
    {
        public TreeFileNode(string name, ILoadedFile file) : base(TreeNodeType.File, name, file)
        {
        }

        public ILoadedFile File { get { return (ILoadedFile)this.DataContext; } }
    }

    public class TreeFolderNode : TreeNode
    {
        public TreeFolderNode(string name, string folderPath) : base(TreeNodeType.FileFolder, name, folderPath) { }

        public string FullPath { get { return (string)this.DataContext; } }
    }

    public class StepBroScripFileElementNode : TreeNode
    {
        public StepBroScripFileElementNode(StepBro.Core.ScriptData.IFileElement element) : base(NodeTypeFromElement(element), element.Name, element)
        {
        }

        static TreeNodeType NodeTypeFromElement(StepBro.Core.ScriptData.IFileElement element)
        {
            if (element.ElementType == Core.ScriptData.FileElementType.ProcedureDeclaration) return TreeNodeType.ScriptFileProcedure;
            if (element.ElementType == Core.ScriptData.FileElementType.FileVariable) return TreeNodeType.ScriptFileVariable;
            if (element.ElementType == Core.ScriptData.FileElementType.TestList) return TreeNodeType.ScriptFileVariable;
            return TreeNodeType.ScriptFileElement;
        }

        public StepBro.Core.ScriptData.IFileElement FileElement { get { return this.DataContext as StepBro.Core.ScriptData.IFileElement; } }
    }

    public class ProjectExplorerViewModel : ItemViewModel
    {




        public ProjectExplorerViewModel() : base(ViewType.StepBroView, "ProjectExplorer")
        {
        }

        public ObservableCollection<TreeNode> RootNodes { get; } = new();

    }
}
