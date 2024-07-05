//#define STOP_BEFORE_PARSING
//#define STOP_BEFORE_SIDEKICK
using StepBro.Core;
using StepBro.Core.Addons;
using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Execution;
using StepBro.Core.File;
using StepBro.Core.General;
using StepBro.Core.IPC;
using StepBro.Core.Logging;
using StepBro.Core.ScriptData;
using StepBro.Core.Tasks;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using StepBroMain = StepBro.Core.Main;

namespace StepBro.Cmd
{
    internal class Program
    {
        private enum Mode
        {
            DryRun,
            SinglePass,
            Loop,   // Not used directly, but states after this all includes looping.
            RepeatedParsing,
            WorkbenchWithSidekick
        }

        private enum StateOrCommand
        {
            AwaitCommand,
            LoadMainFile,
            ParseFiles,
            StartScriptExecution,
            AwaitScriptExecutionEnd,
            //AwaitFileChange,
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
        private static Mode m_mode = Mode.DryRun;
        private static Queue<StateOrCommand> m_next = new Queue<StateOrCommand>();
        private static Pipe m_sideKickPipe = null;
        private static bool sidekickStarted = false;
        private static ILoggerScope m_sidekickLogger = null;
        private static List<Tuple<ulong, object>> m_requestObjectDictionary = new List<Tuple<ulong, object>>();
        private static IExecutionScopeStatus m_statusTop = null;

