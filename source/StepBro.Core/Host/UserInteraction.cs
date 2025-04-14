using StepBro.Core.Api;
using StepBro.Core.Execution;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StepBro.Core.Host
{
    [Public]
    public enum UserResponse
    {
        Pending,
        OK,
        Yes,
        No,
        Cancel,
        Timeout,
        StopRequested
    }

    public abstract class UserInteraction
    {
        private bool m_showOK = true;
        private bool m_showCancel = false;
        private bool m_showYes = false;
        private bool m_showNo = false;
        private bool m_wasOpened = false;
        protected bool m_userClose = false;
        protected UserResponse m_userResponse;
        protected UserResponse m_defaultUserResponse = UserResponse.Cancel;

        protected void SetOpenedFlag()
        {
            m_wasOpened = true;
        }

        public bool WasOpened { get { return m_wasOpened; } }

        public class SectionData
        {
            public string Header { get; set; }
        }
        public class SectionTextBlock : SectionData
        {
            public string Text { get; set; }
        }
        public class SectionSingleSelection : SectionData
        {
            public string Tag { get; set; }
            public List<string> Options { get; } = new List<string>();
        }
        public class SectionImage : SectionData
        {
            public string ImageFile { get; set; } = null;
            //public System.Drawing.Image Image { get; set; } = null;
            public int Width { get; set; } = 0;
            public int Height { get; set; } = 0;
        }

        protected List<SectionData> m_sections = new List<SectionData>();

        public bool ShowOKButton { get => m_showOK; set { m_showOK = value; } }
        public bool ShowYesNoButtons
        {
            get { return m_showYes && m_showNo; }
            set
            {
                if (value == true) m_showOK = false;    // Normalerweise, the OK button is not used when using Yes/No.
                m_showYes = value;
                m_showNo = value;
            }
        }
        public bool ShowYesButton { get { return m_showYes; } set { m_showYes = value; } }
        public bool ShowNoButton { get { return m_showNo; } set { m_showNo = value; } }
        public bool ShowCancelButton { get => m_showCancel; set { m_showCancel = value; } }

        public TimeSpan TimeoutTime { get; set; } = default(TimeSpan);

        protected void CheckIfOpened()
        {
            if (m_wasOpened) throw new InvalidOperationException("Not allowed after being opened.");
        }

        public void AddTextBlock(string header, string text)
        {
            CheckIfOpened();
            m_sections.Add(new SectionTextBlock { Header = header, Text = text });
        }

        public void AddSingleSelectionList(string tag, string header, params string[] options)
        {
            CheckIfOpened();
            var data = new SectionSingleSelection { Tag = tag, Header = header };
            data.Options.AddRange(options);
            m_sections.Add(data);
        }

        //public void AddImage(System.Drawing.Image image)
        //{
        //    CheckIfOpened();
        //    m_sections.Add(new SectionImage { Image = image });
        //}

        public void AddImage(string file)
        {
            CheckIfOpened();
            m_sections.Add(new SectionImage { ImageFile = file });
        }
        public void AddImage(string file, int width, int height)
        {
            CheckIfOpened();
            m_sections.Add(new SectionImage { ImageFile = file });
        }
        //public void AddImage(int width, int height, Color background)
        //{
        //    CheckIfOpened();
        //    m_sections.Add(new SectionImage());
        //}

        public abstract StepBro.Core.Tasks.IAsyncResult<UserResponse> Show([Implicit] ICallContext context, TimeSpan timeout = default(TimeSpan), UserResponse defaultAnswer = UserResponse.OK);

        public virtual void NotifyClose(UserResponse userResponse)
        {
            m_userResponse = userResponse;
            m_userClose = true;
        }

        public UserResponse Result { get { return m_userResponse; } }

        public abstract int GetSelection([Implicit] ICallContext context, string tag);
        public string GetSelectionText([Implicit] ICallContext context, string tag)
        {
            var index = GetSelection(context, tag);
            if (index >= 0)
            {
                var section = m_sections.FirstOrDefault(s => (s is SectionSingleSelection selection) && ((tag == null && selection.Tag == null) || String.Equals(tag, selection.Tag, StringComparison.InvariantCulture))) as SectionSingleSelection;

                if (section != null)
                {
                    return section.Options[index];
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
    }
}
