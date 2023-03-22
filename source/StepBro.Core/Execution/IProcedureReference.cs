using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StepBro.Core.Data;
using StepBro.Core.ScriptData;

namespace StepBro.Core.Execution
{
    /// <summary>
    /// Generic interface for a procedure reference object.
    /// </summary>
    public interface IProcedureReference : IInheritable
    {
        /// <summary>
        ///  Reference to the base type of the procedure.
        /// </summary>
        IFileProcedure ProcedureData { get; }
        string Name { get; }
        string FullName { get; }
        IProcedureReference BaseProcedure { get; }
    }

    /// <summary>
    /// Interface used by the parser to distinguish between procedures and functions.
    /// </summary>
    public interface IFunctionReference : IProcedureReference { }

    /// <summary>
    /// A typed procedure reference, where
    /// </summary>
    /// <typeparam name="T">The delegate type being the signature of the runtime procedure.</typeparam>
    internal interface IProcedureReference<T> : IProcedureReference
    {
        /// <summary>
        /// Reference to the delegate holding the runtime code of the procedure.
        /// </summary>
        T RuntimeProcedure { get; }
    }
}
