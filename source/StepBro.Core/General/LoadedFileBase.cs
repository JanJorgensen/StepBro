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
        private string m_offDiskFileContent = null;
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

        public string OffDiskFileContent
        {
            get
            {
                return m_offDiskFileContent;
            }
            set
            {
                m_offDiskFileContent = value;
            }
        }


        protected void NotifyPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public bool RegisterDependant(object usingObject)
        {
            if (m_dependants.Select(wr => { if (wr.TryGetTarget(out object o)) return o; else return null; }).Contains(usingObject))
            {
                return true;
            }
            m_dependants.Add(new WeakReference<object>(usingObject));
            NotifyPropertyChanged(nameof(RegisteredDependantsCount));
            return false;
        }

        public bool UnregisterDependant(object usingObject, bool throwIfNotFound = true)
        {
            int i = 0;
            bool found = false;
            List<int> elementsToRemove = new List<int>();
            // Check all dependants. Remove specified object and all disposed objects.
            foreach (var dep in m_dependants)
            {
                if (dep.TryGetTarget(out object obj))
                {
                    // If using object specified and the current object is that specified object.
                    if (usingObject != null && Object.ReferenceEquals(usingObject, obj))
                    {
                        found = true;
                        elementsToRemove.Insert(0, i);  // Insert first, to get reverse order for indices to remove.
                    }
                }
                else
                {
                    // Object disposed; remove it from the list
                    elementsToRemove.Insert(0, i);  // Insert first, to get reverse order for indices to remove.
                }
                i++;
            }
            foreach (var j in elementsToRemove)
            {
                m_dependants.RemoveAt(j);
            }
            if (elementsToRemove.Count > 0)
            {
                NotifyPropertyChanged(nameof(RegisteredDependantsCount));
            }
            if (throwIfNotFound && usingObject != null && !found)
            {
                throw new ArgumentException("The specified object is not registered as a dependant.");
            }
            return found;
        }

        public bool IsDependantOf(object @object)
        {
            if (@object == null) throw new ArgumentNullException();
            foreach (var dep in m_dependants)
            {
                if (dep.TryGetTarget(out object obj))
                {
                    if (Object.ReferenceEquals(@object, obj))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void Dispose()
        {
            UnregisterDependant(null);
            //if (this.RegisteredDependantsCount > 0)
            //{
            //    throw new Exception("Disposing file object when still having dependants.");
            //}
            DoDispose();
        }

        protected virtual void DoDispose()
        {
        }

        public IEnumerable<string> ListDependantDescriptors()
        {
            UnregisterDependant(null);
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
