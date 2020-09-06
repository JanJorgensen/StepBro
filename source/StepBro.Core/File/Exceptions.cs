using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.File
{

    [Serializable]
    public class XmlDataReadingException : Exception
    {
        public XmlDataReadingException() { }
        public XmlDataReadingException(string message) : base(message) { }
        public XmlDataReadingException(string message, Exception inner) : base(message, inner) { }
        protected XmlDataReadingException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        { }
    }
}
