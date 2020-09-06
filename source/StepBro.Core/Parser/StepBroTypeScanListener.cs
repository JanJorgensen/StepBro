using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using StepBro.Core.Data;
//using StepBro.Core.ScriptData;
using TSP = StepBro.Core.Parser.TSharp;

namespace StepBro.Core.Parser
{
    internal class StepBroTypeScanListener : TSharpBaseListener
    {
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
            public string ReturnType { get { return this.ReturnTypeData.Item1; } }
            public List<ParameterData> Parameters { get; set; } = null;
            public List<NamedString> Values { get; set; } = null;   // For enum values.
            public List<FileElement> Childs { get; set; } = new List<FileElement>();
            public bool IsFunction { get; set; } = false;
            public bool HasBody { get; set; } = true;
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
            private readonly List<Tuple<string, string>> m_usings = new List<Tuple<string, string>>();
            public FileContent(List<Tuple<string, string>> usings, FileElement topElement)
            {
                m_usings = usings;
                this.TopElement = topElement;
            }
            public FileElement TopElement { get; private set; }

            public IEnumerable<Tuple<string, string>> ListUsings()
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

        private List<Tuple<string, string>> m_usings = new List<Tuple<string, string>>();
        private FileElement m_firstElement = null;
        private bool m_namespaceSet = false;
        private int m_elementStartLine = -1;
        private Stack<FileElement> m_elementStack = new Stack<FileElement>();
        private string m_name = null;
        private string m_type = null;
        private Tuple<string, IToken> m_returnType = null;
        private List<ParameterData> m_procedureParameters = null;
        private List<string> m_modifiers = null;
        private string m_varName = null;
        private Stack<Tuple<string, IToken>> m_typeStack = new Stack<Tuple<string, IToken>>();
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

        public IEnumerable<Tuple<string, string>> ListUsings() { return m_usings; }

        private void PushType(string location, string type, IToken token)
        {
            m_typeStack.Push(new Tuple<string, IToken>(type, token));
            System.Diagnostics.Debug.WriteLine($"   Push type: {type} @ {location}");
        }

        private Tuple<string, IToken> PopType(string location)
        {
            var type = m_typeStack.Pop();
            System.Diagnostics.Debug.WriteLine($"   Pop type: {type} @ {location}");
            return type;
        }

        public override void ExitUsingDeclarationWithIdentifier([NotNull] TSP.UsingDeclarationWithIdentifierContext context)
        {
            if (context.ChildCount == 3)
            {
                m_usings.Add(new Tuple<string, string>("i", context.GetChild(1).GetText()));
            }
            else if (context.ChildCount == 4)
            {
                m_usings.Add(new Tuple<string, string>("I", context.GetChild(2).GetText()));
            }
        }

        public override void ExitUsingDeclarationWithPath([NotNull] TSP.UsingDeclarationWithPathContext context)
        {
            if (context.ChildCount == 3)
            {
                m_usings.Add(new Tuple<string, string>("p", StepBroListener.ParseStringLiteral(context.GetChild(1).GetText(), context)));
            }
            else if (context.ChildCount == 4)
            {
                m_usings.Add(new Tuple<string, string>("P", StepBroListener.ParseStringLiteral(context.GetChild(2).GetText(), context)));
            }
        }

        public override void ExitNamespace([NotNull] TSP.NamespaceContext context)
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

        public override void EnterFileElement([NotNull] TSP.FileElementContext context)
        {
            m_modifiers = null;
        }

        public override void ExitFileElement([NotNull] TSP.FileElementContext context)
        {
        }

        public override void EnterElementModifier([NotNull] TSP.ElementModifierContext context)
        {
            if (m_modifiers == null) m_modifiers = new List<string>();
            m_modifiers.Add(context.GetText());
        }

        public override void EnterFileElementProcedure([NotNull] TSP.FileElementProcedureContext context)
        {
            m_isFunction = false;
            m_elementStartLine = context.Start.Line;
        }
        public override void EnterFileElementFunction([NotNull] TSP.FileElementFunctionContext context)
        {
            m_isFunction = true;
            m_elementStartLine = context.Start.Line;
        }

        public override void EnterProcedureParameters([NotNull] TSP.ProcedureParametersContext context)
        {
            m_procedureParameters = new List<ParameterData>();    // New list; it will be handed to the file element object.
        }

        public override void ExitFormalParameterModifiers([NotNull] TSP.FormalParameterModifiersContext context)
        {
            var m = context.GetText();
            m_parameterModifiers = m.Split(' ');
        }

        public override void ExitFormalParameterDecl([NotNull] TSP.FormalParameterDeclContext context)
        {
            var type = this.PopType("ExitFormalParameterDecl");
            var name = context.GetChild(context.children.Count - 1).GetText();
            m_procedureParameters.Add(new ParameterData(m_parameterModifiers, name, type.Item1, typeToken: type.Item2));
            m_parameterModifiers = null;
        }

