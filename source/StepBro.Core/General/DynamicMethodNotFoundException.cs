using System;

namespace StepBro.Core.General
{
    public class DynamicMethodNotFoundException : Exception
    {
        public DynamicMethodNotFoundException(string method) : base("Method \"" + method + "\" was not found.")
        {
        }
    }
}
