using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    public interface ITextWriter : IDisposable
    {
        void Write(string text);
        void WriteLine(string text);
        /// <summary>
        /// Flush all collected data to the media and close the media. The media will automatically be opened again on next <see cref="Write"/> or <see cref="WriteLine"/> call.
        /// </summary>
        void Flush();
    }
}
