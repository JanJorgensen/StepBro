using FastColoredTextBoxNS;
using System;
using System.Windows.Forms;
using StepBro.Core.Controls;
using StepBro.Core.General;

namespace StepBro.Workbench
{
    public partial class TextDocView : TextEditorDockContent
    {
        protected override TextEditor Editor { get { return textEditor; } }

        public TextDocView() : this(new object()) { }

        public TextDocView(object fileOwner) : base(fileOwner)
        {
            this.InitializeComponent();
        }

        private void menuItem2_Click(object sender, System.EventArgs e)
        {
            MessageBox.Show("This is to demonstrate menu item has been successfully merged into the main form. Form Text=" + this.Text);
        }

        private void menuItemCheckTest_Click(object sender, System.EventArgs e)
        {
            menuItemCheckTest.Checked = !menuItemCheckTest.Checked;
        }

        protected override ILoadedFile DoOpenFile(string filepath)
        {
            return new LoadedFileBase(filepath, LoadedFileType.ClearText);
        }

        private void textEditor_IsChangedChanged(object sender, EventArgs e)
        {
            this.OnEditorIsChangedChanged();
        }
    }
}