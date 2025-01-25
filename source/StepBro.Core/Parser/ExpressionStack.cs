using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Parser
{
    internal class ExpressionStack
    {
        public class LevelData
        {
            public string LevelName;
            public int ArgumentIndex;
            public string ArgumentName;
            public Stack<SBExpressionData> Stack;
            public LevelData(string name)
            {
                LevelName = name;
                ArgumentIndex = -1;
                ArgumentName = null;
                Stack = new Stack<SBExpressionData>();
            }
        }
        private Stack<LevelData> m_expressionData = new Stack<LevelData>();

        public void Clear()
        {
            m_expressionData.Clear();
        }

        public void PushStackLevel(string name)
        {
            //System.Diagnostics.Debug.WriteLine("Pushing expression stack: " + name);
            m_expressionData.Push(new LevelData(name));
        }

        public LevelData PopStackLevel()
        {
            //System.Diagnostics.Debug.WriteLine("Popping expression stack: " + m_levelName.Pop());
            return m_expressionData.Pop();
        }

        public LevelData Peek() { return m_expressionData.Peek(); }

        public void Push(SBExpressionData data)
        {
            //System.Diagnostics.Debug.WriteLine("Push: " + data.ToString());
            m_expressionData.Peek().Stack.Push(data);
        }

        public SBExpressionData Pop()
        {
            return m_expressionData.Peek().Stack.Pop();
        }

        public string TopToString()
        {
            if (m_expressionData.Count > 0)
            {
                var top = m_expressionData.Peek();
                return top.LevelName + ": " + top.Stack.Count;
            }
            else return "empty";
        }
    }
}
