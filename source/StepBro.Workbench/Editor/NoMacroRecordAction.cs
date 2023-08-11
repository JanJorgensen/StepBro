using ActiproSoftware.Windows.Controls.SyntaxEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml;

namespace StepBro.Workbench.Editor
{
    public class NoMacroRecordAction : IEditAction
    {
        private readonly IEditAction m_action;

        public event EventHandler CanExecuteChanged { add { } remove { } }
        public NoMacroRecordAction(IEditAction action)
        {
            m_action = action;
        }
        public CommandBinding CreateCommandBinding() { throw new NotSupportedException(); }
        public CommandBinding CreateCommandBinding(ICommand alternateCommand) { throw new NotSupportedException(); }
        public void ReadFromXml(XmlReader reader) { m_action.ReadFromXml(reader); }
        public void WriteToXml(XmlWriter writer) { m_action.WriteToXml(writer); }
        public bool CanExecute(object parameter) { return m_action.CanExecute(parameter); }
        public void Execute(object parameter) { m_action.Execute(parameter); }
        public bool CanExecute(IEditorView view) { return m_action.CanExecute(view); }
        public void Execute(IEditorView view) { m_action.Execute(view); }
        public string Key { get { return m_action.Key; } }
        public bool CanRecordInMacro { get { return false; } }
    }
}
