using StepBro.Core.Attributes;
using StepBro.Core.Controls;
using StepBro.Core.Data;
using System;
using System.Linq;

namespace StepBro.TestInterface.Controls
{
    [ObjectPanel(allowMultipleInstances: false)]
    public partial class LoggedValuesView : StepBro.Core.Controls.ObjectPanel
    {
        //private class ValueLinePresenter : DataViewControl.SingleLineDataPresenter
        //{
        //    public ValueLinePresenter(string name, int indent, int valueColumn) :base(name, indent, valueColumn)
        //    {
        //    }
        //}

        private DataViewTextBased m_viewControl = null;
        private SerialTestConnection m_connection = null;
        //private LogLineData m_firstSeen = null;
        //private LogLineData m_lastHandled = null;
        //private ILogLineSource m_source = null;

        public LoggedValuesView()
        {
            this.InitializeComponent();
            m_viewControl = dataView.ViewControl;
        }

        public override bool IsBindable { get { return true; } }

        protected override bool TryBind(object @object)
        {
            if (!(@object is SerialTestConnection)) return false;
            m_connection = @object as SerialTestConnection;
            m_connection.LinesAdded += this.Connection_LinesAdded;
            return true;
        }

        protected override void DisconnectBinding()
        {
            if (m_connection != null)
            {
                m_connection.LinesAdded -= this.Connection_LinesAdded;
            }
            //simpleLogViewFull.SetSource(null);
            m_connection = null;
        }

        private void Connection_LinesAdded(object sender, LogLineEventArgs args)
        {
            var line = args.Line;
            if (line.Type == LogLineData.LogType.ReceivedAsync)
            {
                var text = line.LineText;
                int iColon = text.IndexOf(':');
                if (iColon > 0)
                {
                    var name = text.Substring(1, iColon - 1);
                    var value = text.Substring(iColon + 1).Trim();
                    m_viewControl.UpdateLineValue(name, value);
                }
            }
        }
    }
}
