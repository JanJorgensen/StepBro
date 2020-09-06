using System;
using System.Windows.Forms;
using StepBro.Core.Controls;
using StepBro.Core.General;
using StepBro.Core.ScriptData;
using WeifenLuo.WinFormsUI.Docking;

namespace StepBro.Workbench
{
    public partial class StepBroScriptDocView : TextEditorDockContent
    {
        public StepBroScriptDocView() : this(new object())
        {
        }

        public StepBroScriptDocView(object scriptOwner) : base(scriptOwner)
        {
            this.InitializeComponent();
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.DockAreas = DockAreas.Document | DockAreas.Float;
        }

        protected override TextEditor Editor
        {
            get { return scriptFileEditor; }
        }

        public IScriptFile ScriptFile { get { return this.File as IScriptFile; } }


        protected override ILoadedFile DoOpenFile(string filepath)
        {
            return StepBro.Core.Main.CreateScriptFileObject(filepath);
        }


        private void menuItem2_Click(object sender, System.EventArgs e)
        {
            MessageBox.Show("This is to demostrate menu item has been successfully merged into the main form. Form Text=" + this.Text);
        }

        private void menuItemCheckTest_Click(object sender, System.EventArgs e)
        {
            menuItemCheckTest.Checked = !menuItemCheckTest.Checked;
        }

        private void StepBroScriptDocView_FormClosed(object sender, FormClosedEventArgs e)
        {
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.SaveToFile();
        }

        private void scriptFileEditor_IsChangedChanged(object sender, EventArgs e)
        {
            this.OnEditorIsChangedChanged();
        }
    }
}