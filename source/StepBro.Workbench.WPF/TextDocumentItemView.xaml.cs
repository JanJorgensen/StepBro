﻿using ActiproSoftware.Text;
using ActiproSoftware.Text.Implementation;
using StepBro.Core.Parser;
using StepBro.UI.SyntaxEditorSupport;
using StepBro.Workbench.Editor;
using System.Windows;
using System.Windows.Input;

namespace StepBro.Workbench
{

    /// <summary>
    /// Provides the text document item view.
    /// </summary>
    public partial class TextDocumentItemView
    {
        private SyntaxEditorStepBroSyntaxLanguage stepBroSyntaxLanguage;
        private TextDocumentItemViewModel m_currentData = null;

        /////////////////////////////////////////////////////////////////////////////////////////////////////
        // OBJECT
        /////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes an instance of the <c>TextDocumentItemView</c> class.
        /// </summary>
        public TextDocumentItemView()
        {
            InitializeComponent();
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, SaveExecuted, SaveCanExecute));
            AddEditorCommandBinding(CustomEditCommands.RepeatActivationCommand);
            editor.DocumentIsModifiedChanged += Editor_DocumentIsModifiedChanged;
            //this.AddCommandBinding(new InsertCalculatorResultAction());
        }

        private void Editor_DocumentIsModifiedChanged(object sender, RoutedEventArgs e)
        {
            if (m_currentData != null) m_currentData.IsModified = editor.Document.IsModified;
        }

        private void AddEditorCommandBinding(ICommand action)
        {
            editor.CommandBindings.Add(new CommandBinding(action));
        }

        private TextDocumentItemViewModel Data
        {
            get
            {
                return m_currentData;
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property.Name == nameof(DataContext))
            {
                if (m_currentData != null)
                {
                    m_currentData.PropertyChanged -= CurrentData_PropertyChanged;
                    m_currentData = null;
                }
                if (e.NewValue != null)
                {
                    m_currentData = (TextDocumentItemViewModel)DataContext;
                    m_currentData.PropertyChanged += CurrentData_PropertyChanged;
                    LoadFile();
                    AssignLanguage();
                }
            }
        }

        private void CurrentData_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {

        }

        private void LoadFile()
        {
            if (!string.IsNullOrEmpty(m_currentData.LoadedFile.FilePath))
            {
                editor.Document.LoadFile(m_currentData.LoadedFile.FilePath);
            }
        }

        #region Commands

        private void SaveCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            TextDocumentItemView control = (TextDocumentItemView)sender;
            e.CanExecute = control.editor.Document.IsModified && m_currentData != null;
        }

        private void SaveExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            System.Diagnostics.Trace.WriteLine("");
            try
            {
                editor.Document.SaveFile(m_currentData.LoadedFile.FilePath, LineTerminator.CarriageReturnNewline);
            }
            catch
            {
                MessageBox.Show("Error saving file: " + m_currentData.LoadedFile.FilePath, "Save File", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Language

        private void AssignLanguage()
        {
            //var requiresDefaultText = (text == null);
            //if (!requiresDefaultText)
            //    editor.Document.SetText(text);

            var extension = System.IO.Path.GetExtension(Data.FileName).ToLowerInvariant();
            editor.Document.Language = GetOrCreateLanguage(extension);
            //if (requiresDefaultText)
            //    editor.Document.SetText(this.GetDefaultText(extension));

            // Update symbol selector visibility
            symbolSelectorBorder.Visibility = (editor.Document.Language.GetNavigableSymbolProvider() != null ? Visibility.Visible : Visibility.Collapsed);
            symbolSelector.AreMemberSymbolsSupported = (editor.Document.Language.Key != "Python");
        }

        /// <summary>
        /// Returns a language for the specified extension.
        /// </summary>
        /// <param name="extension">The file extension.</param>
        /// <returns>The <see cref="ISyntaxLanguage"/> to use.</returns>
        private ISyntaxLanguage GetOrCreateLanguage(string extension)
        {
            switch (extension)
            {
                case ".sbs":
                    if (stepBroSyntaxLanguage == null)
                        stepBroSyntaxLanguage = new SyntaxEditorStepBroSyntaxLanguage();
                    return stepBroSyntaxLanguage;
                //case ".cs":
                //    if (cSharpSyntaxLanguage == null)
                //    {
                //        cSharpSyntaxLanguage = new CSharpSyntaxLanguage();

                //        var cSharpProjectAssembly = new CSharpProjectAssembly("Sample");
                //        var assemblyLoader = new BackgroundWorker();
                //        assemblyLoader.DoWork += (sender, e) =>
                //        {
                //            // Add some common assemblies for reflection (any custom assemblies could be added using various Add overloads instead)
                //            cSharpProjectAssembly.AssemblyReferences.AddMsCorLib();
                //            cSharpProjectAssembly.AssemblyReferences.Add("System");
                //            cSharpProjectAssembly.AssemblyReferences.Add("System.Core");
                //            cSharpProjectAssembly.AssemblyReferences.Add("System.Xml");
                //        };
                //        assemblyLoader.RunWorkerAsync();
                //        cSharpSyntaxLanguage.RegisterProjectAssembly(cSharpProjectAssembly);
                //    }
                //    return cSharpSyntaxLanguage;
                //case ".py":
                //    if (pythonSyntaxLanguage == null)
                //        pythonSyntaxLanguage = new PythonSyntaxLanguage();
                //    return pythonSyntaxLanguage;
                //case ".xml":
                //    if (xmlSyntaxLanguage == null)
                //        xmlSyntaxLanguage = new XmlSyntaxLanguage();
                //    return xmlSyntaxLanguage;
                default:
                    return SyntaxLanguage.PlainText;
            }
        }

        #endregion

        private void OnEditorDocumentIsModifiedChanged(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void OnEditorDocumentParseDataChanged(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void OnEditorViewSearch(object sender, ActiproSoftware.Windows.Controls.SyntaxEditor.EditorViewSearchEventArgs e)
        {

        }

        private void OnEditorUserInterfaceUpdate(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void editor_ViewSelectionChanged(object sender, ActiproSoftware.Windows.Controls.SyntaxEditor.EditorViewSelectionEventArgs e)
        {
            if (sender == editor.ActiveView && m_currentData != null)
            {
                m_currentData.CurrentSelectionStart = new System.Tuple<int, int>(e.View.Selection.StartPosition.Line, e.View.Selection.StartPosition.Character);
            }
        }
    }

}
