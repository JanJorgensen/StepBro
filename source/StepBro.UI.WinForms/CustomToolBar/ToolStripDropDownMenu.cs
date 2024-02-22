using StepBro.Core.Api;
using StepBro.Core.Execution;
using StepBro.ToolBarCreator;
using System.ComponentModel;

namespace StepBro.UI.WinForms.CustomToolBar
{
    internal class ToolStripDropDownMenu : ToolStripDropDownButton, IToolBarElement, IMenu, IMenuItemHost
    {
        private ICoreAccess m_coreAccess = null;

        public ToolStripDropDownMenu(ICoreAccess coreAccess, string name) : base()
        {
            m_coreAccess = coreAccess;
            this.Margin = new Padding(1, Margin.Top, 1, Margin.Bottom);
            this.Name = name;
            this.Text = name;
        }

        public void SetTitle(string title)
        {
            this.Text = title;
        }

        public void Add(ToolStripMenuItem item)
        {
            this.DropDownItems.Add(item);
        }

        #region IToolBarElement

        public uint Id => throw new NotImplementedException();

        public IToolBarElement ParentElement => throw new NotImplementedException();

        public string PropertyName => throw new NotImplementedException();

        public string ElementName => throw new NotImplementedException();

        public string ElementType => throw new NotImplementedException();

        public event PropertyChangedEventHandler PropertyChanged { add { } remove { } }

        public IEnumerable<IToolBarElement> GetChilds()
        {
            throw new NotImplementedException();
        }

        public object GetProperty([Implicit] ICallContext context, string property)
        {
            throw new NotImplementedException();
        }

        public object GetValue([Implicit] ICallContext context)
        {
            throw new NotImplementedException();
        }

        public void SetProperty([Implicit] ICallContext context, string property, object value)
        {
            throw new NotImplementedException();
        }

        public bool SetValue([Implicit] ICallContext context, object value)
        {
            throw new NotImplementedException();
        }

        public IToolBarElement TryFindChildElement([Implicit] ICallContext context, string name)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
