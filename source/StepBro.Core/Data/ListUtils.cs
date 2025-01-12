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
        public static bool Synchronize<T>(this ObservableCollection<T> list, IEnumerable<T> source)
        {
            // TODO: Make a super smart synchronization.
            list.Clear();
            foreach (var item in source)
            {
                list.Add(item);
            }
            return true;
        }
    }
}
