using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Execution;
using StepBro.Core.Host;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.SimpleWorkbench
{
    public class UserInteraction : StepBro.Core.Host.UserInteraction
    {
        private List<Tuple<string, int>> m_selectionsMade = new List<Tuple<string, int>>();
        private System.Threading.ManualResetEvent m_closeEvent;
        private bool m_userClose = false;
        private UserResponse m_userResponse;
        private UserResponse m_defaultUserResponse = UserResponse.Cancel;

        public UserInteraction()
        {
            m_closeEvent = new ManualResetEvent(false);
        }

        public string HeaderText { get; set; } = string.Empty;

        public event EventHandler OnOpen;
        public event EventHandler OnClose;


        public void NotifyClose(UserResponse userResponse)
        {
            m_userResponse = userResponse;
            m_userClose = true;
            m_closeEvent.Set();
        }

        internal void NotifySelection(string tag, int selection)
        {
            var index = FindSelectionEntry(tag);
            if (index >= 0)
            {
                m_selectionsMade[index] = new Tuple<string, int>(tag, selection);
            }
            else
            {
                m_selectionsMade.Add(new Tuple<string, int>(tag, selection));
            }
        }

        private int FindSelectionEntry(string tag)
        {
            return m_selectionsMade.FindIndex(s => (tag == null && s.Item1 == null) || String.Equals(tag, s.Item1, StringComparison.InvariantCulture));
        }

        #region Script Interface

        public override int GetSelection([Implicit] ICallContext context, string tag)
        {
            var tagIndex = this.FindSelectionEntry(tag);
            if (tagIndex >= 0)
            {
                return m_selectionsMade[tagIndex].Item2;
            }
            else
            {
                // No selection made, or nonexisting section.
                return -1;
            }
        }

        public override UserResponse Open([Implicit] ICallContext context, TimeSpan timeout = default, UserResponse defaultAnswer = UserResponse.OK)
        {
            this.TimeoutTime = timeout;
            string interactionText = String.IsNullOrEmpty(this.HeaderText) ? "User interaction" : $"User interaction \"{this.HeaderText}\"";
            string timeoutText = "";
            if (timeout != default(TimeSpan))
            {
                timeoutText = "Timeout: " + StringUtils.ObjectToString(timeout) + ".";
            }
            context.Logger.Log($"{interactionText}. {timeoutText}");

            m_defaultUserResponse = defaultAnswer;
            var userResponse = defaultAnswer;

            if (timeout == TimeSpan.Zero)
            {
                timeout = TimeSpan.FromSeconds(60);
            }
            bool stopRequested = false;

            this.OnOpen?.Invoke(this, new EventArgs());
            var entryTime = DateTime.UtcNow;

            while (!m_userClose && !m_closeEvent.WaitOne(0) && (DateTime.UtcNow - entryTime) < timeout)
            {
                if (context.StopRequested())
                {
                    stopRequested = true;
                    break;
                }
                if (m_closeEvent.WaitOne(0))
                {
                    break;
                }
            }

            if (m_userClose)
            {
                userResponse = m_userResponse;
                context.Logger.LogUserAction($"{interactionText}. User pressed '{m_userResponse}'.");
            }
            else if (stopRequested)
            {
                userResponse = UserResponse.StopRequested;
                context.Logger.LogUserAction($"{interactionText}. User requested script execution stop.");
            }
            else  // Timeout
            {
                context.Logger.LogUserAction($"{interactionText}. Timeout; '{userResponse}' selected.");
            }

            this.OnClose?.Invoke(this, new EventArgs());

            return userResponse;
        }

        #endregion

        public IEnumerable<SectionData> ListSections()
        {
            foreach (var section in m_sections)
            {
                yield return section;
            }
        }
    }
}
