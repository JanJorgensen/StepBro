using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StepBro.Core.Data;
using System.Linq.Expressions;


namespace StepBro.Core.Parser
{
    public static class ExpressionToDataTree
    {
        public static TreeDataElement CreateDataTree(this Expression expression, TreeDataElement parent)
        {
            TreeDataElement element = null;
            if (expression is BinaryExpression)
            {
                var binary = (BinaryExpression)expression;
                element = new TreeDataElement(parent, expression.NodeType.ToString(), expression.ToString());
                element.AddElement(binary.Left.CreateDataTree(element));
                element.AddElement(binary.Right.CreateDataTree(element));
            }
            else if (expression is ParameterExpression)
            {
                var parameter = (ParameterExpression)expression;
                element = new TreeDataElement(
                    parent,
                    expression.NodeType.ToString(),
                    String.Format("{0}{1} \"{2}\"",
                        parameter.IsByRef ? "ref " : "",
                        parameter.Type.Name,
                        parameter.Name));
            }
            else if (expression is ConstantExpression)
            {
                var constant = (ConstantExpression)expression;
                element = new TreeDataElement(parent, expression.NodeType.ToString(), constant.Value.ToString() + " (" + constant.Type.Name + ")");
            }
            else if (expression is LambdaExpression)
            {
                var lambda = (LambdaExpression)expression;
                element = new TreeDataElement(parent, expression.NodeType.ToString(), expression.ToString());
                foreach (var p in lambda.Parameters)
                {
                    element.AddElement(p.CreateDataTree(element));
                }
                var body = new TreeDataElement(parent, "Body", lambda.Body.ToString());
                element.AddElement(body);
                body.AddElement(lambda.Body.CreateDataTree(element));
            }
            else if (expression is MethodCallExpression)
            {
                var call = (MethodCallExpression)expression;
                element = new TreeDataElement(parent, expression.NodeType.ToString(), call.Method.DeclaringType.Name + "." + call.Method.Name);
                int i = 0;
                foreach (var p in call.Arguments)
                {
                    var argElement = element.AddElement(new TreeDataElement(element, "Argument " + i.ToString()));
                    argElement.AddElement(p.CreateDataTree(element));
                    i++;
                }
            }
            else if (expression is MemberExpression)
            {
                var member = (MemberExpression)expression;
                element = new TreeDataElement(parent, expression.NodeType.ToString(), expression.ToString());
                element.AddElement(new TreeDataElement(element, "Member", member.Member.Name));
                element.AddElement(new TreeDataElement(element, "Type", member.Type.ToString()));
            }
            else
            {
                System.Diagnostics.Debug.WriteLine(expression.GetType().Name);
                System.Diagnostics.Debug.WriteLine(expression.NodeType.ToString());
                throw new NotImplementedException();
            }
            return element;
        }
    }
}
