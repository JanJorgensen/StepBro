using StepBro.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.ScriptData
{
    internal class FileElementOverride : FileElement
    {
        public FileElementOverride(IScriptFile file, int line, IFileElement parentElement, string @namespace, string name) 
            : base(file, line, parentElement, @namespace, name, AccessModifier.None, FileElementType.Override)
        {
            this.BaseElementName = name;
        }

        protected override TypeReference GetDataType()
        {
            return new TypeReference(typeof(IFileElement), this);
        }
    }
}
