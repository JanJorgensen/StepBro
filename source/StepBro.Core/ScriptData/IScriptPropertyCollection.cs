using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.ScriptData
{
    interface IScriptPropertyCollection : IFileElement
    {
        IEnumerable<IScriptProperty> Properties { get; }

        /// <summary>
        /// Updates all properties accorting to the specified rule.
        /// </summary>
        /// <param name="rule">Name of the update-rule, defined for the collection.</param>
        /// <param name="timeout">The maximum time allowed to wait for completion of the update.</param>
        /// <returns>Wheter the properties were updated. If returning <c>false</c>, the method timed out.</returns>
        bool UpdateProperties(string rule, TimeSpan timeout);
    }
}
