//#define PRINT_TREE

using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using StepBro.Core.Data;
//using StepBro.Core.ScriptData;
using SBP = StepBro.Core.Parser.Grammar.StepBro;
using StepBro.Core.ScriptData;
using System.Xml.Linq;
using System.ComponentModel;

namespace StepBro.Core.Parser
{
    internal class StepBroTypeScanListener : StepBro.Core.Parser.Grammar.StepBroBaseListener
    {
        public class UsingData
        {
            public int Line = -1;
            public string Type = null;
            public string Name = null;
            public UsingData(int line, string type, string name)
            {
                this.Line = line;
                this.Type = type;
                this.Name = name;
            }
        }

        public class ScannedTypeDescriptor
        {
            private string m_typeName = null;
            private IToken m_typeNameToken = null;
            private TypeReference m_resolvedType = null;
            private List<ScannedTypeDescriptor> m_typeParameters = null;

            public Tuple<string, IToken> GetGenericType()
            {
                return new Tuple<string, IToken>(m_typeName, m_typeNameToken);
            }

            public int ParameterCount { get { return (m_typeParameters != null) ? m_typeParameters.Count : 0; } }

            public ScannedTypeDescriptor GetParameter(int index) { return m_typeParameters[index]; }

            public string TypeName { get { return m_typeName; } }

            public IToken Token { get { return m_typeNameToken; } }

            public TypeReference ResolvedType { get { return m_resolvedType; } set { m_resolvedType = value; } }

            public void SetTypeName(string name, IToken token)
            {
                m_typeName = name;
                m_typeNameToken = token;
            }

            public void AddParameter(ScannedTypeDescriptor parameter)
            {
                if (m_typeParameters == null) m_typeParameters = new List<ScannedTypeDescriptor>();
                m_typeParameters.Add(parameter);
            }

            public override string ToString()
            {
                var s = m_typeName;
                if (m_typeParameters != null && m_typeParameters.Count > 0)
                {
                    s = s + "<" + String.Join(", ", m_typeParameters) + ">";
                }
                return s;
            }
        }

        public class FileElement
        {
            public FileElement Parent { get; private set; }
            public int Line { get; private set; }
            public ScriptData.FileElementType Type { get; private set; }
            public List<string> Modifiers { get; set; }
            public string ModifiersString()
            {
                return (this.Modifiers != null && this.Modifiers.Count > 0) ? String.Join(" & ", this.Modifiers) : "";
            }
            public string Name { get; private set; }
            public Tuple<string, IToken> ReturnTypeData { get; set; } = null;
            public Tuple<string, IToken> AsType { get; set; } = null;
            public string ReturnType { get { return this.ReturnTypeData.Item1; } }
            public List<ParameterData> Parameters { get; set; } = null;
            public List<NamedString> Values { get; set; } = null;   // For enum values.
            public List<FileElement> Childs { get; set; } = new List<FileElement>();
            public List<string> PropertyFlags { get; set; } = null;
            public bool IsFunction { get; set; } = false;
            public bool HasBody { get; set; } = true;
            public ScannedTypeDescriptor DataType { get; set; } = null;
            public FileElement(FileElement parent, int line, ScriptData.FileElementType type, string name)
            {
                this.Parent = parent;
                this.Line = line;
                this.Type = type;
                this.Name = name;
            }
            public FileElement Next()
            {
                if (this.Parent != null)
                {
                    return this.Parent.Childs.ElementAtOrDefault(this.Parent.Childs.IndexOf(this) + 1);
                }
                return null;
            }

            public override string ToString()
            {
                return $"{this.Type} {this.Name}";
            }
        }

        public class FileContent
        {
            private readonly List<UsingData> m_usings = new List<UsingData>();
            public FileContent(List<UsingData> usings, FileElement topElement)
            {
                m_usings = usings;
                this.TopElement = topElement;
            }
            public FileElement TopElement { get; private set; }

            public IEnumerable<UsingData> ListUsings()
            {
                foreach (var u in m_usings)
                {
                    yield return u;
                }
            }
        }

        public StepBroTypeScanListener(string @namespace)
        {
            this.SetNamespace(@namespace);
        }

        private List<UsingData> m_usings = new List<UsingData>();
        private FileElement m_firstElement = null;
        private bool m_namespaceSet = false;
        private int m_elementStartLine = -1;
        private Stack<FileElement> m_elementStack = new Stack<FileElement>();
        private string m_name = null;
        private string m_type = null;
        private Tuple<string, IToken> m_typedefName = null;
        private Tuple<string, IToken> m_returnType = null;
        private List<ParameterData> m_procedureParameters = null;
        private List<string> m_modifiers = null;
        private List<string> m_elementPropFlags = null;
        private bool m_acceptElementPropFlags = true;
        private Stack<Tuple<string, IToken>> m_typeStack = new Stack<Tuple<string, IToken>>();
        private Stack<ScannedTypeDescriptor> m_typedefStack = new Stack<ScannedTypeDescriptor>();
        private bool m_isFunction = false;
        private string[] m_parameterModifiers = null;


