﻿using Antlr4.Runtime.Misc;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.General;
using StepBro.Core.ScriptData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using StepBroMain = StepBro.Core.Main;

namespace StepBro.HostSupport.Models;

public partial class ToolsInteractionModel : ObservableObject
{
    public partial class SelectableTool : ObservableObject, IEquatable<SelectableTool>
    {
        private TypeReference m_typeReference;
        private IObjectContainer m_objectVariable;

        public SelectableTool(TypeReference typeReference, IObjectContainer variable)
        {
            m_typeReference = typeReference;
            m_objectVariable = variable;
        }

        public TypeReference ToolType { get { return m_typeReference; } }

        public IObjectContainer ToolContainer { get { return m_objectVariable; } }

        public bool HasEnabledTextCommandInput { get { return (m_objectVariable != null && m_objectVariable.Object != null && m_objectVariable.Object is ITextCommandInput) && (m_objectVariable.Object as ITextCommandInput).Enabled; } }

        public string PresentationName
        {
            get
            {
                if (m_objectVariable != null)
                {
                    return m_objectVariable.FullName.Split('.').Last();
                }
                else return m_typeReference.TypeName();
            }
        }

        public string PresentationFullName { get { return (m_objectVariable != null) ? m_objectVariable.FullName : m_typeReference.TypeName(); } }

        private ObservableCollection<string> m_textCommandHistory = new ObservableCollection<string>();

        public bool IsReadyForTextCommand
        {
            get { return this.HasEnabledTextCommandInput && (m_objectVariable.Object as ITextCommandInput).AcceptingCommands(); }
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as SelectableTool);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return "SelectableTool " + this.PresentationName;
        }

