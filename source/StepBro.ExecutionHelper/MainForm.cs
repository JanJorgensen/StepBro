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
        private bool m_closeRequested = false;
        private Dictionary<string, object> m_variables = new Dictionary<string, object>();
        private bool m_shouldAutoSave = true;
        private const string m_executionHelperDataFolder = "ExecutionHelperDataFolder\\";
        private const string m_commandToRunFileName = "CommandToRun.sbd";
        private const string m_logFileName = "ExecutionHelperLog";
        private string m_logData = "";
        private readonly object m_logLock = new object();

        public MainForm()
        {
            InitializeComponent();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (m_pipe != null)
            {
                if (m_pipe.IsConnected() && !m_closeRequested)
                {
                    m_pipe.Send(StepBro.ExecutionHelper.Messages.ShortCommand.CloseApplication);
                    Thread.Sleep(1000);
                }

                m_pipe.Dispose();
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            System.Diagnostics.Trace.WriteLine("Execution Helper STARTING!!");

            System.IO.Directory.CreateDirectory(m_executionHelperDataFolder);

            m_pipe = Pipe.StartServer("StepBroExecutionHelper", null);

            m_pipe.ReceivedData += (sender, e) =>
            {
                ReceivedData(e);
            };

            string[] args = Environment.GetCommandLineArgs();

            if (!(args.Length == 2 && (args[1] == "-drc" || args[1] == "--dont_run_command")))
            {
                RunCommandSet();
            }
        }

        private void RunCommandSet()
        {
            string fileName = m_executionHelperDataFolder + m_commandToRunFileName;
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
                    AddToLogData($"RunCommandSet: {loadedData}");
                    // TODO: Run the command - Remember to do sanity checking, possibly by deserializing into an object that has the specific parameters we look for, i.e. filename, testlist, model, print_report etc.
                    // System.Diagnostics.Process.Start("CMD.exe", "/C " + loadedData);
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
                    AddToLogData("Request: Close");
                    m_closeRequested = true;
                    this.BeginInvoke((MethodInvoker)delegate
                    {
                        // close the form on the forms thread
                        this.Close();
                    });
                }
                else if (cmd == ShortCommand.SuspendAutosave)
                {
                    AddToLogData("Request: Suspend Autosave");
                    m_shouldAutoSave = false;
                }
                else if (cmd == ShortCommand.ResumeAutosave)
                {
                    AddToLogData("Request: Resume Autosave");
                    m_shouldAutoSave = true;
                }
                else if (cmd == ShortCommand.RunPeriodicCheck)
                {
                    // Wait for the StepBro script to finish so we can run checks and restart it safely
                    while (System.Diagnostics.Process.GetProcessesByName("stepbro").Length != 0)
                    {
                        Thread.Sleep(1); // Wait for 1 ms and check again
                    }

                    // TODO: Check if there is a windows update
                    // TODO: If there is a windows update, temporarily add ExecutionHelper to run on startup
                    //       of windows, in the current folder, so we can restart the stepbro script after
                    //       a restart.

                    // Run the cmd set with "CommandToRun"
                    RunCommandSet();
                }
            }
            else if (received.Item1 == nameof(StepBro.ExecutionHelper.Messages.CreateOrSetVariable))
            {
                var data = JsonSerializer.Deserialize<StepBro.ExecutionHelper.Messages.CreateOrSetVariable>(received.Item2);
                if (data != null)
                {
                    bool alreadyExists = m_variables.ContainsKey(data.VariableName);
                    bool isNumberKind = data.Value is System.Text.Json.JsonElement v && v.ValueKind == JsonValueKind.Number;
                    if (alreadyExists && isNumberKind)
                    {
                        AddToLogData($"Request: Set variable: {data.VariableName} to: {data.Value}");
                        long value = 0;
                        Int64.TryParse(data.Value.ToString(), out value);
                        m_variables[data.VariableName] = value;
                    }
                    else if (alreadyExists)
                    {
                        AddToLogData($"Request: Set variable: {data.VariableName} to: {data.Value}");
                        m_variables[data.VariableName] = data.Value;
                    }
                    else if (isNumberKind)
                    {
                        AddToLogData($"Request: Create variable: {data.VariableName} with value: {data.Value}");
                        // It is some sort of number, so we save it as a long like StepBro uses
                        long value = 0;
                        Int64.TryParse(data.Value.ToString(), out value);
                        m_variables.TryAdd(data.VariableName, value);
                    }
                    else
                    {
                        AddToLogData($"Request: Create variable: {data.VariableName} with value: {data.Value}");
                        // It is not a number so we assume the user knows what they are doing
                        m_variables.TryAdd(data.VariableName, data.Value);
                    }

                    m_pipe!.Send(ShortCommand.Acknowledge);
                }
                else
                {
                    AddToLogData($"Request: Create or set variable FAILED, data is null");
                    m_pipe!.Send(new Error("Data in CreateOrSetVariable is null."));
                }
            }
            else if (received.Item1 == nameof(StepBro.ExecutionHelper.Messages.IncrementVariable))
            {
                var data = JsonSerializer.Deserialize<StepBro.ExecutionHelper.Messages.IncrementVariable>(received.Item2);
                if (data != null && m_variables.ContainsKey(data.VariableName))
                {
                    bool isNumberKind = m_variables[data.VariableName] is System.Text.Json.JsonElement j && j.ValueKind == JsonValueKind.Number;

                    if (isNumberKind)
                    {
                        long value = 0;
                        Int64.TryParse(m_variables[data.VariableName].ToString(), out value);
                        m_variables[data.VariableName] = value;
                    }

                    if (m_variables[data.VariableName] is long v)
                    {
                        m_variables[data.VariableName] = ++v;
                        AddToLogData($"Request: Increment: {data.VariableName} to: {m_variables[data.VariableName]}");
                        m_pipe!.Send(ShortCommand.Acknowledge);
                    }
                    else
                    {
                        AddToLogData($"Request: Increment: {data.VariableName} FAILED, {data.VariableName} is not numeric");
                        m_pipe!.Send(new Error($"{data.VariableName} is not a numeric type."));
                    }
                }
                else if (data != null && !m_variables.ContainsKey(data.VariableName))
                {
                    AddToLogData($"Request: Increment: {data.VariableName} FAILED, {data.VariableName} is unknown");
                    m_pipe!.Send(new Error("Variable is unknown in IncrementVariable."));
                }
                else
                {
                    AddToLogData($"Request: Increment FAILED, data is null");
                    m_pipe!.Send(new Error("Data in IncrementVariable is null."));
                }
            }
            else if (received.Item1 == nameof(StepBro.ExecutionHelper.Messages.GetVariable))
            {
                var data = JsonSerializer.Deserialize<StepBro.ExecutionHelper.Messages.GetVariable>(received.Item2);
                if (data != null && m_variables.ContainsKey(data.VariableName))
                {
                    AddToLogData($"Request: Get variable {data.VariableName}");
                    m_pipe!.Send(new SendVariable(data.VariableName, m_variables[data.VariableName]));
                }
                else if (data != null && !m_variables.ContainsKey(data.VariableName))
                {
                    AddToLogData($"Request: Get variable {data.VariableName} FAILED, {data.VariableName} is unknown");
                    m_pipe!.Send(new Error("Variable is unknown in IncrementVariable."));
                }
                else
                {
                    AddToLogData($"Request: Get variable FAILED, data is null");
                    m_pipe!.Send(new Error("Data in IncrementVariable is null."));
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
                else
                {
                    AddToLogData($"Request: Save file FAILED, data is null");
                    m_pipe!.Send(new Error("Data in SaveFile is null."));
                }
            }
            else if (received.Item1 == nameof(StepBro.ExecutionHelper.Messages.LoadFile))
            {
                var data = JsonSerializer.Deserialize<StepBro.ExecutionHelper.Messages.LoadFile>(received.Item2);
                if (data != null)
                {
                    string fileName = data.FileName;
                    string loadedData = "";
                    AddToLogData($"Request: Load {fileName}");

                    try
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
                            Dictionary<string, object>? loadedVariables = JsonSerializer.Deserialize<Dictionary<string, object>>(loadedData);
                            if (loadedVariables != null)
                            {
                                AddToLogData($"Request: Load {fileName}");
                                m_variables = loadedVariables;
                            }
                        }

                        m_pipe!.Send(ShortCommand.Acknowledge);
                    }
                    catch (FileNotFoundException)
                    {
                        m_pipe!.Send(new Error($"File with name: {fileName} not found!"));
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
                else
                {
                    AddToLogData($"Request: Load file FAILED, data is null");
                    m_pipe!.Send(new Error("Data in LoadFile is null."));
                }
            }
            else if (received.Item1 == nameof(StepBro.ExecutionHelper.Messages.SetCommandToRun))
            {
                var data = JsonSerializer.Deserialize<StepBro.ExecutionHelper.Messages.SetCommandToRun>(received.Item2);
                if (data != null)
                {
                    string dataToSave = JsonSerializer.Serialize<string>(data.Command);
                    string backupFileName = m_executionHelperDataFolder + "backup_" + m_commandToRunFileName;
                    string fileName = m_executionHelperDataFolder + m_commandToRunFileName;
                    if (File.Exists(fileName))
                    {
                        if (File.Exists(backupFileName))
                        {
                            File.Delete(backupFileName);
                        }

                        // We rename the old file to a backup in case we crash during writing
                        File.Move(fileName, backupFileName);
                    }

                    using (FileStream fs = File.Create(fileName))
                    {
                        var dataInFile = new UTF8Encoding(true).GetBytes(dataToSave.Trim('\"'));
                        fs.Write(dataInFile, 0, dataInFile.Length);
                    }
                    AddToLogData($"Request: Set command to run to {data.Command}");
                    m_pipe!.Send(ShortCommand.Acknowledge);
                }
                else
                {
                    AddToLogData($"Request: Set command to run FAILED, data is null");
                    m_pipe!.Send(new Error("Data in SetCommandToRun is null."));
                }
            }
        }

        private void SaveFile(string fileName, string dataToSave, bool shouldAppend = false)
        {
            string[] pathIncludingFile = fileName.Split(["/", "\\"], StringSplitOptions.None);
            string pathToFile = String.Join(String.Empty, pathIncludingFile.Take(pathIncludingFile.Length - 1));

            System.IO.Directory.CreateDirectory(pathToFile);

            string fileNameWithoutPath = pathIncludingFile[^1];
            if (!shouldAppend)
            {
                if (File.Exists(fileName))
                {
                    if (File.Exists(pathToFile + "backup_" + fileNameWithoutPath))
                    {
                        File.Delete(pathToFile + "backup_" + fileNameWithoutPath);
                    }

                    // We rename the old file to a backup in case we crash during writing
                    // the new file or in case we accidentally save two files with the same name
                    File.Move(fileName, pathToFile + "backup_" + fileNameWithoutPath);
                }
            }

            if (!File.Exists(fileName) || !shouldAppend)
            {
                using (FileStream fs = File.Create(fileName))
                {
                    var dataInFile = new UTF8Encoding(true).GetBytes(dataToSave);
                    fs.Write(dataInFile, 0, dataInFile.Length);
                }
            }
            else if (shouldAppend)
            {
                using (StreamWriter fs = File.AppendText(fileName))
                {
                    var dataInFile = new UTF8Encoding(true).GetBytes(dataToSave);
                    fs.Write(dataToSave);
                }
            }

            AddToLogData($"Request: Saved File: {fileName}");
        }

        private void SaveTimer_Tick(object sender, EventArgs e)
        {
            if (m_shouldAutoSave)
            {
                string dataToSave = JsonSerializer.Serialize<Dictionary<string, object>>(m_variables);
                // Use a new save file name every hour, utilizing the functionality in SaveFile to overwrite
                // existing filename
                SaveFile(m_executionHelperDataFolder + "Autosave" + DateTime.Now.ToString("yyyy-MM-dd-HH") + ".sbd", dataToSave);
            }

            string localLogData = "";
            lock(m_logLock)
            {
                localLogData = m_logData;
                m_logData = "";
            }
            SaveFile(m_executionHelperDataFolder + m_logFileName + DateTime.Now.ToString("yyyy-MM-dd-HH") + ".sbd", localLogData, true);
        }

        private void AddToLogData(string data)
        {
            lock(m_logLock)
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                m_logData += $"{timestamp} - {data}\n";
            }
        }
    }
}