        public FileElement TopElement
        {
            get
            {
                return (m_elementStack.Count > 0) ? m_elementStack.Peek() : m_firstElement;
            }
        }

        public FileContent GetContent()
        {
            return new FileContent(m_usings, this.TopElement);
        }

        //public IEnumerable<Tuple<string, string>> ListUsings() { return m_usings; }

        private void PushType(string location, string type, IToken token)
        {
            m_typeStack.Push(new Tuple<string, IToken>(type, token));
            //System.Diagnostics.Debug.WriteLine($"   Push type: {type} @ {location}");
        }

        private Tuple<string, IToken> PopType(string location)
        {
            var type = m_typeStack.Pop();
            //System.Diagnostics.Debug.WriteLine($"   Pop type: {type} @ {location}");
            return type;
        }

        public override void ExitUsingDeclarationWithIdentifier([NotNull] SBP.UsingDeclarationWithIdentifierContext context)
        {
            if (context.ChildCount == 3)
            {
                m_usings.Add(new UsingData(context.Start.Line, "i", context.GetChild(1).GetText()));
            }
            else if (context.ChildCount == 4)
            {
                m_usings.Add(new UsingData(context.Start.Line, "I", context.GetChild(2).GetText()));
            }
        }

        public override void ExitUsingDeclarationWithPath([NotNull] SBP.UsingDeclarationWithPathContext context)
        {
            if (context.ChildCount == 3)
            {
                m_usings.Add(new UsingData(context.Start.Line, "p", StepBroListener.ParseStringLiteral(context.GetChild(1).GetText(), context)));
            }
            else if (context.ChildCount == 4)
            {
                m_usings.Add(new UsingData(context.Start.Line, "P", StepBroListener.ParseStringLiteral(context.GetChild(2).GetText(), context)));
            }
        }

        public override void ExitNamespace([NotNull] SBP.NamespaceContext context)
        {
            this.SetNamespace(context.GetText());
            m_namespaceSet = true;  // Indicate namespace was seen in the file.
        }

        private void SetNamespace(string name)
        {
            if (m_namespaceSet)
            {
                throw new Exception("Namespace already set.");
            }
            var element = new FileElement(this.TopElement, -1, ScriptData.FileElementType.Namespace, name);
            if (m_elementStack.Count == 0) m_firstElement = element;
            m_elementStack.Clear();
            m_elementStack.Push(element);
        }

        #region TypeDef

        public override void EnterTypedef([NotNull] SBP.TypedefContext context)
        {
            m_typedefStack.Clear();
            m_typedefStack.Push(new ScannedTypeDescriptor());
            m_typedefName = null;
        }

        public override void ExitTypedefName([NotNull] SBP.TypedefNameContext context)
        {
            m_typedefName = new Tuple<string, IToken>(context.GetText(), context.Start);
        }

        public override void ExitTypeSimple([NotNull] SBP.TypeSimpleContext context)
        {
            var type = this.PopType("ExitTypeSimple");
            m_typedefStack.Peek().SetTypeName(type.Item1, type.Item2);
        }

        public override void ExitTypedef([NotNull] SBP.TypedefContext context)
        {
            var element = new FileElement(this.TopElement, m_elementStartLine, ScriptData.FileElementType.TypeDef, m_typedefName.Item1);
            element.Modifiers = m_modifiers;
            element.DataType = m_typedefStack.Pop();
            this.TopElement.Childs.Add(element);
        }

        public override void EnterTypeParameter([NotNull] SBP.TypeParameterContext context)
        {
            m_typedefStack.Push(new ScannedTypeDescriptor());
        }

        public override void ExitTypeParameter([NotNull] SBP.TypeParameterContext context)
        {
            var par = m_typedefStack.Pop();
            m_typedefStack.Peek().AddParameter(par);
        }

        public override void ExitTypeGeneric([NotNull] SBP.TypeGenericContext context)
        {
            var genericType = PopType("generictype");
            var type = m_typedefStack.Peek();
            type.SetTypeName(genericType.Item1, genericType.Item2);
        }

        #endregion

        public override void EnterFileElement([NotNull] SBP.FileElementContext context)
        {
            m_modifiers = null;
            m_elementPropFlags = null;
            m_acceptElementPropFlags = true;

#if (PRINT_TREE)
            m_indent = m_indent.Substring(0, m_indent.Length - 4) + "|   ";
#endif
        }

        public override void ExitFileElement([NotNull] SBP.FileElementContext context)
        {
        }