        public bool Equals(SelectableTool other)
        {
            if (other == null) return false;
            if (other.m_typeReference != m_typeReference) return false;
            if ((other.m_typeReference == null) != (m_typeReference == null)) return false;
            if (other.m_typeReference != null && !other.m_typeReference.Equals(m_typeReference)) return false;
            if ((other.m_objectVariable == null) != (m_objectVariable == null)) return false;
            if (other.m_objectVariable != null && !other.m_objectVariable.Equals(m_objectVariable)) return false;
            return true;
        }
    }

    public ToolsInteractionModel(IDynamicObjectManager objectManager, ILoadedFilesManager loadedFilesManager)
    {
        m_objectManager = objectManager;
        m_loadedFilesManager = loadedFilesManager;
        m_currentTextCommandHistoryPublic = new ReadOnlyObservableCollection<string>(m_currentTextCommandHistory);
        m_toolProceduresPublic = new ReadOnlyObservableCollection<IFileProcedure>(m_toolProcedures);

        StepBroMain.ParsingCompleted += StepBroMain_ParsingCompleted;
    }

    private void StepBroMain_ParsingCompleted(object sender, EventArgs e)
    {

    }

    private IDynamicObjectManager m_objectManager;
    private ILoadedFilesManager m_loadedFilesManager;
    private ObservableCollection<SelectableTool> m_selectableTools = new ObservableCollection<SelectableTool>();
    private ObservableCollection<SelectableTool> m_textCommandTools = new ObservableCollection<SelectableTool>();
    private List<IFileElementOverride> m_overrideDefinitions = null;
    private Dictionary<string, List<string>> m_textCommandHistory = new Dictionary<string, List<string>>();
    private ObservableCollection<string> m_currentTextCommandHistory = new ObservableCollection<string>();
    private ReadOnlyObservableCollection<string> m_currentTextCommandHistoryPublic = null;
    private ObservableCollection<IFileProcedure> m_toolProcedures = new ObservableCollection<IFileProcedure>();
    private ReadOnlyObservableCollection<IFileProcedure> m_toolProceduresPublic = null;

    [ObservableProperty]
    private SelectableTool m_selectedTool = null;

    public IList<SelectableTool> SelectableTools { get { return m_selectableTools; } }

    [ObservableProperty]
    private SelectableTool m_currentTextCommandTool = null;     // TODO: Make Set change m_currentTextCommandHistory to the history from the new tool.

    [ObservableProperty]
    private string m_currentTextCommand = "";

    private RelayCommand m_commandRunCurrentTextCommand = null;

    public ICommand RunCurrentTextCommandCommand
    {
        get
        {
            if (m_commandRunCurrentTextCommand == null)
                m_commandRunCurrentTextCommand = new RelayCommand(
                    () => /* Action */
                    {
                        var tool = this.CurrentTextCommandTool.ToolContainer.Object as ITextCommandInput;
                        var command = this.CurrentTextCommand;
                        tool.ExecuteCommand(command);
                        var toolName = this.CurrentTextCommandTool.ToolContainer.FullName;
                        int i = m_currentTextCommandHistory.IndexOf(command);
                        if (i >= 0)
                        {
                            m_currentTextCommandHistory.Move(i, 0);
                        }
                        else
                        {
                            m_currentTextCommandHistory.Insert(0, command);
                        }
                        List<string> history = null;
                        if (m_textCommandHistory.ContainsKey(toolName))
                        {
                            history = m_textCommandHistory[toolName];
                        }
                        else
                        {
                            history = new List<string>();
                            m_textCommandHistory[toolName] = history;
                        }
                        history.Clear();
                        history.AddRange(m_currentTextCommandHistory);
                    },
                    () => /* Predicate */
                    {
                        if (this.CurrentTextCommandTool == null) return false;
                        return this.CurrentTextCommandTool.IsReadyForTextCommand;
                    }
                );

            return m_commandRunCurrentTextCommand;
        }
    }

    public ReadOnlyObservableCollection<string> CurrentTextCommandHistory { get { return m_currentTextCommandHistoryPublic; } }

    public bool Synchronize()
    {
        m_selectableTools.Synchronize(Fetch(m_objectManager, m_loadedFilesManager));
        m_textCommandTools.Synchronize(m_selectableTools.Where(t => t.HasEnabledTextCommandInput));
        m_overrideDefinitions = m_loadedFilesManager.ListFiles<IScriptFile>().SelectMany(f => f.ListElements().Where(e => e is IFileElementOverride oe && oe.HasTypeOverride).Cast<IFileElementOverride>()).ToList();

        if (m_selectableTools.Count > 0)
        {
            if (this.SelectedTool == null)
            {
                this.SelectedTool = m_selectableTools[0];
            }
        }
        else
        {
            this.SelectedTool = null;
        }

        if (m_textCommandTools.Count > 0)
        {
            if (this.CurrentTextCommandTool == null)
            {
                this.CurrentTextCommandTool = m_textCommandTools[0];
            }
        }
        else
        {
            this.CurrentTextCommandTool = null;
        }

        return false;
    }

    private static List<SelectableTool> Fetch(IDynamicObjectManager objectManager, ILoadedFilesManager filesManager)
    {
        var objects = objectManager.GetObjectCollection().Where(oc => oc.Object != null && oc.Object.GetType().IsClass && oc.Object.GetType() != typeof(String)).ToList();
        var variables = filesManager.ListFiles<IScriptFile>().SelectMany(f => f.ListFileVariables());
        return objects.Select(o => new SelectableTool(variables.FirstOrDefault(v => Object.ReferenceEquals(v.Value, o))?.DataType, o)).ToList();
    }

    public List<IFileProcedure> ListActivatableToolProcedures(SelectableTool tool)
    {
        System.Diagnostics.Debug.WriteLine($"Tool: {tool.ToolContainer.FullName}");
        var obj = tool.ToolContainer.Object;
        var objType = (tool.ToolType != null) ? tool.ToolType : (TypeReference)(obj.GetType());
        var procedures = m_loadedFilesManager.ListFiles<IScriptFile>().SelectMany(f => f.ListElements()).Where(e => e.ElementType == FileElementType.ProcedureDeclaration).Cast<IFileProcedure>().ToList();

        procedures.Sort((p1, p2) => String.Compare(p1.Name, p2.Name));

        var matching = new Predicate<TypeReference>(t => 
        {
            if (t.IsAssignableFrom(objType))
            {
                return true;
            }
            else if (t.IsTypedef())
            {
                foreach (var oe in m_overrideDefinitions)
                {
                    if (oe.HasBaseVariable(tool.ToolContainer) && t.IsAssignableFrom(oe.OverrideType))
                    {
                        return true;
                    }
                }
            }
            return false;
        });

        return procedures.Where(p => p.IsFirstParameterThisReference && p.Parameters.Length == 1 && matching(p.Parameters[0].Value)).ToList();
        //    if (proc.Parameters.Length == 1)
        //    {
        //        var par = proc.Parameters[0];

        //        System.Diagnostics.Debug.WriteLine($"{proc.Name} {proc.Parameters.Length} {proc.Parameters[0].Value.Type.Name}");
        //    }
        //}

        //return procedures.Where(p => p.IsFirstParameterThisReference && p.Parameters.Length == 1 && p.Parameters[0].Value.Type.IsAssignableFrom(objType.Type)).ToList();
    }
}
