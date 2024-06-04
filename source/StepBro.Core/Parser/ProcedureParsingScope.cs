using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using StepBro.Core.Data;

namespace StepBro.Core.Parser
{
    internal class ProcedureParsingScope : IParsingContext
    {
        public enum ScopeType { Procedure, Block, SubStatement, Lambda }

        private IParsingContext m_parent;
        private string m_identification;
        private ScopeType m_type;
        private List<ProcedureVariable> m_localVariables = new List<ProcedureVariable>();
        private List<Expression> m_scopeStatements = new List<Expression>();
        private List<ProcedureParsingScope> m_subStatements = new List<ProcedureParsingScope>();
        private List<ParameterExpression> m_lambdaParameters = null;
        private List<PropertyBlockEntry> m_propertyBlock = null;
        private List<PropertyBlockEntry> m_attributes = null;

        public ProcedureParsingScope(IParsingContext parent, string id, ScopeType type)
        {
            m_parent = parent;
            m_identification = id;
            m_type = type;
        }

        public ScopeType Type { get { return m_type; } }

        public LabelTarget BreakLabel { get; private set; } = null;
        public LabelTarget ContinueLabel { get; private set; } = null;
        public bool ForConditionExists { get; set; } = false;
        public bool ForUpdateExists { get; set; } = false;
        public Expression UsingVariableAssignment { get; set; }

        public void SetupForLoop()
        {
            if (this.BreakLabel != null) throw new InvalidOperationException("Already setup for loop.");
            this.BreakLabel = Expression.Label();
            this.ContinueLabel = Expression.Label();
        }

        public void AddStatementCode(params Expression[] statements)
        {
            m_scopeStatements.AddRange(statements);
        }

        public int StatementCount { get { return m_scopeStatements.Count; } }

        public Expression GetOnlyStatementCode()
        {
            if (m_scopeStatements.Count != 1)
            {
                throw new NotImplementedException("SOMETHING WRONG !!!");
            }
            else return m_scopeStatements[0];
        }

        public ProcedureVariable AddVariable(string name, TypeReference type, SBExpressionData assignment, EntryModifiers modifiers)
        {
            var v = new ProcedureVariable(false, name, type, modifiers);
            v.Assignment = assignment;
            m_localVariables.Add(v);
            return v;
        }

        public ParameterExpression AddLambdaExpressionParameter(Type type, string name)
        {
            if (type.IsGenericParameter) throw new ArgumentException("GenericParameter type not supported (yet)");
            if (type.IsGenericTypeDefinition) throw new ArgumentException("GenericTypeDefinition type not supported (yet)."); // Not sure if this is always right to do.
            if (m_lambdaParameters == null) m_lambdaParameters = new List<ParameterExpression>();
            var p = Expression.Parameter(type, name);
            m_lambdaParameters.Add(p);
            return p;
        }

        public void AddSubStatement(ProcedureParsingScope statementCode)
        {
            m_subStatements.Add(statementCode);
        }

        public List<ProcedureVariable> GetVariables()
        {
            return m_localVariables;
        }

        public List<ProcedureParsingScope> GetSubStatements()
        {
            var subs = m_subStatements;
            m_subStatements = new List<ProcedureParsingScope>();
            return subs;
        }

        public IEnumerable<ParameterExpression> GetLambdaParameters()
        {
            if (m_lambdaParameters != null) return m_lambdaParameters;
            else return new ParameterExpression[] { };
        }

        public BlockExpression GetBlockCode()
        {
            return GetBlockCode(null, null);
        }
        public BlockExpression GetBlockCode(IEnumerable<Expression> preStatements, IEnumerable<Expression> postStatements)
        {
            List<Expression> blockStatements = new List<Expression>();

            if (preStatements != null) blockStatements.AddRange(preStatements);

            foreach (var v in m_localVariables)
            {
                if (v.Assignment != null)
                {
                    blockStatements.Add(Expression.Assign(v.VariableExpression, v.Assignment.ExpressionCode));
                }
            }

            blockStatements.AddRange(m_scopeStatements);

            if (postStatements != null) blockStatements.AddRange(postStatements);

            if (m_localVariables.Count > 0)
            {
                if (blockStatements.Count > 0)
                {
                    return Expression.Block(m_localVariables.Select(v => ((ParameterExpression)v.VariableExpression)), blockStatements);
                }
                else
                {
                    return Expression.Block(m_localVariables.Select(v => ((ParameterExpression)v.VariableExpression)));
                }
            }
            else
            {
                return (blockStatements.Count > 0) ? Expression.Block(blockStatements) : null;
            }
        }

        public IEnumerable<IIdentifierInfo> KnownIdentifiers()
        {
            if (m_lambdaParameters != null)
            {
                foreach (var lp in m_lambdaParameters)
                {
                    yield return new IdentifierInfo(lp.Name, lp.Name, IdentifierType.LambdaParameter, (TypeReference)lp.Type, lp);
                }
            }
            foreach (var local in m_localVariables)
            {
                if (local.Name != null) yield return local;     // Only return variables with a name.
            }
            if (m_parent != null)
            {
                foreach (var fromParent in m_parent.KnownIdentifiers())
                {
                    yield return fromParent;
                }
            }
        }

        public void SetProperties(List<PropertyBlockEntry> props)
        {
            m_propertyBlock = props;
        }

        public List<PropertyBlockEntry> GetProperties()
        {
            var props = m_propertyBlock;
            m_propertyBlock = null;
            return props;
        }

        public void SetAttributes(List<PropertyBlockEntry> attributes = null)
        {
            m_attributes = attributes;
        }

        public List<PropertyBlockEntry> GetAttributes()
        {
            var attribs = m_attributes;
            m_attributes = null;
            return attribs;
        }
    }
}
