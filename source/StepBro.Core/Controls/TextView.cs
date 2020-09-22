using FastColoredTextBoxNS;
using System;
using System.ComponentModel;

namespace StepBro.Core.Controls
{
    public class TextView : FastColoredTextBox
    {
        public TextView() : base()
        {
            this.ReadOnly = true;
        }

        protected virtual bool IsUpdateNeeded()
        {
            throw new NotImplementedException("Implement in derived class.");
        }

        protected virtual bool DoUpdate(DateTime resentChangedLimit, DateTime timeout)
        {
            throw new NotImplementedException("Implement in derived class.");
        }

        [Description("")]
        [Category("")]
        public TimeSpan MaxUpdateTime { get; set; } = TimeSpan.FromMilliseconds(20);

        [Description("")]
        [Category("")]
        public TimeSpan MaxResentIndicationAge { get; set; } = TimeSpan.FromMilliseconds(5000);

        [Description("")]
        [Category("")]
        public bool AutoScrollToEndEnabled { get; set; } = true;

        public void UpdateView()
        {
            if (this.IsUpdateNeeded())
            {
                DateTime stopTime = DateTime.Now + this.MaxUpdateTime;
                DateTime resentLimit = DateTime.Now - this.MaxResentIndicationAge;
                //some stuffs for best performance
                this.BeginUpdate();
                this.Selection.BeginUpdate();
                //remember user selection
                var userSelection = this.Selection.Clone();
                //add text with predefined style
                this.TextSource.CurrentTB = this;
                bool gotoEnd = this.AutoScrollToEndEnabled && (userSelection.IsEmpty && userSelection.Start.iLine == (this.LinesCount - 1));

                this.DoUpdate(resentLimit, stopTime);

                //restore user selection
                if (gotoEnd)
                {
                    this.GoEnd();//scroll to end of the text
                }
                else
                {
                    this.Selection.Start = userSelection.Start;
                    this.Selection.End = userSelection.End;
                }
                //
                this.Selection.EndUpdate();
                this.EndUpdate();
            }
        }
    }
}
