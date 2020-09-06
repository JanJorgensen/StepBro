using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    /// <summary>
    /// Interface for an object that hosts/creates global accessible objects stored in <seealso cref="IObjectContainer"/> objects. 
    /// </summary>
    public interface IObjectHost
    {
        IEnumerable<IObjectContainer> ListObjectContainers();
        /// <summary>
        /// Notifies changes in the list object containers.
        /// </summary>
        event EventHandler ObjectContainerListChanged;
    }
}
