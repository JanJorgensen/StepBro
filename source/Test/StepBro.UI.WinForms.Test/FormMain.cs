using System.Text;
using StepBro.Core.Data;
using StepBro.Core;
using StepBro.Core.General;
using StepBro.Core.Logging;
using StepBroMain = StepBro.Core.Main;
using StepBro.Core.ScriptData;
using StepBro.Core.File;
using System.Windows.Forms;
using StepBro.Core.Api;
using System.Reflection;
using StepBro.Core.Tasks;
using StepBro.Core.Execution;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using StepBro.Core.Host;

namespace StepBro.UI.WinForms.Test
{
    public partial class FormMain : Form, ICoreAccess
    {
        public class HostAccess : StepBro.Core.Host.HostAccessBase<HostAccess>
        {
            public HostAccess(out IService serviceAccess) : base("Host", out serviceAccess, typeof(ILogger))
            {
            }

            public override HostType Type { get { return HostType.WinForms; } }

            public override IEnumerable<NamedData<object>> ListHostCodeModuleInstances()
            {
                //yield return new NamedData<object>("Host.Console", m_app);
                yield break;
            }

            public override IEnumerable<Type> ListHostCodeModuleTypes()
            {
                yield break;
            }
        }

        private CoreCommandlineOptions m_commandlineOptions = null;
        private HostAccess m_hostAccess = null;
        private IScriptFile m_scriptfile = null;
        private object m_fileUser = new object();
        private LogEntry m_lastLogEntrySeen = null;
        private DateTime m_logZero;

        int ICoreAccess.ExecutionsRunning => throw new NotImplementedException();

        public FormMain()
        {
            InitializeComponent();


            string[] args = Environment.GetCommandLineArgs();
            StringBuilder sb = new StringBuilder();
            System.IO.StringWriter sw = new System.IO.StringWriter(sb);
            m_commandlineOptions = StepBro.Core.General.CommandLineParser.Parse<CoreCommandlineOptions>(null, args.Skip(1).ToArray(), sw);
            // Print output from the command line parsing, and skip empty lines.
            foreach (var line in sb.ToString().Split(System.Environment.NewLine))
            {
                //if (!String.IsNullOrWhiteSpace(line)) ConsoleWriteLine(line);
            }
            IService m_hostService = null;
            m_hostAccess = new HostAccess(out m_hostService);
            StepBroMain.Initialize(m_hostService);

            customPanelContainer.SetCoreAccess(this);

            m_lastLogEntrySeen = StepBroMain.Logger.GetLast();
            m_logZero = m_lastLogEntrySeen.Timestamp;
            AddLogEntry(m_lastLogEntrySeen);
        }

