using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Execution;
using StepBro.Core.Logging;
using StepBro.Core.Tasks;
using StepBro.Streams;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static StepBro.Core.Data.LogLineData;

namespace StepBro.TestInterface
{
    public class SerialTestConnection :
        AvailabilityBase, IConnection, IRemoteProcedures, ILogLineSource, ISettableFromPropertyBlock, INotifyPropertyChanged
    {
        private class CommandData : IAsyncResult<object>, IObjectFaultDescriptor
        {
            public enum CommandState { InQueue, Running, EndResponseReceived, EndTimeout, EndResponseError, EndResultFormatError }
            private readonly DateTime m_start;
            private DateTime m_activated = DateTime.MinValue;
            private TimeSpan m_timeoutTimeSpan;
            private DateTime m_timeoutTime = DateTime.MinValue;
            private readonly string m_command;
            private readonly Type m_expectedReturnType;
            private CommandState m_state = CommandState.InQueue;
            private readonly bool m_syncCompletion;
            private readonly AutoResetEvent m_completeEvent;
            private string m_lastResponse = null;
            private string[] m_responseLines = null;
            private object m_result = null;
            private readonly Func<string[], string, object> m_resultDecoder = null;

            public CommandData(string command, TimeSpan timeout, Type expectedDataType) : base()
            {
                m_start = DateTime.Now;
                m_command = command;
                m_timeoutTimeSpan = timeout;
                m_expectedReturnType = expectedDataType;
                m_completeEvent = new AutoResetEvent(false);
                m_syncCompletion = false;
            }

            //public CommandData(string command, object result) : base()
            //{
            //    m_start = DateTime.Now;
            //    m_command = command;
            //    m_state = CommandState.EndResponseReceived;
            //    m_timeoutTimeSpan = TimeSpan.MinValue; ;
            //    m_completeEvent = null;
            //    m_syncCompletion = true;
            //    m_result = result;
            //}

            public string Command { get { return m_command; } }
            public CommandState State { get { return m_state; } }
            public DateTime ActivationTimestamp { get { return m_activated; } }
            public TimeSpan Timeout { get { return m_timeoutTimeSpan; } }
            public DateTime TimeoutTime { get { return m_timeoutTime; } }

            public object Result
            {
                get
                {
                    if (m_state > CommandState.Running)
                    {
                        return m_result;
                    }
                    else
                    {
                        throw new InvalidOperationException("Command already activated.");
                    }
                }
            }

            public bool IsFaulted { get { return m_state > CommandState.EndResponseReceived; } }

            public bool IsCompleted { get { return m_state > CommandState.Running; } }

            public WaitHandle AsyncWaitHandle { get { return m_completeEvent; } }

            public object AsyncState => throw new NotImplementedException();

            public bool CompletedSynchronously { get { return m_syncCompletion; } }

            public string FaultDescription { get; internal set; } = null;

            public string GetAndMarkActive()
            {
                if (m_state == CommandState.InQueue)
                {
                    m_state = CommandState.Running;
                    m_activated = DateTime.Now;
                    m_timeoutTime = m_activated + m_timeoutTimeSpan;
                    return m_command;
                }
                else
                {
                    throw new InvalidOperationException("Command already activated.");
                }
            }

            public void SetTimeoutResult()
            {
                if (m_state == CommandState.Running)
                {
                    m_state = CommandState.EndTimeout;
                    m_completeEvent.Set();
                }
            }

            public void SetResult(string last, params string[] listResponse)
            {
                //if (listResponse != null && listResponse.Length > 0)
                //{
                //    System.Diagnostics.Debug.WriteLine("SetResult: " + last + ", " + String.Join("|", listResponse));
                //}
                //else
                //{
                //    System.Diagnostics.Debug.WriteLine("SetResult: " + last);
                //}
                if (m_state == CommandState.Running)
                {
                    m_lastResponse = last;
                    m_responseLines = listResponse;
                    if (last.Equals(":END", StringComparison.InvariantCulture))
                    {
                        if (m_resultDecoder != null)
                        {
                            m_result = m_resultDecoder(m_responseLines, last);
                        }
                        else
                        {
                            if (m_responseLines != null && m_responseLines.Length > 0)
                            {
                                if (m_expectedReturnType != null)
                                {
                                    if (m_expectedReturnType == typeof(List<string>))
                                    {
                                        m_result = new List<string>(listResponse);
                                    }
                                    else if (m_expectedReturnType == typeof(List<long>))
                                    {
                                        m_result = listResponse.Select(s => Int64.Parse(s)).ToList();
                                    }
                                    else if (m_expectedReturnType == typeof(List<bool>))
                                    {
                                        m_result = listResponse.Select(s => Boolean.Parse(s)).ToList();
                                    }
                                    else
                                    {
                                        throw new NotSupportedException(m_expectedReturnType.Name);
                                    }
                                }
                                else
                                {
                                    m_result = new List<string>(listResponse);
                                }
                            }
                        }
                        m_state = CommandState.EndResponseReceived;
                    }
                    else if (last.StartsWith(":"))
                    {
                        if (last.StartsWith(":ERROR", StringComparison.InvariantCulture))
                        {
                            m_state = CommandState.EndResponseError;
                            var i = last.IndexOfAny(new char[] { ' ', ':' }, 6);
                            this.FaultDescription = last.Substring(i).TrimStart(' ', ':', '-');
                        }
                        else
                        {
                            m_state = CommandState.EndResponseReceived;
                            var s = last.Substring(1);
                            m_result = s;
                            try
                            {
                                if (m_expectedReturnType == typeof(long) || m_expectedReturnType == typeof(int))
                                {
                                    if (Int64.TryParse(s, out long v))
                                    {
                                        m_result = v;
                                    }
                                    else m_state = CommandState.EndResultFormatError;
                                }
                                else if (m_expectedReturnType == typeof(bool))
                                {
                                    if (TypeUtils.TryParse(s, out bool v))
                                    {
                                        m_result = v;
                                    }
                                    else m_state = CommandState.EndResultFormatError;
                                }
                                else if (m_expectedReturnType == typeof(Identifier))
                                {
                                    m_result = new Identifier(s);
                                }
                                else if (m_expectedReturnType == typeof(TimeSpan))
                                {
                                    try
                                    {
                                        m_result = TimeUtils.ParseTimeSpan(s);
                                    }
                                    catch { m_state = CommandState.EndResultFormatError; }
                                }
                            }
                            catch
                            {
                                m_state = CommandState.EndResultFormatError;
                            }
                        }
                    }
                    else
                    {
                        m_state = CommandState.EndResultFormatError;
                    }
                    m_completeEvent.Set();
                }
                else
                {
                    throw new InvalidOperationException("Command not in activated state.");
                }
            }
        }

        private readonly object m_sync = new object();
        private Stream m_stream = null;
        private IReadBuffer<char> m_inputBuffer = null;
        private LogLineData m_firstLogLine = null;
        private LogLineData m_lastLogLine = null;
        private readonly Queue<string> m_events = new Queue<string>();
        private readonly Queue<string> m_responseLines = new Queue<string>();
        private Task m_receiverTask = null;
        private bool m_stopReceiver = false;
        private readonly AutoResetEvent m_newResponseDataEvent;
        private readonly Queue<CommandData> m_commandQueue = new Queue<CommandData>();
        private CommandData m_currentExecutingCommand = null;
        private string m_nextResponse = null;
        private readonly long m_instanceID;
        private static Random rnd = new Random(DateTime.Now.GetHashCode());
        private Dictionary<string, string> m_loopbackAnswers = null;
        private List<Tuple<string, string>> m_uiCommands = null;

        private List<RemoteProcedureInfo> m_remoteProcedures = new List<RemoteProcedureInfo>();

        public SerialTestConnection()
        {
            m_instanceID = rnd.Next(1000000);
            m_newResponseDataEvent = new AutoResetEvent(false);
            this.SetupDebugCommands();
        }

        protected override void DoDispose(bool disposing)
        {
            this.Disconnect(null);
            this.Stream = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string name)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #region Properties

        public Stream Stream
        {
            get { return m_stream; }
            set
            {
                if (!Object.ReferenceEquals(value, m_stream))
                {
                    if (m_stream != null)
                    {
                        m_stream.IsOpenChanged -= this.m_stream_IsOpenChanged;
                        m_stream = null;
                    }
                    if (value != null)
                    {
                        m_stream = value;
                        m_stream.NewLine = "\n";
                        m_stream.IsOpenChanged += this.m_stream_IsOpenChanged;
                    }
                }
            }
        }

        private void m_stream_IsOpenChanged(object sender, EventArgs e)
        {
            if (!m_stream.IsOpen)
            {
                //m_streamWasClosed = true;
            }
        }

        public char EventLineChar { get; set; } = '!';
        public char ResponseEndLineChar { get; set; } = ':';
        public char ResponseMultiLineChar { get; set; } = '*';
        public string ResponseErrorPrefix { get; set; } = ":ERROR";
        public TimeSpan CommandResponseTimeout { get; set; } = TimeSpan.FromMilliseconds(5000);
        public long InstanceID { get { return m_instanceID; } }

        #endregion

        #region IConnection

        public bool IsConnected()
        {
            return m_stream != null && m_stream.IsOpen;
        }

        public bool Connect([Implicit] ICallContext context)
        {
            if (m_stream.IsOpen || m_stream.Open(context))
            {
                if (m_receiverTask == null)
                {
                    m_inputBuffer = m_stream.GetTextualReadBuffer(16 * 1024);
                    m_stopReceiver = false;
                    m_receiverTask = new Task(
                        this.ReceiverTask,
                        TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach);
                    context.TaskManager.RegisterTask(m_receiverTask);
                    m_receiverTask.Start();
                }
                return true;
            }
            else return false;
        }

        public bool Disconnect([Implicit] ICallContext context)
        {
            m_stream.Close(context);
            if (m_receiverTask != null && !m_receiverTask.IsCompleted)
            {
                m_stopReceiver = true;
            }
            m_receiverTask = null;
            return true;
        }

        public IParametersAccess Parameters { get { throw new NotImplementedException(); } }

        public IRemoteProcedures RemoteProcedures { get { return this; } }

        public IEnumerable<string> ListAvailableProfiles()
        {
            throw new System.NotImplementedException();
        }

        public bool SetProfile(string profile, string accesskey = "")
        {
            throw new System.NotImplementedException();
        }

        public IAsyncResult<object> SendCommand([Implicit] ICallContext context, string command, params object[] arguments)
        {
            var request = new List<string>();
            request.Add(command);
            if (arguments != null && arguments.Length > 0)
            {
                foreach (var a in arguments)
                {
                    request.Add(ArgumentToCommandString(a));
                }
            }
            var commandData = new CommandData(String.Join(" ", request), this.CommandResponseTimeout, null);
            return this.EnqueueCommand(commandData);
        }

        public IAsyncResult<bool> UpdateInterfaceData([Implicit] ICallContext context = null)
        {
            #region dump
            //14326094    Script Execution -GetHelp
            //14326101    sti_commands.GetHelp - < arguments >
            //14326107        SerialPort - Open(but already open)
            //14326114        SerialPort - Write "help"
            //14326119        SerialPort - ReadLine : "*Get list of commands:    list commands"
            //14326126        SerialPort - ReadLine : "*Get help for a command:  help <name of command>"
            //14326131        SerialPort - ReadLine : ":END"
            //14332094        Script Execution -Script execution ended. Result value: < null >
            //14348346    Script Execution -ListCommands
            //14348352    sti_commands.ListCommands - < arguments >
            //14348356        SerialPort - Open(but already open)
            //14348363        SerialPort - Write "list commands"
            //14348368        SerialPort - ReadLine : "*1 - help"
            //14348373        SerialPort - ReadLine : "*2 - list"
            //14348379        SerialPort - ReadLine : "*1000 - henry"
            //14348385        SerialPort - ReadLine : "*1001 - eigil"
            //14348394        SerialPort - ReadLine : ":END"
            //14350350        Script Execution -Script execution ended. Result value: < null >
            //14355597    Script Execution -HelpEigil
            //14355605    sti_commands.HelpEigil - < arguments >
            //14355614        SerialPort - Open(but already open)
            //14355622        SerialPort - Write "help eigil"
            //14355630        SerialPort - ReadLine : "*Command 1001: eigil"
            //14355635        SerialPort - ReadLine : "*Help: Just a prototype command to be deleted."
            //14355642        SerialPort - ReadLine : "*Parameters: int input. Return value: int"
            //14355646        SerialPort - ReadLine : ":END"
            //14359597        Script Execution -Script execution ended. Result value: < null >
            #endregion

            var task = Task.Run<bool>(() =>
            {
                // FIRST GET THE LIST OF AVAILABLE COMMANDS
                var command = new CommandData("list commands", this.CommandResponseTimeout, typeof(List<string>));
                var cmd = this.EnqueueCommand(command);
                if (cmd.CompletedSynchronously)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    if (cmd.AsyncWaitHandle.WaitOne(this.CommandResponseTimeout))
                    {
                        if (cmd.IsFaulted)
                        {
                            return false;
                        }
                        var procedures = new List<RemoteProcedureInfo>();
                        var commandListLines = cmd.Result as List<string>;
                        foreach (var commandListLine in commandListLines)
                        {
                            int id;
                            var commandName = DecodeCommandListLine(commandListLine, out id);

                            // THEN, GET THE INFORMATION FOR EACH COMMAND
                            command = new CommandData("help " + commandName, this.CommandResponseTimeout, typeof(List<string>));
                            cmd = this.EnqueueCommand(command);

                            if (cmd.CompletedSynchronously)
                            {
                                throw new NotImplementedException();
                            }
                            else
                            {
                                if (cmd.AsyncWaitHandle.WaitOne(this.CommandResponseTimeout))
                                {
                                    if (cmd.Result is List<string>)
                                    {
                                        var procedure = DecodeRemoteProcedureInfo(commandName, id, cmd.Result as List<string>);
                                        procedures.Add(procedure);
                                    }
                                    else
                                    {
                                        //TODO: report error somehow
                                    }
                                }
                                else
                                {
                                    return false;
                                }
                            }

                        }
                        m_remoteProcedures = procedures;
                    }
                    else
                    {
                        return false;
                    }
                }
                return true;
            });
            return new TaskToAsyncResult<bool>(task);
        }

