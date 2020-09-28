using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace StepBro.Core.General
{
    public class LoadedFileBase : ILoadedFile
    {
        private readonly string m_filepath;
        private readonly string m_filename;
        private readonly LoadedFileType m_fileType;
        private List<WeakReference<object>> m_dependants = new List<WeakReference<object>>();
        private static int g_nextID = 1000;

        public event PropertyChangedEventHandler PropertyChanged;

        public LoadedFileBase(string filepath, LoadedFileType type)
        {
            m_filepath = filepath;
            m_filename = (filepath != null) ? System.IO.Path.GetFileName(filepath) : null;
            m_fileType = type;
        }

        public string FileName
        {
            get
            {
                return m_filename;
            }
        }

        public string FilePath
        {
            get
            {
                return m_filepath;
            }
        }

        public string GetFullPath()
        {
            return System.IO.Path.GetFullPath(m_filepath);
        }

        public int RegisteredDependantsCount
        {
            get
            {
                return m_dependants.Count;
            }
        }

        public virtual LoadedFileType Type
        {
            get
            {
                return LoadedFileType.Unknown;
            }
        }

        public int UniqueID { get; } = g_nextID++;

        protected void NotifyPropertyChanged(string name)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void RegisterDependant(object usingObject)
        {
            if (m_dependants.Select(wr => { if (wr.TryGetTarget(out object o)) return o; else return null; }).Contains(usingObject)) throw new ArgumentException("The specified object is already registered as a dependant.");
            m_dependants.Add(new WeakReference<object>(usingObject));
        }

        public void UnregisterDependant(object usingObject, bool throwIfNotFound = true)
        {
            int i = 0;
            bool found = false;
            List<int> elementsToRemove = new List<int>();
            foreach (var dep in m_dependants)
            {
                if (dep.TryGetTarget(out object obj))
                {
                    if (usingObject != null && Object.ReferenceEquals(usingObject, obj))
                    {
                        found = true;
                        elementsToRemove.Insert(0, i);  // Insert first, to get reverse order for indices to remove.
                        break;
                    }
                }
                else
                {
                    // Object deleted; remove from list
                    elementsToRemove.Insert(0, i);  // Insert first, to get reverse order for indices to remove.
                }
                i++;
            }
            foreach (var j in elementsToRemove)
            {
                m_dependants.RemoveAt(j);
            }
            if (throwIfNotFound && usingObject != null && !found)
            {
                throw new ArgumentException("The specified object is not registered as a dependant.");
            }
        }

        public void Dispose()
        {
            this.UnregisterDependant(null);
            //if (this.RegisteredDependantsCount > 0)
            //{
            //    throw new Exception("Disposing file object when still having dependants.");
            //}
            this.DoDispose();
        }

        protected virtual void DoDispose()
        {
        }

        public IEnumerable<string> ListDependantDescriptors()
        {
            this.UnregisterDependant(null);
            foreach (var dep in m_dependants)
            {
                if (dep.TryGetTarget(out object obj))
                {
                    //if (obj is )
                    yield return obj.GetType().FullName;
                }
            }
        }
    }
}