        public override void ExitProcedureParameters([NotNull] TSP.ProcedureParametersContext context)
        {
            base.ExitProcedureParameters(context);
        }
        public override void ExitProcedureReturnType([NotNull] TSP.ProcedureReturnTypeContext context)
        {
            m_returnType = this.PopType("ExitProcedureReturnType");
        }

        public override void ExitProcedureName([NotNull] TSP.ProcedureNameContext context)
        {
            m_name = context.GetText();
        }

        public override void ExitVariableType([NotNull] TSP.VariableTypeContext context)
        {
            m_type = context.GetText();
        }

        public override void EnterProcedureBodyOrNothing([NotNull] TSP.ProcedureBodyOrNothingContext context)
        {
            var name = m_name;
            var parameters = m_procedureParameters;

            var element = new FileElement(this.TopElement, m_elementStartLine, ScriptData.FileElementType.ProcedureDeclaration, name);
            element.Modifiers = m_modifiers;
            element.Parameters = parameters;
            element.ReturnTypeData = m_returnType;
            element.IsFunction = m_isFunction;
            element.HasBody = context.Start.Type != TSP.SEMICOLON;
            this.TopElement.Childs.Add(element);
        }

        public override void ExitTestListName([NotNull] TSP.TestListNameContext context)
        {
            m_name = context.GetText();
        }

        public override void EnterTestlist([NotNull] TSP.TestlistContext context)
        {
            m_elementStartLine = context.Start.Line;
        }

        public override void ExitTestlist([NotNull] TSP.TestlistContext context)
        {
            var name = m_name;
            //var parameters = m_procedureParameters;

            var element = new FileElement(this.TopElement, m_elementStartLine, ScriptData.FileElementType.TestList, name);
            element.Modifiers = m_modifiers;
            //element.Parameters = parameters;
            //element.HasBody = context.Start.Type != TSP.SEMICOLON;
            this.TopElement.Childs.Add(element);
        }

        public override void EnterFileVariable([NotNull] TSP.FileVariableContext context)
        {
            m_elementStartLine = context.Start.Line;
        }

        public override void ExitFileVariableSimple([NotNull] TSP.FileVariableSimpleContext context)
        {
            var type = this.PopType("ExitFileVariableSimple");
            var name = m_varName;

            var element = new FileElement(this.TopElement, m_elementStartLine, ScriptData.FileElementType.FileVariable, m_varName);
            element.ReturnTypeData = type;
            element.Modifiers = m_modifiers;
            this.TopElement.Childs.Add(element);
        }

        public override void ExitFileVariableWithPropertyBlock([NotNull] TSP.FileVariableWithPropertyBlockContext context)
        {
            var type = this.PopType("ExitFileVariableWithPropertyBlock");
            var name = m_varName;

            var element = new FileElement(this.TopElement, m_elementStartLine, ScriptData.FileElementType.FileVariable, m_varName);
            element.ReturnTypeData = type;
            element.Modifiers = m_modifiers;
            this.TopElement.Childs.Add(element);
        }

        public override void EnterVariableDeclaratorId([NotNull] TSP.VariableDeclaratorIdContext context)
        {
            m_varName = context.GetText();
        }

        public override void EnterVariableModifier([NotNull] TSP.VariableModifierContext context)
        {
            if (m_modifiers == null) m_modifiers = new List<string>();
            m_modifiers.Add(context.GetText());
        }

        public override void ExitTypeVoid([NotNull] TSP.TypeVoidContext context)
        {
            this.PushType("ExitTypeVoid", "void", context.Start);
        }

        public override void ExitTypePrimitive([NotNull] TSP.TypePrimitiveContext context)
        {
            this.PushType("ExitTypePrimitive", context.GetText(), context.Start);
        }

        public override void ExitTypeProcedure([NotNull] TSP.TypeProcedureContext context)
        {
            this.PushType("ExitTypeProcedure", "procedure", context.Start);
        }

        public override void ExitTypeFunction([NotNull] TSP.TypeFunctionContext context)
        {
            this.PushType("ExitTypeProcedure", "function", context.Start);
        }

        public override void ExitTypeClassOrInterface([NotNull] TSP.TypeClassOrInterfaceContext context)
        {
            this.PushType("ExitTypeClassOrInterface", context.GetText(), context.Start);
        }

        public override void EnterEveryRule([NotNull] ParserRuleContext context)
        {
            base.EnterEveryRule(context);
            System.Diagnostics.Debug.WriteLine("TypeScan Enter " + context.GetType().Name);
        }

        public override void ExitEveryRule([NotNull] ParserRuleContext context)
        {
            System.Diagnostics.Debug.WriteLine("TypeScan Exit " + context.GetType().Name);
            base.ExitEveryRule(context);
        }

        public override void VisitErrorNode([NotNull] IErrorNode node)
        {
            System.Diagnostics.Debug.WriteLine("TypeScan ERROR " + node.GetText());
            base.VisitErrorNode(node);
        }
    }
}
