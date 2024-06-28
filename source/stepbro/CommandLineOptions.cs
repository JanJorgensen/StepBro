using System;
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
        public bool LogToConsoleOld { get; set; } = false;
        [Option('l', "log", Default = false, HelpText = "Prints execution log to the console.")]
        public bool LogToConsole { get; set; } = false;

        [Option("save_log", Default = null, HelpText = "Saves the entire execution log in a text file in the specified folder.")]
        public string LogToFile { get; set; } = null;

        [Option("save_report", Default = null, HelpText = "Saves the generated execution/test report(s) in the specified file.")]
        public string ReportToFile { get; set; } = null;

        [Option("print_report", Default = false, HelpText = "Prints the generated execution/test report in the text format selected by the 'format' option.")]
        public bool PrintReport { get; set; } = false;

        [Option("format", HelpText = "Format of the console output.")]
        public string OutputFormat { get; set; } = null;

        [Option("logfileformat", HelpText = "Format of created execution log files.")]
        public string LogFileFormat { get; set; } = null;

        [Option('w', "wait", Default = false, HelpText = "Awaits key press before terminating.")]
        public bool AwaitKeypress { get; set; } = false;

        [Option("repeat_parsing", Default = false, HelpText = "Repeatedly parsing top file and the dependancy files whenever a file changes.")]
        public bool RepeatedParsing { get; set; } = false;

        [Option("exitcode", Default = ExitValueOption.Normal, HelpText = "Application exit code option (ReturnValue, Verdict or SubVerdict).")]
        public ExitValueOption ExitCode { get; set; } = ExitValueOption.Normal;

        [Option("sidekick", Default = false, HelpText = "Opens a sidekick window for interactive command input and script execution.")]
        public bool Sidekick { get; set; } = false;
        [Option("no_attach", Default = false, HelpText = "Only usable with --sidekick. Makes sidekick not attach to console, useful if running with a non-supported terminal such as Win11 Terminal or VSCode.")]
        public bool NoAttach { get; set; } = false;
    }
}
