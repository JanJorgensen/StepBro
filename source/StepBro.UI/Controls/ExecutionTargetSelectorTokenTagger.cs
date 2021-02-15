using ActiproSoftware.Text;
using ActiproSoftware.Text.Lexing;
using ActiproSoftware.Text.Tagging.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.UI.Controls
{
    public class ExecutionTargetSelectorTokenTagger : TokenTagger
    {

        private IExecutionTargetSelectorClassificationTypeProvider classificationTypeProviderValue;

        /// <summary>
        /// Initializes a new instance of the <c>SimpleTokenTagger</c> class.
        /// </summary>
        /// <param name="document">The specific <see cref="ICodeDocument"/> for which this token tagger will be used.</param>
        /// <param name="classificationTypeProvider">A <see cref="ISimpleClassificationTypeProvider"/> that provides classification types used by this token tagger.</param>
        public ExecutionTargetSelectorTokenTagger(ICodeDocument document, IExecutionTargetSelectorClassificationTypeProvider classificationTypeProvider) :
                base(document)
        {
            if ((classificationTypeProvider == null))
            {
                throw new ArgumentNullException("classificationTypeProvider");
            }

            // Initialize
            this.classificationTypeProviderValue = classificationTypeProvider;
        }

        /// <summary>
        /// Gets the <see cref="ISimpleClassificationTypeProvider"/> that provides classification types used by this token tagger.
        /// </summary>
        /// <value>The <see cref="ISimpleClassificationTypeProvider"/> that provides classification types used by this token tagger.</value>
        public IExecutionTargetSelectorClassificationTypeProvider ClassificationTypeProvider
        {
            get
            {
                return this.classificationTypeProviderValue;
            }
        }

        public override IClassificationType ClassifyToken(IToken token)
        {
            switch (token.Id)
            {
                case ExecutionTargetSelectorTokenId.Whitespace:
                case ExecutionTargetSelectorTokenId.MainIdentifier:
                case ExecutionTargetSelectorTokenId.SubIdentifier:
                case ExecutionTargetSelectorTokenId.Dot:
                case ExecutionTargetSelectorTokenId.OpenParenthesis:
                case ExecutionTargetSelectorTokenId.CloseParenthesis:
                case ExecutionTargetSelectorTokenId.Comma:
                case ExecutionTargetSelectorTokenId.Integer:
                case ExecutionTargetSelectorTokenId.Decimal:
                case ExecutionTargetSelectorTokenId.String:
                case ExecutionTargetSelectorTokenId.True:
                case ExecutionTargetSelectorTokenId.False:
                case ExecutionTargetSelectorTokenId.Verdict:
                case ExecutionTargetSelectorTokenId.TimeSpan:
                    return this.ClassificationTypeProvider.MainIdentifier;

                //case ExecutionTargetSelectorTokenId.Identifier:
                //    return this.ClassificationTypeProvider.Identifier;
                //case SimpleTokenId.Function:
                //case SimpleTokenId.Return:
                //case SimpleTokenId.Var:
                //    return this.ClassificationTypeProvider.Keyword;
                //case SimpleTokenId.Number:
                //    return this.ClassificationTypeProvider.Number;
                //case SimpleTokenId.MultiLineCommentEndDelimiter:
                //case SimpleTokenId.MultiLineCommentLineTerminator:
                //case SimpleTokenId.MultiLineCommentStartDelimiter:
                //case SimpleTokenId.MultiLineCommentText:
                //case SimpleTokenId.SingleLineCommentEndDelimiter:
                //case SimpleTokenId.SingleLineCommentStartDelimiter:
                //case SimpleTokenId.SingleLineCommentText:
                //    return this.ClassificationTypeProvider.Comment;
                default:
                    return null;
            }
        }
    }
}
