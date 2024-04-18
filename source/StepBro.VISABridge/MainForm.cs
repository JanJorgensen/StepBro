﻿using NationalInstruments.Visa;
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

                            break;
                        case ShortCommand.GetInstrumentList:

                            break;
                        case ShortCommand.SessionClosed:

                            break;
                        case ShortCommand.Receive:

                            break;
                    }
                    break;
                case nameof(OpenSession):

                    break;
                case nameof(CloseSession):

                    break;
                case nameof(ConnectedInstruments):

                    break;
                case nameof(Received):

                    break;
                case nameof(Send):

                    break;
                case nameof(SessionOpened):

                    break;
            }
        }

        public MainForm()
        {
            InitializeComponent();
            SetupControlState();
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
