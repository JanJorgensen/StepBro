using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using StepBro.Core.Api;
using StepBro.Core.Data.Report;
using StepBro.Core.Execution;
using StepBro.Core.General;
using StepBro.Core.Logging;

namespace StepBro.Core
{
    [Public]
    public class DataReport : IScriptDisposable
    {
        private bool m_isClosed = false;
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
            if (m_isClosed)
            {
                throw new ObjectDisposedException("This report has already been disposed.");
            }
            if (m_currentGroup == null)
            {
                this.StartGroup(context, "default");
            }
            m_currentGroup.AddData(data);
        }

        public void StartGroup([Implicit] ICallContext context, string name, string description = null)
        {
            ILogEntry logStart = null;
            if (context != null && context.Logger is LoggerScope)
            {
                logStart = context.Logger.Log($"Starting report group \"{name}\". {description}");
            }
            m_currentGroup = new ReportGroup(name, description, (LogEntry)logStart);
            m_groups.Add(m_currentGroup);
        }

        public void AddSection([Implicit] ICallContext context, string header, string subheader = "")
        {
            DateTime timestamp = DateTime.Now;
            if (context.LoggingEnabled)
            {
                var subheaderText = String.IsNullOrEmpty(subheader) ? "" : (", " + subheader);
                var logentry = context.Logger.Log(String.Format("Adding section \"{0}\" to report.{1}", header, subheaderText));
                timestamp = logentry.Timestamp;
            }
            this.AddData(context, new ReportGroupSection(timestamp, header, subheader));
        }

        public ReportTestSummary CreateTestSummary()
        {
            m_summary = new ReportTestSummary(DateTime.Now);
            return m_summary;
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

                if (m_summary != null && m_summary.GetResults().Any())
                {
                    logger.Log("Summary");
                    foreach (var r in m_summary.GetResults())
                    {
                        logger.Log("        " +  r.Item1 + " - " + r.Item2.ToString());
                    }
                }
            }
        }

        public void Dispose()
        {
            if (!m_isClosed)
            {
                this.Disposing?.Invoke(this, EventArgs.Empty);
                m_isClosed = true;
            }
        }

        public void Dispose(ICallContext context)
        {
            if (!m_isClosed)
            {
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
