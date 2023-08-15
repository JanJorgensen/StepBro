using System;
using System.Collections.Generic;
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
        private readonly string m_id;
        private readonly List<ReportGroup> m_groups = new List<ReportGroup>();
        private ReportGroup m_currentGroup = null;

        public event EventHandler Disposing;

        public DataReport(string id)
        {
            m_id = id;
        }

        public string ID { get { return m_id; } }

        public IEnumerable<ReportData> ListData()
        {
            foreach (var g in m_groups)
            {
                foreach (var d in g.ListData()) yield return d;
            }
        }

        public long GroupCount { get { return (long)m_groups.Count; } }

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
            LogEntry logStart = null;
            if (context != null && context.Logger is LoggerScope)
            {
                logStart = ((LoggerScope)(context.Logger)).Log(LogEntry.Type.Normal, $"Starting report group \"{name}\". {description}");
            }
            m_currentGroup = new ReportGroup(name, description, logStart);
            m_groups.Add(m_currentGroup);
        }

        public void AddSection([Implicit] ICallContext context, string header, string subheader = "")
        {
            this.AddData(context, new ReportGroupSection(header, subheader));
        }

        public void DumpToLog([Implicit] ICallContext context)
        {
            if (context.LoggingEnabled)
            {
                var logger = context.Logger;
                logger.Log("<some data to go>");
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
