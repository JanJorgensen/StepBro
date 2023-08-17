using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data.Report
{
    public class ReportTestSummary : ReportData
    {
        private List<Tuple<string,ProcedureResult>> m_procedureResults = new List<Tuple<string, ProcedureResult>>();
        public ReportTestSummary(DateTime timestamp) : base(timestamp, ReportDataType.TestSummary)
        {
        }

        public void AddResult(string reference, ProcedureResult result)
        {
            m_procedureResults.Add(new Tuple<string, ProcedureResult>(reference, result));
        }

        public IEnumerable<Tuple<string, ProcedureResult>> GetResults()
        {
            foreach (var r in m_procedureResults) 
            { 
                yield return r; 
            }
        }
    }
}
