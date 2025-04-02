using StepBro.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.ScriptData
{
    internal class FileElementDocumentation : FileElement
    {
        public FileElementDocumentation(IScriptFile file, int line, string @namespace, string name) :
            base(file, line, null, @namespace, name, AccessModifier.Public, FileElementType.Documentation)
        { }

        protected override TypeReference GetDataType()
        {
            throw new NotImplementedException();
        }
    }
}
