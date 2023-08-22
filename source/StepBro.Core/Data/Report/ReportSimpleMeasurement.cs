﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data.Report
{
    public class ReportSimpleMeasurement : ReportData
    {
        private readonly string m_id;
        private readonly string m_instance;
        private readonly string m_unit;
        private readonly object m_value;

        public ReportSimpleMeasurement(DateTime timestamp, string ID, string instance, string unit, object value) : base(timestamp, ReportDataType.SimpleMeasurement)
        {
            m_id = ID;
            this.m_instance = instance;
            this.m_unit = unit;
            this.m_value = value;
        }

        public string ID => m_id;

        public string Instance => m_instance;

        public string Unit => m_unit;

        public object Value => m_value;

        public override string ToString()
        {
            return String.Format("Measurement '{0}': {1}", this.ID, StringUtils.ObjectToString(this.Value));
        }
    }
}
