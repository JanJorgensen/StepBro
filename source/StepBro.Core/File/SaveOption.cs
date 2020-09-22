using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.File
{
    /// <summary>
    ///  Options for a save operation.
    /// </summary>
    public enum SaveOption
    {
        /// <summary>
        /// Saving new file content to the existing file.
        /// </summary>
        SaveToExisting,
        /// <summary>
        /// Save the new file content to a new file and keep the existing file opened and unchanged.
        /// </summary>
        SaveAsCopy,
        /// <summary>
        /// Save the new content to the existing file and rename the file.
        /// </summary>
        Rename
    }
}
