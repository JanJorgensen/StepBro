using StepBro.Core;
using StepBro.Core.Addons;
using StepBro.Core.Data;
using StepBro.Core.Logging;
using StepBro.Core.ScriptData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using StepBroMain = StepBro.Core.Main;

namespace StepBro.Cmd
{
    internal class Program
    {
        private class ExitException : Exception { }
        private static HostAccess m_hostAccess;
        private static CommandLineOptions m_commandLineOptions = null;
        private static bool m_executionRunning = false;
        private static bool m_dumpingExecutionLog = false;
        private static IOutputFormatterTypeAddon m_logDumpAddon = null;
        private static IOutputFormatter m_logDumpFormatter = null;
        private static List<string> m_bufferedOutput = new List<string>();

        private static int Main(string[] args)
        {
            IService m_hostService = null;
            m_hostAccess = new HostAccess(out m_hostService);
            var selectedLogDumpAddon = OutputConsoleWithColorsAddon.Name;

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
            if (m_commandLineOptions.ReturnValueFromSubVerdict) m_commandLineOptions.ReturnValueFromVerdict = true;
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
                    ConsoleWriteLine("Error: Log dump format (addon) \'" + selectedLogDumpAddon + "\' was found.");
                    retval = -1;
                    throw new ExitException();
                }
                if (m_logDumpAddon.FormatterType != OutputType.Console)
                {
                    m_logDumpAddon = StepBroMain.GetService<Core.Api.IAddonManager>().TryGetAddon<IOutputFormatterTypeAddon>(OutputConsoleWithColorsAddon.Name);
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
                    IScriptFile file = null;
                    try
                    {
                        file = StepBroMain.LoadScriptFile(consoleResourceUserObject, m_commandLineOptions.InputFile);
                        if (file == null)
                        {
                            retval = -1;
                            ConsoleWriteLine("Error: Loading script file failed ( " + m_commandLineOptions.InputFile + " )");
                        }
                    }
                    catch (Exception ex)
                    {
                        retval = -1;
                        ConsoleWriteLine("Error: Loading script file failed: " + ex.GetType().Name + ", " + ex.Message);
                    }

                    if (file != null)
                    {
                        m_executionRunning = true;
                        if (m_commandLineOptions.TraceToConsole)
                        {
                            m_dumpingExecutionLog = true;
                            var logTask = new Task(() => LogDumpTask());
                            logTask.Start();
                        }

                        var parsingSuccess = StepBroMain.ParseFiles(true);
                        if (parsingSuccess)
                        {
                            if (!String.IsNullOrEmpty(m_commandLineOptions.TargetElement))
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
                                                var result = StepBroMain.ExecuteProcedure(procedure, arguments.ToArray());

                                                if (result != null)
                                                {
                                                    if (m_commandLineOptions.ReturnValueFromSubVerdict)
                                                    {
                                                        var verdict = result.ProcedureResult.Verdict;                   // Include verdict from the partner procedure result.
                                                        foreach (var sr in result.ProcedureResult.ListSubResults())     // Check verdict from each sub result.
                                                        {
                                                            if (sr.Verdict > verdict) verdict = sr.Verdict;
                                                        }
                                                        switch (verdict)
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
                                                    else if (m_commandLineOptions.ReturnValueFromVerdict)
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
                                                ConsoleWriteLine("Error: The number of arguments does not match the target procedure.");
                                            }

                                        }
                                        else
                                        {
                                            retval = -1;
                                            ConsoleWriteLine($"Error: The specified file element does not have a model named \"{m_commandLineOptions.Model}\".");
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

                                                if (m_commandLineOptions.ReturnValueFromVerdict)
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
                                                ConsoleWriteLine("Error: The number of arguments does not match the target procedure.");
                                            }
                                        }
                                        else
                                        {
                                            retval = -1;
                                            ConsoleWriteLine($"Error: Target element (type {element.ElementType}) is not a supported type for execution.");
                                        }
                                    }
                                }
                                else
                                {
                                    retval = -1;
                                    ConsoleWriteLine($"Error: File element named '{m_commandLineOptions.TargetElement} was not found.");
                                }
                            }
                            else
                            {
                                if (!String.IsNullOrEmpty(m_commandLineOptions.Model))
                                {
                                    retval = -1;
                                    ConsoleWriteLine("Error: Model has been specified, but not a target element.");
                                }
                                else
                                {
                                    ConsoleWriteLine("No target element specified; no execution started.");
                                }
                            }
                        }
                        else
                        {
                            ConsoleWriteLine("Parsing errors!");
                            foreach (var openedFile in StepBroMain.GetLoadedFilesManager().ListFiles<IScriptFile>())
                            {
                                foreach (var err in openedFile.Errors.GetList())
                                {
                                    if (!err.JustWarning)
                                    {
                                        ConsoleWriteLine($"   {openedFile.FileName} line {err.Line}: {err.Message}");
                                    }
                                }
                            }
                            retval = -1;
                        }
                    }
                }
                else
                {
                    // If no file should be opened, what then?
                    retval = -1;
                    ConsoleWriteLine("Error: File could not be opened.");
                }
            }
            catch (ExitException) { }
            catch (Exception ex)
            {
                ConsoleWriteLine($"Error: {ex.GetType().Name}, {ex.Message}");
                retval = -1;
            }
            finally
            {
                m_executionRunning = false;
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
                Console.ReadKey();
            }
             return retval;
        }

        private static void ConsoleWriteLine(string value, params object[] args)
        {
            if (m_executionRunning)
            {
                m_bufferedOutput.Add(String.Format(value, args));
            }
            else
            {
                FlushBufferedConsoleOutput();
                Console.WriteLine(value, args);
            }
        }

        private static void FlushBufferedConsoleOutput()
        {
            foreach (var s in m_bufferedOutput)
            {
                Console.WriteLine(s);
            }
            m_bufferedOutput.Clear();
        }

        private static void LogDumpTask()
        {
            var logEntry = StepBroMain.Logger.GetOldestEntry();
            var zero = logEntry.Timestamp;
            while (logEntry != null || m_executionRunning)
            {
                m_logDumpFormatter.LogEntry(logEntry, zero);

                // Wait until log is empty and there is no running execution.
                while (logEntry.Next == null && m_executionRunning == true)
                {
                    System.Threading.Thread.Sleep(50);
                }
                logEntry = logEntry.Next;
            }
            Console.ForegroundColor = ConsoleColor.White;
            m_dumpingExecutionLog = false;  // Signal to main thread.
        }
    }
}
