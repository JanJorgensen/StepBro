using ActiproSoftware.Text;
using ActiproSoftware.Text.Tagging.Implementation;
using StepBro.Core.Parser.Grammar;
using System;
using ActiproLex = ActiproSoftware.Text.Lexing;

namespace StepBro.UI.SyntaxEditorSupport
{
    public class StepBroTokenTagger : TokenTagger
    {

        private readonly IStepBroClassificationTypeProvider classificationTypeProviderValue;

        /// <summary>
        /// Initializes a new instance of the <c>SimpleTokenTagger</c> class.
        /// </summary>
        /// <param name="document">The specific <see cref="ICodeDocument"/> for which this token tagger will be used.</param>
        /// <param name="classificationTypeProvider">A <see cref="IStepBroClassificationTypeProvider"/> that provides classification types used by this token tagger.</param>
        public StepBroTokenTagger(ICodeDocument document, IStepBroClassificationTypeProvider classificationTypeProvider) :
                base(document)
        {
            // Initialize
            classificationTypeProviderValue = classificationTypeProvider ?? throw new ArgumentNullException("classificationTypeProvider");
        }

        /// <summary>
        /// Gets the <see cref="IStepBroClassificationTypeProvider"/> that provides classification types used by this token tagger.
        /// </summary>
        /// <value>The <see cref="IStepBroClassificationTypeProvider"/> that provides classification types used by this token tagger.</value>
        public IStepBroClassificationTypeProvider ClassificationTypeProvider
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
                case Core.Parser.Grammar.StepBroLexer.IDENTIFIER:
                    return this.ClassificationTypeProvider.Identifier;
                case Core.Parser.Grammar.StepBroLexer.PROCEDURE:
                case Core.Parser.Grammar.StepBroLexer.FUNCTION:
                case Core.Parser.Grammar.StepBroLexer.TESTLIST:
                case Core.Parser.Grammar.StepBroLexer.RETURN:
                case Core.Parser.Grammar.StepBroLexer.USING:
                case Core.Parser.Grammar.StepBroLexer.EXPECT:
                case Core.Parser.Grammar.StepBroLexer.INT_:
                case Core.Parser.Grammar.StepBroLexer.INTEGER:
                case Core.Parser.Grammar.StepBroLexer.LOG:
                case Core.Parser.Grammar.StepBroLexer.STRING:
                case Core.Parser.Grammar.StepBroLexer.DATETIME:
                case Core.Parser.Grammar.StepBroLexer.TIMESPAN:
                case Core.Parser.Grammar.StepBroLexer.VAR:
                    return this.ClassificationTypeProvider.Keyword;
                case Core.Parser.Grammar.StepBroLexer.INTEGER_LITERAL:
                case Core.Parser.Grammar.StepBroLexer.DateTimeLiteral:
                case Core.Parser.Grammar.StepBroLexer.TimespanLiteral:
                    return this.ClassificationTypeProvider.Number;
                case Core.Parser.Grammar.StepBroLexer.REGULAR_STRING:
                case Core.Parser.Grammar.StepBroLexer.VERBATIUM_STRING:
                    return this.ClassificationTypeProvider.StringLiteral;
                case Core.Parser.Grammar.StepBroLexer.SINGLE_LINE_COMMENT:
                case Core.Parser.Grammar.StepBroLexer.DELIMITED_COMMENT:
                case Core.Parser.Grammar.StepBroLexer.DOC_COMMENT_INDENTED:
                case Core.Parser.Grammar.StepBroLexer.DOC_COMMENT:
                    return this.ClassificationTypeProvider.Comment;
                default:
                    return null;
            }
        }
    }
}
