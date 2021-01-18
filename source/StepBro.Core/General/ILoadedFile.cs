using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace StepBro.Core.General
{
    public interface ILoadedFile : IDisposable, INotifyPropertyChanged
    {
        string FileName { get; }
        string FilePath { get; }
        LoadedFileType Type { get; }
        int UniqueID { get; }

        string OffDiskFileContent { get; set; }

        /// <summary>
        /// Registers an object as a dependant.
        /// </summary>
        /// <param name="usingObject">The dependant object "using" the file.</param>
        /// <returns>Whether the <paramref name="usingObject"/> is already registered.</returns>
        bool RegisterDependant(object usingObject);
        /// <summary>
        /// Returns whether the specified object is in the list of dependants.
        /// </summary>
        /// <param name="object">The object to check for dependency.</param>
        /// <returns></returns>
        bool IsDependantOf(object @object);
        bool UnregisterDependant(object usingObject, bool throwIfNotFound = true);
        int RegisteredDependantsCount { get; }
        IEnumerable<string> ListDependantDescriptors();
    }
}
