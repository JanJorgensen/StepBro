using StepBro.Core.General;
using StepBro.Core.ScriptData;
using System;
using System.Windows.Forms;

namespace StepBro.Core.Controls
{
    public partial class ParsingErrorListView : UserControl
    {
        private ILoadedFilesManager m_fileManager;

        public ParsingErrorListView()
        {
            this.InitializeComponent();

            if (StepBro.Core.Main.IsInitialized)
            {
                m_fileManager = StepBro.Core.Main.GetService<ILoadedFilesManager>();
                m_fileManager.FileLoaded += this.FileManager_FileLoaded;
                m_fileManager.FileClosed += this.FileManager_FileClosed;
                foreach (var file in m_fileManager.ListFiles<IScriptFile>())
                {
                    file.Errors.EventListChanged += this.Errors_EventListChanged;
                }
            }
        }

        private void FileManager_FileClosed(object sender, LoadedFileEventArgs args)
        {
            if (args.File is IScriptFile)
            {
                (args.File as IScriptFile).Errors.EventListChanged -= this.Errors_EventListChanged;
            }
        }

        private void FileManager_FileLoaded(object sender, LoadedFileEventArgs args)
        {
            if (args.File is IScriptFile)
            {
                (args.File as IScriptFile).Errors.EventListChanged += this.Errors_EventListChanged;
            }
        }

        private void Errors_EventListChanged(object sender, EventArgs e)
        {
            this.BeginInvoke(new Action(refreshTimer.Stop));
            this.BeginInvoke(new Action(refreshTimer.Start));
        }

        private void refreshTimer_Tick(object sender, EventArgs e)
        {
            refreshTimer.Stop();
            listView.BeginUpdate();
            listView.Items.Clear();
            foreach (var file in m_fileManager.ListFiles<IScriptFile>())
            {
                var errors = file.Errors.GetList();
                foreach (var error in errors)
                {
                    ListViewItem lvi = new ListViewItem(error.JustWarning ? "W" : "E");
                    lvi.Tag = file;
                    lvi.SubItems.Add(error.Message);
                    lvi.SubItems.Add(file.FileName);
                    lvi.SubItems.Add(error.Line.ToString());
                    var col = error.CharPositionInLine;
                    if (col >= 0) lvi.SubItems.Add(col.ToString());
                    else lvi.SubItems.Add("");
                    listView.Items.Add(lvi);
                }
            }
            listView.EndUpdate();
        }

        private void listView_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.SelectedErrorChanged?.Invoke(this, e);
        }

        public event EventHandler SelectedErrorChanged;

        private void listView_DoubleClick(object sender, EventArgs e)
        {
            var selection = listView.SelectedItems?[0];
            if (selection != null)
            {
                ILoadedFile file = selection.Tag as ILoadedFile;
                if (file != null)
                {
                    int line = -1;
                    if (Int32.TryParse(selection.SubItems[3].Text, out line))
                    {
                        line--; // Make zero-based for the editor interface.
                    }
                    int column = 0;
                    Int32.TryParse(selection.SubItems[4].Text, out column);
                    this.DoubleClickedLine?.Invoke(this, new DoubleClickLineEventArgs(file, line, column));
                }
            }
        }

        public class DoubleClickLineEventArgs : EventArgs
        {
            public DoubleClickLineEventArgs(ILoadedFile file, int line, int column) { this.File = file; this.Line = line; this.Column = column; }
            public ILoadedFile File { get; private set; }
            public int Line { get; private set; }
            public int Column { get; private set; }
        }
        public delegate void DoubleClickLineEventHandler(object sender, DoubleClickLineEventArgs args);

        public event DoubleClickLineEventHandler DoubleClickedLine;
    }
}
