//using CommunityToolkit.Mvvm.ComponentModel;
//using StepBro.Core.Data;
//using System;
//using System.Drawing;
//using System.Linq;
//using static System.Net.Mime.MediaTypeNames;

//namespace StepBro.HostSupport.Models
//{
//    public class ChronoListViewPortModel<TViewEntryType> :
//        ObservableObject,
//        ChronoListViewPortModel<TViewEntryType>.IView 
//        where TViewEntryType : class, ITimestampedViewEntry
//    {
//        public interface IView
//        {
//            int HorizontalScrollPosition { get; }
//            DynamicViewSettings ViewSettings { get; }
//            //Font NormalFont { get; }
//            //Brush NormalTextColor { get; }
//        }

//        public enum TimestampFormat
//        {
//            Seconds,
//            SecondsDelta,
//            HoursMinutesSeconds,
//            LocalTime,
//            LocalDateTime
//        }

//        private IElementIndexer<TViewEntryType> m_source = null;
//        private int m_lineHeight = 20;
//        private TViewEntryType[] m_viewEntries = new TViewEntryType[200];
//        private int m_viewEntryCount = 0;
//        private int m_horizontalScrollPosition = 0;
//        private Point m_mouseDownLocation = new Point();
//        private DateTime m_lastViewScroll = DateTime.MinValue;

//        private long m_topIndex = 0L;
//        private long m_lastShown = -1L;

//        public ChronoListViewPortModel() : base()
//        {
//        }

//        public void SetDataSource(IChronoListViewer viewer)
//        {
//            m_viewer = viewer;
//            m_source = viewer.Source;
//        }

//        public class MouseOnLineEventArgs : MouseEventArgs
//        {
//            private readonly int m_line;
//            private readonly long m_index;

//            public MouseOnLineEventArgs(MouseEventArgs args, int line, long index) : base(args.Button, args.Clicks, args.X, args.Y, args.Delta)
//            {
//                m_line = line;
//                m_index = index;
//            }

//            public int Line { get { return m_line; } }
//            public long Index { get { return m_index; } }
//        }
//        public delegate void MouseOnLineEventHandler(object sender, MouseOnLineEventArgs e);

//        public event MouseOnLineEventHandler MouseDownOnLine;
//        public event MouseOnLineEventHandler MouseUpOnLine;

//        public int HorizontalScrollPosition
//        {
//            get { return m_horizontalScrollPosition; }
//            set
//            {
//                m_horizontalScrollPosition = value;
//            }
//        }

//        public DynamicViewSettings ViewSettings { get { return m_viewSettings; } }

//        public Font NormalFont { get { return this.Font; } }

//        public Brush NormalTextColor { get { return Brushes.White; } }

//        public int MaxLinesVisible { get { return this.Height / m_lineHeight; } }
//        public int MaxLinesPartlyVisible { get { return (this.Height + (m_lineHeight - 1)) / m_lineHeight; } }

//        public long TopEntryIndex { get { return m_topIndex; } }
//        public long LastShownEntryIndex { get { return m_lastShown; } }

//        public DateTime LastViewScrollTime { get { return m_lastViewScroll; } }

//        public bool ViewJustScrolled { get { return (DateTime.UtcNow - m_lastViewScroll) < TimeSpan.FromMilliseconds(500); } }

//        public bool IsViewFilled()
//        {
//            return (m_viewEntryCount >= this.MaxLinesPartlyVisible);
//        }
//    }
//}
