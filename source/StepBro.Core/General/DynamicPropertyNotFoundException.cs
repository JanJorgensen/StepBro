using System;

namespace StepBro.Core.General
{
    public class DynamicPropertyNotFoundException : Exception
    {
        public DynamicPropertyNotFoundException(string property) : base("Property \"" + property + "\" was not found.")
        {
        }
    }
}
