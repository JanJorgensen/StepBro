using StepBro.Core.Logging;
using System;
using System.Collections.Generic;

namespace StepBro.Core.Data.Report
{
    public interface IReportManager
    {
        event EventHandler ReportAdded;
        int Count { get; }
        DataReport GetReport(int id);
        IEnumerable<DataReport> ListReports();
    }


    internal class ReportManager : ServiceBase<IReportManager, ReportManager>, IReportManager
    {
        private List<DataReport> m_reports = new List<DataReport>();

        public ReportManager(out IService serviceAccess) :
            base("ReportManager", out serviceAccess, typeof(ILogger))
        {
        }

        public void AddReport(DataReport report)
        {
            m_reports.Add(report);
            this.ReportAdded?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler ReportAdded;

        public int Count { get {  return m_reports.Count; } }

        public DataReport GetReport(int id)
        {
            return m_reports[id];
        }

        public IEnumerable<DataReport> ListReports()
        {
            yield break;
        }
    }
}
