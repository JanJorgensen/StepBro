﻿using StepBro.Core.Api;
using StepBro.Core.Execution;
using StepBro.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Host
{
    [Public]
    public enum UserResponse
    {
        OK,
        Yes,
        No,
        Cancel
    }

    public abstract class UserInteraction
    {
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
            public System.Drawing.Image Image { get; set; } = null;
        }

        protected List<SectionData> m_sections = new List<SectionData>();

        public bool ShowOK { get; set; } = true;
        public bool ShowYesNo { get; set; } = false;
        public bool ShowCancel { get; set; } = false;
        public void AddTextBlock(string header, string text)
        {
            m_sections.Add(new SectionTextBlock { Header = header, Text = text });
        }

        public void AddSingleSelectionList(string tag, string header, params string[] options)
        {
            var data = new SectionSingleSelection { Tag = tag, Header = header };
            data.Options.AddRange(options);
            m_sections.Add(data);
        }

        public void AddImage(System.Drawing.Image image)
        {
            m_sections.Add(new SectionImage { Image = image });
        }

        public void AddImage(string file)
        {
            m_sections.Add(new SectionImage { ImageFile = file });
        }


        public abstract UserResponse Open([Implicit] ICallContext context, TimeSpan timeout = default(TimeSpan), UserResponse defaultAnswer = UserResponse.OK);

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
