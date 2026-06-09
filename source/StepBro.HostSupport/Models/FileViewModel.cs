using StepBro.Core.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.HostSupport.Models
{
    internal class FileViewModel : ItemViewModel
    {
        public FileViewModel(ILoadedFile file) : base(ToViewType(file.Type), "LoadedFile_" + file.FilePath.Replace(' ', '_'))
        {

        }

        public static ViewType ToViewType(LoadedFileType fileType)
        {
            switch (fileType)
            {
                case LoadedFileType.StepBroScript: return ViewType.LoadedScriptFileDocument;
                default: return ViewType.FileDocument;
            }
        }
    }
}
