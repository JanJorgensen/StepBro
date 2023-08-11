using StepBro.Core.Parser;
using StepBro.Core.ScriptData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Workbench.ToolViews
{
    public enum ErrorType { Environment, ParsingError }

    public class ErrorInfo
    {
        public ErrorType Type { get; private set; }
        public string Severity { get; private set; }
        public string Description { get; private set; }
        public string FileName { get; private set; }
        public string FilePath { get; private set; }
        public string DisplayLine { get; private set; }

        public ErrorInfo(ErrorType type, string severity, string description, string filepath, int line)
        {
            this.Type = type;
            this.Severity = severity;
            this.Description = description;
            this.FilePath = filepath;
            if (String.IsNullOrEmpty(filepath))
            {
                this.FileName = string.Empty;
            }
            else
            {
                this.FileName = System.IO.Path.GetFileName(filepath);
            }
            this.DisplayLine = (line >= 0) ? line.ToString() : string.Empty;
        }

        public ErrorInfo(IScriptFile script, IErrorData parsingError) : 
            this(ErrorType.ParsingError, parsingError.JustWarning ? "Warning" : "Error", parsingError.Message, script.FilePath, parsingError.Line)
        {
        }
    }
}
