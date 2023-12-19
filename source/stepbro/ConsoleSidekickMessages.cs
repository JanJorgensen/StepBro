using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Sidekick
{
    public enum ShortCommand
    {
        None,
        Close,
        Parse,
        ExecutionStarted,
        ExecutionStopped,
        StopScriptExecution
    }

    public class CommandObjectsList
    {
        public string[] Objects { get; set; }
    }

    public class LoadedFiles
    {
        public string[] Files { get; set; }
    }

    public class FileElements
    {
        public string File { get; set; }
        public string[] ElementNames { get; set; }
        public string[] ElementTypes { get; set; }
        public string[][] Partners { get; set; }
    }

    //public class RequestElementInfo
    //{
    //    public string File { get; set; }
    //    public string Element { get; set; }
    //}

    //public class ElementInfo
    //{
    //    public string Element { get; set; }
    //    public string[] Partners { get; set; }
    //}

    public class ObjectCommand
    {
        public ObjectCommand(string @object, string command)
        {
            this.Object = @object;
            this.Command = command;
        }
        public string Object { get; set; }
        public string Command { get; set; }
    }

    public class RunScriptTarget
    {
        public RunScriptTarget(string file, string element, string partner) { this.File = file; this.Element = element; this.Partner = partner; }
        public string File { get; }
        public string Element { get; }
        public string Partner { get; }
    }

    public class RunScriptRequest : RunScriptTarget
    {
        public RunScriptRequest(string file, string element, string partner) : base(file, element, partner) { }
    }
}
