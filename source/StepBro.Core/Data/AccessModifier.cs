using System;
namespace StepBro.Core.Data
{
    public enum AccessModifier
    {
        Private,
        Protected,
        Public,
        None,   // Not specified is considered 'public'.
    }
}
