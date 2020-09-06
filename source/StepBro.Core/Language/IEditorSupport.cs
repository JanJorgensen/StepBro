using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Language
{
    public interface IEditorSupport
    {
        void ResetSyntax();
        void TextChanged(int textVersion, int startLine, int startColumn, int endLine, int endColumn);
        void LinesAdded(int textVersion, int index, int count);
    }
}
