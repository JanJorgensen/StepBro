using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.HostSupport.Models
{
    public class ChronoListViewDynamicSettings
    {
        private bool m_valueChanged = false;
        private DateTime m_zeroTime;
        private ChronoListViewTimestampFormat m_timeFormat = ChronoListViewTimestampFormat.Seconds;
        private int m_timestampWidth = 0;   // The width of the widest seen timestamp.
        private int m_lineHeaderWidth = 0;    // The right side of the widest line header (timestamp and type)

        public bool ValueChanged()
        {
            if (m_valueChanged)
            {
                m_valueChanged = false;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Reset()
        {
            m_timestampWidth = 0;
            m_lineHeaderWidth = 0;
        }

        public DateTime ZeroTime
        {
            get { return m_zeroTime; }
            set
            {
                if (value != m_zeroTime)
                {
                    m_zeroTime = value;
                    m_valueChanged = true;
                }
            }
        }

        public ChronoListViewTimestampFormat TimeFormat
        {
            get { return m_timeFormat; }
            set
            {
                if (value != m_timeFormat)
                {
                    m_timeFormat = value;
                    m_valueChanged = true;
                }
            }
        }

        public int TimeStampWidth
        {
            get { return m_timestampWidth; }
            set
            {
                if (value != m_timestampWidth)
                {
                    m_timestampWidth = value;
                    m_valueChanged = true;
                }
            }
        }
        public int LineHeaderWidth
        {
            get { return m_lineHeaderWidth; }
            set
            {
                if (value != m_lineHeaderWidth)
                {
                    m_lineHeaderWidth = value;
                    m_valueChanged = true;
                }
            }
        }
    }
}
