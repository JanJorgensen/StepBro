using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.SimpleWorkbench
{
    internal class CommandLineOptions : StepBro.Core.General.CoreCommandlineOptions
    {
        [Option("save_log", Default = null, HelpText = "Saves the entire execution log in a text file in the specified folder.")]
        public string LogToFile { get; set; } = null;
    }
}
