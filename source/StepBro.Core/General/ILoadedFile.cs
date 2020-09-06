using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.General
{
    public enum LoadedFileType { Unknown, StepBroScript, ClearText }
    public interface ILoadedFile : IDisposable
    {
        string FileName { get; }
        string FilePath { get; }
        LoadedFileType Type { get; }
        int UniqueID { get; }
        event PropertyChangedEventHandler PropertyChanged;

        void RegisterDependant(object usingObject);
        void UnregisterDependant(object usingObject);
        int RegisteredDependantsCount { get; }
        IEnumerable<string> ListDependantDescriptors();
    }
}