        private static int Main(string[] args)
        {
            IService m_hostService = null;
            m_hostAccess = new HostAccess(out m_hostService);
            var selectedOutputAddon = OutputConsoleWithColorsAddon.Name;
            IExecutionResult result = null;
            DataReport createdReport = null;
            Dictionary<string, ITextCommandInput> commandObjectDictionary = null;
            EventHandler closeEventHandler = null;
            ConsoleCancelEventHandler consoleCancelEventHandler = null;
            IFileElement element = null;
            IPartner partner = null;
            IScriptExecution execution = null;
            bool executionRequestSilent = false;
            string targetFile = null;
            string targetFileFullPath = null;
            string targetElement = null;
            string targetPartner = null;
            string targetObject = null;
            List<object> targetArguments = new List<object>();

            ulong targetExecutionStartRequestID = 0UL;

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

                AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
                {
                    if (m_sideKickPipe.IsConnected())
                    {
                        m_sideKickPipe.Send(StepBro.Sidekick.Messages.ShortCommand.Close);
                    }
                };
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

            if (m_commandLineOptions.LogToConsoleOld)
            {
                m_commandLineOptions.LogToConsole = true;
            }

            try
            {
                StepBroMain.Initialize(m_hostService);
                StepBroMain.Logger.IsDebugging = m_commandLineOptions.Debugging;
                var objectManager = StepBroMain.ServiceManager.Get<IDynamicObjectManager>();

                if (m_commandLineOptions.Verbose)
                {
                    m_commandLineOptions.LogToConsole = true;
                }

                if (!String.IsNullOrEmpty(m_commandLineOptions.OutputFormat))
                {
                    selectedOutputAddon = m_commandLineOptions.OutputFormat;
                    if (!m_commandLineOptions.PrintReport)
                    {
                        m_commandLineOptions.LogToConsole = true;     // If report output is not enabled, the idea MUST be to output the execution log.
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
                OutputFormatOptions options = new OutputFormatOptions { CreateHighLevelLogSections = true };
                m_outputFormatter = m_outputAddon.Create(options);

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
                targetObject = m_commandLineOptions.TargetInstance;
                targetPartner = m_commandLineOptions.TargetModel;
                targetArguments = m_commandLineOptions?.Arguments.Select((a) => StepBroMain.ParseExpression(element?.ParentFile, a)).ToList();


                if (!String.IsNullOrEmpty(targetFile))
                {
                    if (m_commandLineOptions.Verbose || m_commandLineOptions.RepeatedParsing || m_commandLineOptions.Sidekick)
                    {
                        ConsoleWriteLine("File to load: {0}", targetFile);
                    }

                    if (!String.IsNullOrEmpty(targetElement) && m_commandLineOptions.RepeatedParsing && !m_commandLineOptions.Sidekick)
                    {
                        ConsoleWriteErrorLine("Options 'execute' and 'repeated parsing' cannot be used at the same time.");
                        retval = -1;
                        m_next.Enqueue(StateOrCommand.Exit);
                    }
                    else
                    {
                        m_next.Enqueue(StateOrCommand.LoadMainFile);
                    }

                    if (m_commandLineOptions.RepeatedParsing)
                    {
                        m_mode = Mode.RepeatedParsing;
                        if (!m_commandLineOptions.Sidekick)
                        {
                            ConsoleWriteLine("Starting 'repeated parsing'. To exit, press 'x'. To clear view, press 'c'.");
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
                }

                if (retval == 0 && m_commandLineOptions.Sidekick)
                {
                    closeEventHandler = (sender, e) =>
                    {
                        m_sideKickPipe.Send(StepBro.Sidekick.Messages.ShortCommand.Close);
                        Thread.Sleep(1000);     // Leave some time for the sidekick application to receive the command.
                    };

                    consoleCancelEventHandler = (sender, e) =>
                    {
                        m_sideKickPipe.Send(StepBro.Sidekick.Messages.ShortCommand.Close);
                        Thread.Sleep(1000);     // Leave some time for the execution helper application to receive the command.
                    };

                    m_sidekickLogger = StepBroMain.Logger.RootLogger.CreateSubLocation("SideKick");

                    Console.CancelKeyPress += consoleCancelEventHandler;
                    AppDomain.CurrentDomain.ProcessExit += closeEventHandler;

                    var hThis = GetConsoleWindow();

                    string path = Assembly.GetExecutingAssembly().Location;
                    var folder = Path.GetDirectoryName(path);

                    string pipename = hThis.ToString("X");
                    m_sideKickPipe = Pipe.StartServer("StepBroConsoleSidekick", pipename);
                    var sidekick = new System.Diagnostics.Process();
                    sidekick.StartInfo.FileName = Path.Combine(folder, "StepBro.Sidekick.exe");
                    sidekick.StartInfo.Arguments = pipename;
                    if (m_commandLineOptions.NoAttach)
                    {
                        sidekick.StartInfo.Arguments += " --no_attach";
                    }
                    sidekickStarted = sidekick.Start();

#if STOP_BEFORE_SIDEKICK
                Console.WriteLine("<PRESS ANY KEY TO CONTINUE>");
                while (!Console.KeyAvailable)
                {
                    System.Threading.Thread.Sleep(25);
                }
#endif
                    if (sidekickStarted)
                    {
                        commandObjectDictionary = new Dictionary<string, ITextCommandInput>();

                        while (!m_sideKickPipe.IsConnected())
                        {
                            System.Threading.Thread.Sleep(200);
                            // TODO: Timeout
                        }

                        StartLogDumpTask();

                        m_mode = Mode.WorkbenchWithSidekick;
                    }
                }

                m_activitiesRunning = true;

#if STOP_BEFORE_PARSING
                Console.WriteLine("<PRESS ANY KEY TO CONTINUE>");
                while (!Console.KeyAvailable)
                {
                    System.Threading.Thread.Sleep(25);
                }
#endif

                IScriptFile file = null;

                StateOrCommand command;
                while (retval == 0 && (command = m_next.Any() ? m_next.Dequeue() : (m_mode >= Mode.Loop ? StateOrCommand.AwaitCommand : StateOrCommand.Exit)) != StateOrCommand.Exit)
                {
                    if (command != StateOrCommand.AwaitCommand)
                    {
                        Trace.WriteLine("StepBro command: " + command.ToString());
                    }
                    switch (command)
                    {
                        case StateOrCommand.AwaitCommand:
                            if (m_commandLineOptions.RepeatedParsing && StepBroMain.CheckIfFileParsingNeeded(true))
                            {
                                if (sidekickStarted)
                                {
                                    m_next.Enqueue(StateOrCommand.ParseFiles);
                                }
                                else
                                {
                                    m_next.Enqueue(StateOrCommand.CloseAndDisposeAllFiles);
                                }
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
                                        zeroTime = DateTime.UtcNow;
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
                                    if (input.Item1 == nameof(StepBro.Sidekick.Messages.ShortCommand))
                                    {
                                        var shortCommand = JsonSerializer.Deserialize<StepBro.Sidekick.Messages.ShortCommand>(input.Item2);
                                        switch (shortCommand)
                                        {
                                            case Sidekick.Messages.ShortCommand.ClearDisplay:
                                                Console.Clear();
                                                zeroTime = DateTime.UtcNow;
                                                break;
                                            case StepBro.Sidekick.Messages.ShortCommand.RequestClose:
                                                m_next.Enqueue(StateOrCommand.Exit);
                                                break;
                                            case StepBro.Sidekick.Messages.ShortCommand.Parse:
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
                                    else if (input.Item1 == nameof(StepBro.Sidekick.Messages.Log))
                                    {
                                        var data = JsonSerializer.Deserialize<StepBro.Sidekick.Messages.Log>(input.Item2);
                                        switch (data.LogType)
                                        {
                                            case Sidekick.Messages.Log.Type.Normal:
                                                m_sidekickLogger.LogUserAction(data.Text);
                                                break;
                                            case Sidekick.Messages.Log.Type.Error:
                                                m_sidekickLogger.LogError(data.Text);
                                                break;
                                        }
                                    }
                                    else if (input.Item1 == nameof(StepBro.Sidekick.Messages.ObjectCommand))
                                    {
                                        var objectCommand = JsonSerializer.Deserialize<StepBro.Sidekick.Messages.ObjectCommand>(input.Item2);

                                        if (String.IsNullOrEmpty(objectCommand.Object))
                                        {
                                            StepBroMain.Logger.RootLogger.LogError("Missing object reference in object command.");
                                        }
                                        else if (commandObjectDictionary.ContainsKey(objectCommand.Object) && !String.IsNullOrEmpty(objectCommand.Command))
                                        {
                                            string name = objectCommand.Object.Split('.').Last();
                                            StepBroMain.Logger.RootLogger.LogUserAction($"Request run '{name}' command \"{objectCommand.Command}\"");
                                            var obj = commandObjectDictionary[objectCommand.Object];
                                            if (obj.AcceptingCommands())
                                            {
                                                obj.ExecuteCommand(objectCommand.Command);
                                            }
                                            else
                                            {
                                                string errorMessage = $"'{name}' is not accepting commands. Did you forget to open or connect?";

                                                StepBroMain.Logger.RootLogger.LogError(errorMessage);
                                            }
                                        }
                                    }
                                    else if (input.Item1 == nameof(StepBro.Sidekick.Messages.RunScriptRequest))
                                    {
                                        var request = JsonSerializer.Deserialize<StepBro.Sidekick.Messages.RunScriptRequest>(input.Item2);
                                        executionRequestSilent = request.Silent;
                                        targetExecutionStartRequestID = request.RequestID;
                                        targetElement = request.Element;
                                        targetPartner = request.Partner;
                                        targetObject = request.ObjectReference;
                                        targetArguments = new List<object>();
                                        if (request.Arguments != null)
                                        {
                                            targetArguments = request.Arguments.Select((a) => a.GetValue()).ToList();
                                        }

                                        string objectInstanceText = String.IsNullOrEmpty(request.ObjectReference) ? "" : (request.ObjectReference.Split('.').Last() + ".");
                                        string partnertext = String.IsNullOrEmpty(targetPartner) ? "" : (" @ " + targetPartner);
                                        string noteText = String.IsNullOrEmpty(request.ExecutionNote) ? "" : (" - \"" + request.ExecutionNote + "\"");
                                        string elementText = String.IsNullOrEmpty(objectInstanceText) ? targetElement : targetElement.Split('.').Last();
                                        StepBroMain.Logger.RootLogger.LogUserAction("Request script execution: " + objectInstanceText + elementText + partnertext + noteText);
                                        m_next.Enqueue(StateOrCommand.StartScriptExecution);
                                    }
                                    else if (input.Item1 == nameof(StepBro.Sidekick.Messages.StopExecutionRequest))
                                    {
                                        StepBroMain.Logger.RootLogger.LogUserAction("Execution stop requested by user");
                                        var request = JsonSerializer.Deserialize<StepBro.Sidekick.Messages.StopExecutionRequest>(input.Item2);
                                        var reqistration = m_requestObjectDictionary.FirstOrDefault(r => r.Item1 == request.RequestID);
                                        if (reqistration != null)
                                        {
                                            if (reqistration.Item2 is IScriptExecution)
                                            {
                                                (reqistration.Item2 as IScriptExecution).Task.RequestStop();
                                            }
                                        }
                                    }
                                    else if (input.Item1 == nameof(StepBro.Sidekick.Messages.ReleaseRequest))
                                    {
                                        var request = JsonSerializer.Deserialize<StepBro.Sidekick.Messages.ReleaseRequest>(input.Item2);
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
                            }
                            break;

                        case StateOrCommand.LoadMainFile:
                            targetFileFullPath = System.IO.Path.GetFullPath(targetFile);
                            try
                            {
                                file = StepBroMain.LoadScriptFile(consoleResourceUserObject, targetFileFullPath);
                                if (file == null)
                                {
                                    retval = -1;
                                    ConsoleWriteErrorLine("Error: Loading script file failed ( " + targetFileFullPath + " )");
                                }
                                else
                                {
                                    var shortcuts = ServiceManager.Global.Get<IFolderManager>();
                                    var projectShortcuts = new FolderShortcutCollection(FolderShortcutOrigin.Project);
                                    projectShortcuts.AddShortcut(StepBro.Core.Api.Constants.TOP_FILE_FOLDER_SHORTCUT, System.IO.Path.GetDirectoryName(file.FilePath), isResolved: true);
                                    shortcuts.AddSource(projectShortcuts);

                                    m_next.Enqueue(StateOrCommand.ParseFiles);  // File has been loaded; start the parsing.
                                }
                            }
                            catch (Exception ex)
                            {
                                retval = -1;
                                ConsoleWriteErrorLine("Error: Loading script file failed: " + ex.GetType().Name + ", " + ex.Message);
                            }
                            break;

                        case StateOrCommand.ParseFiles:
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
                                    var objects = objectManager.GetObjectCollection();
                                    var commandObjectsContainers = objects.Where(o => o.Object is ITextCommandInput).ToList();
                                    foreach (var o in commandObjectsContainers)
                                    {
                                        commandObjectDictionary[o.FullName] = o.Object as ITextCommandInput;    // Add or override.
                                    }
                                    var commandObjectsMessage = new StepBro.Sidekick.Messages.CommandObjectsList();
                                    commandObjectsMessage.Objects = commandObjectsContainers.Select(o => o.FullName).ToArray();
                                    m_sideKickPipe.Send(commandObjectsMessage);

                                    // Update the list of loaded script files.
                                    var fileManager = StepBroMain.ServiceManager.Get<ILoadedFilesManager>();
                                    var files = fileManager.ListFiles<IScriptFile>().ToList();

                                    var variableTypes = new Dictionary<string, TypeReference>();
                                    for (int i = 0; i < files.Count; i++)
                                    {
                                        foreach (var v in files[i].ListElements().Where(e => e.ElementType == FileElementType.FileVariable))
                                        {
                                            variableTypes[v.FullName] = v.DataType;
                                        }
                                    }

                                    var elementList = new List<StepBro.Sidekick.Messages.Element>();
                                    for (int i = 0; i < files.Count; i++)
                                    {
                                        var f = files[i];

                                        var elements = f.ListElements().Where(e => e.ElementType == FileElementType.ProcedureDeclaration || e.ElementType == FileElementType.TestList).ToList();
                                        foreach (var e in elements)
                                        {
                                            StepBro.Sidekick.Messages.Element elementData = null;
                                            switch (e.ElementType)
                                            {
                                                case FileElementType.ProcedureDeclaration:
                                                    {
                                                        elementData = new StepBro.Sidekick.Messages.Procedure();
                                                        var procedureData = elementData as StepBro.Sidekick.Messages.Procedure;
                                                        var p = e as IFileProcedure;
                                                        if (p.Parameters.Length > 0 && p.IsFirstParameterThisReference)
                                                        {
                                                            var par = p.Parameters[0];
                                                            (elementData as StepBro.Sidekick.Messages.Procedure).FirstParameterIsInstanceReference = true;

                                                            var instances = new List<string>();
                                                            foreach (var v in objects)
                                                            {
                                                                if (par.Value.IsAssignableFrom(variableTypes[v.FullName]))
                                                                {
                                                                    instances.Add(v.FullName);
                                                                }
                                                            }
                                                            if (instances.Count > 0)
                                                            {
                                                                procedureData.CompatibleObjectInstances = instances.ToArray();
                                                            }
                                                        }
                                                        (elementData as StepBro.Sidekick.Messages.Procedure).Parameters = p.Parameters.Select(p => new StepBro.Sidekick.Messages.Parameter(p.Name, p.Value.TypeName())).ToArray();
                                                        (elementData as StepBro.Sidekick.Messages.Procedure).ReturnType = p.ReturnType.TypeName();
                                                    }
                                                    break;
                                                case FileElementType.TestList:
                                                    elementData = new StepBro.Sidekick.Messages.TestList();
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
                                                    var elementPartners = new List<StepBro.Sidekick.Messages.Partner>();
                                                    foreach (var p in e.ListPartners())
                                                    {
                                                        var partnerData = new StepBro.Sidekick.Messages.Partner();
                                                        partnerData.Name = p.Name;
                                                        partnerData.ProcedureReference = p.ProcedureReference.FullName;
                                                        elementPartners.Add(partnerData);
                                                    }
                                                    elementData.Partners = elementPartners.ToArray();
                                                }
                                                elementList.Add(elementData);
                                            }
                                        }
                                    }
                                    foreach (var v in objects)
                                    {
                                        StepBro.Sidekick.Messages.Variable variableData;
                                        if (v.Object is StepBro.ToolBarCreator.ToolBar)
                                        {
                                            variableData = new StepBro.Sidekick.Messages.ToolBarDefinitionVariable();
                                            variableData.DataType = v.Object.GetType().FullName;
                                            variableData.Interfaces |= StepBro.Sidekick.Messages.VariableInterfaces.ToolBarCreator;
                                            var panel = v.Object as StepBro.ToolBarCreator.ToolBar;
                                            ((StepBro.Sidekick.Messages.ToolBarDefinitionVariable)variableData).Title = panel.Title;
                                            ((StepBro.Sidekick.Messages.ToolBarDefinitionVariable)variableData).ToolBarDefinition = panel.Definition.CloneForSerialization();
                                        }
                                        else if (v.Object is StepBro.PanelCreator.Panel)
                                        {
                                            variableData = new StepBro.Sidekick.Messages.PanelDefinitionVariable();
                                            variableData.DataType = v.Object.GetType().FullName;
                                            variableData.Interfaces |= StepBro.Sidekick.Messages.VariableInterfaces.PanelCreator;
                                            var panel = v.Object as StepBro.PanelCreator.Panel;
                                            ((StepBro.Sidekick.Messages.PanelDefinitionVariable)variableData).Title = panel.Title;
                                            ((StepBro.Sidekick.Messages.PanelDefinitionVariable)variableData).PanelDefinition = panel.MainPanelDefinition.CloneForSerialization();
                                        }
                                        else
                                        {
                                            variableData = new StepBro.Sidekick.Messages.Variable();
                                        }
                                        variableData.Name = v.FullName.Split('.').Last();
                                        variableData.FullName = v.FullName;
                                        if (v.Object is ITextCommandInput)
                                        {
                                            variableData.Interfaces |= StepBro.Sidekick.Messages.VariableInterfaces.Command;
                                        }
                                        elementList.Add(variableData);
                                    }

                                    // Send the stuff...

                                    var startMessage = new StepBro.Sidekick.Messages.StartFileElements();
                                    startMessage.TopFile = targetFileFullPath;
                                    startMessage.Files = files.Select(f => f.FilePath).ToArray();
                                    m_sideKickPipe.Send(startMessage);

                                    foreach (var e in elementList)
                                    {
                                        m_sideKickPipe.Send(new StepBro.Sidekick.Messages.FileElement(e));
                                    }

                                    m_sideKickPipe.Send(StepBro.Sidekick.Messages.ShortCommand.EndFileElements);
                                }

                                if (!String.IsNullOrEmpty(targetElement))
                                {
                                    m_next.Enqueue(StateOrCommand.StartScriptExecution);
                                }
                                else
                                {
                                    switch (m_mode)
                                    {
                                        case Mode.RepeatedParsing:
                                        case Mode.WorkbenchWithSidekick:
                                            ConsoleWriteLine("No parsing errors.");
                                            m_dumpBufferedConsoleOutput = true;
                                            break;
                                        default:
                                            m_next.Enqueue(StateOrCommand.Exit);
                                            break;
                                    }
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
                                    case Mode.WorkbenchWithSidekick:
                                        m_dumpBufferedConsoleOutput = true;
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
                                bool executionStarted = false;
                                if (execution != null)
                                {
                                    ConsoleWriteErrorLine("Error: Execution already running.");
                                }
                                else
                                {
                                    bool error = false;
                                    element = StepBroMain.TryFindFileElement(targetElement);
                                    partner = null;
                                    if (element != null)
                                    {
                                        if (!String.IsNullOrEmpty(targetPartner))
                                        {
                                            partner = element.ListPartners().First(p => String.Equals(targetPartner, p.Name, StringComparison.InvariantCultureIgnoreCase));
                                            if (partner == null)
                                            {
                                                ConsoleWriteErrorLine($"Error: The specified file element does not have a model named \"{targetPartner}\".");
                                                error = true;
                                            }
                                        }
                                        else
                                        {
                                            if (element is IFileProcedure)
                                            {
                                                var procedure = (IFileProcedure)element;

                                                // NOTE: targetObject might be set, even if it should not be used.

                                                if (!String.IsNullOrEmpty(targetObject) && procedure.IsFirstParameterThisReference)
                                                {
                                                    var theObject = objectManager.GetObjectCollection().FirstOrDefault(v => string.Equals(v.FullName, targetObject, StringComparison.InvariantCulture));
                                                    if (theObject != null)
                                                    {
                                                        targetArguments.Insert(0, theObject.Object);
                                                    }
                                                    else
                                                    {
                                                        ConsoleWriteErrorLine($"Error: Target object '{targetObject}' was not found in the list of global variables.");
                                                        error = true;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (element.ElementType is FileElementType.TestList)
                                                {
                                                    var partners = String.Join(", ", 
                                                                    (element as ITestList).ListPartners()
                                                                    .Where(a => a.ProcedureReference.IsFirstParameterThisReference &&
                                                                                a.ProcedureReference.Parameters[0].Value.Type == typeof(ITestList))
                                                                    .Select(a => a.Name).Distinct());

                                                    // Write error message
                                                    if (partners.Length > 0)
                                                    {
                                                        ConsoleWriteErrorLine($"Execution of the {element.Name} testlist can only be done through a partner. This testlist has the following partners: {partners}.");
                                                    }
                                                    else
                                                    {
                                                        ConsoleWriteErrorLine($"Execution of the {element.Name} testlist can only be done through a partner.");
                                                    }
                                                }
                                                else
                                                {
                                                    ConsoleWriteErrorLine($"Error: Target element (type {element.ElementType}) is not a supported type for execution.");
                                                }
                                                error = true;
                                            }
                                        }

                                        if (!error)
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
                                                    execution = StepBroMain.StartProcedureExecution(element, partner, targetArguments.ToArray());
                                                    if (targetExecutionStartRequestID != 0UL)
                                                    {
                                                        m_requestObjectDictionary.Add(new Tuple<ulong, object>(targetExecutionStartRequestID, execution));
                                                        execution.Task.CurrentStateChanged += ExecutionTask_CurrentStateChanged;
                                                    }
                                                    m_sideKickPipe.Send(StepBro.Sidekick.Messages.ShortCommand.ExecutionStarted);
                                                }
                                                else
                                                {
                                                    execution = StepBroMain.ExecuteProcedure(element, partner, targetArguments.ToArray());
                                                }

                                                ((INotifyCollectionChanged)execution.StateStack).CollectionChanged += ExecutionState_CollectionChanged;

                                                executionStarted = true;
                                                m_next.Enqueue(StateOrCommand.AwaitScriptExecutionEnd);
                                            }
                                            catch (TargetParameterCountException)
                                            {
                                                ConsoleWriteErrorLine("Error: The number of arguments does not match the target procedure.");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        ConsoleWriteErrorLine($"Error: File element named '{targetElement} was not found.");
                                    }
                                }

                                if (!executionStarted)
                                {
                                    if (sidekickStarted && targetExecutionStartRequestID != 0UL)
                                    {
                                        m_sideKickPipe.Send(new StepBro.Sidekick.Messages.ExecutionStateUpdate(targetExecutionStartRequestID, TaskExecutionState.ErrorStarting));
                                    }
                                    else
                                    {
                                        retval = -1;
                                    }
                                }
                                targetElement = null;
                                targetObject = null;
                                targetPartner = null;
                                targetExecutionStartRequestID = 0UL;
                            }
                            break;

                        case StateOrCommand.AwaitScriptExecutionEnd:
                            {
                                if (execution.Task.Ended())
                                {
                                    if (sidekickStarted)
                                    {
                                        var executionReqistration = m_requestObjectDictionary.FirstOrDefault(e => Object.ReferenceEquals(execution, e.Item2));
                                        if (executionReqistration != null)
                                        {
                                            m_sideKickPipe.Send(new StepBro.Sidekick.Messages.ExecutionStateUpdate(executionReqistration.Item1, execution.Task.CurrentState));
                                        }

                                        m_sideKickPipe.Send(StepBro.Sidekick.Messages.ShortCommand.ExecutionStopped);
                                    }
                                    result = execution.Result;
                                    createdReport = execution.Report;
                                    execution = null;

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
                                    if (sidekickStarted)
                                    {
                                        m_next.Enqueue(StateOrCommand.AwaitCommand);
                                    }
                                    m_next.Enqueue(StateOrCommand.AwaitScriptExecutionEnd);
                                    Thread.Sleep(100);
                                }
                            }
                            break;

                        case StateOrCommand.CloseAndDisposeAllFiles:
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

                    if (m_next.Count == 0)
                    {
                        Thread.Sleep(150);
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
            finally
            {
                if (m_sideKickPipe != null)
                {
                    m_sideKickPipe.Send(StepBro.Sidekick.Messages.ShortCommand.Close);
                    m_sideKickPipe.Dispose();
                }
            }

            Trace.WriteLine("StepBro ended command loop");


            if (m_commandLineOptions.Sidekick)
            {
                AppDomain.CurrentDomain.ProcessExit -= closeEventHandler;
                Console.CancelKeyPress -= consoleCancelEventHandler;
            }

            m_activitiesRunning = false;
            while (m_dumpingExecutionLog)
            {
                System.Threading.Thread.Sleep(50);
            }
            FlushBufferedConsoleOutput();

            if ((m_commandLineOptions.PrintReport || m_commandLineOptions.ReportToFile != null) && createdReport != null)
            {
                if (m_commandLineOptions.ReportToFile != null)
                {
                    try
                    {
                        // Delete an existing report.sbr file if it exists
                        // as that would be from a previous broken run
                        // File.Delete succeeds without throwing an error if the file does not exist
                        System.IO.File.Delete("report.sbr");
                        using (StreamWriter streamWriter = System.IO.File.AppendText("report.sbr"))
                        {
                            streamWriter.WriteLine($"--- BEGAN GENERATION AT {DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss")} ---\n");
                        }
                    }
                    catch (Exception e)
                    {
                        ConsoleWriteErrorLine("Error occurred when creating report file. The following exception was thrown: " + e.Message);
                    }
                }
                m_outputFormatter.WriteReport(createdReport, m_commandLineOptions.PrintReport, m_commandLineOptions.ReportToFile);
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

        private class ExecutionScopeData : IDisposable
        {
            private IExecutionScopeStatus m_status;
            private int m_level;
            private ulong m_id;
            public bool m_isDisposed = false;

            public ExecutionScopeData(IExecutionScopeStatus status, int level)
            {
                m_id = UniqueInteger.GetLongProtected();
                m_status = status;
                m_status.UITag = this;
                m_status.PropertyChanged += Status_PropertyChanged;
                ((INotifyCollectionChanged)m_status.Buttons).CollectionChanged += Buttons_CollectionChanged;
            }

            public int Level { get { return m_level; } }

            private void Buttons_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                System.Diagnostics.Debug.WriteLine("Buttons in state level changed; " + String.Join(", ", m_status.Buttons.Select(b => b.Title)));
            }

            private void Status_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
            {
                System.Diagnostics.Debug.WriteLine("State level property changed: " + e.PropertyName);
            }

            public void Dispose()
            {
                System.Diagnostics.Debug.WriteLine("State level dispose");
                m_status.PropertyChanged -= Status_PropertyChanged;
                m_status = null;
                // TODO: Notify list changed
            }
        }

        private static void ExecutionState_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var collection = sender as ReadOnlyObservableCollection<IExecutionScopeStatus>;
            int level = 0;
            foreach (var item in collection)
            {
                if (item.UITag == null)
                {
                    item.UITag = new ExecutionScopeData(item, level);
                    System.Diagnostics.Debug.WriteLine("New state level created: " + item.MainText);
                    // TODO: Notify list changed
                }
                level++;
            }
            IExecutionScopeStatus top = null;
            while (collection.Count > 0)
            {
                try
                {
                    top = collection[collection.Count - 1];
                    break;
                }
                catch { }
            }
            if (((top == null) != (m_statusTop == null)) || Object.ReferenceEquals(top, m_statusTop))
            {
                m_statusTop = top;  // Can be null.
            }
        }

        private static void ExecutionTask_CurrentStateChanged(object sender, EventArgs e)
        {
            var executionTask = sender as ITaskControl;
            var execution = m_requestObjectDictionary.FirstOrDefault(e => e.Item2 is IScriptExecution && Object.ReferenceEquals(executionTask, e.Item2));
            if (execution != null)
            {
                m_sideKickPipe.Send(new StepBro.Sidekick.Messages.ExecutionStateUpdate(execution.Item1, executionTask.CurrentState));
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
                if (sidekickStarted)
                {
                    m_dumpBufferedConsoleOutput = true;
                }
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
            if (!m_dumpingExecutionLog && m_commandLineOptions.LogToConsole)
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
                    if (m_dumpBufferedConsoleOutput)
                    {
                        FlushBufferedConsoleOutput();
                        m_dumpBufferedConsoleOutput = false;
                    }
                    Thread.Sleep(50);
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
