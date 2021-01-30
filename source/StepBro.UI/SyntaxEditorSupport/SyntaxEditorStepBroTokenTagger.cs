using ActiproSoftware.Text;
using ActiproSoftware.Text.Tagging.Implementation;
using StepBro.Core.Parser.Grammar;
using System;
using ActiproLex = ActiproSoftware.Text.Lexing;

namespace StepBro.UI.SyntaxEditorSupport
{
    public class SyntaxEditorStepBroTokenTagger : TokenTagger
    {

        private readonly ISyntaxEditorStepBroClassificationTypeProvider classificationTypeProviderValue;

        /// <summary>
        /// Initializes a new instance of the <c>SimpleTokenTagger</c> class.
        /// </summary>
        /// <param name="document">The specific <see cref="ICodeDocument"/> for which this token tagger will be used.</param>
        /// <param name="classificationTypeProvider">A <see cref="ISyntaxEditorStepBroClassificationTypeProvider"/> that provides classification types used by this token tagger.</param>
        public SyntaxEditorStepBroTokenTagger(ICodeDocument document, ISyntaxEditorStepBroClassificationTypeProvider classificationTypeProvider) :
                base(document)
        {
            // Initialize
            classificationTypeProviderValue = classificationTypeProvider ?? throw new ArgumentNullException("classificationTypeProvider");
        }

        /// <summary>
        /// Gets the <see cref="ISyntaxEditorStepBroClassificationTypeProvider"/> that provides classification types used by this token tagger.
        /// </summary>
        /// <value>The <see cref="ISyntaxEditorStepBroClassificationTypeProvider"/> that provides classification types used by this token tagger.</value>
        public ISyntaxEditorStepBroClassificationTypeProvider ClassificationTypeProvider
        {
            get
            {
                return classificationTypeProviderValue;
            }
        }

        /// <summary>
        /// Returns an <see cref="IClassificationType"/> for the specified <see cref="IToken"/>, if one is appropriate.
        /// </summary>
        /// <param name="token">The <see cref="IToken"/> to examine.</param>
        /// <returns>An <see cref="IClassificationType"/> for the specified <see cref="IToken"/>, if one is appropriate.</returns>
        /// <remarks>
        /// The default implementation of this method automatically returns the classification type if the token
        /// is an <see cref="IMergableToken"/>.
        /// </remarks>
        public override IClassificationType ClassifyToken(ActiproLex.IToken token)
        {
            switch (token.Id)
            {
                case StepBroLexer.IDENTIFIER:
                    return this.ClassificationTypeProvider.Identifier;
                case StepBroLexer.PROCEDURE:
                case StepBroLexer.FUNCTION:
                case StepBroLexer.TESTLIST:
                case StepBroLexer.RETURN:
                case StepBroLexer.USING:
                case StepBroLexer.EXPECT:
                case StepBroLexer.INT_:
                case StepBroLexer.INTEGER:
                case StepBroLexer.LOG:
                case StepBroLexer.STRING:
                case StepBroLexer.DATETIME:
                case StepBroLexer.TIMESPAN:
                case StepBroLexer.VAR:
                    return this.ClassificationTypeProvider.Keyword;
                case StepBroLexer.INTEGER_LITERAL:
                case StepBroLexer.DateTimeLiteral:
                case StepBroLexer.TimespanLiteral:
                    return this.ClassificationTypeProvider.Number;
                case StepBroLexer.REGULAR_STRING:
                case StepBroLexer.VERBATIUM_STRING:
                    return this.ClassificationTypeProvider.StringLiteral;
                case StepBroLexer.SINGLE_LINE_COMMENT:
                case StepBroLexer.DELIMITED_COMMENT:
                case StepBroLexer.DOC_COMMENT_INDENTED:
                case StepBroLexer.DOC_COMMENT:
                    return this.ClassificationTypeProvider.Comment;
                default:
                    return null;
            }
        }
    }
}
