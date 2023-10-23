using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data.Report
{
    public class ReportException : ReportData
    {
        private readonly Exception m_exception;
        private readonly string[] m_scriptCallstack;

        public ReportException(DateTime timestamp, Exception ex, string[] scriptCallstack) : base(timestamp, ReportDataType.Exception)
        {
            this.m_exception = ex;
            this.m_scriptCallstack = scriptCallstack;
        }


        public Exception Exception => m_exception;

        public string[] ScriptCallstack => m_scriptCallstack;


        public override string ToString()
        {
            return String.Format("Unhandled Exception " + this.FormatString());
        }

        public string FormatString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(m_exception.GetType().Name);
            if (m_scriptCallstack != null && m_scriptCallstack.Length > 0)
            {
                sb.Append(" at ");
                sb.Append(m_scriptCallstack[0]);
            }
            sb.Append(": ");
            sb.Append(m_exception.Message);
            return sb.ToString();
        }
    }
}
