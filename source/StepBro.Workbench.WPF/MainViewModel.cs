using ActiproSoftware.Windows;
using ActiproSoftware.Windows.Input;
using StepBro.Core.General;
using StepBro.Core.ScriptData;
using StepBro.Core.Tasks;
using StepBro.Workbench.ToolViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using StepBroMain = StepBro.Core.Main;

namespace StepBro.Workbench
{

    /// <summary>
    /// Represents the main view-model.
    /// </summary>
    public class MainViewModel : ObservableObjectBase
    {

        private int m_documentIndex = 1;
        private DeferrableObservableCollection<DocumentItemViewModel> m_userDocumentItems = new DeferrableObservableCollection<DocumentItemViewModel>();
        private DeferrableObservableCollection<ToolItemViewModel> m_toolItems = new DeferrableObservableCollection<ToolItemViewModel>();
        private DeferrableObservableCollection<LoadedFileViewModel> m_loadedFilesList = new DeferrableObservableCollection<LoadedFileViewModel>();

        private DelegateCommand<object> m_commandActivateNextDocument;
        private DelegateCommand<object> m_commandCloseActiveDocument;
        //private DelegateCommand<object> createNewImageDocumentCommand;
        private DelegateCommand<object> m_commandCreateNewStepBroDocument;
        private DelegateCommand<object> m_commandCreateNewTextDocument;
        private DelegateCommand<string> m_commandOpenFile;
        private DelegateCommand<object> m_commandSelectFirstDocument;
        private DelegateCommand<object> m_commandParseAllFiles;
        private ICommand m_commandShowCalculatorTool;
        private readonly CalculatorViewModel m_calculatorViewModel = null;
        private readonly CommandLineOptions m_commandLineOptions = null;
        private ILoadedFilesManager m_loadedFiles = null;
        private ITask m_fileParsingTask = null;
        private string m_documentToActivateWhenLoaded = null;

        /////////////////////////////////////////////////////////////////////////////////////////////////////
        // OBJECT
        /////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel"/> class.
        /// </summary>
        public MainViewModel()
        {
            m_calculatorViewModel = new CalculatorViewModel() { State = ToolItemState.Docked };
            m_toolItems.Add(m_calculatorViewModel);
            m_userDocumentItems.CollectionChanged += DocumentItems_CollectionChanged;
            StepBroMain.Initialize();
            m_loadedFiles = StepBroMain.GetLoadedFilesManager();
            m_loadedFiles.FileLoaded += LoadedFiles_FileLoaded;
            m_loadedFiles.FileClosed += LoadedFiles_FileClosed;
            m_loadedFiles.FilePropertyChanged += File_PropertyChanged;

            //this.SeSBPlashScreen();
            m_commandLineOptions = StepBro.Core.General.CommandLineParser.Parse<CommandLineOptions>(null, Environment.GetCommandLineArgs());
        }

        #region Properties

        /// <summary>
        /// Gets the document items associated with this view-model.
        /// </summary>
        /// <value>The document items associated with this view-model.</value>
        public IList<DocumentItemViewModel> DocumentItems
        {
            get
            {
                return m_userDocumentItems;
            }
        }

        /// <summary>
        /// Gets the tool items associated with this view-model.
        /// </summary>
        /// <value>The tool items associated with this view-model.</value>
        public IList<ToolItemViewModel> ToolItems
        {
            get
            {
                return m_toolItems;
            }
        }

        public IList<LoadedFileViewModel> LoadedFiles
        {
            get
            {
                return m_loadedFilesList;
            }
        }

        public bool FileParsingRunning
        {
            get
            {
                return (m_fileParsingTask != null);
            }
        }

        #endregion

        #region VIEWMODEL EVENTS

