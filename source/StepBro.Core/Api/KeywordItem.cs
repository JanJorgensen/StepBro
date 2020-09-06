using System;
using StepBro.Core.Data;
using StepBro.Core.Parser;

namespace StepBro.Core.Api
{
    public sealed class KeywordItem
    {
        private SBExpressionData m_data;
        private readonly int m_index;
        internal KeywordItem(SBExpressionData data, int index)
        {
            m_data = data;
            m_index = index;
        }

        public int Index { get { return m_index; } }

        public TypeReference Type { get { return m_data.DataType; } }

        public object ConstValue
        {
            get
            {
                if (m_data.IsConstant)
                    return m_data.Value;
                else
                    return null;
            }
        }
    }
}
