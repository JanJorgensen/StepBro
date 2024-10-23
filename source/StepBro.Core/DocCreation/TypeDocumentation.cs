using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.ScriptData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using static StepBro.Core.Data.PropertyBlockDecoder;

namespace StepBro.Core.DocCreation
{
    public static class TypeDocumentation
    {

        public static string CreateInternalTypeLink(Type type)
        {
            return $"Type internal: [{type.TypeNameSimple()}](type://{type.TypeName()})</br>";
        }
        public static string CreateInternalMethodLink(System.Reflection.MethodInfo method)
        {
            return $"[{method.DeclaringType.TypeName()}.{method.Name}](method://{method.DeclaringType.TypeName()}.{method.Name})</br>";
        }
        public static string CreateInternalPropertyLink(System.Reflection.PropertyInfo property)
        {
            return $"[{property.DeclaringType.TypeName()}.{property.Name}](property://{property.DeclaringType.TypeName()}.{property.Name})</br>";
        }

        public static string CreateMicrosoftTypeLink(Type type)
        {
            return $"[{type.TypeFullName()}](https://learn.microsoft.com/en-us/dotnet/api/{type.TypeFullName().ToLowerInvariant()})</br>";
        }
        public static string CreateMicrosoftMethodLink(System.Reflection.MethodInfo method)
        {
            return $"[{method.DeclaringType.TypeName()}.{method.Name}](https://learn.microsoft.com/en-us/dotnet/api/{method.DeclaringType.TypeFullName().ToLowerInvariant() + "." + method.Name.ToLowerInvariant()})</br>";
        }
        public static string CreateMicrosoftPropertyLink(System.Reflection.PropertyInfo property)
        {
            return $"[{property.DeclaringType.TypeName()}.{property.Name}](https://learn.microsoft.com/en-us/dotnet/api/{property.DeclaringType.TypeFullName().ToLowerInvariant() + "." + property.Name.ToLowerInvariant()})</br>";
        }

        public static string CreateDoc(int headingLevel, Type type, bool createHeader = true)
        {
            StringBuilder sb = new StringBuilder();

            if (createHeader)
            {
                var basicType = "type";
                if (type.IsEnum) basicType = "enum";
                else if (type.IsClass) basicType = "class";
                else if (type.IsArray) basicType = "array";

                sb.AppendLine($"{new String('#', headingLevel)} {type.TypeName()} ({basicType})");
                headingLevel++;
            }

            sb.AppendLine($"Assembly: {Path.GetFileName(type.Assembly.Location)} </br>");
            if (!type.Assembly.Location.StartsWith(typeof(Verdict).Assembly.Location, StringComparison.InvariantCultureIgnoreCase))
            {
                if (type.Namespace.Equals("System", StringComparison.InvariantCulture) || type.Namespace.StartsWith("System.", StringComparison.InvariantCulture))
                {
                    sb.AppendLine($"Microsoft documentation: {CreateMicrosoftTypeLink(type)}</br>");
                }
            }

            if (type.IsEnum)
            {
                sb.AppendLine($"ENUM INFO </br>");
            }
            else if (type.IsPrimitive)
            {
                sb.AppendLine($"PRIMITIVE INFO </br>");
            }
            else if (type == typeof(String))
            {
                sb.AppendLine($"STRING INFO </br>");
            }
            else if (type.IsClass)
            {
                //sb.AppendLine($"CLASS INFO </br>");
                string inheritance = "";
                var t = type.BaseType;
                while (t != null)
                {
                    if (!String.IsNullOrEmpty(inheritance))
                    {
                        inheritance += " \u2192 ";
                    }
                    inheritance += t.TypeFullName();
                    t = t.BaseType;
                }
                if (!String.IsNullOrEmpty(inheritance))
                {
                    sb.AppendLine($"Inheritance: {inheritance} </br>");
                }


                sb.AppendLine($"{new String('#', headingLevel)} Properties");
                var properties = type.GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.FlattenHierarchy).ToList();
                properties.Sort((a, b) => String.Compare(a.Name, b.Name));
                foreach (var p in properties)
                {
                    //sb.AppendLine($"{p.Name} instance property </br>");
                    sb.AppendLine($"[{p.Name}](property://{p.DeclaringType.TypeFullName()}.{p.Name})</br>");
                }
                properties = type.GetProperties(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.FlattenHierarchy).ToList();
                properties.Sort((a,b) => String.Compare(a.Name,b.Name));
                foreach (var p in properties)
                {
                    //sb.AppendLine($"{p.Name} static property </br>");
                    sb.AppendLine($"[{p.Name}](property://{p.DeclaringType.TypeFullName()}.{p.Name})</br>");
                }

                sb.AppendLine($"{new String('#', headingLevel)} Methods");

                var publicFlagSeen = type.GetMethods().Where(m => !m.IsSpecialName).Any(m => m.GetCustomAttribute<PublicAttribute>() != null);

                var methods = type.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.FlattenHierarchy).Where(m => !m.IsSpecialName);
                if (publicFlagSeen)
                {
                    sb.AppendLine($"Type has the Public attribute </br>");
                    methods = methods.Where(m => m.GetCustomAttribute<PublicAttribute>() != null);
                }
                var uniqueMethods = methods.Select(m => m.Name).Distinct().ToList();
                uniqueMethods.Sort();

