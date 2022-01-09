using Microsoft.Win32;
using StepBro.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StepBro.UI.Controls
{
    /// <summary>
    /// Interaction logic for LogViewer.xaml
    /// </summary>
    public partial class LogViewer : UserControl
    {
        public LogViewer()
        {
            InitializeComponent();
            //this.DataContextChanged += LogViewer_DataContextChanged;
        }

        private LogViewerViewModel ViewModel { get { return this.DataContext as LogViewerViewModel; } }

        private void clearLog_Click(object sender, RoutedEventArgs e)
        {
            this.ViewModel.ClearLog();
        }

        private void saveAsText_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog();
            dialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            dialog.FileName = "StepBro_Log_" + DateTime.Now.ToFileName() + ".txt";
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    this.ViewModel.SaveAsClearText(dialog.FileName);
                }
                catch
                {
                    MessageBox.Show(Application.Current.MainWindow,
                        "Error creating log file.",
                        "StepBro Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private void saveAsHtml_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog();
            dialog.Filter = "Text files (*.html)|*.html|All files (*.*)|*.*";
            dialog.FileName = "StepBro_Log_" + DateTime.Now.ToFileName() + ".html";
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    this.ViewModel.SaveAsHtml(dialog.FileName);
                }
                catch
                {
                    MessageBox.Show(Application.Current.MainWindow,
                        "Error creating log file.",
                        "StepBro Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        //private void LogViewer_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        //{
        //    if (this.DataContext != null)
        //    {
        //        (this.DataContext as LogViewerViewModel).EntriesAdded += LogViewer_EntriesAdded;
        //    }
        //}

        //private void LogViewer_EntriesAdded(object sender, EventArgs e)
        //{
        //    if (VisualTreeHelper.GetChildrenCount(LogEntryList) > 0)
        //    {
        //        Decorator border = VisualTreeHelper.GetChild(LogEntryList, 0) as Decorator;
        //        ScrollViewer scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
        //        scrollViewer.ScrollToBottom();
        //    }
        //    //LogEntryList.SelectedIndex = LogEntryList.Items.Count - 1;
        //    //LogEntryList.ScrollIntoView(LogEntryList.SelectedItem);
        //}
    }
}
