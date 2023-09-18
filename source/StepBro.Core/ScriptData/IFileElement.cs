using System.Collections.Generic;
using StepBro.Core.Data;

namespace StepBro.Core.ScriptData
{
    public interface IFileElement : IIdentifierInfo, IInheritable
    {
        IScriptFile ParentFile { get; }
        int Line { get; }
        IFileElement BaseElement { get; }
        IFileElement ParentElement { get; }
        FileElementType ElementType { get; }
        string Summary { get; }
        string DocReference { get; }
        int UniqueID { get; }
        AccessModifier AccessLevel { get; }
        IEnumerable<IPartner> ListPartners();
    }
}
