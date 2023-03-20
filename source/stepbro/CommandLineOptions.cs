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
        [Option('v', "verbose", Default = false, HelpText = "Prints StepBro setup information, execution log and execution results to the console.")]
        public bool Verbose { get; set; } = false;

        [Option('t', "trace", Default = false, HelpText = "Prints execution log to the console.")]
        public bool TraceToConsole { get; set; } = false;

        [Option('w', "wait", Default = false, HelpText = "Awaits key press before terminating.")]
        public bool AwaitKeypress { get; set; } = false;

        [Option("report_result", Default = false, HelpText = "Dumps the execution result.")]
        public bool ReportResult { get; set; } = false;

        [Option("rv", Default = false, HelpText = "Return value set from procedure verdict.")]
        public bool ReturnValueFromVerdict { get; set; } = false;

        [Option("rvs", Default = false, HelpText = "Return value set from procedure verdict or sub-result verdicts.")]
        public bool ReturnValueFromSubVerdict { get; set; } = false;

        [Option("lf", HelpText = "Format of the printed execution log. Specify the name of the log entry converter addon to use. When used, the execution log will be printed, and the 'trace' option is not necessary")]
        public string LogFormat { get; set; } = null;
    }
}
