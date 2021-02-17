using ActiproSoftware.Windows;
using ActiproSoftware.Windows.Input;
using StepBro.Core.Data;
using StepBro.Core.Execution;
using StepBro.Core.General;
using StepBro.Core.Parser;
using StepBro.Core.ScriptData;
using StepBro.Core.Tasks;
using StepBro.Core.Utils;
using StepBro.Workbench.ToolViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        private readonly DeferrableObservableCollection<DocumentItemViewModel> m_userDocumentItems = new DeferrableObservableCollection<DocumentItemViewModel>();
        private readonly DeferrableObservableCollection<ToolItemViewModel> m_toolItems = new DeferrableObservableCollection<ToolItemViewModel>();
        private readonly DeferrableObservableCollection<ErrorInfo> m_errors = new DeferrableObservableCollection<ErrorInfo>();
        private readonly DeferrableObservableCollection<IFileElement> m_allFileElements = new DeferrableObservableCollection<IFileElement>();

        private DelegateCommand<object> m_commandActivateNextDocument;
        private DelegateCommand<object> m_commandCloseActiveDocument;
        private DelegateCommand<object> m_commandCreateNewStepBroDocument;
        private DelegateCommand<object> m_commandCreateNewTextDocument;
        private DelegateCommand<string> m_commandOpenFile;
        private DelegateCommand<string> m_commandOpenDocument;
        private DelegateCommand<object> m_commandSelectFirstDocument;
        private DelegateCommand<object> m_commandParseAllFiles;
        private DelegateCommand<object> m_commandStartExecution;
        private DelegateCommand<object> m_commandStartExecutionOfSelectedFileElement;
        private DelegateCommand<object> m_commandCreateObjectPanel;
        private ICommand m_commandShowErrorsView;
        private ICommand m_commandShowOutputView;
        private ICommand m_commandShowCalculatorTool;
        private readonly ErrorsViewModel m_errorsViewModel = null;
        private readonly OutputViewModel m_outputViewModel = null;
        private readonly CalculatorViewModel m_calculatorViewModel = null;
        private readonly CommandLineOptions m_commandLineOptions = null;
        private ILoadedFilesManager m_loadedFiles = null;
        private string m_applicationStateMessage = "Ready or not";
        private ITask m_fileParsingTask = null;
        private IScriptExecution m_mainScriptExecution = null;
        private string m_documentToActivateWhenLoaded = null;
        private string m_executionTarget = "tadaa!";
        private Tuple<IFileElement, string> m_executionTargetResolved = null;
        private readonly SynchronizationContext m_syncContext;

        /////////////////////////////////////////////////////////////////////////////////////////////////////
        // OBJECT
        /////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel"/> class.
        /// </summary>
        public MainViewModel()
        {
            m_syncContext = SynchronizationContext.Current;
            var cmd = ApplicationCommands.Save;

            // Initialize before the views start asking for the different services.
            StepBroMain.Initialize();

            m_outputViewModel = new OutputViewModel();
            m_errorsViewModel = new ErrorsViewModel(m_errors);
            m_errors.Add(new ErrorInfo(ErrorType.Environment, "Info", "Remember the breakfast", "todo.txt", 23));
            m_calculatorViewModel = new CalculatorViewModel() { State = ToolItemState.Docked };
            m_toolItems.Add(m_outputViewModel);
            m_toolItems.Add(m_errorsViewModel);
            m_toolItems.Add(m_calculatorViewModel);
            m_userDocumentItems.CollectionChanged += DocumentItems_CollectionChanged;
            m_loadedFiles = StepBroMain.GetLoadedFilesManager();
            m_loadedFiles.FileLoaded += LoadedFiles_FileLoaded;
            m_loadedFiles.FileClosed += LoadedFiles_FileClosed;
            m_loadedFiles.FilePropertyChanged += File_PropertyChanged;

            //this.SeSBPlashScreen();
            m_commandLineOptions = StepBro.Core.General.CommandLineParser.Parse<CommandLineOptions>(null, Environment.GetCommandLineArgs());
        }

        public void LogUserAction(string text)
        {
            StepBroMain.RootLogger.LogSystem("StepBro Workbench", text);
        }

        #region Properties

        public string ApplicationStateMessage
        {
            get { return m_applicationStateMessage; }
            set
            {
                if (!value.Equals(m_applicationStateMessage))
                {
                    m_applicationStateMessage = value;
                    this.NotifyPropertyChanged(nameof(ApplicationStateMessage));
                }
            }
        }

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

        public bool FileParsingRunning
        {
            get
            {
                return (m_fileParsingTask != null);
            }
        }

        public string ExecutionTarget
        {
            get { return m_executionTarget; }
            set
            {
                if (!value.Equals(m_executionTarget))
                {
                    m_executionTarget = value;
                    m_executionTargetResolved = null;
                    this.NotifyPropertyChanged(nameof(ExecutionTarget));
                }
            }
        }

        public Tuple<IFileElement, string> GetExecutionTarget()
        {
            return new Tuple<IFileElement, string>(null, null);
        }

        #endregion

        private DocumentItemViewModel GetActiveDocument()
        {
            return m_userDocumentItems.FirstOrDefault(d => d.IsActive);
        }

        private void UpdateCommandStates()
        {
            m_commandActivateNextDocument?.RaiseCanExecuteChanged();
            m_commandCloseActiveDocument?.RaiseCanExecuteChanged();
            m_commandCreateNewStepBroDocument?.RaiseCanExecuteChanged();
            m_commandCreateNewTextDocument?.RaiseCanExecuteChanged();
            m_commandOpenFile?.RaiseCanExecuteChanged();
            m_commandOpenDocument?.RaiseCanExecuteChanged();
            m_commandSelectFirstDocument?.RaiseCanExecuteChanged();
            m_commandParseAllFiles?.RaiseCanExecuteChanged();
            m_commandStartExecution?.RaiseCanExecuteChanged();
            m_commandStartExecutionOfSelectedFileElement?.RaiseCanExecuteChanged();
            m_commandCreateObjectPanel?.RaiseCanExecuteChanged();
        }

        #region VIEWMODEL EVENTS

        private void DocumentItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (DocumentItemViewModel doc in e.NewItems)
                {
                    if (doc.LoadedFile == null && !String.IsNullOrEmpty(doc.SerializationId))
                    {
                        System.Diagnostics.Debug.WriteLine("TO BE LOADED: " + doc.SerializationId);
                        doc.LoadedFile = this.OpenFile(doc.SerializationId, false);
                    }
                }
            }
            else
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

        private void Invoke(SendOrPostCallback func)
        {
            if (m_syncContext != null)
            {
                m_syncContext.Post(func, null);
            }
            else
            {
                func(null);
            }
        }

        private void LoadedFiles_FileLoaded(object sender, LoadedFileEventArgs args)
        {
            Invoke(o =>
            {
                var alreadyOpenedDoc = m_userDocumentItems.FirstOrDefault(ud => string.Equals(ud.FileName, args.File.FilePath));

                if (alreadyOpenedDoc != null)       // In case document view model was created before the file was loaded.
                {
                    System.Diagnostics.Debug.Assert(alreadyOpenedDoc.LoadedFile == null || Object.Equals(alreadyOpenedDoc.LoadedFile, args.File));
                    alreadyOpenedDoc.LoadedFile = args.File;
                    if (alreadyOpenedDoc.LoadedFile as IScriptFile != null)
                    {
                        (alreadyOpenedDoc.LoadedFile as IScriptFile).Errors.CollectionChanged += ScriptFileErrors_CollectionChanged;
                    }
                }
                else
                {
                    var docViewModel = new TextDocumentItemViewModel();
                    docViewModel.LoadedFile = args.File;
                    docViewModel.IsReadOnly = false;
                    docViewModel.Text = args.File.OffDiskFileContent;
                    docViewModel.PropertyChanged += DocViewModel_PropertyChanged;

                    var scriptFile = args.File as IScriptFile;
                    if (scriptFile != null)
                    {
                        scriptFile.Errors.CollectionChanged += ScriptFileErrors_CollectionChanged;
                    }

                    m_userDocumentItems.Add(docViewModel);
                    if (args.File.IsDependantOf(this))
                    {
                        if (String.Equals(m_documentToActivateWhenLoaded, docViewModel.FileName))
                        {
                            docViewModel.IsActive = true;
                        }
                        else
                        {
                            docViewModel.IsOpen = true;
                        }
                    }
                }
                this.UpdateCommandStates();
            });
        }

        private void ScriptFileErrors_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Invoke(o =>
            {
                switch (e.Action)
                {
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                        m_errors.Add(new ErrorInfo((IScriptFile)sender, (IErrorData)e.NewItems[0]));
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                        for (int i = m_errors.Count - 1; i >= 0; i--)
                        {
                            if (m_errors[i].Type == ErrorType.ParsingError) m_errors.RemoveAt(i);
                        }
                        break;
                    default:
                        break;
                }
                this.UpdateCommandStates();
            });
        }

        private void LoadedFiles_FileClosed(object sender, LoadedFileEventArgs args)
        {
            var foundDocument = m_userDocumentItems.FirstOrDefault(f => Object.ReferenceEquals(args.File, f.LoadedFile));
            if (args.File as IScriptFile != null)
            {
                (args.File as IScriptFile).Errors.CollectionChanged -= ScriptFileErrors_CollectionChanged;
            }

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
            this.UpdateCommandStates();
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
                this.UpdateCommandStates();
            }
        }

        private void File_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (String.Equals(e.PropertyName, nameof(ILoadedFile.RegisteredDependantsCount)))
            {
                Invoke(o =>
                {
                    var file = sender as ILoadedFile;
                    var docItem = m_userDocumentItems.FirstOrDefault(d => d.LoadedFile == file);    // If document has just been closed (removed from the list).
                    if (docItem != null)
                    {
                        if (file.IsDependantOf(this))
                        {
                            if (String.Equals(m_documentToActivateWhenLoaded, docItem.FileName))
                            {
                                docItem.IsActive = true;
                            }
                            else
                            {
                                docItem.IsOpen = true;
                            }
                        }
                        else
                        {
                            docItem.IsOpen = false;
                        }
                        this.UpdateCommandStates();
                    }
                });
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
                this.UpdateCommandStates();
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
                this.UpdateCommandStates();
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
            ILoadedFile loadedFile = null;
            var filesManager = StepBroMain.GetLoadedFilesManager();
            var alreadyLoadedFile = filesManager.ListFiles<ILoadedFile>().FirstOrDefault(f => String.Equals(f.FilePath, filepath));
            if (alreadyLoadedFile != null)
            {
                if (activate)
                {
                    m_documentToActivateWhenLoaded = filepath;
                }
                alreadyLoadedFile.RegisterDependant(this);
                loadedFile = alreadyLoadedFile;
            }
            else
            {
                try
                {
                    if (activate)
                    {
                        m_documentToActivateWhenLoaded = filepath;
                    }
                    var extension = System.IO.Path.GetExtension(filepath);
                    if (extension.Equals(".sbs", StringComparison.InvariantCulture))
                    {
                        loadedFile = StepBroMain.LoadScriptFile(this, filepath);
                    }
                    else
                    {
                        LoadedFileBase file = new LoadedFileBase(filepath, LoadedFileType.ClearText);
                        file.RegisterDependant(this);
                        StepBroMain.GetLoadedFilesManager().RegisterLoadedFile(file);
                        loadedFile = file;
                    }
                }
                finally
                {
                    m_documentToActivateWhenLoaded = null;
                }
            }

            this.UpdateCommandStates();
            return loadedFile;
        }

        public IScriptFile LoadScriptFile(string filepath)
        {
            return StepBroMain.LoadScriptFile(this, filepath);
        }

        public TextDocumentItemViewModel OpenUserDocument(string filepath)
        {
            var alreadyOpenedDoc = m_userDocumentItems.FirstOrDefault(ud => string.Equals(ud.FileName, filepath));
            if (alreadyOpenedDoc != null)
            {
                alreadyOpenedDoc.IsActive = true;
                this.UpdateCommandStates();
                return alreadyOpenedDoc as TextDocumentItemViewModel;
            }
            else
            {
                var docViewModel = new TextDocumentItemViewModel();
                docViewModel.SerializationId = filepath;
                docViewModel.IsReadOnly = false;
                docViewModel.IsOpen = false;
                docViewModel.PropertyChanged += DocViewModel_PropertyChanged;
                m_documentToActivateWhenLoaded = filepath;
                m_userDocumentItems.Add(docViewModel);
                this.UpdateCommandStates();
                return docViewModel;
            }
        }

        #endregion

        /////////////////////////////////////////////////////////////////////////////////////////////////////
        #region COMMANDS
        /////////////////////////////////////////////////////////////////////////////////////////////////////

        #region Application Commands

        public void ShowErrorsTool(bool activate)
        {
            m_errorsViewModel.IsOpen = true;
            if (activate)
            {
                m_errorsViewModel.IsActive = true;
                m_errorsViewModel.IsSelected = true;
            }
            this.UpdateCommandStates();
        }

        public void ShowOutputTool(bool activate)
        {
            m_outputViewModel.IsOpen = true;
            if (activate)
            {
                m_outputViewModel.IsActive = true;
                m_outputViewModel.IsSelected = true;
            }
            this.UpdateCommandStates();
        }

        public void ShowCalculatorTool(bool activate)
        {
            m_calculatorViewModel.IsOpen = true;
            if (activate)
            {
                m_calculatorViewModel.IsActive = true;
                m_calculatorViewModel.IsSelected = true;
            }
            this.UpdateCommandStates();
        }

        public void CreateObjectPanel(bool activate)
        {
            var panel = new ObjectPanelToolViewModel
            {
                IsOpen = true
            };
            m_toolItems.Add(panel);
            if (activate)
            {
                panel.IsActive = true;
                panel.IsSelected = true;
            }
            this.UpdateCommandStates();
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

        public DelegateCommand<string> OpenDocumentCommand
        {
            get
            {
                if (m_commandOpenDocument == null)
                {
                    m_commandOpenDocument = new DelegateCommand<string>(
                        (param) =>
                        {
                            OpenUserDocument(param);
                        });
                }
                return m_commandOpenDocument;
            }
        }

        public ICommand ShowErrorsViewCommand
        {
            get
            {
                if (m_commandShowErrorsView == null)
                    m_commandShowErrorsView = new DelegateCommand<object>(
                        (param) =>
                        {
                            ShowErrorsTool(true);
                        }
                    );

                return m_commandShowErrorsView;
            }
        }

        public ICommand ShowOutputViewCommand
        {
            get
            {
                if (m_commandShowOutputView == null)
                    m_commandShowOutputView = new DelegateCommand<object>(
                        (param) =>
                        {
                            ShowOutputTool(true);
                        }
                    );

                return m_commandShowOutputView;
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
                            LogUserAction("Show Calculator");
                            ShowCalculatorTool(true);
                        }
                    );

                return m_commandShowCalculatorTool;
            }
        }

        public ICommand CreateObjectPanelCommand
        {
            get
            {
                if (m_commandCreateObjectPanel == null)
                    m_commandCreateObjectPanel = new DelegateCommand<object>(
                        (param) =>
                        {
                            CreateObjectPanel(true);
                        }
                    );

                return m_commandCreateObjectPanel;
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
                            this.UpdateCommandStates();
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
                                this.UpdateCommandStates();
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
                                LogUserAction("Close active document; " + activeDocumentItem.FileName);
                            }
                            this.UpdateCommandStates();
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
                            if (m_fileParsingTask != null)
                            {
                                this.ApplicationStateMessage = "Parsing all script files...";
                                m_fileParsingTask.Control.CurrentStateChanged += FileParsing_CurrentStateChanged;
                                m_commandParseAllFiles.RaiseCanExecuteChanged();
                                NotifyPropertyChanged(nameof(FileParsingRunning));
                                this.UpdateCommandStates();
                            }
                            else
                            {
                                this.ApplicationStateMessage = "Failed to start file parsing.";
                            }
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
            if (m_fileParsingTask.Control.CurrentState >= TaskExecutionState.Ended)
            {
                Invoke(o =>
                {
                    m_fileParsingTask.Control.CurrentStateChanged -= FileParsing_CurrentStateChanged;
                    var parsingTime = (m_fileParsingTask.Control.EndTime - m_fileParsingTask.Control.StartTime);
                    var parsingErrorCount = m_errors.Where(err => err.Type == ErrorType.ParsingError).Count();
                    var errors = (parsingErrorCount > 0) ? $"{parsingErrorCount} error(s)!" : "No errors.";
                    this.ApplicationStateMessage = $"Finished file parsing. Time: {parsingTime.ToMinutesTimestamp()} {errors}";
                    m_fileParsingTask = null;
                    m_commandParseAllFiles.RaiseCanExecuteChanged();
                    NotifyPropertyChanged(nameof(FileParsingRunning));
                    this.UpdateCommandStates();
                });
            }
        }

        public ICommand StartOrContinueExecutionCommand
        {
            get
            {
                if (m_commandStartExecution == null)
                    m_commandStartExecution = new DelegateCommand<object>(
                        (param) =>
                        {
                            //m_fileParsingTask = StepBroMain.StartFileParsing(false);
                            //m_fileParsingTask.Control.CurrentStateChanged += FileParsing_CurrentStateChanged;
                            //m_commandParseAllFiles.RaiseCanExecuteChanged();
                            //NotifyPropertyChanged(nameof(FileParsingRunning));
                            this.UpdateCommandStates();
                        },
                        (param) =>
                        {
                            if (m_fileParsingTask == null &&
                                m_userDocumentItems.Count > 0 &&
                                !String.IsNullOrEmpty(this.ExecutionTarget) &&
                                m_mainScriptExecution == null) return true;
                            if (m_mainScriptExecution != null && m_mainScriptExecution.Task.CurrentState == TaskExecutionState.Paused) return true;
                            if (m_mainScriptExecution == null)
                            {
                                return (!String.IsNullOrEmpty(m_executionTarget) && GetExecutionTarget().Item1 != null);
                            }
                            return false;
                        }
                    );

                return m_commandStartExecution;
            }
        }

        public ICommand StartExecutionOfSelectedFileElementCommand
        {
            get
            {
                if (m_commandStartExecutionOfSelectedFileElement == null)
                    m_commandStartExecutionOfSelectedFileElement = new DelegateCommand<object>(
                        (param) =>
                        {
                            var active = GetActiveDocument();
                            if (active != null && active.LoadedFile != null && active.LoadedFile.Type == LoadedFileType.StepBroScript)
                            {
                                var editModel = active as TextDocumentItemViewModel;
                                var file = active.LoadedFile as IScriptFile;
                                IFileElement element = null;
                                var selectionLine = editModel.CurrentSelectionStart.Item1;    // Make line number 1-based
                                foreach (var fe in file.ListElements())
                                {
                                    if (fe.Line <= selectionLine && (element == null || element.Line < fe.Line))
                                    {
                                        element = fe;
                                    }
                                }
                                if (element != null)
                                {
                                    this.ExecutionTarget = element.FullName;
                                    if (element is IFileProcedure)
                                    {
                                        m_executionTargetResolved = new Tuple<IFileElement, string>(element, null);
                                        m_mainScriptExecution = StepBro.Core.Main.StartProcedureExecution(element as IFileProcedure);
                                        m_mainScriptExecution.Task.CurrentStateChanged += MainScriptExecution_CurrentStateChanged;
                                        this.UpdateCommandStates();
                                    }
                                    else
                                    {
                                        // TODO: Show comment in status bar
                                    }
                                }
                                else
                                {
                                    // TODO: Show comment in status bar
                                }
                            }
                        },
                        (param) =>
                        {
                            var active = GetActiveDocument();
                            return (m_fileParsingTask == null &&
                                    m_mainScriptExecution == null &&
                                    m_userDocumentItems.Count > 0 &&
                                    active != null &&
                                    !active.IsModified &&
                                    active.LoadedFile != null &&
                                    active.LoadedFile.Type == LoadedFileType.StepBroScript);
                        }
                    );

                return m_commandStartExecutionOfSelectedFileElement;
            }
        }

        private void MainScriptExecution_CurrentStateChanged(object sender, EventArgs e)
        {
            switch (m_mainScriptExecution.Task.CurrentState)
            {
                case TaskExecutionState.Created:
                    Invoke(OnScriptExecutionRunning);
                    break;
                case TaskExecutionState.Started:
                case TaskExecutionState.AwaitingStartCondition:
                    break;
                case TaskExecutionState.Running:
                    Invoke(OnScriptExecutionRunning);
                    break;
                case TaskExecutionState.RunningNotResponding:
                    break;
                case TaskExecutionState.PauseRequested:
                    break;
                case TaskExecutionState.Paused:
                    Invoke(OnScriptExecutionPaused);
                    break;
                case TaskExecutionState.StopRequested:
                    break;
                case TaskExecutionState.KillRequested:
                    break;
                case TaskExecutionState.Terminating:
                    break;
                case TaskExecutionState.Ended:
                case TaskExecutionState.EndedByException:
                    Invoke(OnScriptExecutionEnded);
                    break;
                default:
                    break;
            }

        }

        void OnScriptExecutionRunning(object? state)
        {
        }
        void OnScriptExecutionPaused(object? state)
        {
        }
        void OnScriptExecutionEnded(object? state)
        {
            m_mainScriptExecution.Task.CurrentStateChanged -= this.MainScriptExecution_CurrentStateChanged;
            m_mainScriptExecution = null;
            this.UpdateCommandStates();
        }

        #endregion

        #endregion
    }

}
