﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StepBro.Core;
using StepBro.Core.Data;
using StepBro.Core.Execution;
using StepBro.Core.General;
using StepBro.Core.Logging;
using StepBro.Core.ScriptData;
using StepBro.Core.Tasks;
using static StepBro.Core.Data.PropertyBlockDecoder;
using StepBroMain = StepBro.Core.Main;

namespace StepBro.HostSupport.Models;

/// <summary>
/// Represents the main view-model.
/// </summary>
public partial class HostAppModel : ObservableObject
{
    public static readonly string WG_PRIMARY = "PRIMARY";
    public static readonly string WG_SECONDARY = "SECONDARY";
    public static readonly string WG_DOCUMENT = "DOCUMENT";
    public static readonly string WG_BOTTOM = "BOTTOM";

    private int m_documentIndex = 1;
    //private readonly ObservableCollection<DocumentItemViewModel> m_userDocumentItems = new ObservableCollection<DocumentItemViewModel>();
    //private readonly DeferrableObservableCollection<ToolItemViewModel> m_toolItems = new DeferrableObservableCollection<ToolItemViewModel>();
    private readonly ObservableCollection<ItemViewModel> m_itemViews = new ObservableCollection<ItemViewModel>();
    private readonly ObservableCollection<ProblemInfo> m_errors = new ObservableCollection<ProblemInfo>();
    //private readonly DeferrableObservableCollection<CreateCustomPanelMenuItemViewModel> m_creatableCustomPanels = new DeferrableObservableCollection<CreateCustomPanelMenuItemViewModel>();
    //private readonly DeferrableObservableCollection<IFileElement> m_allFileElements = new DeferrableObservableCollection<IFileElement>();

    private RelayCommand m_commandActivateNextDocument;
    private RelayCommand m_commandCloseActiveDocument;
    private RelayCommand m_commandCreateNewStepBroDocument;
    private RelayCommand m_commandCreateNewTextDocument;
    private RelayCommand<string> m_commandOpenFile;
    private RelayCommand<string> m_commandOpenDocument;
    private RelayCommand m_commandSelectFirstDocument;
    private RelayCommand m_commandParseAllFiles;
    private RelayCommand<object> m_commandStartExecution;
    private RelayCommand<object> m_commandStartExecutionOfSelectedFileElement;
    private ICommand m_commandShowErrorsView;
    private RelayCommand m_commandShowExecutionLogView;
    private ICommand m_commandShowCalculatorTool;

    private readonly ItemViewModel m_executionLogView = null;
    private readonly ItemViewModel m_problemsView = null;

    //private readonly PropertiesViewModel m_propertiesViewModel = null;
    //private readonly ErrorsViewModel m_errorsViewModel = null;
    //private readonly OutputViewModel m_outputViewModel = null;
    //private readonly CalculatorViewModel m_calculatorViewModel = null;
    //private DocumentItemViewModel m_selectedDocument = null;

    private string m_caretLine = "";
    private string m_caretCharacter = "";
    private string m_caretDisplayCharColumn = "";

    //private readonly CommandLineOptions m_commandLineOptions = null;
    private ILoadedFilesManager m_loadedFiles = null;
    private string m_applicationStateMessage = "Ready or not";
    private IScriptExecution m_mainScriptExecution = null;
    private string m_documentToActivateWhenLoaded = null;
    private string m_executionTarget = "tadaa!";
    private Tuple<IFileElement, string> m_executionTargetResolved = null;
    private static SynchronizationContext g_syncContext = null;
    //private readonly CustomPanelManager m_panelManager = null;
    private ToolsInteractionModel m_toolsInteractionModel = null;

    /////////////////////////////////////////////////////////////////////////////////////////////////////
    // OBJECT
    /////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Constructor.
    /// </summary>
    public HostAppModel()
    {
        g_syncContext = SynchronizationContext.Current;         // The CTOR MUST then be called from the UI thread !!!!

        m_executionLogView = new ItemViewModel("ExecutionLogView") { Title = "EXECUTION LOG", SupportedWindowGroups = [WG_DOCUMENT, WG_SECONDARY, WG_BOTTOM] };
        m_problemsView = new ItemViewModel("ProblemsView") { Title = "PROBLEMS", SupportedWindowGroups = [WG_BOTTOM] };
    }

