using Antlr4.Runtime.Misc;
using System;
using System.Collections.Generic;
using StepBro.Core.Data;
using SBP = StepBro.Core.Parser.Grammar.StepBro;

namespace StepBro.Core.Parser
{
    internal partial class StepBroListener
    {
        private Stack<List<PropertyBlockEntry>> m_propertyBlockOperands = new Stack<List<PropertyBlockEntry>>();
        private PropertyBlock m_lastElementPropertyBlock = null;
        private List<PropertyBlockEntry> m_lastAttributes = null;
        private Stack<Tuple<string, string>> m_entryNameAndTypeStack = new Stack<Tuple<string, string>>();
        private string m_propertyEntryName;
        private string m_propertyEntryType;

        public PropertyBlock PopPropertyBlockData()
        {
            var block = m_lastElementPropertyBlock;
            m_lastElementPropertyBlock = null;
            return block;
            //var homeList = m_propertyBlockOperands.Pop();
            //if (homeList.Count != 1) throw new InvalidOperationException("Unexpected number of entries in the top list.");
            //return (PropertyBlock)homeList[0];
        }

        #region Block

        #region Element

        public override void EnterElementPropertyblock([NotNull] SBP.ElementPropertyblockContext context)
        {
            this.EnterElementProps();
        }
        public override void EnterElementPropertyList([NotNull] SBP.ElementPropertyListContext context)
        {
            this.EnterElementProps();
        }

        private void EnterElementProps()
        {
            m_propertyBlockOperands.Clear();
            m_propertyBlockOperands.Push(new List<PropertyBlockEntry>());
            m_lastElementPropertyBlock = null;
        }

        public override void ExitElementPropertyblock([NotNull] SBP.ElementPropertyblockContext context)
        {
            if (m_propertyBlockOperands.Count != 1) throw new InvalidOperationException("Unexpected stack depth.");
            if (m_propertyBlockOperands.Peek().Count != 1) throw new InvalidOperationException("Unexpected element count on stack base.");
            var block = (PropertyBlock)m_propertyBlockOperands.Peek()[0];
            m_propertyBlockOperands.Pop();
            m_lastElementPropertyBlock = block;
            if (m_currentFileElement != null)
            {
                m_currentFileElement.SetPropertyBlockData(m_lastElementPropertyBlock);
                try
                {
                    m_currentFileElement.ParsePropertyBlock(this);
                }
                catch (ParsingErrorException ex)
                {
                    m_errors.SymanticError(context.Start.Line, -1, false, $"Value '{ex.Name}': {ex.Message}");
                }
            }
        }

        public override void ExitElementPropertyList([NotNull] SBP.ElementPropertyListContext context)
        {
            if (m_propertyBlockOperands.Count != 1) throw new InvalidOperationException("Unexpected stack depth.");
            m_lastElementPropertyBlock = new PropertyBlock(context.Start.Line, m_propertyBlockOperands.Pop());
            if (m_currentFileElement != null)
            {
                m_currentFileElement.SetPropertyBlockData(m_lastElementPropertyBlock);
                try
                {
                    m_currentFileElement.ParsePropertyBlock(this);
                }
                catch (ParsingErrorException ex)
                {
                    m_errors.SymanticError(context.Start.Line, -1, false, $"Value '{ex.Name}': {ex.Message}");
                }
            }
        }

        #endregion

        #region Statement

        public override void EnterStatementPropertyblock([NotNull] SBP.StatementPropertyblockContext context)
        {
            this.EnterStatementProps();
        }
        public override void EnterStatementPropertyList([NotNull] SBP.StatementPropertyListContext context)
        {
            this.EnterStatementProps();
        }

        private void EnterStatementProps()
        {
            m_propertyBlockOperands.Clear();
            m_propertyBlockOperands.Push(new List<PropertyBlockEntry>());
        }

