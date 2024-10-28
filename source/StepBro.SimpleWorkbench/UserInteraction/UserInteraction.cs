using StepBro.Core.Api;
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

        #region Script Interface

        public override int GetSelection([Implicit] ICallContext context, string tag)
        {
            return 30;
        }

        public override UserResponse Open([Implicit] ICallContext context, TimeSpan timeout = default, UserResponse defaultAnswer = UserResponse.OK)
        {
            m_defaultUserResponse = defaultAnswer;
            var userResponse = defaultAnswer;

            if (timeout == TimeSpan.Zero)
            {
                timeout = TimeSpan.FromSeconds(60);
            }
            var entryTime = DateTime.UtcNow;
            bool stopRequested = false;

            this.OnOpen?.Invoke(this, new EventArgs());

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

            string interactionText = String.IsNullOrEmpty(this.HeaderText) ? "User interaction" : $"User interaction \"{this.HeaderText}\"";

            if (m_userClose)
            {
                userResponse = m_userResponse;
                context.Logger.LogUserAction($"{interactionText}. User pressed '{m_userResponse}'.");
            }
            else if (stopRequested)
            {
                context.Logger.LogUserAction($"{interactionText}. User requested script execution stop.");
            }
            else  // Timeout
            {
                context.Logger.LogUserAction($"{interactionText}. Timeout; '{m_userResponse}' selected.");
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
