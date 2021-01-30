using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using StepBro.Core.Data;

namespace StepBroCoreTest
{
    [TestClass]
    public class TestTypeUtils
    {
        [TestMethod]
        public void TestIsAssignableFrom_GenericMethod()
        {
            var method = typeof(Enumerable).GetMethods().Where(m => m.Name == "Select").First();
            Assert.IsTrue(method.GetParameters()[0].IsAssignableFrom(typeof(IEnumerable<int>)));
        }

        [TestMethod]
        public void TestGenericParameterCheck()
        {
            var data = new int[] { 0, 1, 2, 3 };
            var strings = data.Select(i => i.ToString());

            var datatype = typeof(List<int>);
            var typedef = datatype.GetGenericTypeDefinition();
            Assert.IsTrue(datatype.IsGenericType);
            Assert.IsFalse(datatype.IsGenericTypeDefinition);
            Assert.IsTrue(typedef.IsGenericType);
            Assert.IsTrue(typedef.IsGenericTypeDefinition);

            var genericParameters = datatype.GenericTypeArguments;

            var specificEnumerable = typeof(IEnumerable<>).MakeGenericType(genericParameters);

            Assert.IsTrue(specificEnumerable.IsAssignableFrom(datatype));

            Assert.IsTrue(IsGenericInterface(datatype, typedef));



            var source = typeof(IEnumerable<int>);
            var sourceGeneric = source.GetGenericTypeDefinition();

            var extensionMethodsClass = typeof(Enumerable);

            var methodSelect = extensionMethodsClass.GetMethods().Where(m => m.Name == "Select").FirstOrDefault();
            Assert.AreEqual("Select", methodSelect.Name);
            Assert.IsTrue(methodSelect.IsGenericMethod);
            Assert.IsTrue(methodSelect.IsGenericMethodDefinition);

            var methodLast = extensionMethodsClass.GetMethods().Where(m => m.Name == "Last" && m.GetParameters().Length == 2).FirstOrDefault();
            Assert.AreEqual("Last", methodLast.Name);
            Assert.IsTrue(methodLast.IsGenericMethod);
            Assert.IsTrue(methodLast.IsGenericMethodDefinition);

            var neededArgs = methodSelect.GetParameters()[1].ParameterType.ListNeededGenericInputArguments();
            Assert.AreEqual(1, neededArgs.Count());
            Assert.AreSame(neededArgs.First(), methodSelect.GetGenericArguments()[0]);

            //Assert.IsTrue(methodLast.IsAllGenericArgumentsKnown(source));
            //Assert.IsFalse(methodSelect.IsAllGenericArgumentsKnown(source));


            var returnType = methodSelect.ReturnType;
            Assert.IsTrue(returnType.IsGenericType);
            var returnTypeGenericParameters = returnType.GetGenericArguments();
            Assert.AreEqual(1, returnTypeGenericParameters.Length);
            Assert.AreEqual("TResult", returnTypeGenericParameters[0].Name);

            var methodGenericArguments = methodSelect.GetGenericArguments();
            Assert.AreEqual(2, methodGenericArguments.Length);
            Assert.AreEqual("TSource", methodGenericArguments[0].Name);
            Assert.AreEqual("TResult", methodGenericArguments[1].Name);

            var methodParams = methodSelect.GetParameters();
            Assert.AreEqual(2, methodParams.Length);

            Assert.IsTrue(methodParams[0].IsAssignableFrom(source));

            var thisType = methodParams[0].ParameterType;
            Assert.IsTrue(thisType.IsConstructedGenericType);
            Assert.IsTrue(thisType.ContainsGenericParameters);
            Assert.IsTrue(IsGenericInterface(thisType, sourceGeneric));
        }

        private bool IsGenericInterface(Type t, Type g)
        {
            if (t == g) return true;
            if (t.IsGenericType && !t.IsGenericTypeDefinition)
            {
                var tg = t.GetGenericTypeDefinition();
                if (IsGenericInterface(tg, g)) return true;
            }
            foreach (var i in t.GetInterfaces())
            {
                if (IsGenericInterface(i, g)) return true;
            }
            return false;
        }
    }
}