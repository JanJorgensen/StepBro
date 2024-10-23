using StepBro.Core.Data;
using StepBro.Core.General;
using StepBro.Core.ScriptData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StepBro.Core.Api;

namespace StepBro.Core.DocCreation
{
    public static class ScriptDocumentation
    {
        public enum DocCommentLineType { Unspecified, Named, TypeAndNamed }

        public static bool IsDocLine(string line)
        {
            return line.Equals("///") || line.StartsWith("/// ") || line.StartsWith("///\t");
        }

        public static Decoded DecodeDocCommentLines(List<Tuple<DocCommentLineType, string>> lines)
        {
            if (lines == null || lines.Count == 0) return null;

            var decoded = new Decoded();
            foreach (var line in lines)
            {
                var lineContent = line.Item2.Trim().TrimStart('/').TrimStart();

                switch (line.Item1)
                {
                    case DocCommentLineType.Unspecified:
                        {
                            var section = decoded.LastSection;
                            if (section == null)
                            {
                                section = new DecodedSection();
                                decoded.Add(section);
                            }
                            section.AddContent(lineContent);
                        }
                        break;
                    case DocCommentLineType.Named:
                        {
                            var section = new DecodedSection();
                            int colonIndex = lineContent.IndexOf(':');
                            section.Name = lineContent.Substring(0, colonIndex).Trim();
                            section.AddContent(lineContent.Substring(colonIndex + 1).Trim());
                            decoded.Add(section);
                        }
                        break;
                    case DocCommentLineType.TypeAndNamed:
                        {
                            var section = new DecodedSection();
                            int separatorIndex = lineContent.IndexOf(' ');
                            int colonIndex = lineContent.IndexOf(':');
                            section.Type = lineContent.Substring(0, separatorIndex).Trim();
                            section.Name = lineContent.Substring(separatorIndex + 1, colonIndex - separatorIndex - 1).Trim();
                            section.AddContent(lineContent.Substring(colonIndex + 1).Trim());
                            decoded.Add(section);
                        }
                        break;
                    default:
                        break;
                }
            }
            return decoded;
        }

        public static DocCommentLineType GetLineType(string line, ref Tuple<string, string, string> data)
        {
            var firstColon = line.IndexOf(':');
            if (firstColon > 0)
            {
                foreach (var ch in line.Substring(0, firstColon - 1))
                {
                    if (!Char.IsLetterOrDigit(ch) && !Char.IsWhiteSpace(ch) && ch != '_')
                    {
                        return DocCommentLineType.Unspecified;
                    }
                }

                var parts = line.Substring(0, firstColon).Split(' ');
                if (parts.Length == 1)
                {
                    data = new Tuple<string, string, string>(parts[0], null, line.Substring(firstColon + 1));
                    return DocCommentLineType.Named;
                }
                else if (parts.Length == 2)
                {
                    data = new Tuple<string, string, string>(parts[0], parts[1], line.Substring(firstColon + 1));
                    return DocCommentLineType.Named;
                }
            }
            data = new Tuple<string, string, string>(null, null, line);
            return DocCommentLineType.Unspecified;
        }

        public static DocCommentLineType GetLineType(string line)
        {
            line = line.TrimStart(' ');
            var firstColon = line.IndexOf(':');
            if (firstColon > 0)
            {
                foreach (var ch in line.Substring(0, firstColon - 1))
                {
                    if (!Char.IsLetterOrDigit(ch) && !Char.IsWhiteSpace(ch) && ch != '_')
                    {
                        return DocCommentLineType.Unspecified;
                    }
                }

                var parts = line.Substring(0, firstColon).Split(' ');
                if (parts.Length == 1)
                {
                    return DocCommentLineType.Named;
                }
                else if (parts.Length == 2)
                {
                    return DocCommentLineType.TypeAndNamed;
                }
            }
            return DocCommentLineType.Unspecified;
        }

