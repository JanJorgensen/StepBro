using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    /// <summary>
    /// Limit options for numeric comparison operations.
    /// </summary>
    public enum NumericLimitType
    {
        /// <summary>
        /// Limit value is not included in the matching value range.
        /// </summary>
        Exclude,
        /// <summary>
        /// Limit value is included in the matching value range.
        /// </summary>
        Include,
        /// <summary>
        /// Values approximately equal to the limit value is included in the matching value range.
        /// </summary>
        Approx
    }
}
