using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Data.Report;
using StepBro.Core.Execution;
using StepBro.Core.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace StepBro.Core.Addons
{
    [Public]
    public class OutputAzurePipelineFormatAddon : IOutputFormatterTypeAddon
    {
        public string ShortName { get { return "AzureLog"; } }

        public string FullName { get { return "OutputFormatter." + this.ShortName; } }

        public string Description { get { return "Azure Pipelines formatter."; } }

        public OutputType FormatterType { get { return OutputType.Text; } }

        public IOutputFormatter Create(bool createHighLevelLogSections, ITextWriter writer = null)
        {
            return new Outputter(createHighLevelLogSections, writer);
        }

        private class Outputter : IOutputFormatter, ITextWriter
        {
            bool m_createHighLevelLogSections;
            readonly ITextWriter m_writer;

            public Outputter(bool createHighLevelLogSections, ITextWriter writer)
            {
                m_createHighLevelLogSections = createHighLevelLogSections;
                m_writer = (writer != null) ? writer : this;
            }

            public bool WriteLogEntry(LogEntry entry, DateTime zero)
            {
                var txt = entry.ToClearText(zero, false);
                var prefix = "            ";
                if (txt != null)
                {
                    switch (entry.EntryType)
                    {
                        case Logging.LogEntry.Type.PreHighLevel:
                            prefix = "##[section]            ";
                            break;
                        case Logging.LogEntry.Type.Error:
                        case Logging.LogEntry.Type.Failure:
                            prefix = "##[error]   ";
                            break;
                        case Logging.LogEntry.Type.UserAction:
                        case Logging.LogEntry.Type.System:
                            prefix = "##[command]            ";
                            break;
                        default:
                            break;
                    }
                    m_writer.WriteLine(prefix + txt);
                    return true;
                }
                return false;
            }

            public void WriteReport(DataReport report)
            {
                bool oldCcreateHighLevelLogSections = m_createHighLevelLogSections;
                m_createHighLevelLogSections = false;
                try
                {
                    if (report.Summary != null)
                    {
                        var summary = report.Summary;
                        m_writer.WriteLine("##[group] SUMMARY - " + summary.ToString());
                        var width1 = summary.ListResults().Select(r => r.Item1.Length).Max() + 2;
                        foreach (var result in summary.ListResults())
                        {
                            if (result.Item2 != null)
                            {
                                m_writer.WriteLine($"    {result.Item1}:{new String(' ', width1 - result.Item1.Length)}{result.Item2.ToString(false)}");
                            }
                            else
                            {
                                m_writer.WriteLine($"    {result.Item1}:{new String(' ', width1 - result.Item1.Length)}MISSING");
                            }
                        }
                        m_writer.WriteLine("##[endgroup]");
                        m_writer.WriteLine("");     // Empty line
                    }

                    Stack<string> indent = new Stack<string>();
                    indent.Push("");    // Root Indent.

                    foreach (var group in report.ListGroups())
                    {
                        m_writer.WriteLine("##[group] " + group.Name);
                        if (!String.IsNullOrEmpty(group.Description))
                        {
                            m_writer.WriteLine(indent.Peek() + "  Description: " + group.Description);
                        }
                        if (group.ListData().Count(d => d.Type == ReportDataType.ExpectResult || d.Type == ReportDataType.SimpleMeasurement || d.Type == ReportDataType.Exception) > 0)
                        {
                            indent.Push(indent.Peek() + "    ");

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
                                            m_writer.WriteLine("##[section]" + indent.Peek() + sec.Item1.Header);
                                        }
                                        else
                                        {
                                            m_writer.WriteLine(indent.Peek() + "");
                                        }
                                        indent.Push(indent.Peek() + "    ");
                                        this.WriteSection(indent, sec.Item2);
                                        indent.Pop();
                                    }
                                }
                            }
                            else
                            {
                                this.WriteSection(indent, group.ListData().ToList());
                            }
                            indent.Pop();
                        }
                        else
                        {
                            m_writer.WriteLine(indent.Peek() + "    No data.");
                        }
                        m_writer.WriteLine("##[endgroup]");
                        m_writer.WriteLine("");     // Empty line

                        if (group.LogEnd != null && group.LogEnd.Id != group.LogStart.Id)
                        {
                            m_writer.WriteLine("##[group] " + group.Name + " - execution log");
                            indent.Push(indent.Peek() + "    ");

                            var entry = group.LogStart;
                            var endId = group.LogEnd.Id;
                            var timeZero = entry.Timestamp;
                            while (entry.Id <= endId)
                            {
                                WriteLogEntry(entry, timeZero);
                                entry = entry.Next;
                            }

                            indent.Pop();
                            m_writer.WriteLine("##[endgroup]");
                            m_writer.WriteLine("");     // Empty line
                        }
                        else if (group.LogEnd == null) // If group.LogEnd is null, a fatal error has occurred during execution
                        {
                            m_writer.WriteLine("##[error] An error has occurred. Dumping log.");
                            var entry = group.LogStart;
                            while (entry != null)
                            {
                                WriteLogEntry(entry, entry.Timestamp);
                                entry = entry.Next;
                            }
                        }
                    }

                    // RENAME FILE SO WE DO NOT OVERWRITE REPORT
                    System.IO.File.Move("report.sbr", $"report-{DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss")}.sbr");
                }
                finally
                {
                    m_createHighLevelLogSections = oldCcreateHighLevelLogSections;
                }
            }


            private void WriteSection(Stack<string> indent, List<ReportData> sectionData)
            {
                var expects = sectionData.Where(d => d.Type == ReportDataType.ExpectResult).Select(d => d as ExpectResultData).ToList();
                var measurements = sectionData.Where(d => d.Type == ReportDataType.SimpleMeasurement).Select(d => d as ReportSimpleMeasurement).ToList();
                var exceptionEntry = sectionData.FirstOrDefault(d => d.Type == ReportDataType.Exception) as ReportException;

                if (expects.Count > 0)
                {
                    m_writer.WriteLine("##[section]" + indent.Peek() + "Test Results");

                    indent.Push(indent.Peek() + "    ");
                    var indentString = indent.Peek();
                    foreach (var data in expects)
                    {
                        m_writer.WriteLine(indent.Peek() + data.FormatString());
                    }
                    indent.Pop();
                }
                if (measurements.Count > 0)
                {
                    m_writer.WriteLine("##[section]"+ indent.Peek() + "Measurements");

                    indent.Push(indent.Peek() + "    ");
                    var indentString = indent.Peek();
                    foreach (var data in measurements)
                    {
                        m_writer.WriteLine(indent.Peek() + data.FormatString());
                    }
                    indent.Pop();
                }

                if (exceptionEntry != null)
                {
                    m_writer.WriteLine("##[section]" + indent.Peek() + "Exception");

                    indent.Push(indent.Peek() + "    ");
                    var indentString = indent.Peek();
                    var exception = exceptionEntry.Exception;
                    if (exception is UnhandledExceptionInScriptException)
                    {
                        exception = exception.InnerException;
                    }
                    m_writer.WriteLine(indent.Peek() + "Type: " + exception.GetType().FullName);
                    m_writer.WriteLine(indent.Peek() + "Message: " + exception.Message);
                    if (!String.IsNullOrEmpty(exception.HelpLink))
                    {
                        m_writer.WriteLine(indent.Peek() + "Help: " + exception.HelpLink);
                    }

                    string[] stack = UnhandledExceptionInScriptException.CreatePrintableCallStack(exception, exceptionEntry.ScriptCallstack);
                    if (stack != null && stack.Length > 0)
                    {
                        m_writer.WriteLine(indent.Peek() + "Call Stack: ");
                        foreach (var s in stack)
                        {
                            m_writer.WriteLine(indent.Peek() + "    " + s.Trim());
                        }
                    }

                    indent.Pop();
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
