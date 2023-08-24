using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using StepBro.Core;
using StepBro.Core.Addons;
using StepBro.Core.Data;
using StepBro.Core.File;
using StepBro.Core.Logging;
using StepBro.Core.ScriptData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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
        private enum State
        {
            LoadMainFile,
            ParseFiles,
            ExecuteScript,
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
        private static IOutputFormatterTypeAddon m_logDumpAddon = null;
        private static IOutputFormatter m_logDumpFormatter = null;
        private static List<Tuple<bool, string>> m_bufferedOutput = new List<Tuple<bool, string>>();
        //Queue<Tuple<Action<object>, object>> m_commandQueue = new Queue<Tuple<Action<object>, object>>();
        private static Mode m_mode = Mode.RunThrough;
        private static State m_state = State.Exit;

        private static int Main(string[] args)
        {
            IService m_hostService = null;
            m_hostAccess = new HostAccess(out m_hostService);
            var selectedLogDumpAddon = OutputConsoleWithColorsAddon.Name;

            Console.CursorVisible = false;

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
            if (m_commandLineOptions.Verbose || args.Length == 0)
            {
                ConsoleWriteLine("StepBro console application. Type 'stepbro --help' to show the help text.");
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
                if (!String.IsNullOrEmpty(m_commandLineOptions.LogFormat))
                {
                    selectedLogDumpAddon = m_commandLineOptions.LogFormat;
                    m_commandLineOptions.TraceToConsole = true;
                }

                m_logDumpAddon = StepBroMain.GetService<Core.Api.IAddonManager>().TryGetAddon<IOutputFormatterTypeAddon>(selectedLogDumpAddon);
                if (m_logDumpAddon == null)
                {
                    ConsoleWriteErrorLine("Error: Log dump format (addon) \'" + selectedLogDumpAddon + "\' was found.");
                    retval = -1;
                    throw new ExitException();
                }
                m_logDumpFormatter = m_logDumpAddon.Create();
                //m_logDumpFormatter = new OutputConsoleWithColorsAddon.TextToConsoleFormatter(m_logDumpAddon);

                if (m_commandLineOptions.Verbose)
                {
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

                if (!String.IsNullOrEmpty(m_commandLineOptions.InputFile))
                {
                    if (m_commandLineOptions.Verbose) ConsoleWriteLine("Filename: {0}", m_commandLineOptions.InputFile);

                    m_state = State.LoadMainFile;

                    if (!String.IsNullOrEmpty(m_commandLineOptions.TargetElement) && m_commandLineOptions.RepeatedParsing)
                    {
                        ConsoleWriteErrorLine("Options 'execute' and 'repeated parsing' cannot be used at the same time.");
                        retval = -1;
                        m_state = State.Exit;
                    }

                    if (m_commandLineOptions.RepeatedParsing)
                    {
                        m_mode = Mode.RepeatedParsing;
                        ConsoleWriteLine("Starting 'repeated parsing'. To exit, press 'x'. To clear view, press 'c'.");
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(m_commandLineOptions.TargetElement))
                        {
                            m_mode = Mode.ExecuteScript;
                        }
                    }
                }
                else
                {
                    if (!String.IsNullOrEmpty(m_commandLineOptions.Model))
                    {
                        retval = -1;
                        ConsoleWriteErrorLine("Error: Model has been specified, but not a target element.");
                    }
                    //else
                    //{
                    //    ConsoleWriteLine("No target element specified; no execution started.");
                    //}
                }

                m_activitiesRunning = true;

                IScriptFile file = null;

                while (m_state != State.Exit)
                {
                    switch (m_state)
                    {
                        case State.LoadMainFile:
                            var filepath = System.IO.Path.GetFullPath(m_commandLineOptions.InputFile);
                            try
                            {
                                file = StepBroMain.LoadScriptFile(consoleResourceUserObject, filepath);
                                if (file == null)
                                {
                                    m_state = State.Exit;
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
                                        break;
                                    case Mode.ExecuteScript:
                                    case Mode.RepeatedParsing:
                                        m_state = State.ParseFiles;
                                        break;
                                    default:
                                        break;
                                }
                            }
                            catch (Exception ex)
                            {
                                m_state = State.Exit;
                                retval = -1;
                                ConsoleWriteErrorLine("Error: Loading script file failed: " + ex.GetType().Name + ", " + ex.Message);
                            }
                            break;

                        case State.ParseFiles:
                            var parsingSuccess = StepBroMain.ParseFiles(true);
                            if (parsingSuccess)
                            {
                                switch (m_mode)
                                {
                                    case Mode.RunThrough:
                                        m_state = State.Exit;
                                        break;
                                    case Mode.ExecuteScript:
                                        m_state = State.ExecuteScript;
                                        break;
                                    case Mode.RepeatedParsing:
                                        ConsoleWriteLine("No parsing errors.");
                                        m_state = State.AwaitFileChange;
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
                                        m_state = State.AwaitFileChange;
                                        break;
                                    default:
                                        retval = -1;
                                        m_state = State.Exit;
                                        break;
                                }
                            }
                            break;

                        case State.ExecuteScript:
                            {
                                IFileElement element = StepBroMain.TryFindFileElement(m_commandLineOptions.TargetElement);
                                if (element != null)
                                {
                                    List<object> arguments = m_commandLineOptions?.Arguments.Select(
                                        (a) => StepBroMain.ParseExpression(element?.ParentFile, a)).ToList();

                                    if (!String.IsNullOrEmpty(m_commandLineOptions.Model))
                                    {
                                        var partner = element.ListPartners().First(p => String.Equals(m_commandLineOptions.Model, p.Name, StringComparison.InvariantCultureIgnoreCase));
                                        if (partner != null)
                                        {
                                            var procedure = partner.ProcedureReference;
                                            if (procedure.IsFirstParameterThisReference)
                                            {
                                                arguments.Insert(0, element);
                                            }
                                            try
                                            {
                                                // Start logging now.
                                                if (m_commandLineOptions.TraceToConsole)
                                                {
                                                    m_dumpingExecutionLog = true;
                                                    var logTask = new Task(() => LogDumpTask());
                                                    logTask.Start();
                                                }


                                                var result = StepBroMain.ExecuteProcedure(procedure, arguments.ToArray());

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
                                                        if (procedure.ReturnType == TypeReference.TypeInt64)
                                                        {
                                                            retval = (Int32)(Int64)result.ReturnValue;
                                                        }
                                                    }

                                                    if (m_commandLineOptions.Verbose)
                                                    {
                                                        ConsoleWriteLine("Procedure execution ended. " + result.ResultText());
                                                    }
                                                    else
                                                    {
                                                        ConsoleWriteLine("Procedure execution ended.");
                                                    }
                                                }
                                            }
                                            catch (TargetParameterCountException)
                                            {
                                                retval = -1;
                                                ConsoleWriteErrorLine("Error: The number of arguments does not match the target procedure.");
                                            }

                                        }
                                        else
                                        {
                                            retval = -1;
                                            ConsoleWriteErrorLine($"Error: The specified file element does not have a model named \"{m_commandLineOptions.Model}\".");
                                        }
                                    }
                                    else
                                    {
                                        if (element is IFileProcedure)
                                        {
                                            var procedure = element as IFileProcedure;
                                            try
                                            {
                                                var result = StepBroMain.ExecuteProcedure(procedure, arguments.ToArray());

                                                if (m_commandLineOptions.Verbose)
                                                {
                                                    if (result != null)
                                                    {
                                                        ConsoleWriteLine("Procedure execution ended. " + result.ResultText());
                                                    }
                                                    else
                                                    {
                                                        ConsoleWriteLine("Procedure execution ended.");
                                                    }
                                                }
                                                else if (m_commandLineOptions.ExitCode == ExitValueOption.ReturnValue)
                                                {
                                                    if (procedure.ReturnType == TypeReference.TypeInt64)
                                                    {
                                                        retval = (Int32)(Int64)result.ReturnValue;
                                                    }
                                                }

                                                if (m_commandLineOptions.ExitCode == ExitValueOption.Verdict)
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
                                            }
                                            catch (TargetParameterCountException)
                                            {
                                                retval = -1;
                                                ConsoleWriteErrorLine("Error: The number of arguments does not match the target procedure.");
                                            }
                                        }
                                        else
                                        {
                                            retval = -1;
                                            ConsoleWriteErrorLine($"Error: Target element (type {element.ElementType}) is not a supported type for execution.");
                                        }
                                    }
                                }
                                else
                                {
                                    retval = -1;
                                    ConsoleWriteErrorLine($"Error: File element named '{m_commandLineOptions.TargetElement} was not found.");
                                }

                                m_state = State.Exit;   // For now, always exit after execution (or attempt).
                            }
                            break;

                        case State.AwaitFileChange:

                            // Collect all relevant files

                            while (true)
                            {
                                if (StepBroMain.CheckIfFileParsingNeeded(true))
                                {
                                    m_state = State.CloseAndDisposeAllFiles;
                                    System.Threading.Thread.Sleep(200);         // Give editor a chance to save all files before the sparsing starts.
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
                                        }
                                        else if (keyInfo.Key == ConsoleKey.X)
                                        {
                                            m_state = State.Exit;
                                        }
                                    }
                                    break;
                                }
                                System.Threading.Thread.Sleep(200);
                            }
                            break;

                        case State.CloseAndDisposeAllFiles:

                            StepBroMain.UnregisterFileUsage(consoleResourceUserObject, file);
                            switch (m_mode)
                            {
                                case Mode.RepeatedParsing:
                                    ConsoleWriteLine("");
                                    ConsoleWriteLine($"{DateTime.Now.ToString("HH:mm:ss")} - Starting parsing");
                                    m_state = State.LoadMainFile;   // Load the file again and all its dependencies.
                                    break;
                                default:
                                    break;
                            }

                            break;

                        case State.Exit:
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
            finally
            {
                m_activitiesRunning = false;
                while (m_dumpingExecutionLog)
                {
                    System.Threading.Thread.Sleep(50);
                }
                FlushBufferedConsoleOutput();
                if (m_commandLineOptions.Debugging)
                {
                    DebugLogUtils.DumpToFile();
                    ConsoleWriteLine($"Internal Debug Log saved in {DebugLogUtils.DumpFilePath}");
                }

                StepBroMain.Deinitialize();
            }

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

        private static void LogDumpTask()
        {
            var logEntry = StepBroMain.Logger.GetOldestEntry();
            var zero = logEntry.Timestamp;
            while (logEntry != null || m_activitiesRunning)
            {
                m_logDumpFormatter.LogEntry(logEntry, zero);

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
    }
}
