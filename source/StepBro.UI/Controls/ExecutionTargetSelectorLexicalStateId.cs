using ActiproSoftware.Text.Lexing.Implementation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.UI.Controls
{
    public class ExecutionTargetSelectorLexicalStateId : LexicalStateIdProviderBase
    {
        public const Int32 Default = 1;

        public override Int32 MinId { get { return 1; } }
        public override Int32 MaxId { get { return 1; } }

        /// <summary>
        /// Returns whether the specified ID value is valid for this lexical state ID provider.
        /// </summary>
        /// <param name="id">The lexical state ID to examine.</param>
        /// <returns><c>true</c> if the ID value is valid; otherwise, <c>false</c></returns>
        public override Boolean ContainsId(Int32 id)
        {
            return ((id >= MinId)
                        && (id <= MaxId));
        }

        /// <summary>
        /// Returns the actual string representation for the specified lexical state ID.
        /// </summary>
        /// <param name="id">The lexical state ID to examine.</param>
        /// <returns>The actual string representation for the specified lexical state ID.</returns>
        public override String GetDescription(Int32 id)
        {
            FieldInfo[] fields = typeof(ExecutionTargetSelectorLexicalStateId).GetFields((BindingFlags.Public | BindingFlags.Static));
            for (Int32 index = 0; (index < fields.Length); index = (index + 1))
            {
                FieldInfo field = fields[index];
                if (id.Equals(field.GetValue(null)))
                {
                    Object[] customAttributes = field.GetCustomAttributes(typeof(DescriptionAttribute), false);
                    if (((customAttributes != null)
                                && (customAttributes.Length > 0)))
                    {
                        return ((DescriptionAttribute)(customAttributes[0])).Description;
                    }
                    else
                    {
                        return field.Name;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the string-based key for the specified lexical state ID.
        /// </summary>
        /// <param name="id">The lexical state ID to examine.</param>
        /// <returns>The string-based key for the specified lexical state ID.</returns>
        public override String GetKey(Int32 id)
        {
            FieldInfo[] fields = typeof(ExecutionTargetSelectorLexicalStateId).GetFields((BindingFlags.Public | BindingFlags.Static));
            for (Int32 index = 0; (index < fields.Length); index = (index + 1))
            {
                FieldInfo field = fields[index];
                if (id.Equals(field.GetValue(null)))
                {
                    return field.Name;
                }
            }
            return null;
        }
    }
}
