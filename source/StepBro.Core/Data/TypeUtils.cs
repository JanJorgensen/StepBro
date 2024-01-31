using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace StepBro.Core.Data
{
    public static class TypeUtils
    {
        public static bool TryParse(this string s, out bool result)
        {
            if (Boolean.TryParse(s, out result)) return true;
            switch (s.ToLower())
            {
                case "false":
                case "0":
                case "no":
                case "nej":
                case "off":
                case "disable":
                case "disabled":
                case "reset":
                case "unset":
                case "end":
                case "stop":
                case "never":
                case "break":
                case "not":
                case "dont":
                case "fail":
                case "failed":
                case "wrong":
                case "-":
                    result = false;
                    break;
                case "true":
                case "1":
                case "yes":
                case "ja":
                case "on":
                case "enable":
                case "enabled":
                case "set":
                case "begin":
                case "start":
                case "now":
                case "always":
                case "do":
                case "pass":
                case "passed":
                case "ok":
                case "correct":
                case "+":
                    result = true;
                    break;
                default:
                    return false;
            }
            return true;
        }

        public static bool IsDelegate(this Type type)
        {
            return type != null && type.IsSubclassOf(typeof(Delegate)) || type == typeof(Delegate);
        }

        public static bool IsExtension(this MethodInfo method)
        {
            return method.IsDefined(typeof(ExtensionAttribute), true);
        }

        public static string TypeName(this Type type)
        {
            if (type.IsConstructedGenericType)
            {
                var gtd = type.GetGenericTypeDefinition().Name;
                gtd = gtd.Substring(0, gtd.IndexOf('`'));
                var gta = type.GetGenericArguments();
                var args = String.Join(", ", gta.Select(ga => ga.TypeName()));
                return $"{gtd}<{args}>";
            }
            else return type.Name;
        }

        public static string TypeName(this TypeReference type)
        {
            if (type.DynamicType != null) return type.DynamicType.ToString();
            return type.Type.TypeName();
        }

        public static IEnumerable<Type> SelfBasesAndInterfaces(this Type type, bool includeBaseClasses = true, bool includeInterfaces = true)
        {
            yield return type;
            if (includeBaseClasses)
            {
                var t = type.BaseType;
                while (t != null)
                {
                    yield return t;
                    t = t.BaseType;
                }
            }
            if (includeInterfaces)
            {
                foreach (var i in type.GetInterfaces()) yield return i;
            }
        }

        public static IEnumerable<Type> SelfAndInterfaces(this Type type)
        {
            yield return type;
            foreach (var i in type.GetInterfaces()) yield return i;
        }

        public static bool IsAssignableFrom(this ParameterInfo parameter, Type input)
        {
            var pt = parameter.ParameterType;
            if (pt.IsGenericTypeDefinition) throw new ArgumentException("Generic type definition not supported.");
            if (input.IsGenericTypeDefinition) throw new ArgumentException("Generic type definition not supported.");
            if (input.IsGenericType && pt.ContainsGenericParameters && pt.IsGenericType)
            {
                var thistypeGenericDefinition = input.GetGenericTypeDefinition();
                var ptTypedefinition = pt.GetGenericTypeDefinition();
                if (ptTypedefinition == thistypeGenericDefinition)
                {
                    return true;
                }
                foreach (var i in ptTypedefinition.GetInterfaces())
                {
                    if (i == thistypeGenericDefinition)
                    {
                        return true;
                    }
                }
            }
            else
            {
                if (pt.IsAssignableFrom(input))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsAllGenericArgumentsKnown(this MethodInfo method, Type instance)
        {
            return false;
        }

        public static bool HasGenericArguments(this Type type)
        {
            if (type.IsGenericType)
            {
                foreach (Type arg in type.GetGenericArguments())
                {
                    if (arg.IsGenericParameter) return true;
                }
            }
            return false;
        }

        public static MethodInfo MakeGenericMethod(this MethodInfo method, List<Tuple<string, Type>> arguments)
        {
            return method.MakeGenericMethod(
                method.GetGenericArguments().Select(
                    t => arguments.Find(tt => tt.Item1 == t.Name).Item2).ToArray());
        }

        public static Type MakeGenericType(this Type genericType, List<Tuple<string, Type>> arguments)
        {
            return genericType.MakeGenericType(
                genericType.GenericTypeArguments.Select(
                    t => arguments.Find(tt => tt.Item1 == t.Name).Item2).ToArray());
        }

        public static List<Tuple<string, Type>> GetTypedGenericArguments(this Type type, Type genericType = null)
        {
            var gt = (genericType != null) ? genericType : type.GetGenericTypeDefinition();
            var genericArgs = genericType.GetGenericArguments();
            var typedArgs = type.GetGenericArguments();
            return typedArgs.Select((t, i) => new Tuple<string, Type>(genericArgs[i].Name, t)).ToList();
        }

        public static IEnumerable<Type> ListNeededGenericInputArguments(this Type thisDelegateType)
        {
            if (!thisDelegateType.IsDelegate() || !thisDelegateType.IsGenericType)
            {
                throw new ArgumentException("The type is not a generic delelegate type.");
            }

            var inputTypes = new List<Type>();

            foreach (ParameterInfo pi in thisDelegateType.GetMethod("Invoke").GetParameters())
            {
                if (pi.ParameterType.IsGenericParameter)
                {
                    if (!inputTypes.Contains(pi.ParameterType))
                    {
                        inputTypes.Add(pi.ParameterType);
                    }
                }
            }
            return inputTypes;
        }

        public static IEnumerable<TSource> Concat<TSource>(this IEnumerable<TSource> list, TSource element)
        {
            foreach (var e in list)
            {
                yield return e;
            }
            yield return element;
        }

        public static bool IsPrimitiveNarrowableIntType(this Type type)
        {
            return (type == typeof(Int32) ||
                type == typeof(UInt32) ||
                type == typeof(UInt64) ||
                type == typeof(Int16) ||
                type == typeof(UInt16) ||
                type == typeof(SByte) ||
                type == typeof(Byte));
        }

        public static string GetDebugView(this Expression exp)
        {
            if (exp == null)
                return null;

            var propertyInfo = typeof(Expression).GetProperty("DebugView", BindingFlags.Instance | BindingFlags.NonPublic);
            return propertyInfo.GetValue(exp) as string;
        }
    }
}