        internal static string DecodeCommandListLine(string line, out int id)
        {
            var splitted = line.Split(new string[] { " - " }, StringSplitOptions.RemoveEmptyEntries);
            if (splitted.Length > 1)
            {
                id = Int32.Parse(splitted[0]);
                return splitted[1];
            }
            else
            {
                id = -1;
                return splitted[0];
            }
        }

        internal static RemoteProcedureInfo DecodeRemoteProcedureInfo(string command, int id, List<string> responseLines)
        {
            string helpPrefix = "Help: ";
            string parametersPrefix = "Parameters: ";
            string returnTypePrefix = "Return type: ";
            string lisSBPrefix = "Lists: ";

            var returnType = typeof(void);
            string help = null;
            var parameters = new List<Tuple<string, Type>>();
            string lists = null;
            string error = null;
            foreach (var line in responseLines)
            {
                if (line.StartsWith(helpPrefix))
                {
                    help = line.Substring(helpPrefix.Length);
                }
                else
                {
                    if (line.StartsWith(parametersPrefix, StringComparison.InvariantCulture))
                    {
                        var endIndex = line.IndexOf('.');
                        var parametersString = line.Substring(parametersPrefix.Length, ((endIndex > 0) ? endIndex : line.Length) - parametersPrefix.Length);
                        var parametersTexts = parametersString.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var parameter in parametersTexts)
                        {
                            var parts = parameter.Split(' ');
                            Type type = null;
                            string name = "";
                            switch (parts.Length)
                            {
                                case 1:
                                    name = parameter;
                                    type = typeof(object);
                                    break;
                                case 2:
                                    name = parts[1];
                                    type = DecodeTypeString(parts[0]);
                                    break;
                                default:
                                    break;
                            }
                            if (type == null)
                            {
                                error = "Error decoding parameters.";
                            }
                            else
                            {
                                parameters.Add(new Tuple<string, Type>(name, type));
                            }
                        }
                    }
                    else if (line.StartsWith(lisSBPrefix, StringComparison.InvariantCulture))
                    {
                        lists = line.Substring(lisSBPrefix.Length);
                    }
                    int i = line.IndexOf(returnTypePrefix, StringComparison.InvariantCulture);
                    if (i >= 0)
                    {
                        var typeString = line.Substring(i + returnTypePrefix.Length);
                        returnType = DecodeTypeString(typeString);
                    }
                }
            }
            var procInfo = new RemoteProcedureInfo(command, id, help, returnType, lists, error);
            foreach (var p in parameters)
            {
                procInfo.AddParameter(p.Item1, "", p.Item2);
            }
            return procInfo;
        }

        internal static Type DecodeTypeString(string type)
        {
            switch (type)
            {
                case "int": return typeof(long);
                case "float": return typeof(double);
                case "bool": return typeof(bool);
                case "id": return typeof(Identifier);
                case "string": return typeof(string);
                default: return null;
            }
        }

        #endregion

        #region IRemoteProcedures

        public IAsyncResult<object> Invoke(RemoteProcedureInfo procedure, params object[] arguments)
        {
            var request = new List<string>();
            request.Add(procedure.Name);
            if (arguments != null && arguments.Length > 0)
            {
                foreach (var a in arguments)
                {
                    request.Add(ArgumentToCommandString(a));
                }
            }
            var command = new CommandData(String.Join(" ", request), this.CommandResponseTimeout, procedure.ReturnType);
            return this.EnqueueCommand(command);
        }

        public IAsyncResult<object> Invoke(string procedure, params object[] arguments)
        {
            var cmd = new RemoteProcedureInfo(procedure);
            return this.Invoke(cmd, arguments);
        }

        public IEnumerable<RemoteProcedureInfo> Procedures
        {
            get
            {
                foreach (var rp in m_remoteProcedures)
                {
                    yield return rp;
                }
            }
        }

        #endregion

        private void AddToLog(LogType type, uint id, string text)
        {
            try
            {
                m_lastLogLine = new LogLineData(m_lastLogLine, type, id, text);
                if (m_firstLogLine == null)
                {
                    m_firstLogLine = m_lastLogLine;
                }
            }
            catch (Exception ex)
            {
                StepBro.Core.Main.RootLogger.LogError("SerialTestConnection.AddToLog", $"Unexpected error. Exception: {ex}");
            }
            this.LinesAdded?.Invoke(this, new LogLineEventArgs(m_lastLogLine));
        }

        private void ReceiverTask()
        {
            int knownCount = 0;
            int i = 0;
            while (!m_stopReceiver)
            {
                var nextWaitTime = 2000;
                lock (m_sync)
                {
                    if (m_currentExecutingCommand != null)
                    {
                        var to = (m_currentExecutingCommand.TimeoutTime - DateTime.Now).Ticks / TimeSpan.TicksPerMillisecond;
                        if (to <= 0)
                        {
                            m_currentExecutingCommand.SetTimeoutResult();
                            this.AddToLog(LogType.ReceivedError, 0, "<timeout>");
                            this.OnEndCommand();
                        }
                        else if (to < 3000) nextWaitTime = (int)to;
                    }
                }
                if (m_inputBuffer.AwaitNewData(knownCount, nextWaitTime))
                {
                    knownCount = m_inputBuffer.Count;
                    while (i < knownCount)
                    {
                        char ch = m_inputBuffer[i];
                        if (ch == '\n' || ch == '\r')
                        {
                            if (i == 0)
                            {
                                m_inputBuffer.Eat(1);
                                continue;
                            }
                            var line = m_inputBuffer.Get(0, i, i + 1);
                            i = 0;
                            knownCount = m_inputBuffer.Count;

                            var s = new string(line, 1, line.Length - 1);
                            if (line[0] == this.EventLineChar)
                            {
                                lock (m_sync)
                                {
                                    this.AddToLog(LogType.ReceivedAsync, 0, new string(line));
                                }
                                m_events.Enqueue(new string(line, 1, line.Length - 1));
                                while (m_events.Count > 1000)
                                {
                                    m_events.Dequeue();     // Ensure queue buffer is not eating all memory.
                                }
                            }
                            else
                            {
                                if (line[0] == this.ResponseMultiLineChar)
                                {
                                    lock (m_sync)
                                    {
                                        this.AddToLog(LogType.ReceivedPartial, 0, new string(line));
                                    }
                                    m_responseLines.Enqueue(s);
                                }
                                else if (line[0] == this.ResponseEndLineChar)
                                {
                                    var l = new string(line);
                                    lock (m_sync)
                                    {
                                        this.AddToLog((l.StartsWith(this.ResponseErrorPrefix)) ? LogType.ReceivedError : LogType.ReceivedEnd, 0, l);
                                    }
                                    if (m_currentExecutingCommand != null)
                                    {
                                        try
                                        {
                                            m_currentExecutingCommand.SetResult(new string(line), m_responseLines.ToArray());
                                        }
                                        finally
                                        {
                                        }
                                    }
                                    this.OnEndCommand();
                                }
                            }
                        }
                        else i++;
                    }
                }
                //else
                //{
                //    if (m_currentExecutingCommand != null)
                //    {
                //        var timeTillTimeout = (DateTime.Now - m_currentExecutingCommand.TimeoutTime).Ticks;
                //    }
                //}
            }
        }

        private void OnEndCommand()
        {
            m_responseLines.Clear();
            lock (m_sync)
            {
                m_currentExecutingCommand = null;
                if (m_commandQueue.Count > 0)
                {
                    this.DoSendCommand(m_commandQueue.Dequeue());
                }
            }
        }

        private CommandData EnqueueCommand(CommandData command)
        {
            lock (m_sync)
            {
                if (m_currentExecutingCommand == null)
                {
                    this.DoSendCommand(command);
                }
                else
                {
                    m_commandQueue.Enqueue(command);
                }
            }
            return command;
        }

        private void DoSendCommand(CommandData command)
        {
            m_currentExecutingCommand = command;
            var commandstring = command.GetAndMarkActive();
            m_loopbackAnswers?.TryGetValue(commandstring, out commandstring);
            if (m_nextResponse != null) commandstring = m_nextResponse;
            this.AddToLog(LogType.Sent, 0, command.Command);
            m_stream.Write(null, commandstring + "\n");
        }

        private static string ArgumentToCommandString(object arg)
        {
            return StringUtils.ObjectToString(arg, identifierBare: true);
        }

        public void SetNextResponse(string response)
        {
            m_nextResponse = response;
        }

        public void SetLoopbackRespone(string command, string response)
        {
            if (m_loopbackAnswers == null) m_loopbackAnswers = new Dictionary<string, string>();
            m_loopbackAnswers[command] = response;
        }

        public void SetupDebugCommands()
        {
            if (m_remoteProcedures.Count > 0) return;

            //m_remoteProcedures.Add(new RemoteProcedureInfo("Absolut", 20, "So much", typeof(void)));

            //m_remoteProcedures.Add(new RemoteProcedureInfo("Hans", 20, "Hinterseer", typeof(long)));

            //m_remoteProcedures.Add(new RemoteProcedureInfo("Fogter", 21, "X-factor duo", typeof(string)));
            //this.LastProc.AddParameter("isBest", "abcd", typeof(bool));
            //this.LastProc.AddParameter("time", "efgh", typeof(TimeSpan));

            //m_remoteProcedures.Add(new RemoteProcedureInfo("Miksi", 22, "Mikkel mand", typeof(void)));
            //this.LastProc.AddParameter("mukky", "ijkl", typeof(long));
            //this.LastProc.AddParameter("litta", "klmn", typeof(List<string>), false, true);

            //m_remoteProcedures.Add(new RemoteProcedureInfo("Liza", 23, "Minelli", typeof(bool)));
            //m_remoteProcedures.Add(new RemoteProcedureInfo("Bananas", 24, "List of lot sizes.", typeof(List<long>)));
            //m_remoteProcedures.Add(new RemoteProcedureInfo("Apples", 25, "List of names.", typeof(List<string>)));
        }

        public void Setup(ILogger logger, PropertyBlock properties)
        {
            if (m_uiCommands == null)
            {
                var commands = properties.FirstOrDefault(e => String.Equals(e.Name, "commands", StringComparison.InvariantCulture));
                if (commands != null && commands.BlockEntryType == PropertyBlockEntryType.Array)
                {
                    var uiCommands = new List<Tuple<string, string>>();
                    foreach (var e in (commands as PropertyBlockArray))
                    {
                        if (e.BlockEntryType == PropertyBlockEntryType.Value)
                        {
                            var pbv = e as PropertyBlockValue;
                            var value = pbv.Value as string;
                            if (value != null)
                            {
                                uiCommands.Add(new Tuple<string, string>(value, value));
                            }
                            else
                            {
                                logger?.LogError(this.GetType().Name + ".Setup", "commands entry is not a string");
                            }
                        }
                        else if (e.BlockEntryType == PropertyBlockEntryType.Block)
                        {
                            var pb = e as PropertyBlock;
                            if (pb.Count == 1 && pb[0].BlockEntryType == PropertyBlockEntryType.Value)
                            {
                                var pbv = pb[0] as PropertyBlockValue;
                                if (pbv != null && !String.IsNullOrEmpty(pbv.Name) && !String.IsNullOrEmpty(pbv.Value as string))
                                {
                                    uiCommands.Add(new Tuple<string, string>(pbv.Name, pbv.Value as string));
                                }
                                else
                                {
                                    logger?.LogError(this.GetType().Name + ".Setup", "commands entry is not property with name and string value.");
                                }
                            }
                        }
                        else
                        {
                            logger?.LogError(this.GetType().Name + ".Setup", "Command entry type is not supported.");
                            return;
                        }
                    }
                    m_uiCommands = uiCommands;
                    this.NotifyPropertyChanged(nameof(this.UICommands));
                }
                else
                {
                    logger?.LogError(this.GetType().Name + ".Setup", "'commands' type is not an array.");
                }
            }
        }

        public List<Tuple<string, string>> UICommands
        {
            get
            {
                return m_uiCommands?.ToList();   // Create a copy to return.
            }
        }

        private RemoteProcedureInfo LastProc { get { return m_remoteProcedures[m_remoteProcedures.Count - 1]; } }

        #region LogLineSource

        public LogLineData FirstEntry { get { throw new NotImplementedException(); } }

        public event LogLineAddEventHandler LinesAdded;

        #endregion
    }
}