        public override void ExitStatementPropertyblock([NotNull] SBP.StatementPropertyblockContext context)
        {
            if (m_propertyBlockOperands.Count != 1) throw new InvalidOperationException("Unexpected stack depth.");
            if (m_propertyBlockOperands.Peek().Count != 1) throw new InvalidOperationException("Unexpected element count on stack base.");
            var block = (PropertyBlock)m_propertyBlockOperands.Peek()[0];
            m_propertyBlockOperands.Pop();
            //m_scopeStack.Peek().SetProperties()
            //m_lastElementPropertyBlock = block;
            //if (m_currentFileElement != null)
            //{
            //    m_currentFileElement.SetPropertyBlockData(m_lastElementPropertyBlock);
            //    try
            //    {
            //        m_currentFileElement.ParsePropertyBlock(this);
            //    }
            //    catch (ParsingErrorException ex)
            //    {
            //        m_errors.SymanticError(context.Start.Line, -1, false, $"Value '{ex.Name}': {ex.Message}");
            //    }
            //}
        }

        public override void ExitStatementPropertyList([NotNull] SBP.StatementPropertyListContext context)
        {
            if (m_propertyBlockOperands.Count != 1) throw new InvalidOperationException("Unexpected stack depth.");
            m_scopeStack.Peek().SetProperties(m_propertyBlockOperands.Pop());
            //var block = new PropertyBlock(context.Start.Line, m_propertyBlockOperands.Pop());


            //if (m_currentFileElement != null)
            //{
            //    m_currentFileElement.SetPropertyBlockData(m_lastElementPropertyBlock);
            //    try
            //    {
            //        m_currentFileElement.ParsePropertyBlock(this);
            //    }
            //    catch (ParsingErrorException ex)
            //    {
            //        m_errors.SymanticError(context.Start.Line, -1, false, $"Value '{ex.Name}': {ex.Message}");
            //    }
            //}
        }

        #endregion

        public override void EnterPropertyblock([NotNull] SBP.PropertyblockContext context)
        {
            if (m_propertyBlockOperands.Count < 1) throw new InvalidOperationException("Operands stack not initiated.");
            m_propertyBlockOperands.Peek().Add(new PropertyBlock(context.Start.Line));    // Add this block to the current stack level list.
            m_propertyBlockOperands.Push(new List<PropertyBlockEntry>());       // Add new stack level for the children.
        }

        public override void ExitPropertyblock([NotNull] SBP.PropertyblockContext context)
        {
            var childs = m_propertyBlockOperands.Pop();
            var homeList = m_propertyBlockOperands.Peek();
            var block = (PropertyBlock)homeList[homeList.Count - 1];    // This block is last in the current stack level.
            block.AddRange(childs);
        }

        public override void EnterPropertyBlockCommaTooMuch([NotNull] SBP.PropertyBlockCommaTooMuchContext context)
        {
            m_errors.SymanticError(context.Start.Line, context.Start.Column, true, "Redundant comma at the end of elements.");
        }

        public override void EnterPropertyblockStatement([NotNull] SBP.PropertyblockStatementContext context)
        {
            m_propertyEntryType = null;
        }

        //public override void ExitPropertyblockStatement([NotNull] SBP.PropertyblockStatementContext context)
        //{
        //}

        public override void EnterPropertyblockStatementMissingCommaSeparation([NotNull] SBP.PropertyblockStatementMissingCommaSeparationContext context)
        {
            m_errors.SymanticError(context.Start.Line, context.Start.Column, false, "Missing comma separation before this element.");
        }

        public override void EnterPropertyblockStatementNamed([NotNull] SBP.PropertyblockStatementNamedContext context)
        {
            m_expressionData.PushStackLevel("Block Statement Named");   // For the entry name.
            m_propertyEntryName = null;
        }

