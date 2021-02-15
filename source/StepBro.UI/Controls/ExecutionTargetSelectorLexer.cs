using System;
using System.Collections.Generic;
using ActiproSoftware.Text;
using ActiproSoftware.Text.Lexing;
using ActiproSoftware.Text.Lexing.Implementation;
using ActiproSoftware.Text.RegularExpressions;

namespace StepBro.UI.Controls
{

    /// <summary>
    /// Represents a programmatic mergable <c>Simple</c> lexer (lexical analyzer) implementation.
    /// </summary>
    public class ExecutionTargetSelectorLexer : MergableLexerBase
    {
        private LexicalStateCollection lexicalStates;

        public ExecutionTargetSelectorLexer()
        {
            // Create ID providers
            this.LexicalStateIdProviderCore = new ExecutionTargetSelectorLexicalStateId();
            this.TokenIdProviderCore = new ExecutionTargetSelectorTokenId();

            // Create the default lexical state
            ProgrammaticLexicalState lexicalState = new ProgrammaticLexicalState(ExecutionTargetSelectorLexicalStateId.Default, "Default");
            lexicalStates = new LexicalStateCollection(this);
            lexicalStates.Add(lexicalState);
            this.DefaultLexicalStateCore = lexicalState;
        }

        public override IEnumerable<ILexicalStateTransition> GetAllLexicalStateTransitions()
        {
            return lexicalStates.GetAllLexicalStateTransitions();
        }

        public override MergableLexerResult GetNextToken(ITextBufferReader reader, ILexicalState lexicalState)
        {
            // Initialize
            int tokenId = ExecutionTargetSelectorTokenId.Invalid;

            // Get the next character
            char ch = reader.Read();

            if ((Char.IsLetter(ch) || (ch == '_')))
            {
                // Parse the identifier
                tokenId = this.ParseIdentifier(reader, ch);
            }
            //else if (Char.IsWhiteSpace(ch))
            //{
            //    while (Char.IsWhiteSpace(reader.Peek()))
            //        reader.Read();
            //    tokenId = ExecutionTargetSelectorTokenId.Whitespace;
            //}
            //else if (Char.IsNumber(ch))
            //{
            //    tokenId = this.ParseNumber(reader, ch);
            //}
            else if (ch == '.')
            {
                tokenId = ExecutionTargetSelectorTokenId.Dot;
            }
            else
            {
                tokenId = ExecutionTargetSelectorTokenId.Invalid;
            }

            //{
            //	tokenId = ExecutionTargetSelectorTokenId.Invalid;
            //	switch (ch)
            //	{
            //		case ',':
            //			tokenId = ExecutionTargetSelectorTokenId.Comma;
            //			break;
            //		case '(':
            //			tokenId = ExecutionTargetSelectorTokenId.OpenParenthesis;
            //			break;
            //		case ')':
            //			tokenId = ExecutionTargetSelectorTokenId.CloseParenthesis;
            //			break;
            //		case ';':
            //			tokenId = ExecutionTargetSelectorTokenId.SemiColon;
            //			break;
            //		case '\n':
            //			// Line terminator
            //			tokenId = ExecutionTargetSelectorTokenId.Whitespace;
            //			break;
            //		case '{':
            //			tokenId = ExecutionTargetSelectorTokenId.OpenCurlyBrace;
            //			break;
            //		case '}':
            //			tokenId = ExecutionTargetSelectorTokenId.CloseCurlyBrace;
            //			break;
            //		case '/':
            //			tokenId = ExecutionTargetSelectorTokenId.Division;
            //			switch (reader.Peek())
            //			{
            //				case '/':
            //					// Parse a single-line comment
            //					tokenId = this.ParseSingleLineComment(reader);
            //					break;
            //				case '*':
            //					// Parse a multi-line comment
            //					tokenId = this.ParseMultiLineComment(reader);
            //					break;
            //			}
            //			break;
            //		case '=':
            //			if (reader.Peek() == '=')
            //			{
            //				reader.Read();
            //				tokenId = ExecutionTargetSelectorTokenId.Equality;
            //			}
            //			else
            //				tokenId = ExecutionTargetSelectorTokenId.Assignment;
            //			break;
            //		case '!':
            //			if (reader.Peek() == '=')
            //			{
            //				reader.Read();
            //				tokenId = ExecutionTargetSelectorTokenId.Inequality;
            //			}
            //			break;
            //		case '+':
            //			tokenId = ExecutionTargetSelectorTokenId.Addition;
            //			break;
            //		case '-':
            //			tokenId = ExecutionTargetSelectorTokenId.Subtraction;
            //			break;
            //		case '*':
            //			tokenId = ExecutionTargetSelectorTokenId.Multiplication;
            //			break;
            //		default:
            //			if ((ch >= '0') && (ch <= '9'))
            //			{
            //				// Parse the number
            //				tokenId = this.ParseNumber(reader, ch);
            //			}
            //			break;
            //	}
            //}

            if (tokenId != ExecutionTargetSelectorTokenId.Invalid)
            {
                return new MergableLexerResult(MatchType.ExactMatch, new LexicalStateTokenData(lexicalState, tokenId));
            }
            else
            {
                reader.ReadReverse();
                return MergableLexerResult.NoMatch;
            }
        }

        protected virtual int ParseIdentifier(ITextBufferReader reader, char ch)
        {
            // Get the entire word
            int startOffset = reader.Offset - 1;
            while (!reader.IsAtEnd && (char.IsLetterOrDigit(reader.Peek()) || reader.Peek() == '_'))
            {
                reader.Read();
            }
            return ExecutionTargetSelectorTokenId.Identifier;
        }

        protected virtual int ParseNumber(ITextBufferReader reader, char ch)
        {
            while (Char.IsNumber(reader.Peek()))
                reader.Read();
            if (reader.Peek() == '.')
            {
                reader.Read();  // Skip the dot
                while (Char.IsNumber(reader.Peek()))
                    reader.Read();
                return ExecutionTargetSelectorTokenId.Decimal;
            }
            return ExecutionTargetSelectorTokenId.Integer;
        }
    }
}
