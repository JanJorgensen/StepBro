using StepBro.Core.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.HostSupport.Models
{
    internal class FileViewModel : ItemViewModel
    {
        public FileViewModel(ILoadedFile file) : base("LoadedFile_" + file.FilePath.Replace(' ','_'))
        {

        }
    }
}
