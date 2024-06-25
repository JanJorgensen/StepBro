using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Execution;
using StepBro.Core.IPC;
using System;
using System.IO;
using System.Reflection;
using System.Threading;

namespace StepBro.VISA
{
    /// <summary>
    /// Class to connect to an instrument using VISA
    /// This is used to bind the StepBro script to an instrument utilizing the VISA protocol.
    /// <seealso cref="https://www.ni.com//visa/default.htm" />
    /// </summary>
    public class Instrument : INameable, INamedObject, IDisposable
    {
        /// <summary>
        /// The full name resource we are communicating with.
        /// </summary>
        private string m_resource = "";
        /// <summary>
        /// The name of this object.
        /// </summary>
        private string m_name = "instrument";
        /// <summary>
        /// The pipe we use to communicate with VISA.
        /// </summary>
        private static Pipe m_visaPipe = null;
        /// <summary>
        /// Whether the session is currently open.
        /// </summary>
        private static bool m_sessionOpened = false;
        /// <summary>
        /// Event handler for when VISA closes.
        /// </summary>
        private EventHandler m_visaClosedEventHandler = null;

        /// <summary>
        /// Event handler for when data is received over m_visaPipe.
        /// </summary>
        /// <param name="received">The data received</param>
        private void ReceivedData(Tuple<string, string> received)
        {
            switch (received.Item1)
            {
                case nameof(VISABridge.Messages.ShortCommand):
                    switch (System.Text.Json.JsonSerializer.Deserialize<VISABridge.Messages.ShortCommand>(received.Item2))
                    {
                        case VISABridge.Messages.ShortCommand.None:
                            // Should not happen
                            break;
                        case VISABridge.Messages.ShortCommand.GetInstrumentList:
                            // Should not happen
                            break;
                        case VISABridge.Messages.ShortCommand.SessionClosed:
                            // Handled elsewhere
                            break;
                        case VISABridge.Messages.ShortCommand.Receive:
                            // Should not happen
                            break;
                    }
                    break;
                case nameof(VISABridge.Messages.OpenSession):
                    // Should not happen
                    break;
                case nameof(VISABridge.Messages.CloseSession):
                    // Should not happen
                    break;
                case nameof(VISABridge.Messages.ConnectedInstruments):
                    // Handled elsewhere
                    break;
                case nameof(VISABridge.Messages.Received):
                    // Handled elsewhere
                    break;
                case nameof(VISABridge.Messages.Send):
                    // Should not happen
                    break;
                case nameof(VISABridge.Messages.SessionOpened):
                    // Handled elsewhere
                    break;
            }
        }

        /// <summary>
        /// The resource we are communicating with.
        /// </summary>
        public string Resource
        {
            get { return m_resource; }
            set { m_resource = value; }
        }

        /// <summary>
        /// The name of this object.
        /// </summary>
        public string Name { get { return m_name; } set { m_name = value; } }

        /// <summary>
        /// The short name of this object.
        /// </summary>
        public string ShortName { get { return this.Name; } }

        /// <summary>
        /// The full name of this object.
        /// </summary>
        public string FullName { get { return this.Name; } }



        /// <summary>
        /// The max time that may elapse between asking VISA for some information and until VISA sends the information to us.
        /// </summary>
        public TimeSpan ReadTimeout { get; set; } = TimeSpan.FromMilliseconds(2500);

        /// <summary>
        /// Open the pipe and connection to a resource.
        /// </summary>
        /// <param name="context">Context variable</param>
        /// <param name="timeout">Max timeout</param>
        /// <returns>True if connection was opened within the timeout limit</returns>
        public bool Open([Implicit] ICallContext context = null, TimeSpan timeout = new TimeSpan())
        {
            if (timeout.Equals(new TimeSpan()))
            {
                timeout = ReadTimeout;
            }

            string path = Assembly.GetExecutingAssembly().Location;
            var folder = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(path), ".."));

            bool started = true;
            if (System.Diagnostics.Process.GetProcessesByName("StepBro.VISABridge").Length == 0)
            {
                var bridge = new System.Diagnostics.Process();
                bridge.StartInfo.FileName = Path.Combine(folder, "StepBro.VISABridge.exe");
                bridge.StartInfo.Arguments = "--automate";
                started = bridge.Start();
            }

            DateTime start = DateTime.Now;
            if (m_visaPipe == null || !m_visaPipe.IsConnected())
            {
                m_visaPipe = Pipe.StartClient("StepBroVisaPipe", null);

                m_visaPipe.ReceivedData += (sender, e) =>
                {
                    ReceivedData(e);
                };

                m_visaClosedEventHandler = (sender, e) =>
                {
                    context.Logger.LogError("VISA closed unexpectedly");
                };

                m_visaPipe.OnConnectionClosed += m_visaClosedEventHandler;

                if (started)
                {
                    while (!m_visaPipe.IsConnected() && (DateTime.Now - start) < timeout)
                    {
                        int waitTimeMs = 200;
                        Thread.Sleep(waitTimeMs);
                    }
                    started = m_visaPipe.IsConnected();
                }
            }