                if (methods.Any())
                {
                    sb.AppendLine($"_Instance Methods_ </br>");

                    foreach (var methodName in uniqueMethods)
                    {
                        var overloaded = methods.Where(m => m.Name.Equals(methodName)).ToList();
                        if (overloaded.Count > 1)
                        {
                            sb.AppendLine($"[{methodName}](method://{overloaded[0].DeclaringType.TypeFullName()}.{overloaded[0].Name}) ({overloaded.Count} overloaded methods) </br>");
                        }
                        else
                        {
                            //sb.AppendLine($"{m.Name} instance method </br>");
                            sb.AppendLine($"[{methodName}](method://{overloaded[0].DeclaringType.TypeFullName()}.{overloaded[0].Name}-{overloaded[0].GetHashCode()}) </br>");
                        }
                    }
                }

                methods = type.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.FlattenHierarchy).Where(m => !m.IsSpecialName);
                if (publicFlagSeen)
                {
                    methods = methods.Where(m => m.GetCustomAttribute<PublicAttribute>() != null);
                }
                uniqueMethods = methods.Select(m => m.Name).Distinct().ToList();
                uniqueMethods.Sort();

                if (methods.Any())
                {
                    sb.AppendLine($"_Static Methods_ </br>");
                    foreach (var methodName in uniqueMethods)
                    {
                        var overloaded = methods.Where(m => m.Name.Equals(methodName)).ToList();
                        if (overloaded.Count > 1)
                        {
                            sb.AppendLine($"[{methodName}](method://{overloaded[0].DeclaringType.TypeFullName()}.{overloaded[0].Name}) ({overloaded.Count} overloaded methods) </br>");
                        }
                        else
                        {
                            //sb.AppendLine($"{m.Name} instance method </br>");
                            sb.AppendLine($"[{methodName}](method://{overloaded[0].DeclaringType.TypeFullName()}.{overloaded[0].Name}-{overloaded[0].GetHashCode()}) </br>");
                        }
                    }
                }
                //sb.AppendLine($"{new String('#', headingLevel + 1)} Procedures");

                if (type.IsAssignableTo(typeof(ISettableFromPropertyBlock)))
                {
                    sb.AppendLine($"{new String('#', headingLevel)} Special Setup");
                }
            }

            // https://learn.microsoft.com/en-us/dotnet/api/system.math

            return sb.ToString();
        }

        public static string CreateDoc(int headingLevel, System.Reflection.MethodInfo method, bool createHeader = true)
        {
            StringBuilder sb = new StringBuilder();

            if (createHeader)
            {
                sb.AppendLine($"{new String('#', headingLevel)} {method.Name} method");
            }

            sb.AppendLine($"{new String('#', headingLevel + 1)} Definition");
            sb.AppendLine($"``` {CreateSignatureString(method)} ```");


            var type = method.DeclaringType;
            var overloads = type.GetMethods(System.Reflection.BindingFlags.Static | BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.FlattenHierarchy).
                Where(m => !m.IsSpecialName && String.Equals(method.Name, m.Name, StringComparison.InvariantCulture)).ToList();

            if (overloads.Count > 1)
            {
                sb.AppendLine($"{new String('#', headingLevel + 1)} Overloads");
                foreach (var overloaded in overloads)
                {
                    sb.AppendLine($"[{CreateSignatureString(overloaded)}](method://{overloaded.DeclaringType.TypeFullName()}.{overloaded.Name}-{overloaded.GetHashCode()}) </br>");
                }
            }
            else
            {

            }


            // https://learn.microsoft.com/en-us/dotnet/api/system.math

            return sb.ToString();
        }

        public static string CreateDoc(int headingLevel, System.Reflection.PropertyInfo property, bool createHeader = true)
        {
            StringBuilder sb = new StringBuilder();

            if (createHeader)
            {
                sb.AppendLine($"{new String('#', headingLevel)} {property.Name} property");
            }

            // https://learn.microsoft.com/en-us/dotnet/api/system.math

            return sb.ToString();
        }

        public static string CreateSignatureString(System.Reflection.MethodInfo method, bool hideImplicitParameters = true)
        {
            string returnType = method.ReturnType.StepBroTypeName();
            var parameters = method.GetParameters();
            int i = 0;
            StringBuilder paramsText = new StringBuilder();
            paramsText.Append('(');
            while (i < parameters.Length)
            {
                if (hideImplicitParameters && parameters[i].GetCustomAttribute<ImplicitAttribute>() != null)
                {
                    i++;
                    continue;
                }
                if (i > 0) paramsText.Append(',');

                paramsText.Append($" {parameters[i].ParameterType.StepBroTypeName()} {parameters[i].Name}");
                i++;
            }
            if (paramsText.Length > 1) paramsText.Append(' ');
            paramsText.Append(')');
            return $"{returnType} {method.Name}{paramsText.ToString()}";
        }
    }
}
