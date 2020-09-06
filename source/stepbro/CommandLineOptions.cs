using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace StepBro.Cmd
{
    internal class CommandLineOptions : StepBro.Core.General.CoreCommandlineOptions
    {
        //[Option('c', "console", Default = false,
        //    HelpText = "Makes the StepBro console application present itself as a console application rather than a graphical UI application.")]
        //public bool Console { get; set; }

        [Option('w', "wait", Default = false,
            HelpText = "Awaits key press before terminating.")]
        public bool AwaitKeypress { get; set; }

    }
}