        private void DocumentItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (DocumentItemViewModel doc in e.OldItems)
                {
                    StepBroMain.UnregisterFileUsage(this, doc.LoadedFile, false);
                }
            }
        }

        #endregion

        /////////////////////////////////////////////////////////////////////////////////////////////////////
        #region MODEL EVENTS
        /////////////////////////////////////////////////////////////////////////////////////////////////////

        private void LoadedFiles_FileLoaded(object sender, LoadedFileEventArgs args)
        {
            if (args.File.IsDependantOf(this))
            {
                var docViewModel = new TextDocumentItemViewModel();
                docViewModel.LoadedFile = args.File;
                docViewModel.IsReadOnly = false;
                docViewModel.Text = args.File.OffDiskFileContent;
                docViewModel.PropertyChanged += DocViewModel_PropertyChanged;

                m_userDocumentItems.Add(docViewModel);
                if (String.Equals(m_documentToActivateWhenLoaded, docViewModel.FileName, StringComparison.InvariantCulture))
                {
                    docViewModel.IsActive = true;
                }
                else
                {
                    docViewModel.IsOpen = true;
                }
            }
        }

        private void LoadedFiles_FileClosed(object sender, LoadedFileEventArgs args)
        {
            var foundDocument = m_userDocumentItems.FirstOrDefault(f => Object.ReferenceEquals(args.File, false));
            if (foundDocument != null)
            {
                // File was forced closed by the StepBro.Core.
                foundDocument.PropertyChanged -= DocViewModel_PropertyChanged;
                foundDocument.LoadedFile = null;
                foundDocument.IsOpen = false;
            }
            else
            {
                args.File.PropertyChanged -= DocViewModel_PropertyChanged;
            }
        }

        private void DocViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DocumentItemViewModel.IsOpen))
            {
                var document = sender as DocumentItemViewModel;
                if (document.IsOpen == false)
                {
                    document.LoadedFile.UnregisterDependant(this);
                }
            }
        }

        private void File_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (String.Equals(e.PropertyName, nameof(ILoadedFile.RegisteredDependantsCount), StringComparison.InvariantCulture))
            {
                var file = sender as ILoadedFile;
                if (file.IsDependantOf(this))
                {
                    // TODO: Check if not in m_documentItems yet (then add)
                }
                else
                {
                    // TODO: Check if in m_documentItems (then remove)
                }
            }
        }

        #endregion

        /////////////////////////////////////////////////////////////////////////////////////////////////////
        #region PUBLIC PROCEDURES
        /////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates a new text document.
        /// </summary>
        /// <param name="activate">Whether to activate the document.</param>
        /// <param name="nameFormatting">A <seealso cref="string.Format"/> text to use for generating the file name.</param>
        public ILoadedFile CreateNewTextDocument(bool activate, string nameFormatting = "Document{0}.txt", string initialText = "<text document>")
        {
            try
            {
                var name = String.Format(nameFormatting, m_documentIndex++);
                m_documentToActivateWhenLoaded = name;
                ILoadedFile file = new LoadedFileBase(name, LoadedFileType.ClearText);
                if (initialText == null)
                {
                    file.OffDiskFileContent = "procedure void MyProcedure()" + Environment.NewLine +
                        "{" + Environment.NewLine +
                        "}" + Environment.NewLine;
                }
                file.RegisterDependant(this);
                StepBroMain.GetLoadedFilesManager().RegisterLoadedFile(file);
                return file;
            }
            finally
            {
                m_documentToActivateWhenLoaded = null;
            }
        }

        /// <summary>
        /// Creates a new StepBro script.
        /// </summary>
        /// <param name="activate">Whether to activate the document.</param>
        public IScriptFile CreateNewStepBroDocument(bool activate, string nameFormatting = "Script{0}.sbs", string initialText = null)
        {
            try
            {
                var name = String.Format(nameFormatting, m_documentIndex++);
                if (activate)
                {
                    m_documentToActivateWhenLoaded = name;
                }
                IScriptFile file = StepBroMain.CreateScriptFileObject(name);
                if (initialText == null)
                {
                    file.OffDiskFileContent = "procedure void MyProcedure()" + Environment.NewLine +
                        "{" + Environment.NewLine +
                        "}" + Environment.NewLine;
                }
                else
                {
                    file.OffDiskFileContent = initialText;
                }
                file.RegisterDependant(this);
                StepBroMain.GetLoadedFilesManager().RegisterLoadedFile(file);
                return file;
            }
            finally
            {
                m_documentToActivateWhenLoaded = null;
            }
        }

        public ILoadedFile OpenFile(string filepath, bool activate = true)
        {
            if (String.IsNullOrWhiteSpace(filepath)) return null;
            var alreadyLoadedFile = StepBroMain.GetLoadedFilesManager().ListFiles<ILoadedFile>().FirstOrDefault(f => String.Equals(f.FilePath, filepath));
            if (alreadyLoadedFile != null)
            {
                if (activate)
                {
                    m_documentToActivateWhenLoaded = filepath;
                }
                alreadyLoadedFile.RegisterDependant(this);
                return alreadyLoadedFile;
            }
            try
            {
                if (activate)
                {
                    m_documentToActivateWhenLoaded = filepath;
                }
                var extension = System.IO.Path.GetExtension(filepath);
                if (extension.Equals(".sbs", StringComparison.InvariantCulture))
                {
                    return StepBroMain.LoadScriptFile(this, filepath);
                }
                else
                {
                    LoadedFileBase file = new LoadedFileBase(filepath, LoadedFileType.ClearText);
                    file.RegisterDependant(this);
                    StepBroMain.GetLoadedFilesManager().RegisterLoadedFile(file);
                    return file;
                }
            }
            finally
            {
                m_documentToActivateWhenLoaded = null;
            }
        }

        public IScriptFile LoadScriptFile(string filepath)
        {
            return StepBroMain.LoadScriptFile(this, filepath);
        }

        #endregion

        /////////////////////////////////////////////////////////////////////////////////////////////////////
        #region COMMANDS
        /////////////////////////////////////////////////////////////////////////////////////////////////////

        #region Application Commands


        public void ShowCalculatorTool(bool activate)
        {
            m_calculatorViewModel.IsOpen = true;
            if (activate)
            {
                m_calculatorViewModel.IsActive = true;
                m_calculatorViewModel.IsSelected = true;
            }
        }

        ///// <summary>
        ///// Gets the create new image document command.
        ///// </summary>
        ///// <value>The create new image document command.</value>
        //public ICommand CreateNewImageDocumentCommand {
        //	get {
        //		if (createNewImageDocumentCommand == null)
        //			createNewImageDocumentCommand = new DelegateCommand<object>(
        //				(param) => {
        //					this.CreateNewImageDocument(true);
        //				}
        //			);

        //		return createNewImageDocumentCommand;
        //	}
        //}

        /// <summary>
        /// Gets the create new text document command.
        /// </summary>
        /// <value>The create new text document command.</value>
        public ICommand CreateNewTextDocumentCommand
        {
            get
            {
                if (m_commandCreateNewTextDocument == null)
                    m_commandCreateNewTextDocument = new DelegateCommand<object>(
                        (param) =>
                        {
                            CreateNewTextDocument(true);
                        }
                    );

                return m_commandCreateNewTextDocument;
            }
        }

        /// <summary>
        /// Gets the create new text document command.
        /// </summary>
        /// <value>The create new text document command.</value>
        public ICommand CreateNewStepBroDocumentCommand
        {
            get
            {
                if (m_commandCreateNewStepBroDocument == null)
                    m_commandCreateNewStepBroDocument = new DelegateCommand<object>(
                        (param) =>
                        {
                            CreateNewStepBroDocument(true);
                        }
                    );

                return m_commandCreateNewStepBroDocument;
            }
        }

        public DelegateCommand<string> OpenFileCommand
        {
            get
            {
                if (m_commandOpenFile == null)
                {
                    m_commandOpenFile = new DelegateCommand<string>(
                        (param) =>
                        {
                            OpenFile(param);
                        });
                }
                return m_commandOpenFile;
            }
        }

        public ICommand ShowCalculatorToolCommand
        {
            get
            {
                if (m_commandShowCalculatorTool == null)
                    m_commandShowCalculatorTool = new DelegateCommand<object>(
                        (param) =>
                        {
                            ShowCalculatorTool(true);
                        }
                    );

                return m_commandShowCalculatorTool;
            }
        }

        /// <summary>
        /// Gets the select first document command.
        /// </summary>
        /// <value>The select first document command.</value>
        public ICommand SelectFirstDocumentCommand
        {
            get
            {
                if (m_commandSelectFirstDocument == null)
                    m_commandSelectFirstDocument = new DelegateCommand<object>(
                        (param) =>
                        {
                            var documentItem = m_userDocumentItems.FirstOrDefault();
                            if (documentItem != null)
                                documentItem.IsSelected = true;
                        }
                    );

                return m_commandSelectFirstDocument;
            }
        }

        #endregion

        #region Document Commands

        /// <summary>
        /// Gets the activate next document command.
        /// </summary>
        /// <value>The activate next document command.</value>
        public ICommand ActivateNextDocumentCommand
        {
            get
            {
                if (m_commandActivateNextDocument == null)
                    m_commandActivateNextDocument = new DelegateCommand<object>(
                        (param) =>
                        {
                            if (m_userDocumentItems.Count > 0)
                            {
                                var index = 0;
                                var activeDocumentItem = m_userDocumentItems.FirstOrDefault(d => d.IsActive);
                                if (activeDocumentItem != null)
                                    index = m_userDocumentItems.IndexOf(activeDocumentItem) + 1;
                                if (index >= m_userDocumentItems.Count)
                                    index = 0;

                                m_userDocumentItems[index].IsActive = true;
                            }
                        }
                    );

                return m_commandActivateNextDocument;
            }
        }

        /// <summary>
        /// Gets the close active document command.
        /// </summary>
        /// <value>The close active document command.</value>
        public ICommand CloseActiveDocumentCommand
        {
            get
            {
                if (m_commandCloseActiveDocument == null)
                    m_commandCloseActiveDocument = new DelegateCommand<object>(
                        (param) =>
                        {
                            var activeDocumentItem = m_userDocumentItems.FirstOrDefault(d => d.IsActive);
                            if (activeDocumentItem != null)
                            {
                                activeDocumentItem.LoadedFile.UnregisterDependant(this);
                            }
                        }
                    );

                return m_commandCloseActiveDocument;
            }
        }

        #endregion

        #region Script Commands

        public ICommand ParseAllFilesCommand
        {
            get
            {
                if (m_commandParseAllFiles == null)
                    m_commandParseAllFiles = new DelegateCommand<object>(
                        (param) =>
                        {
                            m_fileParsingTask = StepBroMain.StartFileParsing(false);
                            m_fileParsingTask.Control.CurrentStateChanged += FileParsing_CurrentStateChanged;
                            m_commandParseAllFiles.RaiseCanExecuteChanged();
                            NotifyPropertyChanged(nameof(FileParsingRunning));
                        },
                        (param) =>
                        {
                            return (m_fileParsingTask == null);
                        }
                    );

                return m_commandParseAllFiles;
            }
        }

        private void FileParsing_CurrentStateChanged(object sender, EventArgs e)
        {
            if (m_fileParsingTask.Control.CurrentState == TaskExecutionState.Ended)
            {
                m_fileParsingTask.Control.CurrentStateChanged -= FileParsing_CurrentStateChanged;
                m_fileParsingTask = null;
                m_commandParseAllFiles.RaiseCanExecuteChanged();
                NotifyPropertyChanged(nameof(FileParsingRunning));
            }
        }

        #endregion

        #endregion
    }

}
