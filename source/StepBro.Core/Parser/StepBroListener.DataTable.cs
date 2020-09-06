using Antlr4.Runtime.Misc;
using System;
using System.Collections.Generic;
using StepBro.Core.Data;
using StepBro.Core.ScriptData;
using SBP = StepBro.Core.Parser.TSharp;

namespace StepBro.Core.Parser
{
    internal partial class StepBroListener
    {
        private FileDatatable m_currentDatatable = null;
        private FileDatatable m_lastDatatable = null;
        private List<Tuple<string, TypeReference, object>> m_currentDatatableRowData = null;

        public IDatatable GetLastDatatable()
        {
            return m_lastDatatable;
        }

        public List<Tuple<string, TypeReference, object>> GetLastDatatableRow()
        {
            return m_currentDatatableRowData;
        }

        public override void EnterDatatable([NotNull] SBP.DatatableContext context)
        {
            m_elementStart = context.Start;
            m_currentDatatable = null;
            m_lastElementPropertyBlock = null;
        }

        public override void ExitDatatable([NotNull] SBP.DatatableContext context)
        {
            m_currentDatatable.SetElementPropertyBlock(m_lastElementPropertyBlock);
            m_lastElementPropertyBlock = null;
            m_currentDatatable.ParseSource();
            m_lastDatatable = m_currentDatatable;
            if (m_file != null)
            {
                m_file.AddDatatable(m_currentDatatable);
            }
            m_currentDatatable = null;
        }

        public override void ExitDatatableName([NotNull] SBP.DatatableNameContext context)
        {
            m_currentDatatable = new FileDatatable(m_file, m_elementStart.Line, null, m_currentNamespace, context.GetText());
        }

        public override void EnterDatatableRow([NotNull] SBP.DatatableRowContext context)
        {
            m_currentDatatableRowData = new List<Tuple<string, TypeReference, object>>();
        }

        public override void ExitDatatableRow([NotNull] SBP.DatatableRowContext context)
        {
            var startText = context.children[0].GetText();
            if (m_currentDatatable != null)
            {
                m_currentDatatable.AddSourceRow(startText, m_currentDatatableRowData);
                m_currentDatatableRowData = null;
            }
            else
            {
                m_currentDatatableRowData.Insert(0, new Tuple<string, TypeReference, object>(startText, new TypeReference(typeof(void)), null));
            }
        }

        public override void ExitDatatableRowCell([NotNull] SBP.DatatableRowCellContext context)
        {
            var text = context.GetText().Trim();
            TypeReference type = null;
            object value = null;
            if (!String.IsNullOrEmpty(text))
            {
                // TODO: It MUST be possible to make this simpler... (e.g. re-use tokens)
                Antlr4.Runtime.ITokenSource lexer = new TSharpLexer(new Antlr4.Runtime.AntlrInputStream(text));
                Antlr4.Runtime.ITokenStream tokens = new Antlr4.Runtime.CommonTokenStream(lexer);
                var parser = new SBP(tokens);
                parser.RemoveErrorListeners();
                parser.AddErrorListener(m_errors);
                parser.BuildParseTree = true;
                var cellContext = parser.datatableRowCellContent();
                var walker = new Antlr4.Runtime.Tree.ParseTreeWalker();
                m_expressionData.PushStackLevel("VariableType");
                walker.Walk(this, cellContext);
                var stack = m_expressionData.PopStackLevel();
                var expressionValue = stack.Pop();
                if (expressionValue.IsConstant)
                {
                    type = expressionValue.DataType;
                    value = expressionValue.Value;
                }
                else if (expressionValue.IsUnresolvedIdentifier)
                {
                    type = new TypeReference(typeof(StepBro.Core.Data.Identifier));
                    value = expressionValue.Value;
                }
                else
                {
                    m_errors.SymanticError(context.Start.Line, context.Start.Column, false, "Error parsing cell value.");
                }
            }

            m_currentDatatableRowData.Add(new Tuple<string, TypeReference, object>(text, type, value));
        }

        public override void EnterDatatableRowCellContent([NotNull] SBP.DatatableRowCellContentContext context)
        {
            base.EnterDatatableRowCellContent(context);
        }

        public override void ExitDatatableRowCellContent([NotNull] SBP.DatatableRowCellContentContext context)
        {
            base.ExitDatatableRowCellContent(context);
        }
    }
}
