using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.ScriptData
{
    public interface IFileElement : StepBro.Core.Data.IIdentifierInfo
    {
        IScriptFile ParentFile { get; }
        int Line { get; }
        IFileElement BaseElement { get; }
        IFileElement ParentElement { get; }
        FileElementType ElementType { get; }
        string Purpose { get; }
        int UniqueID { get; }
        IEnumerable<IPartner> ListPartners();
    }
}
