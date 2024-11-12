using StepBro.Core.Host.Presentation;
using System.Windows.Forms;

namespace StepBro.UI.WinForms.Dialogs
{
    public partial class DialogOpenProjectFile : Form
    {
        public DialogOpenProjectFile()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.UpdateRecent();
            this.UpdateFavorites();

            labelFavorites.Text += " \u272D";
        }

        private void UpdateRecent()
        {
            listBoxRecent.Items.Clear();
            listBoxRecent.ClearSelected();
            foreach (var f in UserDataStationManager.ListRecentFiles())
            {
                listBoxRecent.Items.Add(new StepBro.UI.WinForms.ComboboxItem(System.IO.Path.GetFileName(f), f));
            }
        }

        private void UpdateFavorites()
        {
            listBoxFavorites.Items.Clear();
            listBoxFavorites.ClearSelected();
            foreach (var f in UserDataStationManager.ListFavoriteFiles())
            {
                listBoxFavorites.Items.Add(new StepBro.UI.WinForms.ComboboxItem(System.IO.Path.GetFileName(f), f));
            }
            listBoxFavorites.Enabled = listBoxFavorites.Items.Count > 0;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.SelectedFile = null;
            this.DialogResult = DialogResult.Cancel;
        }

        private void listBoxRecent_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxRecent.SelectedIndex >= 0)
            {
                var selection = (listBoxRecent.Items[listBoxRecent.SelectedIndex] as StepBro.UI.WinForms.ComboboxItem)?.Value.ToString();
                this.SetSelected(selection);
                listBoxFavorites.ClearSelected();
                buttonAddToFavorites.Visible = !UserDataStationManager.ListFavoriteFiles().Any(f => String.Equals(selection, f));
            }
            else
            {
                buttonAddToFavorites.Visible = false;
            }
        }

        private void listBoxFavorites_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxFavorites.SelectedIndex >= 0)
            {
                var selection = (listBoxRecent.Items[listBoxFavorites.SelectedIndex] as StepBro.UI.WinForms.ComboboxItem)?.Value.ToString();
                this.SetSelected(selection);
                listBoxRecent.ClearSelected();
            }
        }

        private void listBoxRecent_DoubleClick(object sender, EventArgs e)
        {
            if (listBoxRecent.SelectedIndex >= 0)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void listBoxFavorites_DoubleClick(object sender, EventArgs e)
        {
            if (listBoxFavorites.SelectedIndex >= 0)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void buttonAddToFavorites_Click(object sender, EventArgs e)
        {
            UserDataStationManager.AddFavoriteFile(this.SelectedFile);
            buttonAddToFavorites.Visible = false;
            this.UpdateFavorites();
        }

        private void buttonSelectFileOnDisk_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = checkBoxSameFolder.Checked ? System.IO.Path.GetDirectoryName(this.SelectedFile) : System.Environment.CurrentDirectory;
                openFileDialog.Filter = "StepBro Script files (*.sbs)|*.sbs|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.CheckFileExists = true;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    this.SelectedFile = openFileDialog.FileName;
                    this.DialogResult |= DialogResult.OK;
                    this.Close();
                }
            }
        }

        private void SetSelected(string file)
        {
            this.SelectedFile = file;
            if (file != null)
            {
                labelFilePath.Text = this.SelectedFile;
                checkBoxSameFolder.Visible = true;
                buttonOK.Enabled = true;
            }
            else
            {
                labelFilePath.Text = "";
                checkBoxSameFolder.Visible = false;
                buttonOK.Enabled = false;
            }
        }

        private void listBoxRecent_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                if (listBoxRecent.SelectedIndex >= 0)
                {
                    var selection = (listBoxRecent.Items[listBoxRecent.SelectedIndex] as StepBro.UI.WinForms.ComboboxItem)?.Value.ToString();
                    UserDataStationManager.RemoveRecentFile(selection);
                    listBoxRecent.ClearSelected();
                    this.UpdateRecent();
                    this.SetSelected(null);
                }
            }
        }

        private void listBoxFavorites_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                if (listBoxFavorites.SelectedIndex >= 0)
                {
                    var selection = (listBoxFavorites.Items[listBoxFavorites.SelectedIndex] as StepBro.UI.WinForms.ComboboxItem)?.Value.ToString();
                    UserDataStationManager.RemoveFavoriteFile(selection);
                    listBoxFavorites.ClearSelected();
                    this.UpdateFavorites();
                    this.SetSelected(null);
                }
            }
        }

        public string SelectedFile
        {
            get; private set;
        }
    }
}