        public override void EnterElementModifier([NotNull] SBP.ElementModifierContext context)
        {
            if (m_modifiers == null) m_modifiers = new List<string>();
            m_modifiers.Add(context.GetText());
        }

        public override void EnterFileElementProcedure([NotNull] SBP.FileElementProcedureContext context)
        {
            m_isFunction = false;
            m_elementStartLine = context.Start.Line;
        }
        public override void EnterFileElementFunction([NotNull] SBP.FileElementFunctionContext context)
        {
            m_isFunction = true;
            m_elementStartLine = context.Start.Line;
        }

        public override void EnterProcedureParameters([NotNull] SBP.ProcedureParametersContext context)
        {
            m_procedureParameters = new List<ParameterData>();    // New list; it will be handed to the file element object.
        }

        public override void ExitFormalParameterModifiers([NotNull] SBP.FormalParameterModifiersContext context)
        {
            var m = context.GetText();
            m_parameterModifiers = m.Split(' ');
        }

        public override void ExitFormalParameterDecl([NotNull] SBP.FormalParameterDeclContext context)
        {
        }

        public override void ExitFormalParameterDeclStart([NotNull] SBP.FormalParameterDeclStartContext context)
        {
            var type = this.PopType("ExitFormalParameterDecl");
            var name = context.GetChild(context.children.Count - 1).GetText();
            m_procedureParameters.Add(new ParameterData(m_parameterModifiers, name, type.Item1, typeToken: type.Item2));
            m_parameterModifiers = null;
        }

        public override void ExitProcedureParameters([NotNull] SBP.ProcedureParametersContext context)
        {
            base.ExitProcedureParameters(context);
        }
        public override void ExitProcedureReturnType([NotNull] SBP.ProcedureReturnTypeContext context)
        {
            m_returnType = this.PopType("ExitProcedureReturnType");
        }

        public override void ExitProcedureName([NotNull] SBP.ProcedureNameContext context)
        {
            m_name = context.GetText();
        }

        public override void ExitVariableType([NotNull] SBP.VariableTypeContext context)
        {
            m_type = context.GetText();
        }

        #region Element PropertyList

        public override void EnterElementPropertyList([NotNull] SBP.ElementPropertyListContext context)
        {
        }

        public override void ExitElementPropertyList([NotNull] SBP.ElementPropertyListContext context)
        {
        }

        public override void EnterPropertyblockStatementValueIdentifierOnly([NotNull] SBP.PropertyblockStatementValueIdentifierOnlyContext context)
        {
            if (m_acceptElementPropFlags)
            {
                if (m_elementPropFlags == null) m_elementPropFlags = new List<string>();
                m_elementPropFlags.Add(context.GetText());
            }
        }
        public override void EnterPropertyblockStatementNamed([NotNull] SBP.PropertyblockStatementNamedContext context)
        {
            m_acceptElementPropFlags = false;
        }
        public override void EnterPropertyblockEventVerdict([NotNull] SBP.PropertyblockEventVerdictContext context)
        {
            m_acceptElementPropFlags = false;
        }
        public override void EnterPropertyblockEventAssignment([NotNull] SBP.PropertyblockEventAssignmentContext context)
        {
            m_acceptElementPropFlags = false;
        }

        #endregion

        public override void EnterProcedureBodyOrNothing([NotNull] SBP.ProcedureBodyOrNothingContext context)
        {
            var name = m_name;
            var parameters = m_procedureParameters;

            var element = new FileElement(this.TopElement, m_elementStartLine, ScriptData.FileElementType.ProcedureDeclaration, name);
            element.Modifiers = m_modifiers;
            element.Parameters = parameters;
            element.ReturnTypeData = m_returnType;
            element.IsFunction = m_isFunction;
            element.HasBody = context.Start.Type != SBP.SEMICOLON;
            element.PropertyFlags = m_elementPropFlags;
            this.TopElement.Childs.Add(element);
        }

        public override void ExitTestListName([NotNull] SBP.TestListNameContext context)
        {
            m_name = context.GetText();
        }

        public override void EnterTestlist([NotNull] SBP.TestlistContext context)
        {
            m_elementStartLine = context.Start.Line;
        }

        public override void ExitTestlist([NotNull] SBP.TestlistContext context)
        {
            var name = m_name;
            //var parameters = m_procedureParameters;

            var element = new FileElement(this.TopElement, m_elementStartLine, ScriptData.FileElementType.TestList, name);
            element.Modifiers = m_modifiers;
            element.PropertyFlags = m_elementPropFlags;
            //element.Parameters = parameters;
            //element.HasBody = context.Start.Type != SBP.SEMICOLON;
            this.TopElement.Childs.Add(element);
        }

