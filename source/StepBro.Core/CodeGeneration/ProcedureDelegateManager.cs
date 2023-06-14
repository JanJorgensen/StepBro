using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Reflection.Emit;
using System.Reflection;

using StepBro.Core.Data;
using StepBro.Core.Execution;

namespace StepBro.Core.CodeGeneration
{
    public static class ProcedureDelegateManager
    {
        private static readonly ModuleBuilder m_module;
        private static Dictionary<string, Type> g_existingTypes = new Dictionary<string, Type>();

        static ProcedureDelegateManager()
        {
            var assembly = AssemblyBuilder.DefineDynamicAssembly(
                new AssemblyName("ProcedureDelegateManager"),
                AssemblyBuilderAccess.RunAndCollect);
            m_module = assembly.DefineDynamicModule("ProcedureDelegateManager");
        }


        internal static string CreateSignatureString(TypeReference returnType)
        {
            return CreateSignatureString(returnType, (NamedData<TypeReference>[])null);
        }

        internal static string CreateSignatureString(TypeReference returnType, params TypeReference[] parameterTypes)
        {
            return CreateSignatureString(returnType, parameterTypes.Select(t => new NamedData<TypeReference>("", t)).ToArray());
            //var typestring = new StringBuilder();
            //typestring.Append("ret_" + returnType.FullName);
            //foreach (var p in parameterTypes)
            //{
            //    typestring.Append("+par_");
            //    typestring.Append(p.FullName);
            //}
            //return typestring.ToString();
        }

        internal static string CreateSignatureString(TypeReference returnType, params NamedData<TypeReference>[] parameters)
        {
            var typestring = new StringBuilder();
            typestring.Append("ret_" + returnType.Type.FullName.Replace('.', '_'));
            if (parameters != null)
            {
                foreach (var p in parameters)
                {
                    typestring.Append("_PAR_");
                    if (!String.IsNullOrEmpty(p.Name))
                    {
                        typestring.Append(p.Name);
                        typestring.Append("_");
                    }
                    typestring.Append(p.Value.Type.FullName.Replace('.', '_'));
                }
            }
            return typestring.ToString();
        }

        internal static Type CreateOrGetDelegateType(TypeReference returnType, params NamedData<TypeReference>[] parameters)
        {
            var signatureString = CreateSignatureString(returnType, parameters);

            Type delegateType;
            if (g_existingTypes.TryGetValue(signatureString, out delegateType)) return delegateType;

            //try
            //{
            //    delegateType = TryMakeStandardDelegateType(returnType, parameters);
            //}
            //catch
            {
                delegateType = CreateDelegateType(signatureString, returnType, parameters);
            }
            g_existingTypes.Add(signatureString, delegateType);
            return delegateType;






            ////return Delegate.CreateDelegate(delDecltype, method);


            //var fooExpr = Expression.Parameter(typeof(Foo), "f");
            //var parmExpr = Expression.Parameter(typeof(int).MakeByRefType(), "i");
            //var method = typeof(Foo).GetMethod("Method1");
            //var invokeExpr = Expression.Call(fooExpr, method, parmExpr);
            //var delegateType = MakeDelegateType(typeof(void), new[] { typeof(Foo), typeof(int).MakeByRefType() });
            //var lambdaExpr = Expression.Lambda(delegateType, invokeExpr, fooExpr, parmExpr);
            //dynamic func = lambdaExpr.Compile();
            //int x = 4;
            //func(new Foo(), ref x);
            //Console.WriteLine(x);

        }

        //static void Main()
        //{
        //    object[] arguments = new object[1];
        //    MethodInfo method = typeof(Test).GetMethod("SampleMethod");
        //    method.Invoke(null, arguments);
        //    Console.WriteLine(arguments[0]); // Prints Hello
        //}

        //public static void SampleMethod(out string text)
        //{
        //    text = "Hello";
        //}

        // https://blogs.msdn.microsoft.com/thottams/2007/11/01/calling-delegates-using-begininvoke-invoke-dynamicinvoke-and-delegate/


        internal static Type TryMakeStandardDelegateType(Type returnType, params Type[] parameterTypes)
        {
            return Expression.GetDelegateType(parameterTypes.Concat(new[] { returnType }).ToArray());
        }

        internal static Type CreateDelegateType(string name, TypeReference returnType, params NamedData<TypeReference>[] parameters)
        {
            if (name == null) name = CreateSignatureString(returnType, parameters);

            var typeBuilder = m_module.DefineType(
                name, TypeAttributes.Sealed | TypeAttributes.Public, typeof(MulticastDelegate));

            var constructor = typeBuilder.DefineConstructor(
                MethodAttributes.RTSpecialName | MethodAttributes.HideBySig | MethodAttributes.Public,
                CallingConventions.Standard, new[] { typeof(object), typeof(IntPtr) });

            constructor.SetImplementationFlags(MethodImplAttributes.CodeTypeMask);

            var parameterTypes = new List<Type>();
            parameterTypes.Add(typeof(ICallContext));
            parameterTypes.AddRange(parameters.Select(e => e.Value.Type));

            var invokeMethod = typeBuilder.DefineMethod(
                "Invoke", MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.Public,
                returnType.Type, parameterTypes.ToArray());

            invokeMethod.SetImplementationFlags(MethodImplAttributes.CodeTypeMask);

            invokeMethod.DefineParameter(1, ParameterAttributes.None, "callcontext");
            for (int i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                var par = invokeMethod.DefineParameter(i + 2, ParameterAttributes.None, parameter.Name);
            }

            return typeBuilder.CreateType();
        }
    }
}
