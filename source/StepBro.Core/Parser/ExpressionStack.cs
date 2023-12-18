using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Parser
{
    internal class ExpressionStack
    {
        private Stack<Stack<SBExpressionData>> m_expressionData = new Stack<Stack<SBExpressionData>>();
        private Stack<string> m_levelName = new Stack<string>();

        public void Clear()
        {
            m_expressionData.Clear();
            m_levelName.Clear();
        }

        public void PushStackLevel(string name)
        {
            //System.Diagnostics.Debug.WriteLine("Pushing expression stack: " + name);
            m_expressionData.Push(new Stack<SBExpressionData>());
            m_levelName.Push(name);
        }

        public Stack<SBExpressionData> PopStackLevel()
        {
            //System.Diagnostics.Debug.WriteLine("Popping expression stack: " + m_levelName.Pop());
            return m_expressionData.Pop();
        }

        public Stack<SBExpressionData> Peek() { return m_expressionData.Peek(); }

        public void Push(SBExpressionData data)
        {
            //System.Diagnostics.Debug.WriteLine("Push: " + data.ToString());
            m_expressionData.Peek().Push(data);
        }

        public SBExpressionData Pop()
        {
            return m_expressionData.Peek().Pop();
        }

        public string TopToString()
        {
            if (m_expressionData.Count > 0)
            {
                return m_levelName.Peek() + ": " + m_expressionData.Peek().Count;
            }
            else return "empty";
        }
    }
}
