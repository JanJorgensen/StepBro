using StepBro.Core.Data;
using StepBro.Core.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    /// <summary>
    /// A delegate that reconstructs a reference to an object of the specified type.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="constructionData"></param>
    /// <returns>The reconstructed object.</returns>
    internal delegate object Reconstructor(IScriptCallContext context, object constructionData);

    /// <summary>
    /// Interface for an object that can create a delegate that can reconstruct a reference
    /// to an object or an object with the same value as the current object.
    /// </summary>
    internal interface IReconstructable
    {
        /// <summary>
        /// Gets a delegate that can reconstruct a reference to the current object or an equal object.
        /// </summary>
        /// <returns>A Tuple with the type of the reconstructed object, a reconstruction delegate, and an object to use as reconstruction seed.</returns>
        Tuple<Type, Reconstructor, object> GetReconstructor();
    }
}
