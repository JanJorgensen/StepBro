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
    public partial class ExecutionTargetSelectorTokenId : TokenIdProviderBase
    {
        public const Int32 Whitespace = 1;
        public const Int32 MainIdentifier = 2;
        public const Int32 SubIdentifier = 3;
        public const Int32 Identifier = 4;
        public const Int32 Dot = 5;
        public const Int32 OpenParenthesis = 6;
        public const Int32 CloseParenthesis = 7;
        public const Int32 Comma = 8;
        public const Int32 Integer = 9;
        public const Int32 Decimal = 10;
        public const Int32 String = 11;
        public const Int32 True = 12;
        public const Int32 False = 13;
        public const Int32 Verdict = 14;
        public const Int32 TimeSpan = 15;

        public override Int32 MinId
        {
            get
            {
                return 1;
            }
        }

        /// <summary>
        /// Gets the maximum token ID returned by this provider.
        /// </summary>
        /// <value>The maximum token ID returned by this provider.</value>
        public override Int32 MaxId
        {
            get
            {
                return 15;
            }
        }

        /// <summary>
        /// Returns whether the specified ID value is valid for this token ID provider.
        /// </summary>
        /// <param name="id">The token ID to examine.</param>
        /// <returns><c>true</c> if the ID value is valid; otherwise, <c>false</c></returns>
        public override Boolean ContainsId(Int32 id)
        {
            return ((id >= MinId)
                        && (id <= MaxId));
        }

        /// <summary>
        /// Returns the actual string representation for the specified token ID.
        /// </summary>
        /// <param name="id">The token ID to examine.</param>
        /// <returns>The actual string representation for the specified token ID.</returns>
        public override String GetDescription(Int32 id)
        {
            FieldInfo[] fields = typeof(ExecutionTargetSelectorTokenId).GetFields((BindingFlags.Public | BindingFlags.Static));
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
        /// Returns the string-based key for the specified token ID.
        /// </summary>
        /// <param name="id">The token ID to examine.</param>
        /// <returns>The string-based key for the specified token ID.</returns>
        public override String GetKey(Int32 id)
        {
            FieldInfo[] fields = typeof(ExecutionTargetSelectorTokenId).GetFields((BindingFlags.Public | BindingFlags.Static));
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
