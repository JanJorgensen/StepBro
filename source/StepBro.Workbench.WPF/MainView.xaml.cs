using ActiproSoftware.Windows.Controls.Docking.Serialization;
using ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt;
using Microsoft.Win32;
using StepBro.Workbench.ToolViews;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StepBro.Workbench
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : UserControl
    {
        private static string m_layoutfile = string.Empty;
        private DockSiteLayoutSerializer layoutSerializer;

        public MainView()
        {
            layoutSerializer = new DockSiteLayoutSerializer()
            {
                DocumentWindowDeserializationBehavior = DockingWindowDeserializationBehavior.LazyLoad,
                SerializationBehavior = DockSiteSerializationBehavior.All,
                ToolWindowDeserializationBehavior = DockingWindowDeserializationBehavior.LazyLoad
            };
            layoutSerializer.DockingWindowDeserializing += this.OnLayoutSerializerDockingWindowDeserializing;

            InitializeComponent();

            editor.Document.Language = new StepBro.UI.Controls.ExecutionTargetSelectorLanguage();
            editor.Document.Language.RegisterService<ICompletionProvider>(new StepBro.UI.Controls.ExecutionTargetCompletionProvider());
        }

        private string GetLayoutFilePath(string documentFile)
        {
            var path = documentFile;
            var folder = System.IO.Path.GetDirectoryName(path);
            return System.IO.Path.Combine(folder, System.IO.Path.ChangeExtension(documentFile, "layout"));
        }

        private void OnLayoutSerializerDockingWindowDeserializing(object sender, DockingWindowDeserializingEventArgs e)
        {
            var viewModel = (MainViewModel)DataContext;
            if (e.Node.SerializationId.StartsWith("Tool"))
            {
                if (e.Node.SerializationId.StartsWith("ToolCustomPanel"))
                {
                    var panelViewModel = new CustomPanelToolViewModel(StepBro.Core.Main.ServiceManager);
                    panelViewModel.SerializationId = e.Node.SerializationId;
                    viewModel.AddCustomPanel(panelViewModel, false);
                    e.Window = dockSite.ContainerFromToolItem(panelViewModel);
                }
            }
            else
            {
                if (e.Node.IsOpen)
                {
                    var docViewModel = viewModel.OpenUserDocument(e.Node.SerializationId);
                    e.Window = dockSite.ContainerFromDocumentItem(docViewModel);
                }
            }
            System.Diagnostics.Debug.WriteLine($"DockingWindowDeserializing: {e.Node.SerializationId}");
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property.Name == nameof(DataContext))
            {
                if (e.NewValue != null)
                {
                    var viewModel = (MainViewModel)DataContext;
                    (viewModel.DocumentItems as INotifyCollectionChanged).CollectionChanged += MainView_DocumentItemsChanged;
                }
                System.Diagnostics.Debug.WriteLine("DataContext changed");
            }
        }

        private void MainView_DocumentItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add && (sender as IList).Count == 1 && String.IsNullOrEmpty(m_layoutfile))
            {
                // Save the path for the first file loaded.
                m_layoutfile = GetLayoutFilePath((e.NewItems[0] as DocumentItemViewModel).FileName);
            }
        }

        //private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        //{
        //    if (e.PropertyName == nameof(MainViewModel.UILayoutFile))
        //    {
        //        var currentLayoutFile = m_layoutfile;
        //        m_layoutfile = ((MainViewModel)DataContext).UILayoutFile;
        //        if (string.IsNullOrEmpty(currentLayoutFile))
        //        {
        //        }
        //    }
        //}

        #region Open File Command

        private void OpenCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            System.Diagnostics.Trace.WriteLine("OpenCanExecute");
        }

        private void OpenExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "StepBro files (*.sbs)|*.sbs|Text files (*.txt)|*.txt|All files (*.*)|*.*";
            if (dialog.ShowDialog() == true)
            {
                var viewModel = (MainViewModel)DataContext;
                if (viewModel.OpenDocumentCommand.CanExecute(null))
                {
                    // Check if there's a layout file to be loaded instead of the file directly.
                    if (viewModel.DocumentItems.Count == 0)
                    {
                        var path = dialog.FileName;
                        var folder = System.IO.Path.GetDirectoryName(path);
                        var layoutfile = System.IO.Path.Combine(folder, System.IO.Path.ChangeExtension(path, "layout"));
                        if (System.IO.File.Exists(layoutfile))
                        {
                            m_layoutfile = layoutfile;  // Save for later, when the layout should be saved again.
                            layoutSerializer.LoadFromFile(m_layoutfile, dockSite);
                            return;
                        }
                    }

                    // If no layout file was loaded, load the selected file.
                    viewModel.OpenFileCommand.Execute(dialog.FileName);
                }
            }
        }

        #endregion

        #region Close Command

        //private void CloseCanExecute(object sender, CanExecuteRoutedEventArgs e)
        //{
        //    e.CanExecute = true;
        //}

        //private void CloseExecuted(object sender, ExecutedRoutedEventArgs e)
        //{
        //    System.Diagnostics.Trace.WriteLine("CloseExecuted");
        //    if (this.ApplicationCloseActivated != null)
        //    {
        //        this.ApplicationCloseActivated(this, EventArgs.Empty);
        //    }
        //    else
        //    {
        //        System.Windows.Application.Current.Shutdown();
        //    }
        //}

        public bool CanClose
        {
            get
            {
                return true;
            }
        }

        public bool TryClose()
        {
            if (!string.IsNullOrEmpty(m_layoutfile))
            {
                layoutSerializer.SaveToFile(m_layoutfile, dockSite);
            }
            return true;
        }

        //public event EventHandler ApplicationCloseActivated;
 
        #endregion

        //private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        //{
        //    if (!string.IsNullOrEmpty(m_layoutfile))
        //    {
        //        this.SaveLayout();
        //    }
        //}

        private void dockumentList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dockumentList.SelectedIndex >= 0)
            {
                var viewModel = (MainViewModel)DataContext;
                var doc = viewModel.DocumentItems[dockumentList.SelectedIndex];
                doc.IsActive = true;
            }
        }
    }
}
