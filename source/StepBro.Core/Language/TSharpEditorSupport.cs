using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using System;
using System.Collections.Generic;
using StepBro.Core.Parser;

namespace StepBro.Core.Language
{
    public class TSharpEditorSupport : IEditorSupport
    {
        private IEditorTextControl m_editor;
        private List<EditorTextStyles> m_tokenStyles = new List<EditorTextStyles>();

        public TSharpEditorSupport(IEditorTextControl editor)
        {
            m_editor = editor;

            this.AddTokenStyle(TSharpLexer.INTEGER_LITERAL, EditorTextStyles.Numeric);
            this.AddTokenStyle(TSharpLexer.IntegerWithSIPrefixLiteral, EditorTextStyles.Numeric);
            this.AddTokenStyle(TSharpLexer.FloatingPointLiteral, EditorTextStyles.Numeric);
            this.AddTokenStyle(TSharpLexer.HexLiteral, EditorTextStyles.Numeric);

            this.AddTokenStyle(TSharpLexer.REGULAR_STRING, EditorTextStyles.String);
            this.AddTokenStyle(TSharpLexer.VERBATIUM_STRING, EditorTextStyles.String);

            this.AddTokenStyle(TSharpLexer.TimespanLiteral, EditorTextStyles.Timespan);

            this.AddTokenStyle(TSharpLexer.SINGLE_LINE_COMMENT, EditorTextStyles.Comment);
            this.AddTokenStyle(TSharpLexer.DELIMITED_COMMENT, EditorTextStyles.Comment);
            this.AddTokenStyle(TSharpLexer.DOC_COMMENT_INDENTED, EditorTextStyles.DocComment);
            this.AddTokenStyle(TSharpLexer.DOC_COMMENT, EditorTextStyles.DocComment);
            //this.AddTokenStyle(TSharpLexer.DOC_COMMENT_START, EditorTextStyles.DocComment);
            //this.AddTokenStyle(TSharpLexer.DOC_COMMENT_NEWLINE_INDENTED, EditorTextStyles.DocComment);
            //this.AddTokenStyle(TSharpLexer.DOC_COMMENT_NEWLINE, EditorTextStyles.DocComment);
            //this.AddTokenStyle(TSharpLexer.DOC_COMMENT_TEXT, EditorTextStyles.DocComment);

            this.AddTokenStyle(TSharpLexer.DATATABLE_ROW_START, EditorTextStyles.DataTableRow);
            this.AddTokenStyle(TSharpLexer.DATATABLE_CELL_CONTENT, EditorTextStyles.DataTableCell);
            this.AddTokenStyle(TSharpLexer.DATATABLE_END_LINE, EditorTextStyles.DataTableRow);
            this.AddTokenStyle(TSharpLexer.DATATABLE_END_LINE_COMMENT, EditorTextStyles.DataTableRow);
            this.AddTokenStyle(TSharpLexer.DATATABLE_CELL_DELIMITER, EditorTextStyles.DataTableRow);

            this.AddTokenStyle(EditorTextStyles.Keyword,
                TSharpLexer.ALT, TSharpLexer.ASSERT, TSharpLexer.AWAIT, TSharpLexer.BASE, TSharpLexer.BOOL, TSharpLexer.BREAK,
                TSharpLexer.CONST, TSharpLexer.CONTINUE, TSharpLexer.DATATABLE, TSharpLexer.DATETIME, TSharpLexer.DECIMAL,
                TSharpLexer.DO, TSharpLexer.DOUBLE, TSharpLexer.DYNAMIC, TSharpLexer.ELSE, TSharpLexer.EMPTY, TSharpLexer.ENUM,
                TSharpLexer.ERROR, TSharpLexer.EXECUTION, TSharpLexer.EXPECT, TSharpLexer.FAIL, TSharpLexer.FALSE,
                TSharpLexer.FOR, TSharpLexer.FOREACH, TSharpLexer.FUNCTION, TSharpLexer.IF, TSharpLexer.IGNORE,
                TSharpLexer.IN, TSharpLexer.INCONCLUSIVE, TSharpLexer.INTEGER, TSharpLexer.INT_, TSharpLexer.IS,
                TSharpLexer.LOG, TSharpLexer.NAMESPACE, TSharpLexer.NOT, TSharpLexer.NULL, TSharpLexer.OBJECT, TSharpLexer.ON,
                TSharpLexer.OR, TSharpLexer.OUT, TSharpLexer.PASS, TSharpLexer.PRIVATE, TSharpLexer.PROCEDURE,
                TSharpLexer.PROTECTED, TSharpLexer.PUBLIC, TSharpLexer.REF, TSharpLexer.REPORT, TSharpLexer.RETURN,
                TSharpLexer.SINGLESELECTION, TSharpLexer.START, TSharpLexer.STATIC, TSharpLexer.STEP, TSharpLexer.STRING,
                TSharpLexer.TESTLIST, TSharpLexer.THIS, TSharpLexer.THROW, TSharpLexer.TIMEOUT, TSharpLexer.TIMESPAN,
                TSharpLexer.TRUE, TSharpLexer.UNSET, TSharpLexer.USING, TSharpLexer.VAR, TSharpLexer.VERDICT, TSharpLexer.VOID,
                TSharpLexer.WARNING, TSharpLexer.WHILE);
        }

