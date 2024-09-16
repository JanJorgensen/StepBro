using StepBro.Core.Api;
using StepBro.Core.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Host
{
    public enum UserResponse
    {
        OK,
        Yes,
        No,
        Cancel
    }

    public abstract class UserInteraction
    {
        public bool ShowOK { get; set; } = true;
        public bool ShowYesNo { get; set; } = false;
        public bool ShowCancel { get; set; } = false;
        public abstract void AddTextBlock(string text);
        public abstract int AddSingleSelectionList(string tag, string heading, params string[] choices);

        public abstract void AddImage(System.Drawing.Image image);
        public abstract void AddImage(string file);

        public abstract UserResponse Open([Implicit] ICallContext context, TimeSpan timeout = default(TimeSpan), UserResponse defaultAnswer = UserResponse.OK);

        public abstract int GetSelection([Implicit] ICallContext context, string tag);
    }
}
