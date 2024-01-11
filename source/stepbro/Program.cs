using StepBro.Core;
using StepBro.Core.Addons;
using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Execution;
using StepBro.Core.File;
using StepBro.Core.General;
using StepBro.Core.Logging;
using StepBro.Core.ScriptData;
using StepBro.Core.Tasks;
using StepBro.Sidekick;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using static StepBro.Core.Data.ObjectMonitor;
using StepBroMain = StepBro.Core.Main;

namespace StepBro.Cmd
{
    internal class Program
    {
        private enum Mode
        {
            /// <summary>
            /// After checking command line parameters, just exit.
            /// </summary>
            RunThrough,
            ExecuteScript,
            RepeatedParsing
        }
        private enum StateOrCommand
        {
            AwaitCommand,
            LoadMainFile,
            ParseFiles,
            StartScriptExecution,
            AwaitScriptExecutionEnd,
            AwaitFileChange,
            CloseAndDisposeAllFiles,
            Exit
        }

        private class ExitException : Exception { }
        private static HostAccess m_hostAccess;
        private static CommandLineOptions m_commandLineOptions = null;
        private static bool m_activitiesRunning = false;
        private static bool m_dumpingExecutionLog = false;
        private static bool m_dumpBufferedConsoleOutput = false;
        private static DateTime zeroTime;
        private static IOutputFormatterTypeAddon m_outputAddon = null;
        private static IOutputFormatter m_outputFormatter = null;
        private static List<Tuple<bool, string>> m_bufferedOutput = new List<Tuple<bool, string>>();
        private static Mode m_mode = Mode.RunThrough;
        private static Queue<StateOrCommand> m_next = new Queue<StateOrCommand>();
        private static SideKickPipe m_sideKickPipe = null;
        private static List<Tuple<ulong, object>> m_requestObjectDictionary = new List<Tuple<ulong, object>>();

