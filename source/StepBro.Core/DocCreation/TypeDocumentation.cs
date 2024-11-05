using Markdig.Syntax;
using Markdig.Syntax.Inlines;
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
                properties.Sort((a, b) => String.Compare(a.Name, b.Name));
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
                    sb.AppendLine($"{new String('#', headingLevel)} Configuration");

                    try
                    {
                        var instance = Activator.CreateInstance(type) as ISettableFromPropertyBlock;
                        if (instance != null)
                        {
                            var decoder = instance.TryGetDecoder();
                            if (decoder != null)
                            {
                                sb.AppendLine($"THERE IS A DECODER !! <br/>");

                                var blocks = new List<Tuple<Element, Element, List<Element>>>();
                                blocks.Add(new Tuple<Element, Element, List<Element>>(decoder, null, new List<Element>()));

                                var levelBlocks = new List<Element>();
                                levelBlocks.Add(decoder);
                                var nextLevelBlocks = new List<Element>();
                                while (levelBlocks.Count > 0)
                                {
                                    foreach (var block in levelBlocks)
                                    {
                                        sb.AppendLine($"Block {block.TypeOrName} <br/>");
                                        foreach (var child in block.ListChilds().Where(c => c.EntryType == PropertyBlockEntryType.Block && c.Usage == Usage.Element))
                                        {
                                            if (blocks.Exists(e => Object.ReferenceEquals(child, e.Item1)))
                                            {
                                                blocks.First(e => Object.ReferenceEquals(child, e.Item1)).Item3.Add(block);
                                            }
                                            else
                                            {
                                                var entry = new Tuple<Element, Element, List<Element>>(child, block, new List<Element>());
                                                entry.Item3.Add(block);
                                                blocks.Add(entry);

                                                nextLevelBlocks.Add(child);
                                            }
                                        }
                                    }
                                    levelBlocks.Clear();
                                    levelBlocks.AddRange(nextLevelBlocks);
                                    nextLevelBlocks.Clear();
                                }

                                foreach (var block in blocks)
                                {
                                    CreateDecoderBlockDocumentation(block.Item1, block.Item2, block.Item3, headingLevel + 1, sb);
                                }
                            }
                        }
                    }
                    catch { }

                }
            }

            // https://learn.microsoft.com/en-us/dotnet/api/system.math

            return sb.ToString();
        }

        private static string GetBlockHeadingLink(PropertyBlockDecoder.Element block)
        {
            var link = block.TypeOrName;
            var parent = block.ParentType();
            if (parent != null) link += ("-" + parent);
            return link;
        }

        private static void CreateDecoderBlockDocumentation(PropertyBlockDecoder.Element block, PropertyBlockDecoder.Element parent, List<Element> hosting, int headingLevel, StringBuilder output)
        {
            output.Append($"{new String('#', headingLevel)} {block.TypeOrName} {new String('#', headingLevel)} {{#{GetBlockHeadingLink(block)}}}");
            if (!String.IsNullOrEmpty(block.AlternativeTypeOrName))
            {
                output.Append(" (or ");
                output.Append(block.AlternativeTypeOrName);
                output.Append(')');
            }
            if (block.ParentType() != null)
            {
                output.Append($" - on ");
            }
            output.AppendLine();

            if (hosting != null && hosting.Count > 0)
            {
                output.Append("Used in: ");
                output.AppendLine(String.Join(", ", hosting.Select(h => $"[{h.TypeOrName}](#{GetBlockHeadingLink(h)})")));
                output.AppendLine();
            }

            if (!String.IsNullOrEmpty(block.Documentation))
            {
                output.AppendLine(block.Documentation);
                output.AppendLine("");
            }
            output.AppendLine("");

            var props = block.ListChilds().Where(c => c.Usage == Usage.Setting).ToList();
            props.Sort((a, b) => String.Compare(a.TypeOrName, b.TypeOrName, false));
            var elements = block.ListChilds().Where(c => c.Usage == Usage.Element).ToList();
            elements.Sort((a, b) => String.Compare(a.TypeOrName, b.TypeOrName, false));

            if (props.Count > 0)
            {
                output.AppendLine("__Properties__");
                foreach (var prop in props)
                {
                    CreateDecoderBlockChild(block, prop, output);
                }
                output.AppendLine();
                output.AppendLine();
            }
            if (elements.Count > 0)
            {
                output.AppendLine("__Elements__");
                foreach (var element in elements)
                {
                    CreateDecoderBlockChild(block, element, output);
                }
                output.AppendLine();
                output.AppendLine();
            }
        }

        private static void CreateDecoderBlockChild(PropertyBlockDecoder.Element block, PropertyBlockDecoder.Element child, StringBuilder output)
        {
            bool isTypeReference = child.EntryType == PropertyBlockEntryType.Block && child.Usage == Usage.Element;
            output.Append("* ");
            if (isTypeReference)
            {
                output.Append("[");
            }
            else
            {
                output.Append("__");
            }
            output.Append(child.TypeOrName);
            if (isTypeReference)
            {
                output.Append($"](#{GetBlockHeadingLink(child)})");
            }
            else
            {
                output.Append("__");
            }
            if (!String.IsNullOrEmpty(child.AlternativeTypeOrName))
            {
                output.Append(" (or ");
                output.Append(child.AlternativeTypeOrName);
                output.Append(")");
            }

            if (!String.IsNullOrEmpty(child.Documentation))
            {
                output.Append("  \\-  ");
                var doc = child.Documentation;
                var lineEnd = doc.IndexOfAny(['\r', '\n']);
                //if (lineEnd >= 0)
                //{
                //    doc = doc.Substring(0, lineEnd);    // Only take the first line.
                //}
                output.AppendLine(doc);
            }
            output.AppendLine("<br/>");
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
