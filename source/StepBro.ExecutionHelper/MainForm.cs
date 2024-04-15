using StepBro.Core.IPC;
using StepBro.ExecutionHelper.Messages;
using System.Text.Json;

namespace StepBro.ExecutionHelper
{
    public partial class MainForm : Form
    {
        private nint m_consoleWindow = 0;
        private Pipe? m_pipe = null;
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
                    }
                }
                else if (received.Item1 == nameof(StepBro.ExecutionHelper.Messages.IncrementVariable))
                {
                    var data = JsonSerializer.Deserialize<StepBro.ExecutionHelper.Messages.IncrementVariable>(received.Item2);
                    if (data != null)
                    {
                        if (m_variables[data.VariableName] is long v)
                        {
                            m_variables[data.VariableName] = ++v;
                        }
                    }
                }
                else if (received.Item1 == nameof(StepBro.ExecutionHelper.Messages.SetVariable))
                {
                    var data = JsonSerializer.Deserialize<StepBro.ExecutionHelper.Messages.SetVariable>(received.Item2);
                    m_variables[data.VariableName] = data.Value;
                }
                else if (received.Item1 == nameof(StepBro.ExecutionHelper.Messages.GetVariable))
                {
                    var data = JsonSerializer.Deserialize<StepBro.ExecutionHelper.Messages.GetVariable>(received.Item2);
                    if (data != null)
                    {
                        m_pipe.Send(new SendVariable(data.VariableName, m_variables[data.VariableName]));
                    }
                }
            }
        }
    }
}