        private static int Main(string[] args)
        {
            IService m_hostService = null;
            m_hostAccess = new HostAccess(out m_hostService);
            var selectedOutputAddon = OutputConsoleWithColorsAddon.Name;
            IExecutionResult result = null;
            DataReport createdReport = null;
            bool sidekickStarted = false;
            Dictionary<string, ITextCommandInput> commandObjectDictionary = null;
            EventHandler closeEventHandler = null;
            IFileElement element = null;
            IPartner partner = null;
            IScriptExecution execution = null;
            ulong executionStartRequestID = 0UL;
            bool executionRequestSilent = false;
            string targetFile = null;
            string targetElement = null;
            string targetPartner = null;

            try
            {
                Console.CursorVisible = false;      // On the Azure CI, this fail.
            }
            catch { }

            object consoleResourceUserObject = new object();
            int retval = 0;

            StringBuilder sb = new StringBuilder();
            System.IO.StringWriter sw = new System.IO.StringWriter(sb);
            m_commandLineOptions = StepBro.Core.General.CommandLineParser.Parse<CommandLineOptions>(null, args, sw);
            // Print output from the command line parsing, and skip empty lines.
            foreach (var line in sb.ToString().Split(System.Environment.NewLine))
            {
                if (!String.IsNullOrWhiteSpace(line)) ConsoleWriteLine(line);
            }
            if (args.Length == 0)
            {
                Console.WriteLine("StepBro console application. Type 'stepbro --help' to show the help text.");
            }
            else if (m_commandLineOptions.Sidekick)
            {
                var back = Console.BackgroundColor;
                var fore = Console.ForegroundColor;
                Console.BackgroundColor = ConsoleColor.DarkYellow; Console.ForegroundColor = ConsoleColor.Black;

                Console.Write("    StepBro console application. Press 'C' to clear window. Press 'X' to close the application.    ");

                Console.BackgroundColor = back; Console.ForegroundColor = fore;
                Console.WriteLine();
                Console.WriteLine();
            }
            else if (m_commandLineOptions.Verbose)
            {
                Console.WriteLine("StepBro console application. Type 'stepbro --help' to show the help text.");
            }

            if (m_commandLineOptions.HasParsingErrors)
            {
                if (m_commandLineOptions.ParsingErrors.Count() == 1 && m_commandLineOptions.ParsingErrors.First().Tag == CommandLine.ErrorType.HelpRequestedError)
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            }

            try
            {
                StepBroMain.Initialize(m_hostService);
                StepBroMain.Logger.IsDebugging = m_commandLineOptions.Debugging;

                if (m_commandLineOptions.Verbose)
                {
                    m_commandLineOptions.TraceToConsole = true;
                }

                if (!String.IsNullOrEmpty(m_commandLineOptions.OutputFormat))
                {
                    selectedOutputAddon = m_commandLineOptions.OutputFormat;
                    if (!m_commandLineOptions.PrintReport)
                    {
                        m_commandLineOptions.TraceToConsole = true;     // If report output is not enabled, the idea MUST be to output the execution log.
                    }
                }

                m_outputAddon = StepBroMain.GetService<Core.Api.IAddonManager>().TryGetAddon<IOutputFormatterTypeAddon>(selectedOutputAddon);
                if (m_outputAddon == null)
                {
                    ConsoleWriteErrorLine("Error: Output format \'" + selectedOutputAddon + "\' was not found.");
                    var available = String.Join(", ", StepBroMain.GetService<Core.Api.IAddonManager>().Addons.Where(a => a is IOutputFormatterTypeAddon).Select(a => a.ShortName));
                    ConsoleWriteErrorLine("    Available options: " + available);
                    retval = -1;
                    throw new ExitException();
                }
                m_outputFormatter = m_outputAddon.Create(createHighLevelLogSections: true);

                if (m_commandLineOptions.Verbose)
                {
                    Console.WriteLine("Command line args: " + string.Join(" ", args));


                    //var logSinkManager = StepBro.Core.Main.GetService<ILogSinkManager>();
                    //logSinkManager.Add(new ConsoleLogSink());

                    var addonManager = StepBro.Core.Main.GetService<Core.Api.IAddonManager>();
                    foreach (var f in addonManager.ScannedFiles)
                    {
                        String prefix = "Assembly loaded: ";
                        if (f.Item2 == true) prefix = "Assembly skipped: ";
                        else if (f.Item3 != null) prefix = "Assembly error: ";
                        ConsoleWriteLine(prefix + f.Item1);
                    }
                }

                targetFile = m_commandLineOptions.InputFile;
                targetElement = m_commandLineOptions.TargetElement;
                targetPartner = m_commandLineOptions.Model;

                if (!String.IsNullOrEmpty(targetFile))
                {
                    if (m_commandLineOptions.Verbose) ConsoleWriteLine("Filename: {0}", targetFile);

                    m_next.Enqueue(StateOrCommand.LoadMainFile);

                    if (!String.IsNullOrEmpty(targetElement) && m_commandLineOptions.RepeatedParsing)
                    {
                        ConsoleWriteErrorLine("Options 'execute' and 'repeated parsing' cannot be used at the same time.");
                        retval = -1;
                        m_next.Enqueue(StateOrCommand.Exit);
                    }

                    if (m_commandLineOptions.RepeatedParsing)
                    {
                        m_mode = Mode.RepeatedParsing;
                        if (!m_commandLineOptions.Sidekick)
                        {
                            ConsoleWriteLine("Starting 'repeated parsing'. To exit, press 'x'. To clear view, press 'c'.");
                        }
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(targetElement))
                        {
                            m_mode = Mode.ExecuteScript;
                        }
                    }
                }
                else
                {
                    if (!String.IsNullOrEmpty(targetPartner))
                    {
                        retval = -1;
                        ConsoleWriteErrorLine("Error: Model has been specified, but not a target element.");
                    }
                    //else
                    //{
                    //    ConsoleWriteLine("No target element specified; no execution started.");
                    //}
                }

                closeEventHandler = (sender, e) =>
                {
                    m_sideKickPipe.Send(ShortCommand.Close);
                    Thread.Sleep(1000);     // Leave some time for the sidekick application to receive the command.
                };

                if (m_commandLineOptions.Sidekick)
                {
                    AppDomain.CurrentDomain.ProcessExit += closeEventHandler;

                    var hThis = GetConsoleWindow();

                    string path = Assembly.GetExecutingAssembly().Location;
                    var folder = Path.GetDirectoryName(path);

                    string pipename = hThis.ToString("X");
                    m_sideKickPipe = SideKickPipe.StartServer(pipename);
                    var sidekick = new System.Diagnostics.Process();
                    sidekick.StartInfo.FileName = Path.Combine(folder, "StepBro.Sidekick.exe");
                    sidekick.StartInfo.Arguments = pipename;
                    sidekickStarted = sidekick.Start();

                    if (sidekickStarted)
                    {
                        commandObjectDictionary = new Dictionary<string, ITextCommandInput>();

                        while (!m_sideKickPipe.IsConnected())
                        {
                            System.Threading.Thread.Sleep(200);
                            // TODO: Timeout
                        }

                        StartLogDumpTask();

                        if (m_mode == Mode.RunThrough)
                        {
                            m_next.Enqueue(StateOrCommand.AwaitCommand);
                        }
                    }
                }

                m_activitiesRunning = true;

#if DEBUG
                //Console.WriteLine("<PRESS ANY KEY TO CONTINUE>");
                //while (!Console.KeyAvailable)
                //{
                //    System.Threading.Thread.Sleep(25);
                //}
#endif

                IScriptFile file = null;

                StateOrCommand command;
                while ((command = m_next.Any() ? m_next.Dequeue() : StateOrCommand.Exit) != StateOrCommand.Exit)
                {
                    switch (command)
                    {
                        case StateOrCommand.AwaitCommand:
                        case StateOrCommand.AwaitFileChange:
                            if (m_mode == Mode.RepeatedParsing && StepBroMain.CheckIfFileParsingNeeded(true))
                            {
                                m_next.Enqueue(StateOrCommand.CloseAndDisposeAllFiles);
                                System.Threading.Thread.Sleep(200);         // Give editor a chance to save all files before the parsing starts.
                                break;
                            }
                            else if (Console.KeyAvailable)
                            {
                                var keyInfo = Console.ReadKey(true);
                                if ((uint)keyInfo.Modifiers == 0)
                                {
                                    if (keyInfo.Key == ConsoleKey.C)
                                    {
                                        Console.Clear();
                                        zeroTime = DateTime.Now;
                                    }
                                    else if (keyInfo.Key == ConsoleKey.X)
                                    {
                                        m_next.Enqueue(StateOrCommand.Exit);
                                        break;
                                    }
                                }
                            }
                            else if (sidekickStarted)
                            {
                                var input = m_sideKickPipe.TryGetReceived();
                                if (input != null)
                                {
                                    if (input.Item1 == nameof(ShortCommand))
                                    {
                                        var shortCommand = JsonSerializer.Deserialize<ShortCommand>(input.Item2);
                                        switch (shortCommand)
                                        {
                                            case ShortCommand.Close:
                                                break;
                                            case ShortCommand.Parse:
                                                StepBroMain.Logger.RootLogger.LogUserAction("Request file parsing");
                                                m_next.Enqueue(StateOrCommand.ParseFiles);
                                                break;
                                            //case ShortCommand.StopScriptExecution:
                                            //    StepBroMain.Logger.RootLogger.LogUserAction("Request stop script execution");
                                            //    if (execution != null && !execution.Task.Ended())
                                            //    {
                                            //        execution.Task.RequestStop();
                                            //    }
                                            //    break;
                                            default:
                                                break;
                                        }
                                    }
                                    else if (input.Item1 == nameof(ObjectCommand))
                                    {
                                        var objectCommand = JsonSerializer.Deserialize<ObjectCommand>(input.Item2);

                                        if (commandObjectDictionary.ContainsKey(objectCommand.Object) && !String.IsNullOrEmpty(objectCommand.Command))
                                        {
                                            string name = objectCommand.Object.Split('.').Last();
                                            StepBroMain.Logger.RootLogger.LogUserAction($"Request run '{name}' command \"{objectCommand.Command}\"");
                                            var obj = commandObjectDictionary[objectCommand.Object];
                                            if (obj.AcceptingCommands())
                                            {
                                                obj.ExecuteCommand(objectCommand.Command);
                                            }
                                        }
                                    }
                                    else if (input.Item1 == nameof(RunScriptRequest))
                                    {
                                        var request = JsonSerializer.Deserialize<RunScriptRequest>(input.Item2);
                                        executionRequestSilent = request.Silent;
                                        executionStartRequestID = request.RequestID;
                                        targetElement = request.Element;
                                        targetPartner = request.Partner;

                                        string partnertext = String.IsNullOrEmpty(targetPartner) ? "" : (" @ " + targetPartner);
                                        StepBroMain.Logger.RootLogger.LogUserAction("Request script execution: " + targetElement + partnertext);

                                        m_next.Enqueue(StateOrCommand.StartScriptExecution);
                                    }
                                    else if (input.Item1 == nameof(StopExecutionRequest))
                                    {
                                        var request = JsonSerializer.Deserialize<StopExecutionRequest>(input.Item2);
                                        var reqistration = m_requestObjectDictionary.FirstOrDefault(r => r.Item1 == request.RequestID);
                                        if (reqistration != null)
                                        {
                                            if (reqistration.Item2 is IScriptExecution)
                                            {
                                                (reqistration.Item2 as IScriptExecution).Task.RequestStop();
                                            }
                                        }
                                    }
                                    else if (input.Item1 == nameof(ReleaseRequest))
                                    {
                                        var request = JsonSerializer.Deserialize<ReleaseRequest>(input.Item2);
                                        var reqistration = m_requestObjectDictionary.FirstOrDefault(r => r.Item1 == request.RequestID);
                                        if (reqistration != null)
                                        {
                                            if (reqistration.Item2 is IScriptExecution)
                                            {
                                                (reqistration.Item2 as IScriptExecution).Task.CurrentStateChanged -= ExecutionTask_CurrentStateChanged;
                                            }
                                            m_requestObjectDictionary.RemoveAt(m_requestObjectDictionary.FindIndex(e => e.Item1 == request.RequestID));
                                        }
                                    }
                                }
                            }
                            if (m_next.Count == 0)
                            {
                                System.Threading.Thread.Sleep(150);
                                m_next.Enqueue(StateOrCommand.AwaitCommand);
                            }
                            break;

                        case StateOrCommand.LoadMainFile:
                            Trace.WriteLine("StepBro command: " + command.ToString());
                            var filepath = System.IO.Path.GetFullPath(targetFile);
                            try
                            {
                                file = StepBroMain.LoadScriptFile(consoleResourceUserObject, filepath);
                                if (file == null)
                                {
                                    m_next.Enqueue(StateOrCommand.Exit);
                                    retval = -1;
                                    ConsoleWriteErrorLine("Error: Loading script file failed ( " + filepath + " )");
                                }
                                var shortcuts = ServiceManager.Global.Get<IFolderManager>();
                                var projectShortcuts = new FolderCollection(FolderShortcutOrigin.Project);
                                projectShortcuts.AddShortcut(StepBro.Core.Api.Constants.TOP_FILE_FOLDER_SHORTCUT, System.IO.Path.GetDirectoryName(file.FilePath));
                                shortcuts.AddSource(projectShortcuts);

                                switch (m_mode)
                                {
                                    case Mode.RunThrough:
                                        if (sidekickStarted)
                                        {
                                            m_next.Enqueue(StateOrCommand.ParseFiles);
                                        }
                                        break;
                                    case Mode.ExecuteScript:
                                    case Mode.RepeatedParsing:
                                        m_next.Enqueue(StateOrCommand.ParseFiles);
                                        break;
                                    default:
                                        break;
                                }
                            }
                            catch (Exception ex)
                            {
                                m_next.Enqueue(StateOrCommand.Exit);
                                retval = -1;
                                ConsoleWriteErrorLine("Error: Loading script file failed: " + ex.GetType().Name + ", " + ex.Message);
                            }
                            break;

                        case StateOrCommand.ParseFiles:
                            Trace.WriteLine("StepBro command: " + command.ToString());
                            if (m_commandLineOptions.Verbose)
                            {
                                StepBroMain.Logger.RootLogger.LogDetail("Request parsing files");
                            }
                            var parsingSuccess = StepBroMain.ParseFiles(true);
                            if (parsingSuccess)
                            {
                                if (sidekickStarted)
                                {
                                    // Update list of variables containing objects with the ITextCommandInput interface.
                                    var objectManager = StepBroMain.ServiceManager.Get<IDynamicObjectManager>();
                                    var objects = objectManager.GetObjectCollection();
                                    var commandObjectsContainers = objects.Where(o => o.Object is ITextCommandInput).ToList();
                                    foreach (var o in commandObjectsContainers)
                                    {
                                        commandObjectDictionary.Add(o.FullName, o.Object as ITextCommandInput);
                                    }
                                    var commandObjectsMessage = new CommandObjectsList();
                                    commandObjectsMessage.Objects = commandObjectsContainers.Select(o => o.FullName).ToArray();
                                    m_sideKickPipe.Send(commandObjectsMessage);

                                    // Update the list of loaded script files.
                                    var fileManager = StepBroMain.ServiceManager.Get<ILoadedFilesManager>();
                                    var files = fileManager.ListFiles<IScriptFile>().ToList();

                                    var elementsMessage = new StepBro.Sidekick.FileElements();
                                    var elementList = new List<StepBro.Sidekick.FileElements.Element>();
                                    for (int i = 0; i < files.Count; i++)
                                    {
                                        var f = files[i];

                                        var elements = f.ListElements().Where(e => e.ElementType == FileElementType.ProcedureDeclaration || e.ElementType == FileElementType.TestList).ToList();
                                        foreach (var e in elements)
                                        {
                                            StepBro.Sidekick.FileElements.Element elementData = null;
                                            switch (e.ElementType)
                                            {
                                                case FileElementType.ProcedureDeclaration:
                                                    {
                                                        elementData = new FileElements.Procedure();
                                                        var p = e as IFileProcedure;
                                                        if (p.Parameters.Length > 0 && p.IsFirstParameterThisReference)
                                                        {
                                                            var par = p.Parameters[0];
                                                            if (par.Value.HasProcedureReference)
                                                            {
                                                                // TODO: add more type checking on that first parameter.
                                                                (elementData as FileElements.Procedure).FirstParameterIsInstanceReference = true;
                                                            }
                                                        }
                                                        (elementData as FileElements.Procedure).Parameters = p.Parameters.Select(p => new FileElements.Parameter(p.Name, p.Value.TypeName())).ToArray();
                                                        (elementData as FileElements.Procedure).ReturnType = p.ReturnType.TypeName();
                                                    }
                                                    break;
                                                case FileElementType.TestList:
                                                    elementData = new FileElements.TestList();
                                                    break;
                                                default:
                                                    break;
                                            }
                                            if (elementData != null)
                                            {
                                                elementData.File = i;
                                                elementData.Name = e.Name;
                                                elementData.FullName = e.FullName;

                                                if (e.ListPartners().Any())
                                                {
                                                    var elementPartners = new List<FileElements.Partner>();
                                                    foreach (var p in e.ListPartners())
                                                    {
                                                        var partnerData = new FileElements.Partner();
                                                        partnerData.Name = p.Name;
                                                        partnerData.ProcedureType = p.ProcedureName;
                                                        elementPartners.Add(partnerData);
                                                    }
                                                    elementData.Partners = elementPartners.ToArray();
                                                }
                                                //elementPartners.Add(partners);

                                                elementList.Add(elementData);
                                            }
                                        }
                                    }
                                    foreach (var v in objects)
                                    {
                                        StepBro.Sidekick.FileElements.Variable variableData;
                                        if (v.Object is StepBro.PanelCreator.Panel)
                                        {
                                            variableData = new StepBro.Sidekick.FileElements.PanelDefinitionVariable();
                                            variableData.DataType = v.Object.GetType().FullName;
                                            variableData.Interfaces |= StepBro.Sidekick.FileElements.VariableInterfaces.PanelCreator;
                                            var panel = v.Object as StepBro.PanelCreator.Panel;
                                            ((FileElements.PanelDefinitionVariable)variableData).Title = panel.Title;
                                            ((FileElements.PanelDefinitionVariable)variableData).PanelDefinition = panel.MainPanelDefinition.CloneForSerialization();
                                        }
                                        else
                                        {
                                            variableData = new StepBro.Sidekick.FileElements.Variable();
                                        }
                                        variableData.Name = v.FullName.Split('.').Last();
                                        variableData.FullName = v.FullName;
                                        if (v.Object is ITextCommandInput)
                                        {
                                            variableData.Interfaces |= StepBro.Sidekick.FileElements.VariableInterfaces.Command;
                                        }
                                        elementList.Add(variableData);
                                    }
                                    elementsMessage.Files = files.Select(f => f.FilePath).ToArray();
                                    elementsMessage.Elements = elementList.ToArray();
                                    m_sideKickPipe.Send(elementsMessage);
                                }

                                switch (m_mode)
                                {
                                    case Mode.RunThrough:
                                        if (!sidekickStarted)
                                        {
                                            m_next.Enqueue(StateOrCommand.Exit);
                                        }
                                        break;
                                    case Mode.ExecuteScript:
                                        m_next.Enqueue(StateOrCommand.StartScriptExecution);
                                        break;
                                    case Mode.RepeatedParsing:
                                        ConsoleWriteLine("No parsing errors.");
                                        m_next.Enqueue(StateOrCommand.AwaitFileChange);
                                        break;
                                    default:
                                        break;
                                }
                            }
                            else
                            {
                                int num = StepBroMain.GetLoadedFilesManager().ListFiles<IScriptFile>().Sum(f => f.Errors.GetList().Where(e => !e.JustWarning).Count());
                                ConsoleWriteErrorLine($"{num} parsing error(s)!");
                                foreach (var openedFile in StepBroMain.GetLoadedFilesManager().ListFiles<IScriptFile>())
                                {
                                    foreach (var err in openedFile.Errors.GetList())
                                    {
                                        if (!err.JustWarning)
                                        {
                                            ConsoleWriteErrorLine($"   {openedFile.FileName} line {err.Line}: {err.Message}");
                                        }
                                    }
                                }

                                switch (m_mode)
                                {
                                    case Mode.RepeatedParsing:
                                        m_dumpBufferedConsoleOutput = true;
                                        m_next.Enqueue(StateOrCommand.AwaitFileChange);
                                        break;
                                    default:
                                        retval = -1;
                                        m_next.Enqueue(StateOrCommand.Exit);
                                        break;
                                }
                            }
                            break;

                        case StateOrCommand.StartScriptExecution:
                            {
                                Trace.WriteLine("StepBro command: " + command.ToString());
                                element = StepBroMain.TryFindFileElement(targetElement);
                                partner = null;
                                if (element != null)
                                {
                                    List<object> arguments = m_commandLineOptions?.Arguments.Select(
                                        (a) => StepBroMain.ParseExpression(element?.ParentFile, a)).ToList();

                                    if (!String.IsNullOrEmpty(targetPartner))
                                    {
                                        partner = element.ListPartners().First(p => String.Equals(targetPartner, p.Name, StringComparison.InvariantCultureIgnoreCase));
                                        if (partner == null)
                                        {
                                            retval = -1;
                                            ConsoleWriteErrorLine($"Error: The specified file element does not have a model named \"{targetPartner}\".");
                                        }
                                    }
                                    else
                                    {
                                        if (!(element is IFileProcedure))
                                        {
                                            retval = -1;
                                            ConsoleWriteErrorLine($"Error: Target element (type {element.ElementType}) is not a supported type for execution.");
                                        }
                                    }

                                    if (retval == 0)
                                    {
                                        try
                                        {
                                            StartLogDumpTask();

                                            if (m_commandLineOptions.Verbose)
                                            {
                                                StepBroMain.Logger.RootLogger.LogDetail("Request script execution");
                                            }
                                            if (sidekickStarted)
                                            {
                                                execution = StepBroMain.StartProcedureExecution(element, partner, arguments.ToArray());
                                                if (executionStartRequestID != 0UL)
                                                {
                                                    m_requestObjectDictionary.Add(new Tuple<ulong, object>(executionStartRequestID, execution));
                                                    execution.Task.CurrentStateChanged += ExecutionTask_CurrentStateChanged;
                                                    executionStartRequestID = 0UL;
                                                }
                                                m_next.Enqueue(StateOrCommand.AwaitCommand);
                                                m_sideKickPipe.Send(ShortCommand.ExecutionStarted);
                                            }
                                            else
                                            {
                                                execution = StepBroMain.ExecuteProcedure(element, partner, arguments.ToArray());
                                            }
                                            m_next.Enqueue(StateOrCommand.AwaitScriptExecutionEnd);
                                        }
                                        catch (TargetParameterCountException)
                                        {
                                            retval = -1;
                                            ConsoleWriteErrorLine("Error: The number of arguments does not match the target procedure.");
                                        }
                                    }
                                }
                                else
                                {
                                    retval = -1;
                                    ConsoleWriteErrorLine($"Error: File element named '{targetElement} was not found.");
                                }
                            }
                            if (m_next.Count == 0)
                            {
                                System.Threading.Thread.Sleep(150);
                                m_next.Enqueue(StateOrCommand.AwaitCommand);
                            }
                            break;

                        case StateOrCommand.AwaitScriptExecutionEnd:
                            {
                                if (sidekickStarted)
                                {
                                    m_next.Enqueue(StateOrCommand.AwaitCommand);
                                }
                                if (execution.Task.Ended())
                                {
                                    if (sidekickStarted)
                                    {
                                        var executionReqistration = m_requestObjectDictionary.FirstOrDefault(e => Object.ReferenceEquals(execution, e.Item2));
                                        if (executionReqistration != null)
                                        {
                                            m_sideKickPipe.Send(new ExecutionStateUpdate(executionReqistration.Item1, execution.Task.CurrentState));
                                        }

                                        m_sideKickPipe.Send(ShortCommand.ExecutionStopped);
                                    }
                                    result = execution.Result;
                                    createdReport = execution.Report;

                                    if (result != null)
                                    {
                                        if (m_commandLineOptions.ExitCode == ExitValueOption.SubVerdict)
                                        {
                                            switch (result.ProcedureResult.Verdict)
                                            {
                                                case Verdict.Unset:
                                                case Verdict.Pass:
                                                    break;
                                                case Verdict.Inconclusive:
                                                case Verdict.Fail:
                                                case Verdict.Abandoned:
                                                    retval = 1;
                                                    break;
                                                case Verdict.Error:
                                                    retval = -1;
                                                    break;
                                            }
                                        }
                                        else if (m_commandLineOptions.ExitCode == ExitValueOption.Verdict)
                                        {
                                            switch (result.ProcedureResult.Verdict)
                                            {
                                                case Verdict.Unset:
                                                case Verdict.Pass:
                                                    break;
                                                case Verdict.Inconclusive:
                                                case Verdict.Fail:
                                                case Verdict.Abandoned:
                                                    retval = 1;
                                                    break;
                                                case Verdict.Error:
                                                    retval = -1;
                                                    break;
                                            }
                                        }
                                        else if (m_commandLineOptions.ExitCode == ExitValueOption.ReturnValue)
                                        {
                                            if (partner == null &&
                                                element is IFileProcedure &&
                                                (element as IFileProcedure).ReturnType == TypeReference.TypeInt64)
                                            {
                                                retval = (Int32)(Int64)result.ReturnValue;
                                            }
                                        }

                                        if (!sidekickStarted)
                                        {
                                            if (m_commandLineOptions.Verbose)
                                            {
                                                ConsoleWriteLine("Procedure execution ended. " + result.ResultText());
                                                m_dumpBufferedConsoleOutput = true;
                                            }
                                            else
                                            {
                                                if (!m_commandLineOptions.PrintReport)
                                                {
                                                    ConsoleWriteLine("Procedure execution ended.");
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    m_next.Enqueue(StateOrCommand.AwaitScriptExecutionEnd);
                                }
                            }
                            break;

                        case StateOrCommand.CloseAndDisposeAllFiles:
                            Trace.WriteLine("StepBro command: " + command.ToString());
                            StepBroMain.UnregisterFileUsage(consoleResourceUserObject, file);
                            switch (m_mode)
                            {
                                case Mode.RepeatedParsing:
                                    ConsoleWriteLine("");
                                    ConsoleWriteLine($"{DateTime.Now.ToString("HH:mm:ss")} - Starting parsing");
                                    m_next.Enqueue(StateOrCommand.LoadMainFile);   // Load the file again and all its dependencies.
                                    break;
                                default:
                                    break;
                            }

                            break;

                        case StateOrCommand.Exit:
                            break;
                    }
                }
            }
            catch (ExitException) { }
            catch (Core.Parser.ParsingErrorException ex)
            {
                StringBuilder extraErrorMessage = new StringBuilder();
                if (ex.FileName != "")
                {
                    extraErrorMessage.Append(" File: ");
                    extraErrorMessage.Append(ex.FileName);
                }
                if (ex.Line != -1)
                {
                    extraErrorMessage.Append(" Line: ");
                    extraErrorMessage.Append(ex.Line);
                }
                if (ex.Name != "")
                {
                    extraErrorMessage.Append(" Name: ");
                    extraErrorMessage.Append(ex.Name);
                }
                ConsoleWriteErrorLine($"{ex.Message}{extraErrorMessage}");
            }
            catch (Exception ex)
            {
                ConsoleWriteErrorLine($"Error: {ex.GetType().Name}, {ex.Message}");
                retval = -1;
            }
            Trace.WriteLine("StepBro ended command loop");


            if (m_commandLineOptions.Sidekick)
            {
                AppDomain.CurrentDomain.ProcessExit -= closeEventHandler;
            }

            m_activitiesRunning = false;
            while (m_dumpingExecutionLog)
            {
                System.Threading.Thread.Sleep(50);
            }
            FlushBufferedConsoleOutput();

            if (m_commandLineOptions.PrintReport && createdReport != null)
            {
                m_outputFormatter.WriteReport(createdReport);
            }
            else
            {
                if (result != null && result.Exception != null)
                {
                    ConsoleWriteLine("");
                    ConsoleWriteErrorLine("EXCEPTION DETAILS");
                    ConsoleWriteLine($"    Type: {result.Exception.GetType().FullName}");
                    ConsoleWriteLine($"    Message: {result.Exception.Message}");
                    if (!String.IsNullOrEmpty(result.Exception.HelpLink)) ConsoleWriteLine($"    Help: {result.Exception.HelpLink}");
                    List<string> callstack = new List<string>();
                    if (result.Exception is UnhandledExceptionInScriptException)
                    {
                        callstack.AddRange((result.Exception as UnhandledExceptionInScriptException).GetPrintableCallStack());
                    }
                    else
                    {
                        if (result.Exception.StackTrace != null)
                        {
                            callstack.AddRange(result.Exception.StackTrace.Split("\r\n"));
                        }
                    }
                    if (callstack.Count > 0)
                    {
                        ConsoleWriteLine("    Call Stack:");
                        foreach (var l in callstack)
                        {
                            ConsoleWriteLine("        " + l.Trim());
                        }
                    }
                }
            }

            if (m_commandLineOptions.Debugging)
            {
                DebugLogUtils.DumpToFile();
                ConsoleWriteLine($"Internal Debug Log saved in {DebugLogUtils.DumpFilePath}");
            }

            if (m_sideKickPipe != null)
            {
                m_sideKickPipe.Send(ShortCommand.Close);

                Trace.WriteLine("StepBro dispose sidekick");
                m_sideKickPipe.Dispose();
                Trace.WriteLine("StepBro sidekick disposed");
            }

            StepBroMain.Deinitialize();

            if (m_commandLineOptions.AwaitKeypress)
            {
                ConsoleWriteLine("<press any key to continue>");
                while (!Console.KeyAvailable)
                {
                    System.Threading.Thread.Sleep(25);
                }
                Console.ReadKey(true);
            }
            return retval;
        }

        private static void ExecutionTask_CurrentStateChanged(object sender, EventArgs e)
        {
            var executionTask = sender as ITaskControl;
            var execution = m_requestObjectDictionary.FirstOrDefault(e => e.Item2 is IScriptExecution && Object.ReferenceEquals(executionTask, e.Item2));
            if (execution != null)
            {
                m_sideKickPipe.Send(new ExecutionStateUpdate(execution.Item1, executionTask.CurrentState));
            }
        }

        private static void ConsoleWriteLine(string value, params object[] args)
        {
            if (m_activitiesRunning && m_dumpingExecutionLog)
            {
                m_bufferedOutput.Add(new Tuple<bool, string>(false, String.Format(value, args)));
            }
            else
            {
                FlushBufferedConsoleOutput();
                Console.WriteLine(value, args);
            }
        }

        private static void ConsoleWriteErrorLine(string value)
        {
            if (m_activitiesRunning && m_dumpingExecutionLog)
            {
                m_bufferedOutput.Add(new Tuple<bool, string>(true, value));
            }
            else
            {
                FlushBufferedConsoleOutput();
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine(value);
                Console.ForegroundColor = color;
            }
        }

        private static void FlushBufferedConsoleOutput()
        {
            foreach (var s in m_bufferedOutput)
            {
                if (s.Item1 == false) Console.WriteLine(s.Item2);
                else
                {
                    var color = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Error.WriteLine(s.Item2);
                    Console.ForegroundColor = color;
                }
            }
            m_bufferedOutput.Clear();
        }

        private static void StartLogDumpTask()
        {
            if (!m_dumpingExecutionLog && m_commandLineOptions.TraceToConsole)
            {
                
                m_dumpingExecutionLog = true;
                var logTask = new Task(() => LogDumpTask());
                logTask.Start();
            }
        }

        private static void LogDumpTask()
        {
            var logEntry = StepBroMain.Logger.GetOldestEntry();
            zeroTime = logEntry.Timestamp;
            while (logEntry != null || m_activitiesRunning)
            {
                m_outputFormatter.WriteLogEntry(logEntry, zeroTime);

                // Wait until log is empty and there is no running execution.
                while (logEntry.Next == null && m_activitiesRunning == true)
                {
                    System.Threading.Thread.Sleep(50);
                    if (m_dumpBufferedConsoleOutput)
                    {
                        FlushBufferedConsoleOutput();
                        m_dumpBufferedConsoleOutput = false;
                    }
                }
                logEntry = logEntry.Next;
            }
            Console.ForegroundColor = ConsoleColor.White;
            m_dumpingExecutionLog = false;  // Signal to main thread.
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();
    }
}