        private void AddTokenStyle(int token, EditorTextStyles style)
        {
            while (m_tokenStyles.Count <= token) m_tokenStyles.Add(EditorTextStyles.Normal);
            m_tokenStyles[token] = style;
        }
        private void AddTokenStyle(EditorTextStyles style, params int[] tokens)
        {
            foreach (var token in tokens)
            {
                this.AddTokenStyle(token, style);
            }
        }

        public void LinesAdded(int textVersion, int index, int count)
        {
            throw new NotImplementedException();
        }

        public void ResetSyntax()
        {
            var iLastLine = m_editor.GetLastLineInfo(out int lastLineLength);
            this.SyntaxColorRange(0, 0, iLastLine, lastLineLength - 1);
        }

        public void TextChanged(int textVersion, int startLine, int startColumn, int endLine, int endColumn)
        {
            this.SyntaxColorRange(startLine, startColumn, endLine, endColumn);
        }

        internal AntlrInputStream GetParserStream(int startLine, int startColumn, int endLine, int endColumn)
        {
            var text = m_editor.GetText(startLine, startColumn, endLine, endColumn);
            System.Diagnostics.Debug.WriteLine($"Parser stream ([{startLine}, {startColumn}] - [{endLine}, {endColumn}]:");
            System.Diagnostics.Debug.WriteLine(text);
            return new AntlrInputStream(text);
        }

        private void SyntaxColorRange(int startLine, int startColumn, int endLine, int endColumn)
        {
            var firstLine = startLine;
            m_editor.ClearStyles(startLine, startColumn, endLine, endColumn);

            if (startLine != endLine || startColumn != endColumn)
            {
                ITokenSource lexer = new TSharpLexer(this.GetParserStream(startLine, startColumn, endLine, endColumn));
                var tokens = new CommonTokenStream(lexer);
                tokens.Fill();

                foreach (var token in tokens.GetTokens())
                {
                    EditorTextStyles style = EditorTextStyles.Normal;
                    if (token.Type >= 0 && m_tokenStyles.Count > token.Type)
                    {
                        style = m_tokenStyles[token.Type];
                    }
                    if (style != EditorTextStyles.Normal)
                    {
                        m_editor.SetStyle(
                            style,
                            (token.Line - 1) + firstLine,
                            token.Column,
                            (token.Line - 1) + firstLine,
                            token.Column + (token.StopIndex - token.StartIndex) + 1);
                    }
                }
            }
        }
    }

    internal class TSharpFileSection
    {
        public TSharpFileSection()
        {

        }

        public void AddSubSection(TSharpFileSection section)
        {
        }
    }

    internal class TSharpEditorSupportVisitor : TSharpBaseVisitor<TSharpFileSection>
    {
        public override TSharpFileSection VisitExpression([NotNull] Parser.TSharp.ExpressionContext context)
        {
            return base.VisitExpression(context);
        }

        public override TSharpFileSection VisitCompilationUnit([NotNull] Parser.TSharp.CompilationUnitContext context)
        {
            return base.VisitCompilationUnit(context);
        }
    }
}
