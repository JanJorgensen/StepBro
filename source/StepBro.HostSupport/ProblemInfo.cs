using StepBro.Core.Parser;
using StepBro.Core.ScriptData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.HostSupport
{
    public enum ProblemType { Environment, Parsing, HostApp, ScriptExecution }
    public enum ProblemSeverity { Info, Warning, Error }

    public class ProblemInfo
    {
        public ProblemType Type { get; private set; }
        public ProblemSeverity Severity { get; private set; }
        public string Description { get; private set; }
        public string FileName { get; private set; }
        public string FilePath { get; private set; }
        public string DisplayLine { get; private set; }

        public ProblemInfo(ProblemType type, ProblemSeverity severity, string description, string filepath, int line)
        {
            Type = type;
            Severity = severity;
            Description = description;
            FilePath = filepath;
            if (string.IsNullOrEmpty(filepath))
            {
                FileName = string.Empty;
            }
            else
            {
                FileName = System.IO.Path.GetFileName(filepath);
            }
            DisplayLine = line >= 0 ? line.ToString() : string.Empty;
        }

        public ProblemInfo(IScriptFile script, IErrorData parsingError) :
            this(ProblemType.Parsing, parsingError.JustWarning ? ProblemSeverity.Warning : ProblemSeverity.Error, parsingError.Message, script.FilePath, parsingError.Line)
        {
        }
    }
}
