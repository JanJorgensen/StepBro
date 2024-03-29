﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace StepBro.Cmd
{
    enum ExitValueOption
    {
        Normal,
        ReturnValue,
        Verdict,
        SubVerdict
    }

    internal class CommandLineOptions : StepBro.Core.General.CoreCommandlineOptions
    {
        [Option('v', "verbose", Default = false, HelpText = "Prints StepBro setup information, execution log and execution results to the console.")]
        public bool Verbose { get; set; } = false;

        [Option('t', "trace", Default = false, HelpText = "Prints execution log to the console.")]
        public bool TraceToConsole { get; set; } = false;

        [Option('w', "wait", Default = false, HelpText = "Awaits key press before terminating.")]
        public bool AwaitKeypress { get; set; } = false;

        [Option("print_report", Default = false, HelpText = "Prints the generated execution/test report in the text format selected by the 'format' option.")]
        public bool PrintReport { get; set; } = false;

        [Option("repeat_parsing", Default = false, HelpText = "Repeatedly parsing top file and the dependancy files whenever a file changes.")]
        public bool RepeatedParsing { get; set; } = false;

        [Option("exitcode", Default = ExitValueOption.Normal, HelpText = "Application exit code option (ReturnValue, Verdict or SubVerdict).")]
        public ExitValueOption ExitCode { get; set; } = ExitValueOption.Normal;

        [Option("format", HelpText = "Format of the printed execution log and report. Specify the name of the format to use. When used, the execution log will be printed, and the 'trace' option is not necessary")]
        public string OutputFormat { get; set; } = null;

        [Option("sidekick", Default = false, HelpText = "Opens a sidekick window for interactive command input and script execution.")]
        public bool Sidekick { get; set; } = false;
    }
}
