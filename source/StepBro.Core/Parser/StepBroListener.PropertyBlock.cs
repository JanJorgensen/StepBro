using Antlr4.Runtime.Misc;
using System;
using System.Collections.Generic;
using StepBro.Core.Data;
using TSP = StepBro.Core.Parser.TSharp;

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

        public override void EnterElementPropertyblock([NotNull] TSP.ElementPropertyblockContext context)
        {
            this.EnterElementProps();
        }
        public override void EnterElementPropertyList([NotNull] TSP.ElementPropertyListContext context)
        {
            this.EnterElementProps();
        }

        private void EnterElementProps()
        {
            m_propertyBlockOperands.Clear();
            m_propertyBlockOperands.Push(new List<PropertyBlockEntry>());
            m_lastElementPropertyBlock = null;
        }

        public override void ExitElementPropertyblock([NotNull] TSP.ElementPropertyblockContext context)
        {
            if (m_propertyBlockOperands.Count != 1) throw new InvalidOperationException("Unexpected stack depth.");
            if (m_propertyBlockOperands.Peek().Count != 1) throw new InvalidOperationException("Unexpected element count on stack base.");
            var block = (PropertyBlock)m_propertyBlockOperands.Peek()[0];
            m_propertyBlockOperands.Pop();
            m_lastElementPropertyBlock = block;
            if (m_currentFileElement != null)
            {
                m_currentFileElement.SetPropertyBlockData(m_lastElementPropertyBlock);
                m_currentFileElement.ParsePropertyBlock(this);
            }
        }

        public override void ExitElementPropertyList([NotNull] TSP.ElementPropertyListContext context)
        {
            if (m_propertyBlockOperands.Count != 1) throw new InvalidOperationException("Unexpected stack depth.");
            m_lastElementPropertyBlock = new PropertyBlock(m_propertyBlockOperands.Pop());
            if (m_currentFileElement != null)
            {
                m_currentFileElement.SetPropertyBlockData(m_lastElementPropertyBlock);
                m_currentFileElement.ParsePropertyBlock(this);
            }
        }

        public override void EnterPropertyblock([NotNull] TSP.PropertyblockContext context)
        {
            if (m_propertyBlockOperands.Count < 1) throw new InvalidOperationException("Operands stack not initiated.");
            m_propertyBlockOperands.Peek().Add(new PropertyBlock());    // Add this block to the current stack level list.
            m_propertyBlockOperands.Push(new List<PropertyBlockEntry>());       // Add new stack level for the children.
        }

        public override void ExitPropertyblock([NotNull] TSP.PropertyblockContext context)
        {
            var childs = m_propertyBlockOperands.Pop();
            var homeList = m_propertyBlockOperands.Peek();
            var block = (PropertyBlock)homeList[homeList.Count - 1];    // This block is last in the current stack level.
            block.AddRange(childs);
        }

        //public override void EnterPropertyblockStatement([NotNull] TSP.PropertyblockStatementContext context)
        //{
        //}

        //public override void ExitPropertyblockStatement([NotNull] TSP.PropertyblockStatementContext context)
        //{
        //}

        public override void EnterPropertyblockStatementNamed([NotNull] TSP.PropertyblockStatementNamedContext context)
        {
            m_expressionData.PushStackLevel("Block Statement Named");   // For the entry name.
            m_propertyEntryName = null;
            m_propertyEntryType = null;
        }

        public override void ExitPropertyblockStatementNamed([NotNull] TSP.PropertyblockStatementNamedContext context)
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
            var stack = m_expressionData.PopStackLevel();
        }

        public override void EnterPropertyblockStatementNamedValue([NotNull] TSP.PropertyblockStatementNamedValueContext context)
        {
            m_entryNameAndTypeStack.Push(new Tuple<string, string>(m_propertyEntryName, m_propertyEntryType));
        }

        public override void ExitPropertyblockStatementNameSpecifier([NotNull] TSP.PropertyblockStatementNameSpecifierContext context)
        {
            m_propertyEntryName = this.GetNamedPropertyBlockValueNameOrType(context);
        }

        public override void ExitPropertyblockStatementTypeSpecifier([NotNull] TSP.PropertyblockStatementTypeSpecifierContext context)
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
            if (context.Start.Type == TSP.IDENTIFIER)
            {
                var stack = m_expressionData.Peek();
                var identifier = stack.Pop();
                return (string)identifier.Value;
            }
            else if (context.Start.Type == TSP.STRING)
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

        public override void EnterPropertyblockStatementValueNormal([NotNull] TSP.PropertyblockStatementValueNormalContext context)
        {
            m_expressionData.PushStackLevel("Block statement value");   // For the value.
        }

        public override void ExitPropertyblockStatementValueNormal([NotNull] TSP.PropertyblockStatementValueNormalContext context)
        {
            this.PopValuePushEntry();
        }

        private void PopValuePushEntry()
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
            m_propertyBlockOperands.Peek().Add(new PropertyBlockValue(null, value));
        }

        #endregion

        #region Array

        public override void EnterPropertyblockArray([NotNull] TSP.PropertyblockArrayContext context)
        {
            if (m_propertyBlockOperands.Count < 1) throw new InvalidOperationException("Operands stack not initiated.");
            m_propertyBlockOperands.Peek().Add(new PropertyBlockArray());    // Add this array to the current stack level list.
            m_propertyBlockOperands.Push(new List<PropertyBlockEntry>());       // Add new stack level for the children.
        }

        public override void ExitPropertyblockArray([NotNull] TSP.PropertyblockArrayContext context)
        {
            var childs = m_propertyBlockOperands.Pop();
            var homeList = m_propertyBlockOperands.Peek();
            var array = (PropertyBlockArray)homeList[homeList.Count - 1];    // This block is last in the current stack level.
            array.AddRange(childs);
        }

        public override void EnterPropertyblockArrayEntry([NotNull] TSP.PropertyblockArrayEntryContext context)
        {
        }
        public override void ExitPropertyblockArrayEntry([NotNull] TSP.PropertyblockArrayEntryContext context)
        {
        }

        public override void EnterPropertyblockArrayEntryPropertyBlock([NotNull] TSP.PropertyblockArrayEntryPropertyBlockContext context)
        {
        }

        public override void ExitPropertyblockArrayEntryPropertyBlock([NotNull] TSP.PropertyblockArrayEntryPropertyBlockContext context)
        {
        }

        public override void EnterPropertyblockArrayEntryArray([NotNull] TSP.PropertyblockArrayEntryArrayContext context)
        {
        }

        public override void ExitPropertyblockArrayEntryArray([NotNull] TSP.PropertyblockArrayEntryArrayContext context)
        {
        }

        public override void EnterPropertyblockArrayEntryPrimary([NotNull] TSP.PropertyblockArrayEntryPrimaryContext context)
        {
            m_expressionData.PushStackLevel("Array Entry Value");   // For the value.
        }

        public override void ExitPropertyblockArrayEntryPrimary([NotNull] TSP.PropertyblockArrayEntryPrimaryContext context)
        {
            this.PopValuePushEntry();
        }

        #endregion

        public override void EnterPropertyblockStatementValueIdentifierOnly([NotNull] TSP.PropertyblockStatementValueIdentifierOnlyContext context)
        {
            m_expressionData.PushStackLevel("Block statement value");   // For the value.
        }
        public override void ExitPropertyblockStatementValueIdentifierOnly([NotNull] TSP.PropertyblockStatementValueIdentifierOnlyContext context)
        {
            string name = context.GetText();
            m_propertyBlockOperands.Peek().Add(new PropertyBlockFlag(name));
        }

        #region Event

        public override void EnterPropertyblockEventVerdict([NotNull] TSP.PropertyblockEventVerdictContext context)
        {
            m_expressionData.PushStackLevel("Block Event Verdict");
        }

        public override void ExitPropertyblockEventVerdict([NotNull] TSP.PropertyblockEventVerdictContext context)
        {
            var stack = m_expressionData.PopStackLevel();
            var verdict = (Verdict)(stack.Pop().Value);
            var name = (string)(stack.Pop().Value);
            m_propertyBlockOperands.Peek().Add(new PropertyBlockEvent(name, verdict));
        }

        public override void EnterPropertyblockEventAssignment([NotNull] TSP.PropertyblockEventAssignmentContext context)
        {
            m_expressionData.PushStackLevel("Block Event Assignment");
        }

        public override void ExitPropertyblockEventAssignment([NotNull] TSP.PropertyblockEventAssignmentContext context)
        {
        }

        #endregion

        #region Attributes

        public override void EnterAttributes([NotNull] TSP.AttributesContext context)
        {
            m_lastAttributes = new List<PropertyBlockEntry>();
        }

        public override void ExitAttributes([NotNull] TSP.AttributesContext context)
        {
        }

        public override void EnterAttribute_section([NotNull] TSP.Attribute_sectionContext context)
        {
            m_propertyBlockOperands.Clear();
            m_propertyBlockOperands.Push(new List<PropertyBlockEntry>());
        }

        public override void ExitAttribute_section([NotNull] TSP.Attribute_sectionContext context)
        {
            if (m_propertyBlockOperands.Count != 1) throw new InvalidOperationException("Unexpected stack depth.");
            m_lastAttributes.AddRange(m_propertyBlockOperands.Pop());
        }

        #endregion
    }
}