    public void Initialize(object logViewerModel, params IService[] hostServices)
    {
        //var cmd = ApplicationCommands.Save;

        //IService panelManagerService;
        //m_panelManager = new CustomPanelManager(out panelManagerService);

        StepBroMain.Initialize(hostServices);
        m_executionLogView.DataContext = logViewerModel;

        m_itemViews.Add(m_executionLogView);
        m_itemViews.Add(m_problemsView);

        //m_propertiesViewModel = new PropertiesViewModel();
        //m_propertiesViewModel.IsOpen = true;
        //m_outputViewModel = new OutputViewModel();
        //m_errorsViewModel = new ErrorsViewModel(m_errors);
        //m_errors.Add(new ErrorInfo(ProblemType.Environment, "Info", "Remember the breakfast", "todo.txt", 23));
        //m_calculatorViewModel = new CalculatorViewModel() { State = ToolItemState.Docked };
        //m_toolItems.Add(m_propertiesViewModel);
        //m_toolItems.Add(m_outputViewModel);
        //m_toolItems.Add(m_errorsViewModel);
        //m_toolItems.Add(m_calculatorViewModel);
        //m_userDocumentItems.CollectionChanged += DocumentItems_CollectionChanged;
        //m_toolItems.CollectionChanged += ToolItems_CollectionChanged;
        m_loadedFiles = StepBroMain.GetLoadedFilesManager();
        m_loadedFiles.FileLoaded += LoadedFiles_FileLoaded;
        m_loadedFiles.FileClosed += LoadedFiles_FileClosed;
        m_loadedFiles.FilePropertyChanged += File_PropertyChanged;
        //StepBro.Core.Main.GetService<UICalculator>().ResultUpdated += MainViewModel_ResultUpdated;

        //m_commandLineOptions = StepBro.Core.General.CommandLineParser.Parse<CommandLineOptions>(null, Environment.GetCommandLineArgs(), System.Console.Out);

        m_commandShowExecutionLogView = new RelayCommand(() => { this.ShowView(m_executionLogView, true); });

        //this.UpdateCustomPanelsMenu();
    }

    public void LogUserAction(string text)
    {
        StepBroMain.RootLogger.LogSystem("StepBro Workbench: " + text);
    }

    public ILogger RootLogger { get => StepBroMain.RootLogger; }

    public static void Invoke(SendOrPostCallback func, object state = null)
    {
        if (g_syncContext != null)
        {
            g_syncContext.Post(func, state);
        }
        else
        {
            func(state);
        }
    }

    #region Properties

    public string ApplicationStateMessage
    {
        get { return m_applicationStateMessage; }
        set => SetProperty(ref m_applicationStateMessage, value, nameof(ApplicationStateMessage));
    }

    ///<summary>
    /// Gets the active view models.
    /// </summary>
    /// <value>The view models associated with this model.</value>
    public IList<ItemViewModel> Views
    {
        get
        {
            return m_itemViews;
        }
    }

    public ILoadedFilesManager LoadedFiles { get { return m_loadedFiles; } }

    public ToolsInteractionModel ToolsInteraction
    {
        get
        {
            // Create on demand.
            if (m_toolsInteractionModel == null)
            {
                m_toolsInteractionModel = new ToolsInteractionModel(StepBroMain.GetService<IDynamicObjectManager>(), m_loadedFiles);
            }
            return m_toolsInteractionModel;
        }
    }

    /// <summary>
    /// Gets the tool items associated with this view-model.
    /// </summary>
    /// <value>The tool items associated with this view-model.</value>
    //public IList<ToolItemViewModel> ToolItems
    //{
    //    get
    //    {
    //        return m_toolItems;
    //    }
    //}

    //public IList<CreateCustomPanelMenuItemViewModel> CreateableCustomPanelMenuItems
    //{
    //    get
    //    {
    //        return m_creatableCustomPanels;
    //    }
    //}

    [ObservableProperty]
    private ITask m_fileParsingTask = null;

    private bool m_fileParsingRunning = false;
    public bool FileParsingRunning
    {
        get
        {
            SetProperty(ref m_fileParsingRunning, this.FileParsingTask != null);
            return m_fileParsingRunning;
        }
    }

    //public string ExecutionTarget
    //{
    //    get { return m_executionTarget; }
    //    set
    //    {
    //        if (!value.Equals(m_executionTarget))
    //        {
    //            m_executionTarget = value;
    //            m_executionTargetResolved = null;
    //            this.NotifyPropertyChanged(nameof(ExecutionTarget));
    //        }
    //    }
    //}

    public Tuple<IFileElement, string> GetExecutionTarget()
    {
        return new Tuple<IFileElement, string>(null, null);
    }

    //public string LastCalculatorResult
    //{
    //    get { return StringUtils.ObjectToString(StepBro.Core.Main.GetService<UICalculator>().LastResult); }
    //    set
    //    {
    //        this.NotifyPropertyChanged(nameof(LastCalculatorResult));
    //    }
    //}

    //public string CaretLine
    //{
    //    get
    //    {
    //        return m_caretLine;
    //    }
    //    set
    //    {
    //        if (m_caretLine != value)
    //        {
    //            m_caretLine = value;
    //            this.NotifyPropertyChanged(nameof(CaretLine));
    //        }
    //    }
    //}


    //public string CaretCharacter
    //{
    //    get
    //    {
    //        return m_caretCharacter;
    //    }
    //    set
    //    {
    //        if (m_caretCharacter != value)
    //        {
    //            m_caretCharacter = value;
    //            this.NotifyPropertyChanged(nameof(CaretCharacter));
    //        }
    //    }
    //}