        private void buttonLoadFile_Click(object sender, EventArgs e)
        {
            if (!m_commandlineOptions.HasParsingErrors && m_commandlineOptions.InputFile != null)
            {
                var filepath = System.IO.Path.GetFullPath(m_commandlineOptions.InputFile);
                try
                {
                    if (m_scriptfile != null)
                    {
                        StepBroMain.UnregisterFileUsage(m_fileUser, m_scriptfile);
                    }
                    m_scriptfile = StepBroMain.LoadScriptFile(m_fileUser, filepath);
                    if (m_scriptfile != null)
                    {
                        var parsingSuccess = StepBroMain.ParseFiles(true);
                        if (parsingSuccess)
                        {
                            var objectManager = StepBroMain.ServiceManager.Get<IDynamicObjectManager>();
                            var objects = objectManager.GetObjectCollection();

                            var panelVar = objects.Where(v => v.Object is StepBro.PanelCreator.Panel).FirstOrDefault();
                            if (panelVar != null)
                            {
                                var panel = panelVar.Object as StepBro.PanelCreator.Panel;
                                customPanelContainer.SetCustomPanelDefinition(panel.Title, panel.MainPanelDefinition);
                                //((FileElements.PanelDefinitionVariable)variableData).Title = panel.Title;
                                //((FileElements.PanelDefinitionVariable)variableData).PanelDefinition = panel.MainPanelDefinition.CloneForSerialization();
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Failed to load file: " + filepath, "StepBro WinForms Test", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Exception parsing file", "StepBro WinForms Test", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void timerLogUpdate_Tick(object sender, EventArgs e)
        {
            if (m_lastLogEntrySeen.Next != null)
            {
                listViewLog.SuspendLayout();
                while (m_lastLogEntrySeen.Next != null)
                {
                    m_lastLogEntrySeen = m_lastLogEntrySeen.Next;
                    AddLogEntry(m_lastLogEntrySeen);
                }
                listViewLog.EnsureVisible(listViewLog.Items.Count - 1);
                listViewLog.ResumeLayout();
            }
        }

        private void AddLogEntry(LogEntry entry)
        {
            if (!string.IsNullOrEmpty(entry.Text) || !string.IsNullOrEmpty(entry.Location))
            {
                string type = entry.EntryType switch
                {
                    LogEntry.Type.Async => "<A>",
                    LogEntry.Type.CommunicationOut => "<Out>",
                    LogEntry.Type.CommunicationIn => "<In>",
                    LogEntry.Type.TaskEntry => "TaskEntry",
                    LogEntry.Type.Error => "Error",
                    LogEntry.Type.Failure => "Fail",
                    LogEntry.Type.UserAction => "UserAction",
                    _ => ""
                };
                var item = new ListViewItem();
                item.Text = entry.Timestamp.ToSecondsTimestamp(m_logZero);
                item.SubItems.Add(type);
                item.SubItems.Add(string.IsNullOrEmpty(entry.Location) ? "" : entry.Location);
                item.SubItems.Add(string.IsNullOrEmpty(entry.Text) ? "" : entry.Text);
                listViewLog.Items.Add(item);
            }
        }

        IExecutionAccess ICoreAccess.StartExecution(string element, string model, string objectVariable, object[] args)
        {
            IFileElement elementData = StepBroMain.TryFindFileElement(element);
            if (elementData != null)
            {
                IPartner partner = null;
                List<object> arguments = new List<object>();
                //m_commandLineOptions?.Arguments.Select((a) => StepBroMain.ParseExpression(element?.ParentFile, a)).ToList();

                if (!String.IsNullOrEmpty(model))
                {
                    partner = elementData.ListPartners().First(p => String.Equals(model, p.Name, StringComparison.InvariantCultureIgnoreCase));
                    if (partner == null)
                    {
                        return new ExecutionAccessDummy(Core.Tasks.TaskExecutionState.ErrorStarting);
                        //retval = -1;
                        //ConsoleWriteErrorLine($"Error: The specified file element does not have a model named \"{targetPartner}\".");
                    }
                }
                else
                {
                    if (!(elementData is IFileProcedure))
                    {
                        return new ExecutionAccessDummy(Core.Tasks.TaskExecutionState.ErrorStarting);
                        //retval = -1;
                        //ConsoleWriteErrorLine($"Error: Target element (type {element.ElementType}) is not a supported type for execution.");
                    }
                }

                try
                {
                    var execution = StepBroMain.StartProcedureExecution(elementData, partner, arguments.ToArray());
                    return new ExecutionAccessWrapper(execution);
                }
                catch (TargetParameterCountException)
                {
                    return new ExecutionAccessDummy(Core.Tasks.TaskExecutionState.ErrorStarting);
                }
                catch (Exception)
                {
                    return new ExecutionAccessDummy(Core.Tasks.TaskExecutionState.ErrorStarting);
                }
            }
            return new ExecutionAccessDummy(Core.Tasks.TaskExecutionState.ErrorStarting);
        }

        void ICoreAccess.ExecuteObjectCommand(string objectVariable, string command)
        {
            throw new NotImplementedException();
        }
    }

    public class ExecutionAccessWrapper : IExecutionAccess
    {
        IScriptExecution m_execution;
        public ExecutionAccessWrapper(IScriptExecution execution)
        {
            m_execution = execution;
            m_execution.Task.CurrentStateChanged += Task_CurrentStateChanged;
        }

        private void Task_CurrentStateChanged(object sender, EventArgs e)
        {
            if (this.CurrentStateChanged != null) { this.CurrentStateChanged(this, new EventArgs()); }
        }

        public TaskExecutionState State { get { return m_execution.Task.CurrentState; } }

        public object ReturnValue => throw new NotImplementedException();

        public event EventHandler CurrentStateChanged;

        public void RequestStopExecution()
        {
            m_execution.Task.RequestStop();
        }

        public void Dispose()
        {
            m_execution.Task.CurrentStateChanged -= Task_CurrentStateChanged;
        }
    }
}