using NationalInstruments.Visa;
using StepBro.Core.IPC;
using StepBro.VISABridge.Messages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace StepBro.VISABridge
{
    public partial class MainForm : Form
    {
        private string m_lastResourceString = "";
        private MessageBasedSession m_session = null;
        private Pipe m_pipe = null;

        private void ReceivedData(Tuple<string, string> received)
        {
            switch(received.Item1)
            {
                case nameof(ShortCommand):
                    switch(JsonSerializer.Deserialize<ShortCommand>(received.Item2))
                    {
                        case ShortCommand.None:
                            // Should not happen
                            break;
                        case ShortCommand.GetInstrumentList:
                            // TODO: Handle Get Instrument List

                            break;
                        case ShortCommand.SessionClosed:
                            // Should not happen
                            break;
                        case ShortCommand.Receive:
                            byte[] readData = m_session.RawIO.Read();
                            m_pipe.Send(new Received(new UTF8Encoding(true).GetString(readData)));
                            break;
                    }
                    break;
                case nameof(OpenSession):
                    var openSessionData = JsonSerializer.Deserialize<OpenSession>(received.Item2);
                    if (m_lastResourceString != null)
                    {
                        m_lastResourceString = openSessionData.Resource;
                        Open();
                    }
                    break;
                case nameof(CloseSession):
                    var closeSessionData = JsonSerializer.Deserialize<CloseSession>(received.Item2);
                    if (m_lastResourceString.Equals(closeSessionData.Resource))
                    {
                        m_session.Dispose();
                        m_session = null;
                        m_lastResourceString = null;
                    }
                    break;
                case nameof(ConnectedInstruments):
                    // Should not happen
                    break;
                case nameof(Received):
                    // Should not happen
                    break;
                case nameof(Send):
                    var sendData = JsonSerializer.Deserialize<Send>(received.Item2);
                    m_session.RawIO.Write(sendData.Request);
                    break;
                case nameof(SessionOpened):
                    // Should not happen
                    break;
            }
        }

        public MainForm()
        {
            InitializeComponent();
            SetupControlState();
        }

        private void Open()
        {
            using (SelectResource sr = new SelectResource())
            {
                if (m_lastResourceString != null)
                {
                    sr.ResourceName = m_lastResourceString;
                }
                m_lastResourceString = sr.ResourceName;
                using (var rmSession = new ResourceManager())
                {
                    m_session = (MessageBasedSession)rmSession.Open(sr.ResourceName);
                }
            }
        }

        private void SetupControlState()
        {
            bool isSessionOpen = (m_session != null);
            bool isSessionOpenedAndNotAutomated = isSessionOpen && !checkBoxAutomated.Checked;
            buttonOpenSession.Enabled = !isSessionOpen && !checkBoxAutomated.Checked;
            buttonCloseSession.Enabled = isSessionOpenedAndNotAutomated;
            buttonQuery.Enabled = isSessionOpenedAndNotAutomated;
            buttonWrite.Enabled = isSessionOpenedAndNotAutomated;
            buttonRead.Enabled = isSessionOpenedAndNotAutomated;
            textBoxCommand.Enabled = isSessionOpenedAndNotAutomated;
            buttonClearHistory.Enabled = isSessionOpenedAndNotAutomated;
            if (isSessionOpenedAndNotAutomated)
            {
                //richTextBoxHistory.Text = String.Empty;
                textBoxCommand.Focus();
            }
        }
        private void buttonOpenSession_Click(object sender, EventArgs e)
        {
            using (SelectResource sr = new SelectResource())
            {
                if (m_lastResourceString != null)
                {
                    sr.ResourceName = m_lastResourceString;
                }
                DialogResult result = sr.ShowDialog(this);
                if (result == DialogResult.OK)
                {
                    m_lastResourceString = sr.ResourceName;
                    Cursor.Current = Cursors.WaitCursor;
                    using (var rmSession = new ResourceManager())
                    {
                        try
                        {
                            m_session = (MessageBasedSession)rmSession.Open(sr.ResourceName);
                            //SetupControlState(true);
                        }
                        catch (InvalidCastException)
                        {
                            MessageBox.Show("Resource selected must be a message-based session");
                        }
                        catch (Exception exp)
                        {
                            MessageBox.Show(exp.Message);
                        }
                        finally
                        {
                            Cursor.Current = Cursors.Default;
                        }
                    }
                }
                SetupControlState();
            }
        }

        private void buttonCloseSession_Click(object sender, EventArgs e)
        {
            m_session.Dispose();
            m_session = null;
            SetupControlState();
        }

        private string ReplaceCommonEscapeSequences(string s)
        {
            return s.Replace("\\n", "\n").Replace("\\r", "\r");
        }

        private string InsertCommonEscapeSequences(string s)
        {
            return s.Replace("\n", "\\n").Replace("\r", "\\r");
        }

        private void buttonQuery_Click(object sender, EventArgs e)
        {
            //readTextBox.Text = String.Empty;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                richTextBoxHistory.AppendText("> " + textBoxCommand.Text + Environment.NewLine);
                string textToWrite = textBoxCommand.Text + "\n";
                m_session.RawIO.Write(textToWrite);
                var response = m_session.RawIO.ReadString().Replace("\r", "").Replace("\n", "");
                richTextBoxHistory.AppendText("-   " + response + Environment.NewLine);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void buttonWrite_Click(object sender, EventArgs e)
        {
            try
            {
                richTextBoxHistory.AppendText("> " + textBoxCommand.Text + Environment.NewLine);
                string textToWrite = textBoxCommand.Text + "\n";
                m_session.RawIO.Write(textToWrite);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
            }
        }

        private void buttonRead_Click(object sender, EventArgs e)
        {
            //readTextBox.Text = String.Empty;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                var response = m_session.RawIO.ReadString().Replace("\r", "").Replace("\n", "");
                richTextBoxHistory.AppendText("-   " + response + Environment.NewLine);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            string[] args = Environment.GetCommandLineArgs();

            m_pipe = Pipe.StartClient("StepBroVisaPipe", "1234");

            m_pipe.ReceivedData += (_, eventArgs) =>
            {
                ReceivedData(eventArgs);
            };

            if (args.Length == 2)
            {
                if (args[1] == "--automate")
                {
                    checkBoxAutomated.Checked = true;
                    this.WindowState = FormWindowState.Minimized;
                }
            }
            else
            {
            }
            SetupControlState();
        }
    }
}