        public override void ExitPropertyblockStatementNamed([NotNull] SBP.PropertyblockStatementNamedContext context)
        {
            var homeList = m_propertyBlockOperands.Peek();
            var entry = homeList[homeList.Count - 1];    // This block is last in the current stack level.
            var nameAndType = m_entryNameAndTypeStack.Pop();
            var name = nameAndType.Item1;
            var type = nameAndType.Item2;
            if (name[0] == '\"') name = name.Substring(1, name.Length - 2);
            if (type != null && type[0] == '\"') type = type.Substring(1, type.Length - 2);
            entry.Name = name;
            entry.SpecifiedTypeName = type;
            if (context.op.Type == SBP.OP_ADD_ASSIGNMENT)
            {
                entry.IsAdditionAssignment = true;
            }
            var stack = m_expressionData.PopStackLevel();
        }

        public override void EnterPropertyblockStatementNamedValue([NotNull] SBP.PropertyblockStatementNamedValueContext context)
        {
            m_entryNameAndTypeStack.Push(new Tuple<string, string>(m_propertyEntryName, m_propertyEntryType));
        }

        public override void ExitPropertyblockStatementNameSpecifier([NotNull] SBP.PropertyblockStatementNameSpecifierContext context)
        {
            m_propertyEntryName = this.GetNamedPropertyBlockValueNameOrType(context);
        }

        public override void ExitPropertyblockStatementTypeSpecifier([NotNull] SBP.PropertyblockStatementTypeSpecifierContext context)
        {
            var type = this.GetNamedPropertyBlockValueNameOrType(context);
            if (m_propertyEntryType != null)    // In case more type specifiers are used
            {
                m_propertyEntryType += " " + type;
            }
            else
            {
                m_propertyEntryType = type;
            }
        }

        private string GetNamedPropertyBlockValueNameOrType(Antlr4.Runtime.ParserRuleContext context)
        {
            if (context.Start.Type == SBP.IDENTIFIER)
            {
                var stack = m_expressionData.Peek();
                var identifier = stack.Pop();
                return (string)identifier.Value;
            }
            else if (context.Start.Type == SBP.STRING)
            {
                return ParseStringLiteral(context.GetText(), context);
            }
            else
            {
                return context.GetText();
            }
        }

        #endregion

        #region Value

        public override void EnterPropertyblockStatementValueNormal([NotNull] SBP.PropertyblockStatementValueNormalContext context)
        {
            m_expressionData.PushStackLevel("Block statement value");   // For the value.
        }

        public override void ExitPropertyblockStatementValueNormal([NotNull] SBP.PropertyblockStatementValueNormalContext context)
        {
            this.PopValuePushEntry(context.Start.Line);
        }

        private void PopValuePushEntry(int line)
        {
            var stack = m_expressionData.PopStackLevel();
            var expression = stack.Pop();
            object value = null;
            if (expression.IsConstant)
            {
                value = expression.Value;
            }
            else if (expression.IsUnresolvedIdentifier)
            {
                value = new Identifier((string)expression.Value);
            }
            else
            {
                throw new NotImplementedException();
            }
            m_propertyBlockOperands.Peek().Add(new PropertyBlockValue(line, null, value));
        }

        #endregion

        #region Array

        public override void EnterPropertyblockArray([NotNull] SBP.PropertyblockArrayContext context)
        {
            if (m_propertyBlockOperands.Count < 1) throw new InvalidOperationException("Operands stack not initiated.");
            m_propertyBlockOperands.Peek().Add(new PropertyBlockArray(context.Start.Line));    // Add this array to the current stack level list.
            m_propertyBlockOperands.Push(new List<PropertyBlockEntry>());       // Add new stack level for the children.
        }

        public override void ExitPropertyblockArray([NotNull] SBP.PropertyblockArrayContext context)
        {
            var childs = m_propertyBlockOperands.Pop();
            var homeList = m_propertyBlockOperands.Peek();
            var array = (PropertyBlockArray)homeList[homeList.Count - 1];    // This block is last in the current stack level.
            array.AddRange(childs);
        }

