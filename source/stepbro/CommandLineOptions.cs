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

        [Option('v', "verbose", Default = false,
            HelpText = "Prints StepBro setup information, execution log and execution results to the console.")]
        public bool Verbose { get; set; } = false;

        [Option('t', "trace", Default = false,
            HelpText = "Prints execution log to the console.")]
        public bool TraceToConsole { get; set; } = false;

        [Option('w', "wait", Default = false,
            HelpText = "Awaits key press before terminating.")]
        public bool AwaitKeypress { get; set; } = false;

        [Option("lf", HelpText = "Format of the printed execution log. Specify the name of the log entry converter addon to use. When used, the execution log will be printed, and the 'trace' option is not necessary")]
        public string LogFormat { get; set; } = null;
    }
}