            m_visaPipe.Send(new VISABridge.Messages.OpenSession(m_resource));

            Tuple<string, string> input = null;
            do
            {
                input = m_visaPipe.TryGetReceived();
                if (input != null)
                {
                    break;
                }
                // Wait
                Thread.Sleep(1);
            } while ((DateTime.Now - start) < timeout);

            started = m_visaPipe.IsConnected();

            if (input != null && input.Item1 != nameof(VISABridge.Messages.SessionOpened))
            {
                context.ReportError("Received different message than SessionOpened.");
            }
            else
            {
                m_sessionOpened = true;
            }

            return started;
        }

        /// <summary>
        /// Close a session.
        /// </summary>
        /// <param name="context">Context variable</param>
        /// <param name="timeout">Max timeout</param>
        public void Close([Implicit] ICallContext context = null, TimeSpan timeout = new TimeSpan())
        {
            if (timeout.Equals(new TimeSpan()))
            {
                timeout = ReadTimeout;
            }

            m_visaPipe.Send(new VISABridge.Messages.CloseSession(m_resource));

            Tuple<string, string> input = null;
            DateTime start = DateTime.Now;
            do
            {
                input = m_visaPipe.TryGetReceived();
                if (input != null)
                {
                    break;
                }
                // Wait
                Thread.Sleep(1);
            } while ((DateTime.Now - start) < timeout);

            if (input != null && input.Item1 == nameof(VISABridge.Messages.ShortCommand))
            {
                var cmd = System.Text.Json.JsonSerializer.Deserialize<VISABridge.Messages.ShortCommand>(input.Item2);
                if (cmd != VISABridge.Messages.ShortCommand.SessionClosed)
                {
                    context.ReportError("Received different message than SessionClosed.");
                }
                else
                {
                    m_sessionOpened = false;
                }
            }
            else
            {
                context.ReportError("Received different message than SessionClosed.");
            }
        }

        /// <summary>
        /// Query a command to the VISA resource.
        /// </summary>
        /// <param name="context">Context variable</param>
        /// <param name="command">Command to query</param>
        /// <param name="timeout">Max timeout</param>
        /// <returns>The result of the query</returns>
        public string Query([Implicit] ICallContext context, string command, TimeSpan timeout = new TimeSpan())
        {
            if (timeout.Equals(new TimeSpan()))
            {
                timeout = ReadTimeout;
            }

            string received = null;
            if (m_sessionOpened)
            {
                m_visaPipe.Send(new VISABridge.Messages.Send(command));
                m_visaPipe.Send(VISABridge.Messages.ShortCommand.Receive);

                Tuple<string, string> input = null;
                DateTime start = DateTime.Now;
                do
                {
                    input = m_visaPipe.TryGetReceived();

                    // If we have received an answer that is not empty we break
                    if (input != null &&
                        (!String.IsNullOrEmpty(System.Text.Json.JsonSerializer.Deserialize<VISABridge.Messages.Received>(input.Item2).Line) ||
                        input.Item1 != nameof(VISABridge.Messages.Received)))
                    {
                        break;
                    }
                    else if (input != null)
                    {
                        // If we received an empty answer, we try to get a new answer
                        m_visaPipe.Send(VISABridge.Messages.ShortCommand.Receive);
                    }

                    // Wait
                    Thread.Sleep(1);
                } while ((DateTime.Now - start) < timeout);

                if (input != null && input.Item1 == nameof(VISABridge.Messages.Received))
                {   
                    var data = System.Text.Json.JsonSerializer.Deserialize<VISABridge.Messages.Received>(input.Item2);
                    received = data.Line.TrimEnd('\n','\r',' ');
                }
                else if (input == null)
                {
                    received = "Nothing received.";
                }
            }
            else
            {
                context.ReportError("Session is not open.");
            }

            return received;
        }

        /// <summary>
        /// Write a command to the VISA resource.
        /// Will not automatically return the answer.
        /// Use the <see cref="Read"/> method to get the answer.
        /// </summary>
        /// <param name="context">Context variable</param>
        /// <param name="command">The command to send to the VISA resrouce</param>
        public void Write([Implicit] ICallContext context, string command)
        {
            if (m_sessionOpened)
            {
                m_visaPipe.Send(new VISABridge.Messages.Send(command));
            }
            else
            {
                context.ReportError("Session is not open.");
            }
        }

        /// <summary>
        /// Reads an answer from the VISA resource.
        /// </summary>
        /// <param name="context">Context variable</param>
        /// <param name="timeout">Max timeout</param>
        /// <returns>The answer from the VISA resource.</returns>
        public string Read([Implicit] ICallContext context, TimeSpan timeout = new TimeSpan())
        {
            if (timeout.Equals(new TimeSpan()))
            {
                timeout = ReadTimeout;
            }

            string received = null;
            if (m_sessionOpened)
            {
                m_visaPipe.Send(VISABridge.Messages.ShortCommand.Receive);

                Tuple<string, string> input = null;
                DateTime start = DateTime.Now;
                do
                {
                    input = m_visaPipe.TryGetReceived();
                    if (input != null && 
                        (!String.IsNullOrEmpty(System.Text.Json.JsonSerializer.Deserialize<VISABridge.Messages.Received>(input.Item2).Line) || 
                        input.Item1 != nameof(VISABridge.Messages.Received)))
                    {
                        break;
                    }
                    // Wait
                    Thread.Sleep(1);
                } while ((DateTime.Now - start) < timeout);

                if (input != null && input.Item1 == nameof(VISABridge.Messages.Received))
                {
                    var data = System.Text.Json.JsonSerializer.Deserialize<VISABridge.Messages.Received>(input.Item2);
                    received = data.Line.TrimEnd('\n', '\r', ' ');
                }
            }
            else
            {
                context.ReportError("Session is not open.");
            }

            return received;
        }

        /// <summary>
        /// Reads a single line in the data the resource is trying to send us.
        /// </summary>
        /// <param name="context">Context variable</param>
        /// <param name="timeout">Max timeout</param>
        /// <returns>A line of the data the resource is trying to send us</returns>
        public string ReadLine([Implicit] ICallContext context, TimeSpan timeout = new TimeSpan())
        {
            if (timeout.Equals(new TimeSpan()))
            {
                timeout = ReadTimeout;
            }

            string received = null;
            if (m_sessionOpened)
            {
                m_visaPipe.Send(VISABridge.Messages.ShortCommand.ReadLine);

                Tuple<string, string> input = null;
                DateTime start = DateTime.Now;
                do
                {
                    input = m_visaPipe.TryGetReceived();
                    if (input != null &&
                        (!String.IsNullOrEmpty(System.Text.Json.JsonSerializer.Deserialize<VISABridge.Messages.Received>(input.Item2).Line) ||
                        input.Item1 != nameof(VISABridge.Messages.Received)))
                    {
                        break;
                    }
                    // Wait
                    Thread.Sleep(1);
                } while ((DateTime.Now - start) < timeout);

                if (input != null && input.Item1 == nameof(VISABridge.Messages.Received))
                {
                    var data = System.Text.Json.JsonSerializer.Deserialize<VISABridge.Messages.Received>(input.Item2);
                    received = data.Line.TrimEnd('\n', '\r', ' ');
                }
            }
            else
            {
                context.ReportError("Session is not open.");
            }

            return received;
        }

        /// <summary>
        /// Lists the resources/instruments connected to the PC.
        /// </summary>
        /// <param name="context">Context variable</param>
        /// <param name="timeout">Max timeout</param>
        /// <returns>Array of strings that contain the names of the resources/instruments</returns>
        public static string[] ListAvailableResources([Implicit] ICallContext context, TimeSpan timeout = new TimeSpan())
        {
            if (timeout.Equals(new TimeSpan()))
            {
                timeout = TimeSpan.FromSeconds(20); // List Available Resources can often take quite a bit longer than 2,5s
            }

            string[] instruments = null;

            if (m_sessionOpened)
            {
                m_visaPipe.Send(VISABridge.Messages.ShortCommand.GetInstrumentList);

                Tuple<string, string> input = null;
                DateTime start = DateTime.Now;
                do
                {
                    input = m_visaPipe.TryGetReceived();
                    if (input != null)
                    {
                        break;
                    }
                    // Wait
                    Thread.Sleep(1);
                } while ((DateTime.Now - start) < timeout);

                if (input != null && input.Item1 == nameof(VISABridge.Messages.ConnectedInstruments))
                {
                    var data = System.Text.Json.JsonSerializer.Deserialize<VISABridge.Messages.ConnectedInstruments>(input.Item2);
                    instruments = data.Instruments;
                }
            }
            else
            {
                context.ReportError("Session is not open.");
            }

            return instruments;
        }

        /// <summary>
        /// Disposes the instrument object
        /// </summary>
        public void Dispose()
        {
            m_visaPipe.OnConnectionClosed -= m_visaClosedEventHandler;
            if (m_visaPipe.IsConnected())
            {
                m_visaPipe.Dispose();
            }
        }
    }
}
