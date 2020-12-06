using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.General
{
    public interface ILoadedFile : IDisposable
    {
        string FileName { get; }
        string FilePath { get; }
        LoadedFileType Type { get; }
        int UniqueID { get; }
        event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Registers an object as a dependant.
        /// </summary>
        /// <param name="usingObject">The dependant object "using" the file.</param>
        /// <returns>Whether the <paramref name="usingObject"/> is already registered.</returns>
        bool RegisterDependant(object usingObject);
        void UnregisterDependant(object usingObject, bool throwIfNotFound = true);
        int RegisteredDependantsCount { get; }
        IEnumerable<string> ListDependantDescriptors();
    }
}