        public override void EnterFileElementOverride([NotNull] SBP.FileElementOverrideContext context)
        {
            m_name = null;
            m_typedefName = null;
        }

        public override void ExitFileElementOverride([NotNull] SBP.FileElementOverrideContext context)
        {
            var element = new FileElement(this.TopElement, m_elementStartLine, ScriptData.FileElementType.Override, m_name);
            element.AsType = m_typedefName;
            this.TopElement.Childs.Add(element);
        }

        public override void EnterOverrideReference([NotNull] SBP.OverrideReferenceContext context)
        {
            m_name = context.GetText();
        }

        public override void EnterFileVariableSimple([NotNull] SBP.FileVariableSimpleContext context)
        {
            m_elementStartLine = context.Start.Line;
        }

        public override void EnterFileVariableWithPropertyBlock([NotNull] SBP.FileVariableWithPropertyBlockContext context)
        {
            m_elementStartLine = context.Start.Line;
        }

        public override void ExitFileVariableSimple([NotNull] SBP.FileVariableSimpleContext context)
        {
            var type = this.PopType("ExitFileVariableSimple");

            var element = new FileElement(this.TopElement, m_elementStartLine, ScriptData.FileElementType.FileVariable, m_name);
            element.ReturnTypeData = type;
            element.Modifiers = m_modifiers;
            this.TopElement.Childs.Add(element);
        }

        public override void ExitFileVariableWithPropertyBlock([NotNull] SBP.FileVariableWithPropertyBlockContext context)
        {
            var type = this.PopType("ExitFileVariableWithPropertyBlock");
            var name = m_name;

            var element = new FileElement(this.TopElement, m_elementStartLine, ScriptData.FileElementType.FileVariable, m_name);
            element.ReturnTypeData = type;
            element.Modifiers = m_modifiers;
            this.TopElement.Childs.Add(element);
        }

        public override void EnterVariableDeclaratorId([NotNull] SBP.VariableDeclaratorIdContext context)
        {
            m_name = context.GetText();
        }

        public override void EnterVariableModifier([NotNull] SBP.VariableModifierContext context)
        {
            if (m_modifiers == null) m_modifiers = new List<string>();
            m_modifiers.Add(context.GetText());
        }

        public override void ExitTypeVoid([NotNull] SBP.TypeVoidContext context)
        {
            this.PushType("ExitTypeVoid", "void", context.Start);
        }

        public override void ExitTypePrimitive([NotNull] SBP.TypePrimitiveContext context)
        {
            this.PushType("ExitTypePrimitive", context.GetText(), context.Start);
        }

        public override void ExitTypeProcedure([NotNull] SBP.TypeProcedureContext context)
        {
            this.PushType("ExitTypeProcedure", "procedure", context.Start);
        }

        public override void ExitTypeFunction([NotNull] SBP.TypeFunctionContext context)
        {
            this.PushType("ExitTypeProcedure", "function", context.Start);
        }

        public override void ExitTypeClassOrInterface([NotNull] SBP.TypeClassOrInterfaceContext context)
        {
            this.PushType("ExitTypeClassOrInterface", context.GetText(), context.Start);
        }


#if (PRINT_TREE)
        private string m_indent = "";

        public override void EnterEveryRule([NotNull] ParserRuleContext context)
        {
            System.Diagnostics.Debug.WriteLine(m_indent + "Enter" + this.ContextName(context) + ":             " + this.ShortContextText(context));
            m_indent += "    ";
        }

        public override void ExitEveryRule([NotNull] ParserRuleContext context)
        {
            m_indent = m_indent.Substring(0, m_indent.Length - 4);
            //Console.WriteLine(m_indent + "EXIT  - " + this.ContextName(context) + ":             " + this.ShortContextText(context));
        }

        private string ShortContextText(ParserRuleContext context)
        {
            string text = context.GetText();
            if (text.Length > 80)
            {
                text = text.Substring(0, 75) + " ... " + text.Substring(text.Length - 10);
            }
            return text;
        }

        private string ContextName(ParserRuleContext context)
        {
            string name = context.GetType().Name;
            return name.Substring(0, name.Length - "Context".Length);
        }
#endif

        public override void VisitErrorNode([NotNull] IErrorNode node)
        {
#if (PRINT_TREE)
            System.Diagnostics.Debug.WriteLine(m_indent + "TypeScan ERROR - " + node.GetText());
#endif
            //var t = node.Payload as CommonToken;
            //if (t != null)
            //{
            //    m_errors.SyntaxError(null, null, t.Type, t.Line, t.Column, node.GetText(), null);
            //}
            //else
            //{
            //    m_errors.SyntaxError(null, null, -1, -1, -1, node.GetText(), null);
            //}
            base.VisitErrorNode(node);
        }
    }
}
