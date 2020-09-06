using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace StepBroCoreTest.Utils
{
    public class MiniLoggerWalker
    {
        private readonly MiniLogger.Entry m_first;
        private MiniLogger.Entry m_next;
        private MiniLogger.Entry m_lastSeen = null;
        private DateTime m_lastTime;

        public MiniLoggerWalker(MiniLogger.Entry first)
        {
            m_first = first;
            m_next = first;
            m_lastSeen = first;
            m_lastTime = first.Timestamp - TimeSpan.FromSeconds(1000);
        }

        private void StepForward()
        {
            m_lastTime = m_next.Timestamp;
            m_next = m_next.Next;
            if (m_next != null) m_lastSeen = m_next;
        }

        public void ExpectNext(string text)
        {
            if (m_next == null) Assert.Fail("No next log entry");
            Assert.IsTrue(m_next.Text.Equals(text, StringComparison.InvariantCulture), $"Expected \"{text}\", but found \"{m_next.Text}\"");
            this.StepForward();
        }

        public void ExpectAltNext(params string[] texts)
        {
            var picker = new StepBro.Core.Data.ListElementPicker<string>(texts.ToList());
            for (int i = 0; i < texts.Length; i++)
            {
                var txt = m_next.Text;
                Assert.IsTrue(picker.FindUnpicked(s => String.Equals(s, txt)), $"Expected {txt}");
                this.StepForward();
            }
        }

        public void ExpectNextContains(string text)
        {
            Assert.IsTrue(m_next.Text.Contains(text));
            this.StepForward();
        }

        public void ExpectNear(string text)
        {
            DateTime now = m_lastTime;
            int i = 0;
            while (m_next.Timestamp < (now + TimeSpan.FromSeconds(2)) || i < 20)
            {
                if (m_next.Text.Equals(text)) return;
                this.StepForward();
                i++;
            }
            Assert.Fail("Match for \"" + text + "\" was not found.");
        }

        public void ExpectEnd()
        {
            if (m_next != null)
            {
                Assert.Fail($"Assumed end of log, but found \"{m_next.Text}\"");
            }
        }

        public void ContinueAgain()
        {
            if (m_next != null)
            {
                Assert.Fail("Continuing, but never found the end");
            }
            m_next = m_lastSeen.Next;
        }
    }
}
