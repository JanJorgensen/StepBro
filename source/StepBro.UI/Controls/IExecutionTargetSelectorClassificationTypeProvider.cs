using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ActiproSoftware.Text;

namespace StepBro.UI.Controls
{
    public interface IExecutionTargetSelectorClassificationTypeProvider
    {
        IClassificationType Delimiter
        {
            get;
        }
        IClassificationType MainIdentifier
        {
            get;
        }
        IClassificationType SubIdentifier
        {
            get;
        }
        IClassificationType Identifier
        {
            get;
        }
        IClassificationType Value
        {
            get;
        }
        IClassificationType String
        {
            get;
        }
    }
}
