using ActiproSoftware.Windows;
using ActiproSoftware.Windows.Input;
using StepBro.Workbench.Editor;
using StepBro.Workbench.ToolViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace StepBro.Workbench
{

    /// <summary>
    /// Represents the main view-model.
    /// </summary>
    public class MainViewModel : ObservableObjectBase
    {

        private int m_documentIndex = 1;
        private DeferrableObservableCollection<DocumentItemViewModel> m_documentItems = new DeferrableObservableCollection<DocumentItemViewModel>();
        private DeferrableObservableCollection<ToolItemViewModel> m_toolItems = new DeferrableObservableCollection<ToolItemViewModel>();

        private DelegateCommand<object> m_commandActivateNextDocument;
        private DelegateCommand<object> m_commandCloseActiveDocument;
        //private DelegateCommand<object> createNewImageDocumentCommand;
        private DelegateCommand<object> m_commandCreateNewStepBroDocument;
        private DelegateCommand<object> m_commandCreateNewTextDocument;
        private DelegateCommand<object> m_commandSelectFirstDocument;
        private ICommand m_commandShowCalculatorTool;
        private readonly CalculatorViewModel m_calculatorViewModel = null;

        /////////////////////////////////////////////////////////////////////////////////////////////////////
        // OBJECT
        /////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel"/> class.
        /// </summary>
        public MainViewModel()
        {
            //this.CreateNewTextDocument(false);
            m_calculatorViewModel = new CalculatorViewModel() { State = ToolItemState.Docked };
            m_toolItems.Add(m_calculatorViewModel);
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////
        // PUBLIC PROCEDURES
        /////////////////////////////////////////////////////////////////////////////////////////////////////

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
                            if (m_documentItems.Count > 0)
                            {
                                var index = 0;
                                var activeDocumentItem = m_documentItems.FirstOrDefault(d => d.IsActive);
                                if (activeDocumentItem != null)
                                    index = m_documentItems.IndexOf(activeDocumentItem) + 1;
                                if (index >= m_documentItems.Count)
                                    index = 0;

                                m_documentItems[index].IsActive = true;
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
                            var activeDocumentItem = m_documentItems.FirstOrDefault(d => d.IsActive);
                            if (activeDocumentItem != null)
                                m_documentItems.Remove(activeDocumentItem);
                        }
                    );

                return m_commandCloseActiveDocument;
            }
        }

        /// <summary>
        /// Creates a new text document.
        /// </summary>
        /// <param name="activate">Whether to activate the document.</param>
        /// <param name="nameFormatting">A <seealso cref="string.Format"/> text to use for generating the file name.</param>
        public void CreateNewTextDocument(bool activate, string nameFormatting = "Document{0}.txt", string initialText = "<text document>")
        {
            var viewModel = new TextDocumentItemViewModel();
            viewModel.SerializationId = String.Format(nameFormatting, m_documentIndex);  // NOTE: Every docking window must have a unique SerializationId if you wish to use layout serialization
            viewModel.FileName = String.Format(nameFormatting, m_documentIndex++);
            viewModel.Title = viewModel.FileName;
            viewModel.Text = initialText;

            m_documentItems.Add(viewModel);

            if (activate)
                viewModel.IsActive = true;
            else
                viewModel.IsOpen = true;
        }

        /// <summary>
        /// Creates a new text document.
        /// </summary>
        /// <param name="activate">Whether to activate the document.</param>
        public void CreateNewStepBroDocument(bool activate)
        {
            this.CreateNewTextDocument(activate, "Script{0}.sbs",
                "procedure void MyProcedure()" + Environment.NewLine +
                "{" + Environment.NewLine +
                "}" + Environment.NewLine);
        }

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
                            this.CreateNewTextDocument(true);
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
                            this.CreateNewStepBroDocument(true);
                        }
                    );

                return m_commandCreateNewStepBroDocument;
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
                            this.ShowCalculatorTool(true);
                        }
                    );

                return m_commandShowCalculatorTool;
            }
        }

        public ICommand RepeatActivationCommand
        {
            get
            {
                return CustomEditCommands.RepeatActivationCommand;
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
                return m_documentItems;
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
                            var documentItem = m_documentItems.FirstOrDefault();
                            if (documentItem != null)
                                documentItem.IsSelected = true;
                        }
                    );

                return m_commandSelectFirstDocument;
            }
        }

    }

}
