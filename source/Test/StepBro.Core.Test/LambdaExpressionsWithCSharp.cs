using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core.Data;
using StepBro.Core.Parser;

namespace StepBroCoreTest
{
    [TestClass]
    public class LambdaExpressionsWithCSharp
    {
        [TestMethod]
        public void GetLinqExpressionInExpressionTree()
        {
            List<long> array = new List<long>();
            Expression<Func<long>> expression = () => array.Where(v => v > 10L).FirstOrDefault();

            ITreeDataElement treeElement = expression.CreateDataTree(null);
        }

        [TestMethod]
        public void GetAndSetParameterValues()
        {
            Expression<Predicate<int>> expression = n => n < 10;

            ITreeDataElement treeElement = expression.CreateDataTree(null);
        }
        [TestMethod]
        public void ListIndexing()
        {
            var list = new List<int>();
            Expression<Func<int>> expression = () => list[0];

            ITreeDataElement treeElement = expression.CreateDataTree(null);
        }

        //[TestMethod]
        //public void CheckDelegateCallExpressionCode()
        //{
        //    Func<long> d = new Func<long>(() => 73L);
        //    Expression<Func<long>> expression = () => d();  // Call the delegate d.

        //    ITreeDataElement treeElement = expression.CreateDataTree(null);
        //}

        [TestMethod]
        public void TryCreate()
        {
            var expression = Expression.Add(Expression.Constant(15L), Expression.Constant(12L));
            //expression.

        }


        [TestMethod]
        public void TryExample()
        {
            BlockExpression blockExpr = Expression.Block(
                Expression.Call(
                    null,
                    typeof(Console).GetMethod("Write", new Type[] { typeof(String) }),
                    Expression.Constant("Hello ")
                   ),
                Expression.Call(
                    null,
                    typeof(Console).GetMethod("WriteLine", new Type[] { typeof(String) }),
                    Expression.Constant("World!")
                    ),
                Expression.Constant(42)
            );

            Console.WriteLine("The result of executing the expression tree:");
            // The following statement first creates an expression tree,
            // then compiles it, and then executes it.           
            var result = Expression.Lambda<Func<int>>(blockExpr).Compile()();

            // Print out the expressions from the block expression.
            Console.WriteLine("The expressions from the block expression:");
            foreach (var expr in blockExpr.Expressions)
                Console.WriteLine(expr.ToString());

            // Print out the result of the tree execution.
            Console.WriteLine("The return value of the block expression:");
            Console.WriteLine(result);
        }

        [TestMethod]
        public void InstanceMethodOnConstantObject()
        {
            var exp = Expression.Call(
                    Expression.Constant(5L),
                    typeof(long).GetMethod("ToString", new Type[] { })
                   );

            var result = Expression.Lambda<Func<string>>(exp).Compile()();
            Assert.AreEqual("5", result);
        }

        [TestMethod]
        public void StaticPropertyGetAndSet()
        {
            var exp = Expression.Property(null, typeof(System.AppDomain).GetProperty("MonitoringIsEnabled"));

            var result = Expression.Lambda<Func<bool>>(exp).Compile()();
            Assert.AreEqual(false, result);
        }
    }
}
