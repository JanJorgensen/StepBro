using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.HostSupport.Models
{
    public class ItemViewModelCollection : ObservableCollection<ItemViewModel>
    {
        public ItemViewModelCollection()
        {
        }

        public T TryGetViewModel<T>() where T : ItemViewModel
        {
            return (T)this.FirstOrDefault(m => m is T);
        }
    }
}
