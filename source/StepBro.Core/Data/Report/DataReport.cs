using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Antlr4.Runtime.Atn;
using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Data.Report;
using StepBro.Core.Execution;
using StepBro.Core.General;
using StepBro.Core.Logging;

namespace StepBro.Core
{
    [Public]
    public class DataReport : IScriptDisposable
    {
        private static List<DataReport> m_savedReports = new List<DataReport>();
        private bool m_isDisposed = false;
        private bool m_isOpen = true;
        private readonly string m_type;
        private readonly string m_title;
        private ReportTestSummary m_summary = null;
        private readonly List<ReportGroup> m_groups = new List<ReportGroup>();
        private ReportGroup m_currentGroup = null;

        public event EventHandler Disposing;

        public DataReport(string type, string title)
        {
            m_type = type;
            m_title = title;
        }

        public string Type { get { return m_type; } }
        public string Title { get { return m_title; } }

        public IEnumerable<ReportGroup> ListGroups()
        {
            foreach (var g in m_groups)
            {
                yield return g;
            }
        }

        public IEnumerable<ReportData> ListData()
        {
            foreach (var g in m_groups)
            {
                foreach (var d in g.ListData()) yield return d;
            }
        }

        public long GroupCount { get { return (long)m_groups.Count; } }

        public ReportTestSummary Summary { get { return m_summary; } }

        public void AddData([Implicit] ICallContext context, ReportData data)
        {
            if (m_isDisposed && !m_isOpen)
            {
                throw new OperationNotAllowedException("This report has already been closed.");
            }
            if (m_currentGroup == null)
            {
                this.StartGroup(context, "default");
            }
            if (m_currentGroup.IsLocked)
            {
                context.ReportError("Trying to write data to a locked group.");
            }
            m_currentGroup.AddData(data);
        }

        public void StartGroup([Implicit] ICallContext context, string name, string description = null)
        {
            if (m_currentGroup != null)
            {
                if (context != null && context.Logger is LoggerScope)
                {
                    var end = ((LoggerScope)context.Logger).Logger.GetLast();
                    m_currentGroup.Lock(end);
                }
                else
                {
                    m_currentGroup.Lock(null);
                }
                m_currentGroup = null;
            }
            ILogEntry logStart = null;
            if (context != null && context.Logger is LoggerScope)
            {
                if (String.IsNullOrEmpty(description)) description = "";
                else description = " " + description;
                logStart = context.Logger.Log($"Starting report group \"{name}\".{description}");
            }
            m_currentGroup = new ReportGroup(name, description, (LogEntry)logStart);
            m_groups.Add(m_currentGroup);
        }

        public void AddSection([Implicit] ICallContext context, string header, string subheader = "")
        {
            DateTime timestamp = DateTime.Now;
            if (context != null && context.LoggingEnabled)
            {
                var subheaderText = String.IsNullOrEmpty(subheader) ? "." : (", " + subheader);
                var logentry = context.Logger.Log(String.Format("Adding section \"{0}\" to report{1}", header, subheaderText));
                timestamp = logentry.Timestamp;
            }
            this.AddData(context, new ReportGroupSection(timestamp, header, subheader));
        }

        public void AddUnhandledExeception(ICallContext context, Exception ex, string[] scriptCallstack)
        {
            var data = new ReportException(DateTime.Now, ex, scriptCallstack);
            this.AddData(context, data);
        }

        public ReportTestSummary CreateTestSummary()
        {
            m_summary = new ReportTestSummary(DateTime.Now);
            return m_summary;
        }

        public ReportDataTable CreateDataTable([Implicit] ICallContext context, string title, string subttitle, ReportDataTable.DataSetDirection dataSetDirection)
        {
            var table = new ReportDataTable(DateTime.Now, title, subttitle, dataSetDirection);
            this.AddData(context, table);
            return table;
        }

        static public void AddMeasurement(ICallContext context, DataReport report, string id, string instance, string unit, object value)
        {
            DateTime timestamp;
            if (context.LoggingEnabled)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("Measurement '");
                sb.Append(id);
                sb.Append("'");
                if (!String.IsNullOrEmpty(instance))
                {
                    sb.Append(" - ");
                    sb.Append(instance);
                }
                sb.Append(": ");
                sb.Append(StringUtils.ObjectToString(value));
                if (!String.IsNullOrEmpty(unit))
                {
                    sb.Append(" [");
                    sb.Append(unit);
                    sb.Append("]");
                }
                timestamp = context.Logger.Log(sb.ToString()).Timestamp;
            }
            else
            {
                timestamp = DateTime.Now;
            }
            if (report != null)
            {
                report.AddData(context, new ReportSimpleMeasurement(timestamp, id, instance, unit, value));
            }
        }

        public void AddMeasurement([Implicit] ICallContext context, string id, string instance, string unit, object value)
        {
            AddMeasurement(context, this, id, instance, unit, value);
        }

        public void DumpToLog([Implicit] ICallContext context)
        {
            if (context.LoggingEnabled)
            {
                var logger = context.Logger;
                logger.Log($"{this.Type}: \"{this.m_title}\"");
                foreach (var group in m_groups)
                {
                    logger.Log($"Group: {group.Name}. {group.Description}");
                    foreach (var r in group.ListData())
                    {
                        logger.Log("        " + r.ToString());
                    }
                }

                if (m_summary != null && m_summary.ListResults().Any())
                {
                    logger.Log("Summary");
                    foreach (var r in m_summary.ListResults())
                    {
                        logger.Log("        " +  r.Item1 + " - " + r.Item2.ToString(r.Item2.Verdict > Verdict.Pass));
                    }
                }
            }
        }

        public void Close([Implicit] ICallContext context)
        {
            if (!m_isDisposed && m_isOpen)
            {
                if (m_currentGroup != null)
                {
                    if (context != null && context.Logger is LoggerScope)
                    {
                        var end = ((LoggerScope)context.Logger).Logger.GetLast();
                        m_currentGroup.Lock(end);
                    }
                    else
                    {
                        m_currentGroup.Lock(null);
                    }
                }
                m_isOpen = false;
            }
        }

        public void Dispose()
        {
            if (!m_isDisposed)
            {
                this.Disposing?.Invoke(this, EventArgs.Empty);
                this.Close(null);
                m_isDisposed = true;
            }
        }

        public void Dispose(ICallContext context)
        {
            if (!m_isDisposed)
            {
                this.Close(context);
                if (context is ScriptCallContext)
                {
                    var registeredReport = (context as ScriptCallContext).TryGetReport();
                    if (registeredReport != null && Object.ReferenceEquals(registeredReport, this))
                    {
                        (context as ScriptCallContext).RemoveReport();
                    }
                }
                this.Dispose();
            }
        }
    }
}
