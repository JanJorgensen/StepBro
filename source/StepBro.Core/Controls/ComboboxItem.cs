using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Controls
{
    public class ComboboxItem
    {
        private string m_text;
        private object m_value;
        private string m_toolTipTitle;
        private string m_toolTipText;
        
        public ComboboxItem(string text, object value)
        {
            m_text = text;
            m_value = value;
        }

        public override string ToString()
        {
            return m_text;
        }

        public string Text { get { return m_text; } }
        public object Value { get { return m_value; } }
    }
}
