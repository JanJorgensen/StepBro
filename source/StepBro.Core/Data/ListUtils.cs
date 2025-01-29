using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    public static class ListUtils
    {
        /// <summary>
        /// Synchronizes an existing list with a source list, keeping existing elements that equals a source element.
        /// </summary>
        /// <typeparam name="T">List element type.</typeparam>
        /// <param name="list">The list to synchronize.</param>
        /// <param name="source">The source list to synchronize against.</param>
        /// <param name="actionOnNewEntries">An action to perform on entries not already found in the existing list.</param>
        /// <returns>Whether the list is changed.</returns>
        public static bool Synchronize<T>(
            this ObservableCollection<T> list, 
            IEnumerable<T> source, 
            Action<T> actionOnNewEntries = null) where T : class, IEquatable<T>
        {
            var listChanged = false;
            int i = 0;
            foreach (var sourceEntry in source)
            {
                var found = false;
                for (int j = i; j < list.Count; j++)
                {
                    if (list[j].Equals(sourceEntry))
                    {
                        if (j == i)
                        {
                            found = true;   // Let it be.
                            break;
                        }
                        else
                        {
                            list.Move(j, i);    // Move the entry forward.
                            found = true;
                            listChanged = true;
                            break;
                        }
                    }
                }
                if (!found)
                {
                    listChanged = true;
                    if (actionOnNewEntries != null)
                    {
                        actionOnNewEntries(sourceEntry);
                    }
                    list.Insert(i, sourceEntry);
                }
                i++;
            }
            while (list.Count > i)
            {
                listChanged = true;
                list.RemoveAt(i);
            }

            return listChanged;
        }
    }
}
