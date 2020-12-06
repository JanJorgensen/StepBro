using System;

namespace StepBro.Core.General
{
    public class FileAlreadyLoadedException : Exception
    {
        public FileAlreadyLoadedException(string message) : base(message)
        {
        }
    }
}
