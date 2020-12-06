using System;
using System.Collections.Generic;
using StepBro.Core.Api;
using StepBro.Core.Data.Report;
using StepBro.Core.Execution;
using StepBro.Core.General;

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

        public void AddData(ReportData data)
        {
            if (m_isClosed)
            {
                throw new ObjectDisposedException("This report has already been disposed.");
            }
            if (m_currentGroup == null)
            {
                this.StartGroup("default");
            }
            m_currentGroup.AddData(data);
        }

        public void StartGroup(string name, string description = null)
        {
            m_currentGroup = new ReportGroup(name, description);
            m_groups.Add(m_currentGroup);
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
                    (context as ScriptCallContext).RemoveReport(this.ID);
                }
                this.Dispose();
            }
        }
    }
}
