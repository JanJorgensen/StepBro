using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using System;
using System.Collections.Generic;
using SBP = StepBro.Core.Parser.Grammar.StepBro;
using Lexer = StepBro.Core.Parser.Grammar.StepBroLexer;

namespace StepBro.Core.Language
{
    public class StepBroEditorSupport : IEditorSupport
    {
        private IEditorTextControl m_editor;
        private List<EditorTextStyles> m_tokenStyles = new List<EditorTextStyles>();

        public StepBroEditorSupport(IEditorTextControl editor)
        {
            m_editor = editor;

            this.AddTokenStyle(Lexer.INTEGER_LITERAL, EditorTextStyles.Numeric);
            this.AddTokenStyle(Lexer.IntegerWithSIPrefixLiteral, EditorTextStyles.Numeric);
            this.AddTokenStyle(Lexer.FloatingPointLiteral, EditorTextStyles.Numeric);
            this.AddTokenStyle(Lexer.HexLiteral, EditorTextStyles.Numeric);

            this.AddTokenStyle(Lexer.REGULAR_STRING, EditorTextStyles.String);
            this.AddTokenStyle(Lexer.VERBATIUM_STRING, EditorTextStyles.String);

            this.AddTokenStyle(Lexer.TimespanLiteral, EditorTextStyles.Timespan);

            this.AddTokenStyle(Lexer.SINGLE_LINE_COMMENT, EditorTextStyles.Comment);
            this.AddTokenStyle(Lexer.DELIMITED_COMMENT, EditorTextStyles.Comment);
            this.AddTokenStyle(Lexer.DOC_COMMENT_INDENTED, EditorTextStyles.DocComment);
            this.AddTokenStyle(Lexer.DOC_COMMENT, EditorTextStyles.DocComment);
            //this.AddTokenStyle(Lexer.DOC_COMMENT_START, EditorTextStyles.DocComment);
            //this.AddTokenStyle(Lexer.DOC_COMMENT_NEWLINE_INDENTED, EditorTextStyles.DocComment);
            //this.AddTokenStyle(Lexer.DOC_COMMENT_NEWLINE, EditorTextStyles.DocComment);
            //this.AddTokenStyle(Lexer.DOC_COMMENT_TEXT, EditorTextStyles.DocComment);

            this.AddTokenStyle(Lexer.DATATABLE_ROW_START, EditorTextStyles.DataTableRow);
            this.AddTokenStyle(Lexer.DATATABLE_CELL_CONTENT, EditorTextStyles.DataTableCell);
            this.AddTokenStyle(Lexer.DATATABLE_END_LINE, EditorTextStyles.DataTableRow);
            this.AddTokenStyle(Lexer.DATATABLE_END_LINE_COMMENT, EditorTextStyles.DataTableRow);
            this.AddTokenStyle(Lexer.DATATABLE_CELL_DELIMITER, EditorTextStyles.DataTableRow);

            this.AddTokenStyle(EditorTextStyles.Keyword,
                Lexer.ALT, Lexer.ASSERT, Lexer.AWAIT, Lexer.BASE, Lexer.BOOL, Lexer.BREAK,
                Lexer.CONST, Lexer.CONTINUE, Lexer.DATATABLE, Lexer.DATETIME, Lexer.DECIMAL,
                Lexer.DO, Lexer.DOUBLE, Lexer.DYNAMIC, Lexer.ELSE, Lexer.EMPTY, Lexer.ENUM,
                Lexer.ERROR, Lexer.EXECUTION, Lexer.EXPECT, Lexer.FAIL, Lexer.FALSE,
                Lexer.FOR, Lexer.FOREACH, Lexer.FUNCTION, Lexer.IF, Lexer.IGNORE,
                Lexer.IN, Lexer.INCONCLUSIVE, Lexer.INTEGER, Lexer.INT_, Lexer.IS,
                Lexer.LOG, Lexer.NAMESPACE, Lexer.NOT, Lexer.NULL, Lexer.OBJECT, Lexer.ON,
                Lexer.OR, Lexer.OUT, Lexer.PASS, Lexer.PRIVATE, Lexer.PROCEDURE,
                Lexer.PROTECTED, Lexer.PUBLIC, Lexer.REF, Lexer.REPORT, Lexer.RETURN,
                Lexer.START, Lexer.STATIC, Lexer.STEP, Lexer.STRING,
                Lexer.TESTLIST, Lexer.THIS, Lexer.THROW, Lexer.TIMEOUT, Lexer.TIMESPAN,
                Lexer.TRUE, Lexer.UNSET, Lexer.USING, Lexer.VAR, Lexer.VERDICT, Lexer.VOID,
                Lexer.WARNING, Lexer.WHILE);
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
                ITokenSource lexer = new Lexer(this.GetParserStream(startLine, startColumn, endLine, endColumn));
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

    internal class StepBroFileSection
    {
        public StepBroFileSection()
        {

        }

        public void AddSubSection(StepBroFileSection section)
        {
        }
    }

    internal class StepBroEditorSupportVisitor : Parser.Grammar.StepBroBaseVisitor<StepBroFileSection>
    {
        public override StepBroFileSection VisitExpression([NotNull] SBP.ExpressionContext context)
        {
            return base.VisitExpression(context);
        }

        public override StepBroFileSection VisitCompilationUnit([NotNull] SBP.CompilationUnitContext context)
        {
            return base.VisitCompilationUnit(context);
        }
    }
}
