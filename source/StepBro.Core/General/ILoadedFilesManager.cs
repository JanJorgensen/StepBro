using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StepBro.Core.ScriptData;

namespace StepBro.Core.General
{
    public class LoadedFileEventArgs : EventArgs
    {
        private ILoadedFile m_file;

        public LoadedFileEventArgs(ILoadedFile file)
        {
            m_file = file;
        }

        public ILoadedFile File { get { return m_file; } }
    }

    public delegate void LoadedFileEventHandler(object sender, LoadedFileEventArgs args);

    public interface ILoadedFilesManager
    {
        void RegisterLoadedFile(ILoadedFile file);
        IEnumerable<T> ListFiles<T>() where T : class, ILoadedFile;
        void UnloadAllFilesWithoutDependants();
        ILoadedFile TryGetOrLoadFile(string currentpath, string filepath);
        IReadOnlyCollection<IScriptFile> TryGetNamespaceList(string @namespace);
        event LoadedFileEventHandler FileLoaded;
        event LoadedFileEventHandler FileClosed;
        event PropertyChangedEventHandler FilePropertyChanged;
    }
}