        public static string CreateFileElementDocumentation(int headingLevel, ILoadedFilesManager fileManager, IFileElement element, List<Tuple<DocCommentLineType, string>> docComments = null)
        {
            var docText = new StringBuilder();

            var decoded = (docComments != null && docComments.Count > 0) ? DecodeDocCommentLines(docComments) : null;

            if (decoded == null && element != null)
            {
                docComments = ((FileElement)element).GetDocumentation();
                if (docComments != null && docComments.Count > 0)
                {
                    decoded = DecodeDocCommentLines(docComments);
                }
            }

            if (element != null)
            {
                var idLink = GetElementLinkID(element);
                docText.AppendLine($"{new String('#', headingLevel)} {element.Name} ({element.ElementType.ToSimpleName()}) {new String('#', headingLevel)} {{#{idLink}}}");
                docText.AppendLine($"Namespace: {element.ParentFile.Namespace} </br>");
                docText.AppendLine($"File: {element.ParentFile.FilePath} </br>");
            }

            //if (decoded != null && decoded.ErrorLine >= 0)
            //{
            //    docText.AppendLine("Documentation Formatting Error");
            //}

            var summary = decoded?.TryGetByName(Constants.STEPBRO_DOCCOMMENT_SUMMARY);
            if (summary != null)
            {
                docText.AppendLine($"{new String('#', headingLevel + 1)} Summary");
                foreach (var line in summary.Content)
                {
                    docText.AppendLine(line);
                }
            }

            if (element != null)
            {
                docText.AppendLine($"{new String('#', headingLevel + 1)} Definition");

                string returnType = "";
                string parameters = "";
                if (element is IFileProcedure proc)
                {
                    //docText.AppendLine("</br>");
                    returnType = proc.ReturnType.StepBroTypeName() + " ";
                    int i = 0;
                    StringBuilder paramsText = new StringBuilder();
                    paramsText.Append('(');
                    while (i < proc.Parameters.Length)
                    {
                        if (i > 0) paramsText.Append(", ");
                        if (i == 0 && proc.IsFirstParameterThisReference)
                        {
                            paramsText.Append("this ");
                        }
                        paramsText.Append($"{proc.Parameters[i].Value.StepBroTypeName()} {proc.Parameters[i].Name}");
                        i++;
                    }
                    paramsText.Append(')');
                    parameters = paramsText.ToString();
                    docText.AppendLine($"``` {returnType}{element.Name}{parameters} ```");
                }
                else if (element is FileVariable variable)
                {
                    docText.AppendLine($"{new String('#', headingLevel + 1)} Type {variable.Value.DataType.Type.TypeNameSimple()}");
                    docText.AppendLine(TypeDocumentation.CreateDoc(headingLevel + 2, variable.DataType.Type, false));
                    //docText.AppendLine($"Type external: [{variable.Value.DataType.Type.TypeNameSimple()}](http://www.google.com)</br>");
                    //docText.AppendLine($"Type internal: [{variable.Value.DataType.Type.TypeNameSimple()}](type://{variable.Value.DataType.Type.TypeNameSimple()})</br>");
                }
                else
                {
                    docText.AppendLine($"Documentation not implemented for this file element type. ");
                }
                if (element.ParentElement != null)
                {
                    string inheritance = element.ParentElement.FullName;
                    var parent = element.ParentElement.ParentElement;
                    while (parent != null)
                    {
                        inheritance += "  \u2192  " + parent.FullName;
                        parent = parent.ParentElement;
                    }
                    docText.AppendLine($"Inheritance: {inheritance}");
                }
            }

            var remarks = decoded?.TryGetByName(Constants.STEPBRO_DOCCOMMENT_REMARKS);
            if (remarks != null)
            {
                docText.AppendLine($"{new String('#', headingLevel + 1)} Remarks");
                foreach (var line in remarks.Content)
                {
                    docText.AppendLine(line);
                }
            }

            return docText.ToString();
        }

        public static string GetElementLinkID(IFileElement element)
        {
            return element.FullName.Replace(' ', '-').Replace('.', '-');  // TODO: Create utils-function and add parameter-types too.
        }
    }


    public class DecodedSection
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public List<string> Content { get; set; } = null;
        public void AddContent(string content)
        {
            if (this.Content == null)
            {
                this.Content = new List<string>();
            }
            this.Content.Add(content);
        }
    }

    public class Decoded
    {
        public List<DecodedSection> Sections { get; set; } = null;

        public void Add(DecodedSection section)
        {
            if (this.Sections == null)
            {
                this.Sections = new List<DecodedSection>();
            }
            this.Sections.Add(section);
        }

        public DecodedSection LastSection { get { return (this.Sections != null && this.Sections.Count > 0) ? this.Sections[this.Sections.Count - 1] : null; } }

        public DecodedSection TryGetByName(string name)
        {
            if (this.Sections == null || this.Sections.Count == 0) return null;
            return this.Sections.FirstOrDefault(s => s.Name != null && String.Equals(s.Name, name, StringComparison.InvariantCultureIgnoreCase));
        }

        public List<DecodedSection> TryGetByType(string type)
        {
            return this.Sections.Where(s => s.Type != null && String.Equals(s.Type, type, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }

        //public int ErrorLine { get; set; } = -1;
        //public string ErrorDescription { get; set; } = null;
    }
}
