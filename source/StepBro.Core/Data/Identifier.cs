using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    /// <summary>
    /// Encapsulated string to act as an identifier and not a 'just a string value'.
    /// </summary>
    public struct Identifier
    {
        /// <summary>
        /// The name of the identifier.
        /// </summary>
        public string Name;
        /// <summary>
        /// Constructor for an identifier with the specified name.
        /// </summary>
        /// <param name="name"></param>
        public Identifier(string name) { this.Name = name; }

        public static explicit operator Identifier(string id) => new Identifier(id);

        public override string ToString()
        {
            return "Identifier: " + this.Name;
        }
    }
}
