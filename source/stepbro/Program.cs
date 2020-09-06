using StepBro.Core.Logging;
using StepBro.Core.ScriptData;
using System;
using System.Linq;
using System.Reflection;
using StepBroMain = StepBro.Core.Main;

namespace StepBro.Cmd
{
    internal class Program
    {
        private static CommandLineOptions m_commandLineOptions = null;

        private class ConsoleLogSink : ILogSink
        {
            private static readonly string[] indentStrings;
            private DateTime m_startTime = DateTime.Now;
            static ConsoleLogSink()
            {
                indentStrings = new string[32];
                for (int i = 0; i < 32; i++) indentStrings[i] = new string(' ', i * 4);
            }

            public void Add(LogEntry entry)
            {
                string time = String.Format("{0,6}", ((long)(((TimeSpan)(DateTime.Now - m_startTime)).TotalMilliseconds)));
                //string time = ((long)(((TimeSpan)(DateTime.Now - m_startTime)).TotalMilliseconds)).ToString("D,6");
                if (String.IsNullOrEmpty(entry.Location))
                {
                    if (!String.IsNullOrEmpty(entry.Text))
                    {
                        Console.WriteLine(String.Concat(
                        time,
                        indentStrings[entry.IndentLevel],
                        entry.Text));
                    }
                }
                else if (!String.IsNullOrEmpty(entry.Text))
                {
                    if (String.IsNullOrEmpty(entry.Text))
                    {
                        Console.WriteLine(String.Concat(
                        time,
                        indentStrings[entry.IndentLevel],
                        entry.Text));
                    }
                    else
                    {
                        Console.WriteLine(String.Concat(
                            time,
                            indentStrings[entry.IndentLevel],
                            entry.Location,
                            " - ",
                            entry.Text));
                    }
                }
            }

            public void Start(LogEntry entry)
            {
                m_startTime = DateTime.Now;
            }

            public void Stop() { }
        }

        private static int Main(string[] args)
        {
            object consoleResourceUserObject = new object();
            int retval = 0;
            Console.WriteLine("StepBro console application. Type 'stepbro --help' to show the help text.");

            m_commandLineOptions = StepBro.Core.General.CommandLineParser.Parse<CommandLineOptions>(null, args);

            if (m_commandLineOptions.HasParsingErrors)
            {
                return -1;
            }

            try
            {
                StepBroMain.Initialize();

                if (m_commandLineOptions.Verbose)
                {
                    var logSinkManager = StepBro.Core.Main.GetService<ILogSinkManager>();
                    logSinkManager.Add(new ConsoleLogSink());
                }

                if (!String.IsNullOrEmpty(m_commandLineOptions.InputFile))
                {
                    if (m_commandLineOptions.Verbose) Console.WriteLine("Filename: {0}", m_commandLineOptions.InputFile);
                    var file = StepBroMain.LoadScriptFile(consoleResourceUserObject, m_commandLineOptions.InputFile);

                    var parsingSuccess = StepBroMain.ParseFiles(true);
                    if (parsingSuccess)
                    {
                        if (!String.IsNullOrEmpty(m_commandLineOptions.TargetElement))
                        {
                            IFileElement element = StepBroMain.TryFindFileElement(m_commandLineOptions.TargetElement);
                            if (element != null && element is IFileProcedure)
                            {
                                var procedure = element as IFileProcedure;
                                object[] arguments = m_commandLineOptions?.Arguments.Select(
                                    (a) => StepBroMain.ParseExpression(procedure?.ParentFile, a)).ToArray();
                                try
                                {
                                    object result = StepBroMain.ExecuteProcedure(procedure, arguments);
                                    if (result != null)
                                    {
                                        Console.WriteLine("Procedure execution ended. Result: " + result.ToString());
                                    }
                                    else
                                    {
                                        Console.WriteLine("Procedure execution ended.");
                                    }
                                }
                                catch (TargetParameterCountException)
                                {
                                    retval = -1;
                                    Console.WriteLine("Error: The number of arguments does not match the target procedure.");
                                }
                            }
                            else if (element != null && element is ITestList)
                            {
                                throw new NotImplementedException("Handling of test list tarteg not implemented.");
                            }
                            else
                            {
                                retval = -1;
                                if (element == null)
                                {
                                    Console.WriteLine($"Error: File element named '{m_commandLineOptions.TargetElement} was not found.");
                                }
                                else
                                {
                                    Console.WriteLine($"Error: File element type for '{m_commandLineOptions.TargetElement} cannot be used as an execution target.");
                                }
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Parsing errors!");
                        foreach (var err in file.Errors.GetList())
                        {
                            if (!err.JustWarning)
                            {
                                Console.WriteLine($"    Line {err.Line}: {err.Message}");
                            }
                        }
                        retval = -1;
                    }
                }
                else
                {
                    // If no file should be opened, what then?
                    retval = -1;
                    Console.WriteLine("Error: File could not be opened.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.GetType().Name}, {ex.Message}");
                retval = -1;
            }
            finally
            {
                StepBroMain.Deinitialize();
            }

            if (m_commandLineOptions.AwaitKeypress)
            {
                Console.WriteLine("<press any key to continue>");
                while (!Console.KeyAvailable)
                {
                    System.Threading.Thread.Sleep(25);
                }
                Console.ReadKey();
            }
            return retval;
        }
    }
}
