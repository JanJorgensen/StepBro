using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace StepBro.Core.General
{
    public sealed class CommandLineParser
    {
        private CommandLineParser() { }

        static public T Parse<T>(Logging.ILogger logging, string[] arguments, System.IO.TextWriter outputTarget = null) where T : CoreCommandlineOptions
        {
            var parser = new CommandLine.Parser(config => config.HelpWriter = outputTarget);
            T options = null;
            IEnumerable<Error> errors = null;

            var result = parser.ParseArguments<T>(arguments)
                .WithParsed<T>(opts => options = opts)
                .WithNotParsed<T>(errs => errors = errs);

            if (errors != null)
            {
                options = System.Activator.CreateInstance<T>();
                options.ParsingErrors = errors;
            }

            return options;
        }
    }
}
