using ActiproSoftware.Windows;
using StepBro.Core.Data;
using StepBro.UI.Panels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace StepBro.Workbench
{
    public class CreateCustomPanelMenuItemViewModel
    {
        public CreateCustomPanelMenuItemViewModel(string header)
        {
            this.Header = header;
        }
        public CreateCustomPanelMenuItemViewModel(CustomPanelType type, ICommand command)
        {
            this.PanelType = type;
            this.Header = type.Name;
            this.Command = command;
        }

        public CreateCustomPanelMenuItemViewModel(CustomPanelType type, string @object, ICommand command)
        {
            this.PanelType = type;
            this.Header = type.Name + ((String.IsNullOrEmpty(@object)) ? "" : (" for " + @object));
            this.Command = command;
        }

        public CreateCustomPanelMenuItemViewModel(CustomPanelType type, IObjectContainer variable, ICommand command, string header)
        {
            this.PanelType = type;
            this.Header = header;
            this.Variable = variable;
            this.Command = command;
        }

        public void AddSubItem(CreateCustomPanelMenuItemViewModel item)
        {
            if (this.SubItems == null)
            {
                this.SubItems = new DeferrableObservableCollection<CreateCustomPanelMenuItemViewModel>();
            }
            this.SubItems.Add(item);
        }

        public CreateCustomPanelMenuItemViewModel Self { get { return this; } }
        public CustomPanelType PanelType { get; private set; } = null;
        public string Header { get; private set; }
        public string ToolTip { get; set; } = null;
        public IObjectContainer Variable { get; private set; } = null;
        public ICommand Command { get; private set; } = null;
        public DeferrableObservableCollection<CreateCustomPanelMenuItemViewModel> SubItems { get; set; } = null;
    }
}
