using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StepBro.Core.Data;

namespace StepBro.Core.Parser
{
    internal class ProjectParsingScope : IParsingContext
    {
        public IEnumerable<IIdentifierInfo> KnownIdentifiers()
        {
            throw new NotImplementedException();
        }
    }
}
