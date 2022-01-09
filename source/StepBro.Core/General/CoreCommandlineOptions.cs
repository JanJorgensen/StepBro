using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace StepBro.Core.General
{
    public class CoreCommandlineOptions
    {

        //[Option('r', "read", HelpText = "Script file to load.")]
        [Value(0, MetaName = "file", HelpText = "Script file to load.")]
        public string InputFile { get; set; }

        [Option('e', "execute", HelpText = "File element (procedure or test suite) to execute.")]
        public string TargetElement { get; set; }

        [Option('m', "model", HelpText = "Model procedure to handle the execution of the target element.")]
        public string Model { get; set; }

        //[Option('p', "project", HelpText = "Project file to use.")]
        //public string ProjectFile { get; set; }

        /// <summary>
        /// Whether there were errors when parsing the command line.
        /// </summary>
        public bool HasParsingErrors { get { return this.ParsingErrors != null; } }

        /// <summary>
        /// Lists the command line parsing errors, if any.
        /// </summary>
        public IEnumerable<Error> ParsingErrors { get; internal set; } = null;

        [Option('a', "arguments", Separator = ',')]
        public IEnumerable<string> Arguments { get; set; }

    }
}
