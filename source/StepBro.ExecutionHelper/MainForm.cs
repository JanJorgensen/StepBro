using StepBro.Core.IPC;
using StepBro.ExecutionHelper.Messages;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

namespace StepBro.ExecutionHelper
{
    public partial class MainForm : Form
    {
        private nint m_consoleWindow = 0;
        private Pipe? m_pipe = null;
        FormClosingEventHandler? formCloseEventHandler = null;
        private bool m_closeRequested = false;
        private Dictionary<string, object> m_variables = new Dictionary<string, object>();

        public MainForm()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            System.Diagnostics.Trace.WriteLine("Execution Helper STARTING!!");

            string[] args = Environment.GetCommandLineArgs();

            if (args.Length == 2)
            {
                m_consoleWindow = nint.Parse(args[1], System.Globalization.NumberStyles.HexNumber);
                m_pipe = Pipe.StartClient("StepBroExecutionHelper", args[1]);
            }
            else
            {
                return;
            }

            formCloseEventHandler = (sender, e) =>
            {
                m_pipe.Send(StepBro.ExecutionHelper.Messages.ShortCommand.Close);
                Thread.Sleep(1000);     // Leave some time for the execution helper application to receive the command.
            };

            FormClosing += formCloseEventHandler;
        }

        private void timerMasterPull_Tick(object sender, EventArgs e)
        {
            Tuple<string, string> received;
            while (m_pipe != null && (received = m_pipe.TryGetReceived()) != null)
            {
                if (received.Item1 == nameof(ShortCommand))
                {
                    var cmd = JsonSerializer.Deserialize<ShortCommand>(received.Item2);
                    if (cmd == ShortCommand.Close)
                    {
                        m_closeRequested = true;
                        m_pipe.Send(cmd); // Send back, to continue the closing process.
                        Thread.Sleep(100);
                        m_pipe.Dispose();
                        this.Close();
                    }
                }
                else if (received.Item1 == nameof(StepBro.ExecutionHelper.Messages.CreateVariable))
                {
                    var data = JsonSerializer.Deserialize<StepBro.ExecutionHelper.Messages.CreateVariable>(received.Item2);
                    if (data != null)
                    {
                        if (data.InitialValue is System.Text.Json.JsonElement v && v.ValueKind == JsonValueKind.Number)
                        {
                            // It is some sort of number, so we save it as a long like StepBro uses
                            long initialValue = 0;
                            Int64.TryParse(data.InitialValue.ToString(), out initialValue);
                            m_variables.TryAdd(data.VariableName, initialValue);
                        }
                        else
                        {
                            // It is not a number so we assume the user knows what they are doing
                            m_variables.TryAdd(data.VariableName, data.InitialValue);
                        }
                        m_pipe.Send(ShortCommand.Acknowledge);
                    }
                }
                else if (received.Item1 == nameof(StepBro.ExecutionHelper.Messages.IncrementVariable))
                {
                    var data = JsonSerializer.Deserialize<StepBro.ExecutionHelper.Messages.IncrementVariable>(received.Item2);
                    if (data != null && m_variables.ContainsKey(data.VariableName))
                    {
                        if (m_variables[data.VariableName] is long v)
                        {
                            m_variables[data.VariableName] = ++v;
                            m_pipe.Send(ShortCommand.Acknowledge);
                        }
                    }
                }
                else if (received.Item1 == nameof(StepBro.ExecutionHelper.Messages.SetVariable))
                {
                    var data = JsonSerializer.Deserialize<StepBro.ExecutionHelper.Messages.SetVariable>(received.Item2);
                    if (data != null && m_variables.ContainsKey(data.VariableName))
                    {
                        m_variables[data.VariableName] = data.Value;
                        m_pipe.Send(ShortCommand.Acknowledge);
                    }
                }
                else if (received.Item1 == nameof(StepBro.ExecutionHelper.Messages.GetVariable))
                {
                    var data = JsonSerializer.Deserialize<StepBro.ExecutionHelper.Messages.GetVariable>(received.Item2);
                    if (data != null)
                    {
                        m_pipe.Send(new SendVariable(data.VariableName, m_variables[data.VariableName]));
                    }
                }
                else if (received.Item1 == nameof(StepBro.ExecutionHelper.Messages.SaveFile))
                {
                    string dataToSave = JsonSerializer.Serialize<Dictionary<string, object>>(m_variables);

                    var data = JsonSerializer.Deserialize<StepBro.ExecutionHelper.Messages.SaveFile>(received.Item2);
                    if (data != null)
                    {
                        string fileName = data.FileName;
                        if (File.Exists(fileName))
                        {
                            if (File.Exists("backup_" + fileName))
                            {
                                File.Delete("backup_" + fileName);
                            }

                            // We rename the old file to a backup in case we crash during writing
                            // the new file or in case we accidentally save two files with the same name
                            File.Move(fileName, "backup_" + fileName);
                        }

                        using (FileStream fs = File.Create(fileName))
                        {
                            var dataInFile = new UTF8Encoding(true).GetBytes(dataToSave);
                            fs.Write(dataInFile, 0, dataInFile.Length);
                        }

                        m_pipe.Send(ShortCommand.Acknowledge);
                    }
                }
                else if (received.Item1 == nameof(StepBro.ExecutionHelper.Messages.LoadFile))
                {
                    var data = JsonSerializer.Deserialize<StepBro.ExecutionHelper.Messages.LoadFile>(received.Item2);
                    if (data != null)
                    {
                        string fileName = data.FileName;
                        string loadedData = "";

                        using (FileStream fs = File.Open(fileName, FileMode.Open, FileAccess.Read))
                        {
                            byte[] b = new byte[1024];
                            UTF8Encoding temp = new UTF8Encoding(true);

                            while (fs.Read(b, 0, b.Length) > 0)
                            {
                                loadedData += temp.GetString(b);
                            }

                            int firstIndexOfNull = loadedData.IndexOf('\0');
                            loadedData = loadedData.Substring(0, firstIndexOfNull);
                        }

                        if (!String.IsNullOrEmpty(loadedData))
                        {
                            Dictionary<string, object>? loadedVariables = JsonSerializer.Deserialize<Dictionary<string, object>>(loadedData);
                            if (loadedVariables != null)
                            {
                                m_variables = loadedVariables;
                            }
                        }

                        m_pipe.Send(ShortCommand.Acknowledge);
                    }
                }
            }
        }
    }
}
