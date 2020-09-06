using System;
using StepBro.Core.Tasks;
using StepBroCoreTest.Utils;

namespace StepBroCoreTest.Mocks
{
    public class TaskContextMock : ITaskContext
    {
        private MiniLogger m_logger;
        private bool m_logProgressSetup = false;
        public TaskContextMock(MiniLogger logger)
        {
            if (logger == null) logger = new MiniLogger();
            m_logger = logger;
        }

        public bool PauseRequested { get; set; } = false;

        public bool EnterPauseIfRequested(string state)
        {
            return false;
        }

        public void ProgressAliveSignal()
        {
            m_logger.Add("progress alive");
        }

        public void ProgressSetup(long start, long length, Func<long, string> toText)
        {
            if (m_logProgressSetup)
            {
                m_logger.Add($"progress setup, start: {start}, length: {length}");
            }
        }

        public void UpdateStatus(string text = null, long progress = -1)
        {
            if (text == null)
            {
                if (progress >= 0)
                {
                    m_logger.Add($"Progress: {progress}");
                }
                else
                {
                    m_logger.Add("<no status>");
                }
            }
            else
            {
                if (progress >= 0)
                {
                    m_logger.Add($"{text} - Progress: {progress}");
                }
                else
                {
                    m_logger.Add(text);
                }
            }
        }
    }
}
