using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    public enum ElementOperator
    {
        /// <summary>
        /// The data should be used to modify existing element.
        /// </summary>
        Modify,
        /// <summary>
        /// The data should be appended to the existing element data.
        /// </summary>
        Append,
        /// <summary>
        /// The data should be assigned as new element data. 
        /// </summary>
        Assign
    }

    public interface ITreeDataElement
    {
        ITreeDataElement Parent { get; }
        string Name { get; }
        bool HasValue { get; }
        string Value { get; }
        bool IsEmpty { get; }
        int ElementCount { get; }
        IEnumerable<ITreeDataElement> SubElements { get; }
        string DataOrigin { get; }
        IEnumerable<NamedString> Attributes { get; }
    }
}