        public override void EnterPropertyblockArrayEntryMissingCommaSeparation([NotNull] SBP.PropertyblockArrayEntryMissingCommaSeparationContext context)
        {
            m_errors.SymanticError(context.Start.Line, context.Start.Column, false, "Missing comma separation before this element.");
        }

        public override void EnterPropertyblockArrayEntryPropertyBlock([NotNull] SBP.PropertyblockArrayEntryPropertyBlockContext context)
        {
        }

        public override void ExitPropertyblockArrayEntryPropertyBlock([NotNull] SBP.PropertyblockArrayEntryPropertyBlockContext context)
        {
        }

        public override void EnterPropertyblockArrayEntryArray([NotNull] SBP.PropertyblockArrayEntryArrayContext context)
        {
        }

        public override void ExitPropertyblockArrayEntryArray([NotNull] SBP.PropertyblockArrayEntryArrayContext context)
        {
        }

        public override void EnterPropertyblockArrayEntryPrimary([NotNull] SBP.PropertyblockArrayEntryPrimaryContext context)
        {
            m_expressionData.PushStackLevel("Array Entry Value");   // For the value.
        }

        public override void ExitPropertyblockArrayEntryPrimary([NotNull] SBP.PropertyblockArrayEntryPrimaryContext context)
        {
            this.PopValuePushEntry(context.Start.Line);
        }

        #endregion

        public override void EnterPropertyblockStatementValueIdentifierOnly([NotNull] SBP.PropertyblockStatementValueIdentifierOnlyContext context)
        {
            m_expressionData.PushStackLevel("Block statement value");   // For the value.
        }
        public override void ExitPropertyblockStatementValueIdentifierOnly([NotNull] SBP.PropertyblockStatementValueIdentifierOnlyContext context)
        {
            var stack = m_expressionData.PopStackLevel();
            PropertyBlockFlag flag;
            if (m_propertyEntryType == null)
            {
                string name = context.GetText();
                flag = new PropertyBlockFlag(context.Start.Line, name);
            }
            else
            {
                string name = context.children[1].GetText();
                flag = new PropertyBlockFlag(context.Start.Line, name);
                flag.SpecifiedTypeName = m_propertyEntryType;
            }
            m_propertyBlockOperands.Peek().Add(flag);
        }

        #region Event

        public override void EnterPropertyblockEventVerdict([NotNull] SBP.PropertyblockEventVerdictContext context)
        {
            m_expressionData.PushStackLevel("Block Event Verdict");
        }

        public override void ExitPropertyblockEventVerdict([NotNull] SBP.PropertyblockEventVerdictContext context)
        {
            var stack = m_expressionData.PopStackLevel();
            var verdict = (Verdict)(stack.Pop().Value);
            var name = (string)(stack.Pop().Value);
            m_propertyBlockOperands.Peek().Add(new PropertyBlockEvent(context.Start.Line, name, verdict));
        }

        public override void EnterPropertyblockEventAssignment([NotNull] SBP.PropertyblockEventAssignmentContext context)
        {
            m_expressionData.PushStackLevel("Block Event Assignment");
        }

        public override void ExitPropertyblockEventAssignment([NotNull] SBP.PropertyblockEventAssignmentContext context)
        {
        }

        #endregion

        #region Attributes

        public override void EnterAttributes([NotNull] SBP.AttributesContext context)
        {
            m_lastAttributes = new List<PropertyBlockEntry>();
        }

        public override void ExitAttributes([NotNull] SBP.AttributesContext context)
        {
        }

        public override void EnterAttribute_section([NotNull] SBP.Attribute_sectionContext context)
        {
            m_propertyBlockOperands.Clear();
            m_propertyBlockOperands.Push(new List<PropertyBlockEntry>());
        }

        public override void ExitAttribute_section([NotNull] SBP.Attribute_sectionContext context)
        {
            if (m_propertyBlockOperands.Count != 1) throw new InvalidOperationException("Unexpected stack depth.");
            m_lastAttributes.AddRange(m_propertyBlockOperands.Pop());
        }

        #endregion
    }
}
