using ActiproSoftware.Text;
using ActiproSoftware.Text.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.UI.SyntaxEditorSupport
{
    public interface ISyntaxEditorStepBroClassificationTypeProvider
    {
        /// <summary>
        /// Gets the <c>Comment</c> classification type.
        /// </summary>
        /// <value>The <c>Comment</c> classification type.</value>
        IClassificationType Comment
        {
            get;
        }

        /// <summary>
        /// Gets the <c>Delimiter</c> classification type.
        /// </summary>
        /// <value>The <c>Delimiter</c> classification type.</value>
        IClassificationType Delimiter
        {
            get;
        }

        /// <summary>
        /// Gets the <c>Identifier</c> classification type.
        /// </summary>
        /// <value>The <c>Identifier</c> classification type.</value>
        IClassificationType Identifier
        {
            get;
        }

        /// <summary>
        /// Gets the <c>Keyword</c> classification type.
        /// </summary>
        /// <value>The <c>Keyword</c> classification type.</value>
        IClassificationType Keyword
        {
            get;
        }

        /// <summary>
        /// Gets the <c>Number</c> classification type.
        /// </summary>
        /// <value>The <c>Number</c> classification type.</value>
        IClassificationType Number
        {
            get;
        }

        /// <summary>
        /// Gets the <c>StringLiteral</c> classification type.
        /// </summary>
        /// <value>The <c>StringLiteral</c> classification type.</value>
        IClassificationType StringLiteral
        {
            get;
        }

        /// <summary>
        /// Gets the <c>Operator</c> classification type.
        /// </summary>
        /// <value>The <c>Operator</c> classification type.</value>
        IClassificationType Operator
        {
            get;
        }
    }
}
