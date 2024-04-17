using StepBro.Core.IPC;
using StepBro.ExecutionHelper.Messages;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

namespace StepBro.ExecutionHelper
{
    public partial class MainForm : Form
    {
        private Pipe? m_pipe = null;
        FormClosingEventHandler? formCloseEventHandler = null;
        private bool m_closeRequested = false;
        private Dictionary<string, object> m_variables = new Dictionary<string, object>();
        EventHandler<Tuple<string, string>>? receivedDataEventHandler = null;

        public MainForm()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            System.Diagnostics.Trace.WriteLine("Execution Helper STARTING!!");

            m_pipe = Pipe.StartServer("StepBroExecutionHelper", "1998");

            formCloseEventHandler = (sender, e) =>
            {
                if (m_pipe.IsConnected() && !m_closeRequested)
                {
                    m_pipe.Send(StepBro.ExecutionHelper.Messages.ShortCommand.CloseApplication);
                    Thread.Sleep(1000);
                }
            };

            FormClosing += formCloseEventHandler;

            receivedDataEventHandler = (sender, e) =>
            {
                ReceivedData(e);
            };

            Pipe.ReceivedData += receivedDataEventHandler;

            RunOnStartup();
        }

        private void RunOnStartup()
        {
            string fileName = "CommandToRunOnStartup.sbd";
            string loadedData = "";

            if (File.Exists(fileName))
            {
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
                    // TODO: Run the command - Remember to do sanity checking, possibly by deserializing into an object that has the specific parameters we look for, i.e. filename, testlist, model, print_report etc.
                }
            }
        }

        private void ReceivedData(Tuple<string, string> received)
        {
            if (received.Item1 == nameof(ShortCommand))
            {
                var cmd = JsonSerializer.Deserialize<ShortCommand>(received.Item2);
                if (cmd == ShortCommand.CloseApplication)
                {
                    m_closeRequested = true;
                    m_pipe!.Send(cmd);
                    Pipe.ReceivedData -= receivedDataEventHandler;
                    this.Close();
                }
            }
            else if (received.Item1 == nameof(StepBro.ExecutionHelper.Messages.CreateOrSetVariable))
            {
                var data = JsonSerializer.Deserialize<StepBro.ExecutionHelper.Messages.CreateOrSetVariable>(received.Item2);
                if (data != null)
                {
                    bool alreadyExists = m_variables.ContainsKey(data.VariableName);
                    bool isNumberKind = data.Value is System.Text.Json.JsonElement v && v.ValueKind == JsonValueKind.Number;
                    if (alreadyExists)
                    {
                        m_variables[data.VariableName] = data.Value;
                    }
                    else if (isNumberKind)
                    {
                        // It is some sort of number, so we save it as a long like StepBro uses
                        long value = 0;
                        Int64.TryParse(data.Value.ToString(), out value);
                        m_variables.TryAdd(data.VariableName, value);
                    }
                    else
                    {
                        // It is not a number so we assume the user knows what they are doing
                        m_variables.TryAdd(data.VariableName, data.Value);
                    }

                    m_pipe!.Send(ShortCommand.Acknowledge);
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
                        m_pipe!.Send(ShortCommand.Acknowledge);
                    }
                }
            }
            else if (received.Item1 == nameof(StepBro.ExecutionHelper.Messages.GetVariable))
            {
                var data = JsonSerializer.Deserialize<StepBro.ExecutionHelper.Messages.GetVariable>(received.Item2);
                if (data != null)
                {
                    m_pipe!.Send(new SendVariable(data.VariableName, m_variables[data.VariableName]));
                }
            }
            else if (received.Item1 == nameof(StepBro.ExecutionHelper.Messages.SaveFile))
            {
                string dataToSave = JsonSerializer.Serialize<Dictionary<string, object>>(m_variables);

                var data = JsonSerializer.Deserialize<StepBro.ExecutionHelper.Messages.SaveFile>(received.Item2);
                if (data != null)
                {
                    string fileName = data.FileName;
                    SaveFile(fileName, dataToSave);

                    m_pipe!.Send(ShortCommand.Acknowledge);
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

                    m_pipe!.Send(ShortCommand.Acknowledge);
                }
            }
            else if (received.Item1 == nameof(StepBro.ExecutionHelper.Messages.SetCommandRunOnStartup))
            {
                var data = JsonSerializer.Deserialize<StepBro.ExecutionHelper.Messages.SetCommandRunOnStartup>(received.Item2);
                if (data != null)
                {
                    string dataToSave = JsonSerializer.Serialize<string>(data.Command);
                    string fileName = "CommandToRunOnStartup.sbd";
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
                }
            }
        }

        private void SaveFile(string fileName, string dataToSave)
        {
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
        }
    }
}
