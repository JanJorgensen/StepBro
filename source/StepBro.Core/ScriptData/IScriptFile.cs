using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StepBro.Core.General;
using StepBro.Core.Parser;

namespace StepBro.Core.ScriptData
{
    /// <summary>
    /// Interface to an object containing information about a specific script file.
    /// </summary>
    public interface IScriptFile : ILoadedFile, Data.IObjectHost
    {
        /// <summary>
        /// The revision of the file.
        /// </summary>
        /// <remarks>If the file has been changed, a timestamp is appended to the revision name/number.</remarks>
        string FileRevision { get; }
        /// <summary>
        /// The last author of the script file.
        /// </summary>
        string Author { get; }
        string Namespace { get; }
        IEnumerable<IFileElement> ListElements();
        IErrorCollector Errors { get; }
    }
}
