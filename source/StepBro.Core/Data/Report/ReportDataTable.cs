using StepBro.Core.Api;
using StepBro.Core.Execution;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StepBro.Core
{
    public class ReportDataTable : ReportData
    {
        public enum DataSetDirection { Row, Column }
        public class DataFieldtInfo
        {
            public string Title { get; private set; }
            public System.Type DataType { get; private set; }
            public DataFieldtInfo(string title, System.Type type)
            {
                this.Title = title;
                this.DataType = type;
            }
        }

        private string m_title;
        private string m_subtitle;
        private DataSetDirection m_direction;
        private List<DataFieldtInfo> m_fields = new List<DataFieldtInfo>();
        private List<List<object>> m_sets = new List<List<object>>();

        public ReportDataTable(DateTime timestamp, string title, string subttitle, DataSetDirection dataSetDirection) : base(timestamp, ReportDataType.DataTable)
        {
            m_title = title;
            m_subtitle = subttitle;
            m_direction = dataSetDirection;
        }

        public string Title { get { return m_title; } }
        public string Subtitle { get { return m_subtitle; } }

        public long FieldCount { get { return m_fields.Count; } }
        public long DataSetCount { get { return m_sets.Count; } }
        public DataSetDirection Direction { get { return m_direction; } }

        public void CreateHeaderField([Implicit] ICallContext context, string title, System.Type datatype)
        {
            if (this.IsStillOpen(context))
            {
                if (m_sets.Count > 0)
                {
                    m_fields.Add(new DataFieldtInfo(title, datatype));
                }
                else
                {
                    context?.ReportError("The data table has already some datasets, and therefore no more header fields can be added.");
                }
            }
        }

        public void AddDataSet([Implicit] ICallContext context, params object[] data)
        {
            if (this.IsStillOpen(context))
            {
                if (m_fields.Count == 0)
                {
                    context?.ReportError("The data table has no data fields defined.");
                }
                else if (m_fields.Count != data.Length)
                {
                    context?.ReportError("The specified dataset does not have the same number of values as the created fields in the table.");
                }
                else
                {
                    // TODO: check that the data values matches the field types.
                    m_sets.Add(data.ToList());
                }
            }
        }

        public DataFieldtInfo GetFieldInfo([Implicit] ICallContext context, long index)
        {
            if (index < 0L || (int)index > (m_fields.Count - 1))
            {
                return m_fields[(int)index];
            }
            else
            {
                context?.ReportError($"Index is out of range. Table has {m_fields.Count} fields.");
                return null;
            }
        }

        public object GetFieldValue([Implicit] ICallContext context, long field, long set)
        {
            if (field < 0L || (int)field > (m_fields.Count - 1))
            {
                if (set < 0L || (int)set > (m_sets.Count - 1))
                {
                    return m_sets[(int)set][(int)field];
                }
                else
                {
                    context?.ReportError($"Dataset index is out of range. Table has {m_sets.Count} datasets.");
                    return null;
                }
            }
            else
            {
                context?.ReportError($"Field index is out of range. Table has {m_fields.Count} fields.");
                return null;
            }
        }
    }
}
