using FastColoredTextBoxNS;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StepBro.Core.Controls
{
    public class DataViewControl : TextView
    {
        public class SingleLineDataPresenter
        {
            //private readonly object m_parent;
            private readonly string m_name;
            private string m_headerText;
            private readonly int m_nameIndent = 0;
            private int m_valueColumn;
            //private readonly PropertyInfo m_property = null;
            protected string m_currentValue = "";
            private bool m_isInvalidated = true;
            protected bool m_isUpdated = false;
            protected string m_newValue = "";
            protected DateTime m_lastUpdate;

            public SingleLineDataPresenter(string name, int indent, int valueColumn)
            {
                m_name = name;
                m_nameIndent = indent;
                m_valueColumn = valueColumn;
                m_lastUpdate = DateTime.Now;
                //m_property = m_parent?.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                this.SetupFromNameAndIndent();
            }

            private void SetupFromNameAndIndent()
            {
                m_valueColumn = Math.Max(m_valueColumn, m_nameIndent + m_name.Length + 3);
                m_headerText = new String(' ', m_nameIndent) + m_name + new string(' ', m_valueColumn - (m_name.Length + m_nameIndent));
                m_isInvalidated = true;
            }

            public bool IsInvalidated { get { return m_isInvalidated; } }

            public int ValueColumn { get { return m_valueColumn; } }

            public string Name { get { return m_name; } }

            public string HeaderText { get { m_isInvalidated = false; return m_headerText; } }

            public DateTime LastUpdate { get { return m_lastUpdate; } }

            public void UpdateValue(string value)
            {
                m_newValue = value;
                m_lastUpdate = DateTime.Now;
                m_isUpdated = true;
            }

            public virtual bool GetValue(ref string value, ref int lastWidth, bool forceGet = false)
            {
                if (forceGet || m_isUpdated || m_isInvalidated)
                {
                    m_isUpdated = false;
                    lastWidth = m_currentValue.Length;
                    value = m_newValue;
                    m_currentValue = m_newValue;
                    return true;
                }
                return false;

                //object propValue = m_property.GetValue(m_parent);
                //if (!Object.Equals(m_lastValue, propValue))
                //{
                //    m_lastValue = propValue;
                //    lastWidth = m_valueString.Length;
                //    m_valueString = propValue.ToString();
                //    value = m_valueString;
                //    m_lastUpdate = DateTime.Now;
                //    return true;
                //}}
            }
        }
        public class TestLine : SingleLineDataPresenter
        {
            private static Random rnd = new Random();
            public TestLine(string name) : base(name, 0, 30) { m_isUpdated = true; }
            public override bool GetValue(ref string value, ref int lastWidth, bool forceGet = false)
            {
                if (m_isUpdated || rnd.Next(100) > 70)
                {
                    m_isUpdated = false;
                    lastWidth = m_currentValue.Length;
                    m_newValue = m_currentValue = rnd.Next(10000).ToString();
                    value = m_currentValue;
                    return true;
                }
                else return false;
            }
        }

        private int m_nextUpdateIndex = 0;
        private readonly List<SingleLineDataPresenter> m_lines = new List<SingleLineDataPresenter>();
        private Queue<Tuple<string, string>> m_updateQueue = new Queue<Tuple<string, string>>();
        private int m_widestName = 25;

        public DataViewControl() : base()
        {
            //var rnd = new Random();
            //for (int i = 0; i < 2000; i++)
            //{
            //    m_lines.Add(new TestLine(Data.AlphaID.Create(rnd.Next(200), 5)));
            //}
            this.AutoScrollToEndEnabled = false;
        }

        public IEnumerable<SingleLineDataPresenter> ListLines()
        {
            foreach (var l in m_lines) yield return l;
        }

        public void InsertLine(int index, SingleLineDataPresenter line)
        {
            if (index >= 0 && index < m_lines.Count)
            {
                m_lines.Insert(index, line);
            }
            else
            {
                m_lines.Add(line);
            }
        }

        public void UpdateLineValue(string name, string value)
        {
            m_updateQueue.Enqueue(new Tuple<string, string>(name, value));

        }

        protected override bool IsUpdateNeeded()
        {
            return true;
        }

        protected override bool DoUpdate(DateTime resentChangedLimit, DateTime timeout)
        {
            while (m_updateQueue.Count > 0 && DateTime.Now < timeout)
            {
                var entry = m_updateQueue.Dequeue();
                var displayLine = m_lines.FirstOrDefault(l => String.Equals(entry.Item1, l.Name));
                if (displayLine != null)
                {
                    displayLine.UpdateValue(entry.Item2);
                }
                else
                {
                    var newLine = new DataViewControl.SingleLineDataPresenter(entry.Item1, 0, 40);
                    newLine.UpdateValue(entry.Item2);
                    this.InsertLine(-1, newLine);
                }
            }
            if (m_lines.Count == 0 || m_lines.Count != this.LinesCount - 1)
            {
                this.Clear();
                try
                {
                    int widestName = 0;
                    for (int i = 0; i < m_lines.Count; i++)
                    {
                        var line = m_lines[i];
                        widestName = Math.Max(widestName, line.Name.Length);
                        string value = "";
                        int lastWidth = 0;
                        line.GetValue(ref value, ref lastWidth, true);
                        this.AppendText(String.Concat(line.HeaderText, value, Environment.NewLine), this.DefaultStyle);
                    }
                    m_widestName = widestName;
                }
                finally
                {
                    m_nextUpdateIndex = 0;
                }
            }
            else
            {
                int startIndex = m_nextUpdateIndex;
                do
                {
                    var line = m_lines[m_nextUpdateIndex];

                    string value = null;
                    int lastWidth = 0;
                    bool isNew = line.IsInvalidated;
                    if (line.GetValue(ref value, ref lastWidth))
                    {
                        System.Diagnostics.Debug.WriteLine("DoUpdate-update; existing: " + this.GetLineText(m_nextUpdateIndex + 1));
                        if (isNew)
                        {
                            System.Diagnostics.Debug.WriteLine("DoUpdate-update-new: " + line.HeaderText + value);
                            var range = new Range(this, 0, m_nextUpdateIndex, this.GetLineLength(m_nextUpdateIndex), m_nextUpdateIndex);
                            range.ClearStyle(StyleIndex.All);
                            this.InsertTextAndRestoreSelection(range, line.HeaderText + value, this.DefaultStyle);
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("DoUpdate-update: " + line.HeaderText + value);
                            var range = new Range(this, line.ValueColumn, m_nextUpdateIndex, line.ValueColumn + lastWidth, m_nextUpdateIndex);
                            range.ClearStyle(StyleIndex.All);
                            this.InsertTextAndRestoreSelection(range, value, this.DefaultStyle);
                        }
                    }

                    m_nextUpdateIndex++;
                    if (m_nextUpdateIndex >= m_lines.Count) m_nextUpdateIndex = 0;
                } while (DateTime.Now < timeout && m_nextUpdateIndex != startIndex);
            }
            return true;
        }
    }
}
