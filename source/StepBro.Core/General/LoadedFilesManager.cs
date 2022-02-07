using StepBro.Core.Data;
using StepBro.Core.ScriptData;
using StepBro.Core.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace StepBro.Core.General
{
    internal class LoadedFilesManager : ServiceBase<ILoadedFilesManager, LoadedFilesManager>, ILoadedFilesManager
    {
        private IDynamicObjectManager m_objectManager = null;
        private ObservableCollection<ILoadedFile> m_loadedFiles = new ObservableCollection<ILoadedFile>();
        private readonly List<NamedData<Tuple<List<IScriptFile>, IReadOnlyCollection<IScriptFile>>>> m_namespaceLists =
            new List<NamedData<Tuple<List<IScriptFile>, IReadOnlyCollection<IScriptFile>>>>();

        public LoadedFilesManager(out IService serviceAccess) :
            base("LoadedFilesManager", out serviceAccess, typeof(IDynamicObjectManager), typeof(IConfigurationFileManager))
        {
            System.Diagnostics.Debug.WriteLine("LoadedFilesManager instance created.");
        }

        public event LoadedFileEventHandler FileLoaded;
        public event LoadedFileEventHandler FileClosed;
        public event PropertyChangedEventHandler FilePropertyChanged;

        private void NotifyFileLoaded(ILoadedFile file)
        {
            this.FileLoaded?.Invoke(this, new LoadedFileEventArgs(file));
        }
        private void NotifyFileClosed(ILoadedFile file)
        {
            this.FileClosed?.Invoke(this, new LoadedFileEventArgs(file));
        }

        public IEnumerable<T> ListFiles<T>() where T : class, ILoadedFile
        {
            // Special implementation, to support files being added while listing the files.
            int i = 0;
            while (m_loadedFiles.Count > i)
            {
                var f = m_loadedFiles[i];
                if (f is T) yield return f as T;
                i++;
            }
        }

        public IScriptFile TopScriptFile
        {
            get
            {
                return m_loadedFiles.Where(f => f is IScriptFile).FirstOrDefault() as IScriptFile;
            }
        }

        public void RegisterLoadedFile(ILoadedFile file)
        {
            if (m_loadedFiles.Contains(file)) throw new ArgumentException("The specified file is already registered.");
            m_loadedFiles.Add(file);
            if (file is IScriptFile)
            {
                this.ReInsertFileInNamespaceList(file as IScriptFile);
            }
            file.PropertyChanged += this.File_PropetyChanged;
            this.NotifyFileLoaded(file);
            if (file is IObjectHost)
            {
                m_objectManager.RegisterObjectHost(file as IObjectHost);
            }
        }

        private void File_PropetyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is IScriptFile && String.Equals(e.PropertyName, "Namespace", StringComparison.InvariantCulture))
            {
                this.ReInsertFileInNamespaceList((IScriptFile)sender);
            }
            this.FilePropertyChanged?.Invoke(sender, e);
        }

        private void ReInsertFileInNamespaceList(IScriptFile file)
        {
            // Remove from any list
            foreach (var list in m_namespaceLists)
            {
                if (list.Value.Item1.Contains(file))
                {
                    list.Value.Item1.Remove(file);
                }
            }
            // Add to existing list if on exist
            foreach (var list in m_namespaceLists)
            {
                if (String.Equals(file.Namespace, list.Name, StringComparison.InvariantCulture))
                {
                    list.Value.Item1.Add(file);
                    return;
                }
            }
            // Create new list and add file to that
            var newlist = this.CreateNewList(file.Namespace);
            newlist.Value.Item1.Add(file);
        }

        private NamedData<Tuple<List<IScriptFile>, IReadOnlyCollection<IScriptFile>>> CreateNewList(string @namespace)
        {
            var newlist = new List<IScriptFile>();
            var list = new NamedData<Tuple<List<IScriptFile>, IReadOnlyCollection<IScriptFile>>>(
                @namespace,
                new Tuple<List<IScriptFile>, IReadOnlyCollection<IScriptFile>>(
                newlist,
                newlist.AsReadOnly()));
            m_namespaceLists.Add(list);
            return list;
        }

        public IReadOnlyCollection<IScriptFile> TryGetNamespaceList(string @namespace)
        {
            foreach (var list in m_namespaceLists)
            {
                if (String.Equals(list.Name, @namespace, StringComparison.InvariantCulture))
                {
                    return list.Value.Item2;
                }
            }
            return null;
        }

        public ILoadedFile TryGetOrLoadFile(string currentpath, string filepath)
        {
            throw new NotImplementedException();
        }

        public void UnloadAllFilesWithoutDependants()
        {
            bool checkFiles = true;
            while (checkFiles)
            {
                checkFiles = false;
                for (int i = 0; i < m_loadedFiles.Count;)
                {
                    var file = m_loadedFiles[i];
                    file.UnregisterDependant(null); // Make the file object check its dependencies
                    if (file.RegisteredDependantsCount == 0)
                    {
                        if (file is IObjectHost)
                        {
                            m_objectManager.DeRegisterObjectHost(file as IObjectHost);
                        }
                        file.PropertyChanged -= this.File_PropetyChanged;
                        m_loadedFiles.RemoveAt(i);
                        this.NotifyFileClosed(file);
                        file.Dispose();
                        foreach (var lf in m_loadedFiles)
                        {
                            // If file is dependant on the disposed file, unregister the disposed file.
                            // In case the file has no dependants, the file will also be closed and disposed.
                            lf.UnregisterDependant(file, false);
                        }
                        checkFiles = true;  // Run one more time, to check if more should be removed when this one is disposed.
                    }
                    else
                    {
                        i++;
                    }
                }
            }
        }

        protected override void Start(ServiceManager manager, ITaskContext context)
        {
            m_objectManager = manager.Get<IDynamicObjectManager>();
        }

        protected override void Stop(ServiceManager manager, ITaskContext context)
        {
            int c = 0;
            while (m_loadedFiles.Count != c)
            {
                c = m_loadedFiles.Count;

                this.UnloadAllFilesWithoutDependants();
            }

            foreach (var f in m_loadedFiles)
            {
                try
                {
                    f.Dispose();
                }
                catch (Exception)
                {

                }
            }
        }
    }
}
