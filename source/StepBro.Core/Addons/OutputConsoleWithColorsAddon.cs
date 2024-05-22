using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Data.Report;
using StepBro.Core.Execution;
using StepBro.Core.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StepBro.Core.Addons
{
    [Public]
    public class OutputConsoleWithColorsAddon : IOutputFormatterTypeAddon
    {
        static public string Name { get { return "Console"; } }

        public string ShortName { get { return Name; } }

        public string FullName { get { return "OutputFormatter." + this.ShortName; } }

        public string Description { get { return "Text output in colors on console."; } }

        public OutputType FormatterType { get { return OutputType.Console; } }

        public IOutputFormatter Create(OutputFormatOptions options, ITextWriter writer = null)
        {
            if (writer != null)
            {
                throw new NotSupportedException();
            }
            return new TextToConsoleFormatter(options);
        }

        public IOutputFormatter Create(ITextWriter writer)
        {
            throw new NotImplementedException();
        }

        public class TextToConsoleFormatter : IOutputFormatter, ITextWriter
        {
            OutputFormatOptions m_options;
            ITextWriter m_writer = null;

            public TextToConsoleFormatter(OutputFormatOptions options, ITextWriter writer = null)
            {
                m_options = options;
                m_writer = (writer != null ? writer : this);
            }

            public void Dispose()
            {
            }

            public void Flush()
            {
                // No action needed.
            }

            public bool WriteLogEntry(LogEntry entry, DateTime zero)
            {
                var txt = entry.ToClearText(zero, false, false);
                if (txt != null)
                {
                    switch (entry.EntryType)
                    {
                        case Logging.LogEntry.Type.Pre:
                        case Logging.LogEntry.Type.PreHighLevel:
                        case Logging.LogEntry.Type.TaskEntry:
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            break;
                        case Logging.LogEntry.Type.Normal:
                        case Logging.LogEntry.Type.Post:
                            Console.ForegroundColor = ConsoleColor.White;
                            break;
                        case Logging.LogEntry.Type.Async:
                        case Logging.LogEntry.Type.CommunicationOut:
                        case Logging.LogEntry.Type.CommunicationIn:
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            break;
                        case Logging.LogEntry.Type.Error:
                        case Logging.LogEntry.Type.Failure:
                            Console.ForegroundColor = ConsoleColor.Red;
                            break;
                        case Logging.LogEntry.Type.UserAction:
                            Console.ForegroundColor = ConsoleColor.Blue;
                            break;
                        case Logging.LogEntry.Type.Detail:
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            break;
                        case Logging.LogEntry.Type.System:
                            Console.ForegroundColor = ConsoleColor.Blue;
                            break;
                        default:
                            Console.ForegroundColor = ConsoleColor.White;
                            break;
                    }
                    m_writer.WriteLine(txt);
                    Console.ForegroundColor = ConsoleColor.White;
                    return true;
                }
                return false;
            }

            public void WriteReport(DataReport report)
            {
                try
                {
                    // WRITE GROUPS
                    bool firstGroup = true;
                    foreach (var group in report.ListGroups())
                    {
                        // If it is not the first group, we write a couple empty lines to separate the groups
                        if (firstGroup)
                        {
                            firstGroup = false;
                        }
                        else
                        {
                            for (int i = 0; i < 3; i++) m_writer.WriteLine(""); // Empty lines
                        }

                        m_writer.WriteLine($"--- GROUP: {group.Name} ---");
                        if (!String.IsNullOrEmpty(group.Description))
                        {
                            m_writer.WriteLine($"Description:{group.Description}");
                        }

                        // WRITE RESULTS IN GROUP
                        if (group.ListData().Count(d => d.Type == ReportDataType.ExpectResult || d.Type == ReportDataType.SimpleMeasurement || d.Type == ReportDataType.Exception) > 0)
                        {
                            if (group.ListData().Any(d => d.Type == ReportDataType.Section))
                            {
                                var sections = new List<Tuple<ReportGroupSection, List<ReportData>>>();
                                ReportGroupSection section = null;
                                var sectionData = new List<ReportData>();
                                foreach (var data in group.ListData())
                                {
                                    switch (data.Type)
                                    {
                                        case ReportDataType.Section:
                                            if (sectionData.Count() > 0)
                                            {
                                                sections.Add(new Tuple<ReportGroupSection, List<ReportData>>(section, sectionData));
                                            }
                                            section = data as ReportGroupSection;
                                            sectionData = new List<ReportData>();
                                            continue;
                                        case ReportDataType.ExpectResult:
                                        case ReportDataType.SimpleMeasurement:
                                        case ReportDataType.Exception:
                                            sectionData.Add(data);
                                            break;
                                        case ReportDataType.TextLine:
                                        case ReportDataType.DataTable:
                                        case ReportDataType.TestSummary:
                                        default:
                                            break;
                                    }
                                }
                                if (section != null && sectionData.Count > 0)   // Add last section, if not empty.
                                {
                                    sections.Add(new Tuple<ReportGroupSection, List<ReportData>>(section, sectionData));
                                }

                                foreach (var sec in sections)
                                {
                                    if (sec.Item2 != null && sec.Item2.Count() > 0)     // Only if any data.
                                    {
                                        if (sec.Item1 != null)
                                        {
                                            m_writer.WriteLine($"Section - {sec.Item1.Header}");
                                        }
                                        else
                                        {
                                            m_writer.WriteLine("");     // Empty line
                                        }
                                        this.WriteSection(sec.Item2);
                                    }
                                }
                            }
                            else
                            {
                                this.WriteSection(group.ListData().ToList());
                            }
                        }
                        else
                        {
                            m_writer.WriteLine("No data.");
                        }

                        m_writer.WriteLine("");     // Empty line

                        // WRITE EXECUTION LOG
                        if (group.LogEnd != null && group.LogEnd.Id != group.LogStart.Id)
                        {
                            m_writer.WriteLine("Execution Log:");

                            var entry = group.LogStart;
                            var endId = group.LogEnd.Id;
                            var timeZero = entry.Timestamp;
                            while (entry.Id <= endId)
                            {
                                WriteLogEntry(entry, timeZero);
                                entry = entry.Next;
                            }
                        }
                        else if (group.LogEnd == null) // If group.LogEnd is null, a fatal error has occurred during execution
                        {
                            m_writer.WriteLine("An error has occurred. Dumping log.");
                            var entry = group.LogStart;
                            while (entry != null)
                            {
                                WriteLogEntry(entry, entry.Timestamp);
                                entry = entry.Next;
                            }
                        }
                    }

                    // WRITE SUMMARY
                    // This is done at the end of the report on console so it is easier to get a quick overview
                    if (report.Summary != null)
                    {
                        for (int i = 0; i < 3; i++) m_writer.WriteLine(""); // Empty lines
                        var summary = report.Summary;
                        m_writer.WriteLine("--- SUMMARY ---");
                        foreach (var result in summary.ListResults())
                        {
                            m_writer.WriteLine($"{result.Item1}: {(result.Item2 != null ? result.Item2.ToString(false) : "MISSING")}");
                        }
                    }

                    // RENAME FILE SO WE DO NOT OVERWRITE REPORT
                    System.IO.File.Move("report.sbr", $"report-{DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss")}.sbr");
                }
                catch (Exception)
                {
                    throw new NotImplementedException("Test reports are experimental for console format and are not fully implemented yet.");
                }
            }

            private void WriteSection(List<ReportData> sectionData)
            {
                var expects = sectionData.Where(d => d.Type == ReportDataType.ExpectResult).Select(d => d as ExpectResultData).ToList();
                var measurements = sectionData.Where(d => d.Type == ReportDataType.SimpleMeasurement).Select(d => d as ReportSimpleMeasurement).ToList();
                var exceptionEntry = sectionData.FirstOrDefault(d => d.Type == ReportDataType.Exception) as ReportException;

                // Write Test Results
                if (expects.Count > 0)
                {
                    m_writer.WriteLine("Test Results:");

                    foreach (var data in expects)
                    {
                        m_writer.WriteLine(data.FormatString());
                    }
                }

                // Write Measurements
                if (measurements.Count > 0)
                {
                    m_writer.WriteLine("Measurements");

                    foreach (var data in measurements)
                    {
                        m_writer.WriteLine(data.FormatString());
                    }
                }

                // Write Exceptions
                if (exceptionEntry != null)
                {
                    m_writer.WriteLine("Exception");

                    var exception = exceptionEntry.Exception;
                    if (exception is UnhandledExceptionInScriptException)
                    {
                        exception = exception.InnerException;
                    }
                    m_writer.WriteLine("Type: " + exception.GetType().FullName);
                    m_writer.WriteLine("Message: " + exception.Message);
                    if (!String.IsNullOrEmpty(exception.HelpLink))
                    {
                        m_writer.WriteLine("Help: " + exception.HelpLink);
                    }

                    string[] stack = UnhandledExceptionInScriptException.CreatePrintableCallStack(exception, exceptionEntry.ScriptCallstack);
                    if (stack != null && stack.Length > 0)
                    {
                        m_writer.WriteLine("Call Stack: ");
                        foreach (var s in stack)
                        {
                            m_writer.WriteLine(s.Trim());
                        }
                    }
                }
            }

            void ITextWriter.Write(string text)
            {
                System.Console.Write(text);

                using (StreamWriter sw = System.IO.File.AppendText("report.sbr"))
                {
                    sw.Write(text);
                }
            }

            void ITextWriter.WriteLine(string text)
            {
                System.Console.WriteLine(text);

                using (StreamWriter sw = System.IO.File.AppendText("report.sbr"))
                {
                    sw.WriteLine(text);
                }
            }
        }
    }
}