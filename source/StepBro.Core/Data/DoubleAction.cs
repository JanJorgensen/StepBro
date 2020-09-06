using System;

namespace StepBro.Core.Data
{
    public class DoubleAction
    {
        private readonly Action m_first, m_second;
        public DoubleAction(Action first, Action second)
        {
            m_first = first;
            m_second = second;
        }
        public void ActionSimple()
        {
            m_first();
            m_second();
        }

        public void ActionDoSecondAlways()
        {
            Exception exception = null;
            try
            {
                m_first();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                m_second();
                if (exception != null)
                {
                    throw exception;
                }
            }
        }
    }
}
