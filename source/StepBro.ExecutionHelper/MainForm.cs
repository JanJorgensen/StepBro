using StepBro.Core.IPC;
using StepBro.ExecutionHelper.Messages;
using System.Text.Json;

namespace StepBro.ExecutionHelper
{
    public partial class MainForm : Form
    {
        private nint m_consoleWindow = 0;
        private Pipe m_pipe = null;
        private bool m_closeRequested = false;
        private int m_testCounter = 0;

        public MainForm()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            // m_topControl = this.TopLevelControl;

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
            // m_forceResize = true;
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
                    else if (cmd == ShortCommand.IncrementTestCounter)
                    {
                        m_testCounter++;
                        textBoxTestCounter.Text = m_testCounter.ToString();
                    }
                    else if (cmd == ShortCommand.GetTestCounter)
                    {
                        m_pipe.Send(new SendTestCounter(m_testCounter));
                    }
                }
            }
        }
    }
}
