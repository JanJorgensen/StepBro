using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using StepBro.Core.Data;

namespace StepBro.Core.Parser
{
    internal class ProcedureVariable : IIdentifierInfo
    {
        public bool IsParameter { get; set; }
        public string Name { get; set; }
        public TypeReference Type { get; set; }
        public ParameterExpression VariableExpression { get; private set; }
        public SBExpressionData Assignment { get; set; }
        public EntryModifiers Modifiers { get; set; }

        public bool IsLocalVariable
        {
            get
            {
                return ((this.Modifiers & (EntryModifiers.Static | EntryModifiers.Execution)) == 0);
            }
        }

        public ProcedureVariable(bool isParameter, string name, TypeReference type, EntryModifiers modifiers)
        {
            this.IsParameter = isParameter;
            this.Name = name;
            this.Type = type;
            this.Modifiers = modifiers;
            if (this.IsLocalVariable)
            {
                this.VariableExpression = Expression.Variable(type.Type, name);
            }
            else
            {
                this.VariableExpression = null;
            }
        }

        #region IIdentifierInfo

        IdentifierType IIdentifierInfo.Type
        {
            get
            {
                return this.IsParameter ? IdentifierType.Parameter : IdentifierType.Variable;
            }
        }

        TypeReference IIdentifierInfo.DataType
        {
            get
            {
                return this.Type;
            }
        }

        object IIdentifierInfo.Reference
        {
            get
            {
                return this.VariableExpression;
            }
        }

        public string FullName
        {
            get
            {
                return this.Name;
            }
        }

        #endregion

        public override string ToString()
        {
            return "Variable \"" + this.Name + "\" of type " + this.Type.Type.Name;
        }
    }
}
