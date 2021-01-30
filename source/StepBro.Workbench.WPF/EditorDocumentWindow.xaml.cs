using ActiproSoftware.Text;
using ActiproSoftware.Text.Implementation;
using ActiproSoftware.Windows.Controls.Docking;
using ActiproSoftware.Windows.Controls.SyntaxEditor;
using ActiproSoftware.Windows.Extensions;
using System;
using System.Windows;
using System.Windows.Data;
using System.ComponentModel;
using StepBro.UI.SyntaxEditorSupport;

namespace StepBro.Workbench
{
    /// <summary>
    /// Interaction logic for EditorDocumentWindow.xaml
    /// </summary>
    public partial class EditorDocumentWindow : DocumentWindow
    {
        private bool hasPendingParseData;
        //private CSharpSyntaxLanguage cSharpSyntaxLanguage;
        private SyntaxEditorStepBroSyntaxLanguage stepBroSyntaxLanguage;

        /// <summary>
        /// Initializes an instance of the <c>EditorDocumentWindow</c> class.
        /// </summary>
        /// <param name="data">The document data.</param>
        /// <param name="text">The text to show in the editor.</param>
        public EditorDocumentWindow(DocumentData data, string text) : this()
        {
            if (data == null)
                throw new ArgumentNullException("data");

            this.Data = data;
            this.AssignLanguageAndTextForFileType(text);
        }
        public EditorDocumentWindow()
        {
            this.InitializeComponent();
        }



        /////////////////////////////////////////////////////////////////////////////////////////////////////
        // NON-PUBLIC PROCEDURES
        /////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Asssign a language and default text based on the current file type.
        /// </summary>
        /// <param name="text">The text to show in the editor.</param>
        private void AssignLanguageAndTextForFileType(string text)
        {
            var requiresDefaultText = (text == null);
            if (!requiresDefaultText)
                editor.Document.SetText(text);

            var extension = System.IO.Path.GetExtension(this.Data.FileName).ToLowerInvariant();
            editor.Document.Language = this.GetOrCreateLanguage(extension);
            if (requiresDefaultText)
                editor.Document.SetText(this.GetDefaultText(extension));

            // Update symbol selector visibility
            symbolSelectorBorder.Visibility = (editor.Document.Language.GetNavigableSymbolProvider() != null ? Visibility.Visible : Visibility.Collapsed);
            symbolSelector.AreMemberSymbolsSupported = (editor.Document.Language.Key != "Python");
        }

        /// <summary>
        /// Gets the document data.
        /// </summary>
        /// <value>The document data.</value>
        private DocumentData Data
        {
            get
            {
                return (DocumentData)this.DataContext;
            }
            set
            {
                this.DataContext = value;

                this.BindToProperty(DocumentWindow.FileNameProperty, value, "FileName", BindingMode.OneWay);
                this.BindToProperty(DocumentWindow.TitleProperty, value, "Title", BindingMode.OneWay);
            }
        }

        /// <summary>
        /// Returns the default text for the specified extension.
        /// </summary>
        /// <param name="extension">The file extension.</param>
        /// <returns>The default text to use.</returns>
        private string GetDefaultText(string extension)
        {
            switch (extension)
            {
                case ".sbs":
                    return @"procedure MyProc()
{
    log(""Hello World"");
}
";
                case ".cs":
                    return @"using System;

public class Class1 {

	public Class1() {
	}

}
";
                case ".js":
                    return @"// JavaScript source code
";
                case ".py":
                    return @"# Python source code
";
                case ".vb":
                    return @"Imports Microsoft.VisualBasic

Public Class Class1

End Class
";
                case ".xml":
                    return @"<?xml version=""1.0"" encoding=""utf-8""?>
";
                default:
                    return String.Empty;
            }
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

        /// <summary>
        /// Occues when the document's modified state changes.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The <c>RoutedEventArgs</c> that contains data related to this event.</param>
        private void OnEditorDocumentIsModifiedChanged(object sender, RoutedEventArgs e)
        {
            this.Data.IsModified = editor.Document.IsModified;
        }

        /// <summary>
        /// Occurs when the document's parse data has changed.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The <c>EventArgs</c> that contains data related to this event.</param>
        private void OnEditorDocumentParseDataChanged(object sender, RoutedEventArgs e)
        {
            //
            // NOTE: The parse data here is generated in a worker thread... this event handler is called 
            //         back in the UI thread immediately when the worker thread completes... it is best
            //         practice to delay UI updates until the end user stops typing... we will flag that
            //         there is a pending parse data change, which will be handled in the 
            //         UserInterfaceUpdate event
            //

            hasPendingParseData = true;
        }

        /// <summary>
        /// Occurs when a search operation occurs in a view.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">A <see cref="EditorViewSearchEventArgs"/> that contains the event data.</param>
        private void OnEditorViewSearch(object sender, EditorViewSearchEventArgs e)
        {
            if (this.Data.NotifySearchAction != null)
                this.Data.NotifySearchAction(this, e.ResultSet);
        }

        /// <summary>
        /// Occurs after a brief delay following any document text, parse data, or view selection update, allowing consumers to update the user interface during an idle period.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> that contains data related to this event.</param>
        private void OnEditorUserInterfaceUpdate(object sender, RoutedEventArgs e)
        {
            // If there is a pending parse data change...
            if (hasPendingParseData)
            {
                // Clear flag
                hasPendingParseData = false;

                if (this.Data.NotifyDocumentOutlineUpdated != null)
                    this.Data.NotifyDocumentOutlineUpdated(this);
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////
        // NON-PUBLIC PROCEDURES
        /////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the embedded <see cref="SyntaxEditor"/> control.
        /// </summary>
        /// <value>The embedded <see cref="SyntaxEditor"/> control.</value>
        public SyntaxEditor Editor
        {
            get
            {
                return editor;
            }
        }



    }
}
