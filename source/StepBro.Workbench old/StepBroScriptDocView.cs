using StepBro.Core.Controls;
using StepBro.Core.General;
using StepBro.Core.ScriptData;
using System;
using System.Linq;
using System.Windows.Forms;
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
            var loadedFile = StepBro.Core.Main.GetLoadedFilesManager().ListFiles<IScriptFile>().FirstOrDefault(lf => String.Equals(lf.FilePath, filepath, StringComparison.InvariantCulture));
            if (loadedFile == null)
            {
                loadedFile = StepBro.Core.Main.CreateScriptFileObject(filepath);
                if (loadedFile != null)
                {
                    StepBro.Core.Main.GetLoadedFilesManager().RegisterLoadedFile(loadedFile);
                }
            }
            return loadedFile;
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
            this.SaveFile(Core.File.SaveOption.SaveToExisting);
        }

        private void scriptFileEditor_IsChangedChanged(object sender, EventArgs e)
        {
            this.OnEditorIsChangedChanged();
        }
    }
}