    //public string CaretDisplayCharColumn
    //{
    //    get
    //    {
    //        return m_caretDisplayCharColumn;
    //    }
    //    set
    //    {
    //        if (m_caretDisplayCharColumn != value)
    //        {
    //            m_caretDisplayCharColumn = value;
    //            this.NotifyPropertyChanged(nameof(CaretDisplayCharColumn));
    //        }
    //    }
    //}

    #endregion

    //private void MainViewModel_ResultUpdated(object sender, EventArgs e)
    //{
    //    // TODO: Check if macro execution is running on active editor.
    //    this.NotifyPropertyChanged(nameof(LastCalculatorResult));
    //}

    //private DocumentItemViewModel GetActiveDocument()
    //{
    //    return m_userDocumentItems.FirstOrDefault(d => d.IsActive);
    //}

    private void UpdateCommandStates()
    {
        m_commandShowExecutionLogView.NotifyCanExecuteChanged();
        //m_commandActivateNextDocument?.RaiseCanExecuteChanged();
        //m_commandCloseActiveDocument?.RaiseCanExecuteChanged();
        //m_commandCreateNewStepBroDocument?.RaiseCanExecuteChanged();
        //m_commandCreateNewTextDocument?.RaiseCanExecuteChanged();
        //m_commandSelectFirstDocument?.RaiseCanExecuteChanged();
        //m_commandParseAllFiles?.RaiseCanExecuteChanged();
        //m_commandStartExecution?.RaiseCanExecuteChanged();
        //m_commandStartExecutionOfSelectedFileElement?.RaiseCanExecuteChanged();
        this.UpdatingCommandStates?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler<EventArgs> UpdatingCommandStates;

    #region VIEWMODEL EVENTS

    //private void DocumentItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    //{
    //    if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
    //    {
    //        foreach (DocumentItemViewModel doc in e.NewItems)
    //        {
    //            if (doc.LoadedFile == null && !String.IsNullOrEmpty(doc.SerializationId))
    //            {
    //                System.Diagnostics.Debug.WriteLine("TO BE LOADED: " + doc.SerializationId);
    //                doc.LoadedFile = this.OpenFile(doc.SerializationId, false);
    //            }
    //        }
    //    }
    //    else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
    //    {
    //        foreach (DocumentItemViewModel doc in e.OldItems)
    //        {
    //            StepBroMain.UnregisterFileUsage(this, doc.LoadedFile, false);
    //        }
    //    }
    //}

    //public void SetSelectedDocument(DocumentItemViewModel doc)
    //{
    //    if (m_selectedDocument != null)
    //    {
    //        m_selectedDocument.PropertyChanged -= SelectedDocument_PropertyChanged;
    //    }
    //    m_selectedDocument = doc;
    //    if (m_selectedDocument != null)
    //    {
    //        System.Diagnostics.Debug.WriteLine($"MainViewModel.SetSelectedDocument({doc.FileName})");
    //        m_selectedDocument.PropertyChanged += SelectedDocument_PropertyChanged;
    //        if (m_selectedDocument is TextDocumentItemViewModel)
    //        {
    //            var textdoc = m_selectedDocument as TextDocumentItemViewModel;
    //            this.CaretLine = $"Ln {textdoc.CaretLine}";
    //            this.CaretLine = $"Ch {textdoc.CaretCharacter}";
    //            this.CaretLine = $"Col {textdoc.CaretDisplayCharColumn}";
    //        }
    //    }
    //    else
    //    {
    //        System.Diagnostics.Debug.WriteLine("MainViewModel.SetSelectedDocument(null)");
    //        this.CaretLine = "";
    //        this.CaretCharacter = "";
    //        this.CaretDisplayCharColumn = "";
    //    }
    //}

    //private void SelectedDocument_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    //{
    //    System.Diagnostics.Debug.WriteLine($"MainViewModel.SelectedDocument_PropertyChanged: {e.PropertyName}");
    //    var doc = m_selectedDocument as TextDocumentItemViewModel;
    //    if (e.PropertyName == nameof(DocumentItemViewModel.IsSelected))
    //    {
    //        if (!m_selectedDocument.IsSelected)
    //        {
    //            this.SetSelectedDocument(null);
    //        }
    //    }
    //    else if (e.PropertyName == nameof(TextDocumentItemViewModel.CaretLine))
    //    {
    //        this.CaretLine = $"Ln {doc.CaretLine}";
    //    }
    //    else if (e.PropertyName == nameof(TextDocumentItemViewModel.CaretPosition))
    //    {
    //        this.CaretCharacter = $"Ch {doc.CaretCharacter}";
    //    }
    //    else if (e.PropertyName == nameof(TextDocumentItemViewModel.CaretDisplayCharColumn))
    //    {
    //        this.CaretDisplayCharColumn = $"Col {doc.CaretDisplayCharColumn}";
    //    }
    //}

    //private void ToolItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    //{
    //}

    //private void Tool_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    //{
    //    System.Diagnostics.Debug.WriteLine($"MainViewModel.Tool_PropertyChanged: {e.PropertyName}");
    //    if (e.PropertyName == nameof(ToolItemViewModel.IsOpen))
    //    {
    //        if ((sender is ToolItemViewModel) &&
    //            (sender as ToolItemViewModel).DestructWhenClosed &&
    //            !(sender as ToolItemViewModel).IsOpen)
    //        {
    //            (sender as ToolItemViewModel).PropertyChanged -= Tool_PropertyChanged;
    //            m_toolItems.Remove(sender as ToolItemViewModel);
    //        }
    //    }
    //}

    #endregion

    /////////////////////////////////////////////////////////////////////////////////////////////////////
    #region MODEL EVENTS
    /////////////////////////////////////////////////////////////////////////////////////////////////////

    #region LoadedFiles Events

    private void LoadedFiles_FileLoaded(object sender, LoadedFileEventArgs args)
    {
        System.Diagnostics.Debug.WriteLine("MainViewModel.LoadedFiles_FileLoaded");
        Invoke(o =>
        {
            //var alreadyOpenedDoc = m_userDocumentItems.FirstOrDefault(ud => string.Equals(ud.FileName, args.File.FilePath));

            //if (alreadyOpenedDoc != null)       // In case document view model was created before the file was loaded.
            //{
            //    System.Diagnostics.Debug.WriteLine("MainViewModel.LoadedFiles_FileLoaded - Invoked - already opened");
            //    System.Diagnostics.Debug.Assert(alreadyOpenedDoc.LoadedFile == null || Object.Equals(alreadyOpenedDoc.LoadedFile, args.File));
            //    alreadyOpenedDoc.LoadedFile = args.File;
            //    if (alreadyOpenedDoc.LoadedFile as IScriptFile != null)
            //    {
            //        (alreadyOpenedDoc.LoadedFile as IScriptFile).Errors.CollectionChanged += ScriptFileErrors_CollectionChanged;
            //    }
            //}
            //else
            //{
            //    System.Diagnostics.Debug.WriteLine("MainViewModel.LoadedFiles_FileLoaded - Invoked - not opened");
            //    var docViewModel = new TextDocumentItemViewModel();
            //    docViewModel.LoadedFile = args.File;
            //    docViewModel.IsReadOnly = false;
            //    docViewModel.Text = args.File.OffDiskFileContent;
            //    docViewModel.PropertyChanged += DocViewModel_PropertyChanged;

            //    var scriptFile = args.File as IScriptFile;
            //    if (scriptFile != null)
            //    {
            //        scriptFile.Errors.CollectionChanged += ScriptFileErrors_CollectionChanged;
            //    }

            //    m_userDocumentItems.Add(docViewModel);
            //    if (args.File.IsDependantOf(this))
            //    {
            //        if (String.Equals(m_documentToActivateWhenLoaded, docViewModel.FileName))
            //        {
            //            docViewModel.IsActive = true;
            //        }
            //        else
            //        {
            //            docViewModel.IsOpen = true;
            //        }
            //    }
            //}
            UpdateCommandStates();
        });
    }

    private void LoadedFiles_FileClosed(object sender, LoadedFileEventArgs args)
    {
        //var foundDocument = m_userDocumentItems.FirstOrDefault(f => Object.ReferenceEquals(args.File, f.LoadedFile));
        //if (args.File as IScriptFile != null)
        //{
        //    (args.File as IScriptFile).Errors.CollectionChanged -= ScriptFileErrors_CollectionChanged;
        //}

        //if (foundDocument != null)
        //{
        //    // File was forced closed by the StepBro.Core.
        //    foundDocument.PropertyChanged -= DocViewModel_PropertyChanged;
        //    foundDocument.LoadedFile = null;
        //    foundDocument.IsOpen = false;
        //}
        //else
        //{
        //    args.File.PropertyChanged -= DocViewModel_PropertyChanged;
        //}
        UpdateCommandStates();
    }

    private void File_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"MainViewModel.File_PropertyChanged: {e.PropertyName}");
        if (string.Equals(e.PropertyName, nameof(ILoadedFile.RegisteredDependantsCount)))
        {
            Invoke(o =>
            {
                System.Diagnostics.Debug.WriteLine($"MainViewModel.File_PropertyChanged: RegisteredDependantsCount - invoked");
                var file = o as ILoadedFile;
                //var docItem = m_userDocumentItems.FirstOrDefault(d => d.LoadedFile == file);    // If document has just been closed (removed from the list).
                //if (docItem != null)
                //{
                //    System.Diagnostics.Debug.WriteLine($"MainViewModel.File_PropertyChanged: RegisteredDependantsCount - invoked - doc found");
                //    if (file.IsDependantOf(this))
                //    {
                //        if (String.Equals(m_documentToActivateWhenLoaded, docItem.FileName))
                //        {
                //            System.Diagnostics.Debug.WriteLine($"MainViewModel.File_PropertyChanged: RegisteredDependantsCount - invoked - doc made active");
                //            m_documentToActivateWhenLoaded = null;
                //            docItem.IsActive = true;
                //        }
                //        else
                //        {
                //            System.Diagnostics.Debug.WriteLine($"MainViewModel.File_PropertyChanged: RegisteredDependantsCount - invoked - doc just made open");
                //            docItem.IsOpen = true;
                //        }
                //    }
                //    else
                //    {
                //        docItem.IsOpen = false;
                //    }
                //    this.UpdateCommandStates();
                //}
            }, sender);
        }
    }

    #endregion

    private void ScriptFileErrors_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        Invoke(o =>
        {
            //switch (e.Action)
            //{
            //    case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
            //        m_errors.Add(new ErrorInfo((IScriptFile)sender, (IErrorData)e.NewItems[0]));
            //        break;
            //    case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
            //        for (int i = m_errors.Count - 1; i >= 0; i--)
            //        {
            //            if (m_errors[i].Type == ProblemType.ParsingError) m_errors.RemoveAt(i);
            //        }
            //        break;
            //    default:
            //        break;
            //}
            UpdateCommandStates();
        });
    }

    //private void DocViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    //{
    //    System.Diagnostics.Debug.WriteLine($"MainViewModel.DocViewModel_PropertyChanged: {e.PropertyName}");
    //    if (e.PropertyName == nameof(DocumentItemViewModel.IsOpen))
    //    {
    //        var document = sender as DocumentItemViewModel;
    //        if (document.IsOpen == false)
    //        {
    //            document.LoadedFile.UnregisterDependant(this);
    //        }
    //        this.UpdateCommandStates();
    //    }
    //}

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
            System.Diagnostics.Debug.WriteLine($"MainViewModel.CreateNewTextDocument({(activate ? "activate" : "don't activate")})");
            var name = string.Format(nameFormatting, m_documentIndex++);
            if (activate)
            {
                m_documentToActivateWhenLoaded = name;
            }
            ILoadedFile file = new LoadedFileBase(name, LoadedFileType.ClearText);
            if (initialText == null)
            {
                file.OffDiskFileContent = "procedure void MyProcedure()" + Environment.NewLine +
                    "{" + Environment.NewLine +
                    "}" + Environment.NewLine;
            }
            file.RegisterDependant(this);
            StepBroMain.GetLoadedFilesManager().RegisterLoadedFile(file);
            UpdateCommandStates();
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
            System.Diagnostics.Debug.WriteLine($"MainViewModel.CreateNewStepBroDocument({(activate ? "activate" : "don't activate")})");
            var name = string.Format(nameFormatting, m_documentIndex++);
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
            UpdateCommandStates();
            return file;
        }
        finally
        {
            m_documentToActivateWhenLoaded = null;
        }
    }

    private ILoadedFile OpenFile(string filepath, bool activate = true)
    {
        System.Diagnostics.Debug.WriteLine($"MainViewModel.OpenFile({filepath}, {(activate ? "activate" : "don't activate")})");
        if (string.IsNullOrWhiteSpace(filepath)) return null;
        ILoadedFile loadedFile = null;
        var alreadyLoadedFile = m_loadedFiles.ListFiles<ILoadedFile>().FirstOrDefault(f => string.Equals(f.FilePath, filepath));
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
                //m_documentToActivateWhenLoaded = null;
            }
        }

        UpdateCommandStates();
        return loadedFile;
    }

    public IScriptFile LoadScriptFile(string filepath)
    {
        System.Diagnostics.Debug.WriteLine($"MainViewModel.LoadScriptFile({filepath})");
        return StepBroMain.LoadScriptFile(this, filepath);
    }

    //public TextDocumentItemViewModel OpenDocumentWindow(string filepath, bool activate)
    //{
    //    System.Diagnostics.Debug.WriteLine($"MainViewModel.OpenUserDocument({filepath})");
    //    var alreadyOpenedDoc = m_userDocumentItems.FirstOrDefault(ud => string.Equals(ud.FileName, filepath));
    //    if (alreadyOpenedDoc != null)
    //    {
    //        if (activate)
    //        {
    //            alreadyOpenedDoc.IsActive = true;
    //        }
    //        this.UpdateCommandStates();
    //        return alreadyOpenedDoc as TextDocumentItemViewModel;
    //    }
    //    else
    //    {
    //        var docViewModel = new TextDocumentItemViewModel();
    //        docViewModel.SerializationId = filepath;
    //        docViewModel.IsReadOnly = false;
    //        docViewModel.IsOpen = false;
    //        docViewModel.PropertyChanged += DocViewModel_PropertyChanged;
    //        if (activate)
    //        {
    //            m_documentToActivateWhenLoaded = filepath;
    //        }
    //        m_userDocumentItems.Add(docViewModel);
    //        this.UpdateCommandStates();
    //        return docViewModel;
    //    }
    //}

    #endregion

    /////////////////////////////////////////////////////////////////////////////////////////////////////
    #region COMMANDS
    /////////////////////////////////////////////////////////////////////////////////////////////////////

    #region Application Commands

    //public void ShowErrorsTool(bool activate)
    //{
    //    m_errorsViewModel.IsOpen = true;
    //    if (activate)
    //    {
    //        m_errorsViewModel.IsActive = true;
    //        m_errorsViewModel.IsSelected = true;
    //    }
    //    this.UpdateCommandStates();
    //}

    private void ShowView(ItemViewModel view, bool activate)
    {
        view.IsOpen = true;
        if (activate)
        {
            view.IsActive = true;
            view.IsSelected = true;
        }
        this.UpdateCommandStates();
    }

    //public void ShowExecutionLog(bool activate)
    //{
    //    m_executionLogView.IsOpen = true;
    //    if (activate)
    //    {
    //        m_executionLogView.IsActive = true;
    //        m_executionLogView.IsSelected = true;
    //    }
    //    this.UpdateCommandStates();
    //}

    //public void ShowCalculatorTool(bool activate)
    //{
    //    m_calculatorViewModel.IsOpen = true;
    //    if (activate)
    //    {
    //        m_calculatorViewModel.IsActive = true;
    //        m_calculatorViewModel.IsSelected = true;
    //        var selectedDoc = m_userDocumentItems.FirstOrDefault(d => d.IsOpen && d.IsSelected);
    //        m_calculatorViewModel.ActivatingDocument = selectedDoc;
    //    }
    //    this.UpdateCommandStates();
    //}

    //public void CreateCustomPanel(bool activate)
    //{
    //    var panel = new CustomPanelToolViewModel(StepBroMain.ServiceManager)
    //    {
    //        IsOpen = true
    //    };
    //    this.AddCustomPanel(panel, activate);
    //}

    //internal void AddCustomPanel(CustomPanelToolViewModel panel, bool activate = true)
    //{
    //    this.AddTool(panel, activate);
    //}

    //private void AddTool(ToolItemViewModel tool, bool activate = true)
    //{
    //    m_toolItems.Add(tool);
    //    tool.PropertyChanged += Tool_PropertyChanged;
    //    if (activate)
    //    {
    //        tool.IsActive = true;
    //        tool.IsSelected = true;
    //    }
    //    this.UpdateCommandStates();
    //}

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
                m_commandCreateNewTextDocument = new RelayCommand(
                    () =>
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
                m_commandCreateNewStepBroDocument = new RelayCommand(
                    () =>
                    {
                        CreateNewStepBroDocument(true);
                    }
                );

            return m_commandCreateNewStepBroDocument;
        }
    }

    public RelayCommand<string> OpenFileCommand
    {
        get
        {
            if (m_commandOpenFile == null)
            {
                m_commandOpenFile = new RelayCommand<string>(
                    (param) =>
                    {
                        OpenFile(param);
                    });
            }
            return m_commandOpenFile;
        }
    }

    //public DelegateCommand<string> OpenDocumentCommand
    //{
    //    get
    //    {
    //        if (m_commandOpenDocument == null)
    //        {
    //            m_commandOpenDocument = new DelegateCommand<string>(
    //                (filepath) =>
    //                {
    //                    OpenUserDocument(filepath, true);
    //                });
    //        }
    //        return m_commandOpenDocument;
    //    }
    //}

    public ICommand ShowErrorsViewCommand
    {
        get
        {
            if (m_commandShowErrorsView == null)
                m_commandShowErrorsView = new RelayCommand<object>(
                    (param) =>
                    {
                        //ShowErrorsTool(true);
                    }
                );

            return m_commandShowErrorsView;
        }
    }

    public ICommand ShowExecutionLogViewCommand
    {
        get
        {
            return m_commandShowExecutionLogView;
        }
    }

    public ICommand ShowCalculatorToolCommand
    {
        get
        {
            if (m_commandShowCalculatorTool == null)
                m_commandShowCalculatorTool = new RelayCommand<object>(
                    (param) =>
                    {
                        LogUserAction("Show Calculator");
                        //ShowCalculatorTool(true);
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
                m_commandSelectFirstDocument = new RelayCommand(
                    () =>
                    {
                        //var documentItem = m_userDocumentItems.FirstOrDefault();
                        //if (documentItem != null)
                        //    documentItem.IsSelected = true;
                        UpdateCommandStates();
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
                m_commandActivateNextDocument = new RelayCommand(
                    () =>
                    {
                        //if (m_userDocumentItems.Count > 0)
                        //{
                        //    var index = 0;
                        //    var activeDocumentItem = m_userDocumentItems.FirstOrDefault(d => d.IsActive);
                        //    if (activeDocumentItem != null)
                        //        index = m_userDocumentItems.IndexOf(activeDocumentItem) + 1;
                        //    if (index >= m_userDocumentItems.Count)
                        //        index = 0;

                        //    m_userDocumentItems[index].IsActive = true;
                        //    this.UpdateCommandStates();
                        //}
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
                m_commandCloseActiveDocument = new RelayCommand(
                    () =>
                    {
                        //var activeDocumentItem = m_userDocumentItems.FirstOrDefault(d => d.IsActive);
                        //if (activeDocumentItem != null)
                        //{
                        //    activeDocumentItem.LoadedFile.UnregisterDependant(this);
                        //    LogUserAction("Close active document; " + activeDocumentItem.FileName);
                        //}
                        UpdateCommandStates();
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
                m_commandParseAllFiles = new RelayCommand(
                    () =>
                    {
                        System.Diagnostics.Debug.WriteLine("MainViewModel.ParseAllFilesCommand.Execute");
                        this.FileParsingTask = StepBroMain.StartFileParsing(false);
                        if (this.FileParsingTask != null)
                        {
                            ApplicationStateMessage = "Parsing all script files...";
                            this.FileParsingTask.Control.CurrentStateChanged += FileParsing_CurrentStateChanged;
                            m_commandParseAllFiles.NotifyCanExecuteChanged();
                            //NotifyPropertyChanged(nameof(FileParsingRunning));    
                            UpdateCommandStates();
                        }
                        else
                        {
                            ApplicationStateMessage = "Failed to start file parsing.";
                        }
                    },
                    () =>
                    {
                        return this.FileParsingTask == null;
                    }
                );

            return m_commandParseAllFiles;
        }
    }

    private void FileParsing_CurrentStateChanged(object sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"MainViewModel.FileParsing_CurrentStateChanged: {this.FileParsingTask.Control.CurrentState}");
        if (this.FileParsingTask.Control.CurrentState >= TaskExecutionState.Ended)
        {
            Invoke(o =>
            {
                this.FileParsingTask.Control.CurrentStateChanged -= FileParsing_CurrentStateChanged;
                var parsingTime = this.FileParsingTask.Control.EndTime - this.FileParsingTask.Control.StartTime;
                var parsingErrorCount = m_errors.Where(err => err.Type == ProblemType.Parsing && err.Severity >= ProblemSeverity.Error).Count();
                var errors = parsingErrorCount > 0 ? $"{parsingErrorCount} error(s)!" : "No errors.";
                ApplicationStateMessage = $"Finished file parsing. Time: {parsingTime.ToMinutesTimestamp()} {errors}";
                this.FileParsingTask = null;
                m_commandParseAllFiles.NotifyCanExecuteChanged();
                //NotifyPropertyChanged(nameof(FileParsingRunning));
                UpdateCommandStates();
                //this.UpdateCustomPanelsMenu();
            });
        }
    }

    public ICommand StartOrContinueExecutionCommand
    {
        get
        {
            if (m_commandStartExecution == null)
                m_commandStartExecution = new RelayCommand<object>(
                    /* Action */ (param) =>
                    {
                        //m_fileParsingTask = StepBroMain.StartFileParsing(false);
                        //m_fileParsingTask.Control.CurrentStateChanged += FileParsing_CurrentStateChanged;
                        //m_commandParseAllFiles.RaiseCanExecuteChanged();
                        //NotifyPropertyChanged(nameof(FileParsingRunning));
                        UpdateCommandStates();
                    },
                    /* Predicate */ (param) =>
                    {
                        //if (m_fileParsingTask == null &&
                        //    m_userDocumentItems.Count > 0 &&
                        //    !String.IsNullOrEmpty(this.ExecutionTarget) &&
                        //    m_mainScriptExecution == null) return true;
                        if (m_mainScriptExecution != null && m_mainScriptExecution.Task.CurrentState == TaskExecutionState.Paused) return true;
                        if (m_mainScriptExecution == null)
                        {
                            return !string.IsNullOrEmpty(m_executionTarget) && GetExecutionTarget().Item1 != null;
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
                m_commandStartExecutionOfSelectedFileElement = new RelayCommand<object>(
                    /* Action */ (param) =>
                    {
                        //var active = GetActiveDocument();
                        //if (active != null && active.LoadedFile != null && active.LoadedFile.Type == LoadedFileType.StepBroScript)
                        //{
                        //    var editModel = active as TextDocumentItemViewModel;
                        //    var file = active.LoadedFile as IScriptFile;
                        //    IFileElement element = null;
                        //    var selectionLine = editModel.CaretPosition.Item1;    // Make line number 1-based
                        //    foreach (var fe in file.ListElements())
                        //    {
                        //        if (fe.Line <= selectionLine && (element == null || element.Line < fe.Line))
                        //        {
                        //            element = fe;
                        //            break;
                        //        }
                        //    }
                        //    if (element != null)
                        //    {
                        //        //this.ExecutionTarget = element.FullName;
                        //        if (element is IFileProcedure)
                        //        {
                        //            m_executionTargetResolved = new Tuple<IFileElement, string>(element, null);
                        //            m_mainScriptExecution = StepBro.Core.Main.StartProcedureExecution(element as IFileProcedure, null);
                        //            m_mainScriptExecution.Task.CurrentStateChanged += MainScriptExecution_CurrentStateChanged;
                        //            this.UpdateCommandStates();
                        //        }
                        //        else
                        //        {
                        //            // TODO: Show comment in status bar
                        //        }
                        //    }
                        //    else
                        //    {
                        //        // TODO: Show comment in status bar
                        //    }
                        //}
                    },
                    /* Predicate */ (param) =>
                    {
                        //var active = GetActiveDocument();
                        //return (m_fileParsingTask == null &&
                        //        m_mainScriptExecution == null &&
                        //        m_userDocumentItems.Count > 0 &&
                        //        active != null &&
                        //        !active.IsModified &&
                        //        active.LoadedFile != null &&
                        //        active.LoadedFile.Type == LoadedFileType.StepBroScript);
                        return true;
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
                Invoke(OnScriptExecutionStarted);
                break;
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
                Invoke(OnScriptExecutionEnded, m_mainScriptExecution.Result);
                break;
            default:
                break;
        }

    }

    void OnScriptExecutionStarted(object state)
    {
        ApplicationStateMessage = $"Started script execution.";
    }
    void OnScriptExecutionRunning(object state)
    {
        ApplicationStateMessage = $"Script execution running ...";
    }
    void OnScriptExecutionPaused(object state)
    {
    }
    void OnScriptExecutionEnded(object state)
    {
        m_mainScriptExecution.Task.CurrentStateChanged -= MainScriptExecution_CurrentStateChanged;
        m_mainScriptExecution = null;

        ApplicationStateMessage = $"Finished script execution. {((IExecutionResult)state).ResultText()}";
        UpdateCommandStates();
    }

    #endregion

    #endregion

    /////////////////////////////////////////////////////////////////////////////////////////////////////
    #region PRIVATE FUNCTIONS

    //private void UpdateCustomPanelsMenu()
    //{
    //    var command = new DelegateCommand<CreateCustomPanelMenuItemViewModel>(
    //    (param) =>
    //    {
    //        this.OpenCustomPanel(param);
    //    });
    //    var dynamicObjects = StepBroMain.GetService<IDynamicObjectManager>().GetObjectCollection().ToList();
    //    m_creatableCustomPanels.BeginUpdate();
    //    m_creatableCustomPanels.Clear();
    //    foreach (var pt in m_panelManager.ListPanelTypes())
    //    {
    //        if (!pt.IsObjectPanel)
    //        {
    //            var menuItem = new CreateCustomPanelMenuItemViewModel(pt, command);
    //            m_creatableCustomPanels.Add(menuItem);
    //        }
    //    }

    //    foreach (var vc in dynamicObjects)
    //    {
    //        var objectMenu = new CreateCustomPanelMenuItemViewModel((vc as IValueContainer).Name);
    //        if (vc.Object != null)
    //        {
    //            foreach (var pt in m_panelManager.ListPanelTypes())
    //            {
    //                if (pt.IsObjectPanel && pt.IsCompatibleWithType(vc.Object.GetType())) // TODO: Check if more of this type can be created.
    //                {
    //                    var menuItem = new CreateCustomPanelMenuItemViewModel(pt, vc, command, pt.Name);
    //                    objectMenu.AddSubItem(menuItem);
    //                }
    //            }
    //        }
    //        if (objectMenu.SubItems != null)
    //        {
    //            m_creatableCustomPanels.Add(objectMenu);
    //        }
    //    }

    //    m_creatableCustomPanels.EndUpdate();
    //}

    //private void OpenCustomPanel(CreateCustomPanelMenuItemViewModel activation)
    //{
    //    CustomPanelInstanceData panelData;
    //    if (activation.PanelType.IsObjectPanel)
    //    {
    //        panelData = m_panelManager.CreateObjectPanel(activation.PanelType, activation.Variable);
    //    }
    //    else
    //    {
    //        panelData = m_panelManager.CreateStaticPanel(activation.PanelType);
    //    }

    //    var panel = new CustomPanelToolViewModel(StepBroMain.ServiceManager, panelData)
    //    {
    //        IsOpen = true
    //    };
    //    m_toolItems.Add(panel);
    //    panel.IsActive = true;
    //    panel.IsSelected = true;
    //    this.UpdateCommandStates();
    //}

    #endregion
}
