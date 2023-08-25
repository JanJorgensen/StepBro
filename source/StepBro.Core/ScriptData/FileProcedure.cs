using StepBro.Core.CodeGeneration;
using StepBro.Core.Data;
using StepBro.Core.Execution;
using StepBro.Core.Logging;
using StepBro.Core.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace StepBro.Core.ScriptData
{
    internal class FileProcedure : FileElement, IFileProcedure, IIdentifierInfo
    {
        private Type m_delegateType = null;
        private IProcedureReference m_reference = null;
        private Type m_referenceType = null;
        private TypeReference m_dataType = null;
        private Tuple<string, Antlr4.Runtime.IToken> m_returnTypeData = null;
        private TypeReference m_returnType = null;
        private List<IdentifierInfo> m_parametersInternal = new List<IdentifierInfo>();
        private List<ParameterData> m_formalParameters = new List<ParameterData>();
        private readonly ParameterExpression m_callContextParameter;
        private LabelTarget m_returnLabel;
        private Expression m_bodyCode = null;
        private readonly List<int> m_breakpoints = null;
        private int m_nextTestStepIndex = 1;
        private Delegate m_runtimeProcedure = null;

        public FileProcedure(
            IScriptFile file,
            AccessModifier access,
            int line,
            IFileElement parentElement,
            string @namespace,
            string name,
            ContextLogOption logOption = ContextLogOption.Normal,
            bool separateStateLevel = true) :
                base(file, line, parentElement, @namespace, name, access, FileElementType.ProcedureDeclaration)
        {
            m_callContextParameter = Expression.Parameter(typeof(ICallContext), "callcontext");
            //var delegatetype = (m_runtimeProcedure != null) ? m_runtimeProcedure.GetType() : typeof(UnresolvedProcedureType);
            //m_parametersInternal.Add(new IdentifierInfo("callcontext", "callcontext", IdentifierType.Parameter, delegatetype, m_callContextParameter));
            m_returnLabel = Expression.Label();
            this.LogOption = logOption;
            this.Flags = 
                (this.Flags & ProcedureFlags.SeparateStateLevel) |
                (separateStateLevel ? ProcedureFlags.SeparateStateLevel : ProcedureFlags.None);
        }

        internal static IProcedureReference<T> Create<T>(IScriptFile file, string @namespace, string name, ContextLogOption logOption, T runtime) where T : class
        {
            if (file == null) throw new ArgumentNullException("file");
            if (runtime == null) throw new ArgumentNullException("runtime");
            var fp = new FileProcedure(file, AccessModifier.Public, -1, null, @namespace, name, logOption, true);
            fp.SetRuntimeProcedure(runtime as Delegate);
            var referenceType = typeof(Reference<>).MakeGenericType(typeof(T));
            fp.m_reference = (IProcedureReference)Activator.CreateInstance(referenceType, fp);
            return fp.m_reference as IProcedureReference<T>;
        }

        internal void SetRuntimeProcedure(Delegate @delegate)
        {
            if (m_runtimeProcedure != null) throw new InvalidOperationException("The runtime procedure has already been set.");
            m_runtimeProcedure = @delegate;
        }

        public Delegate RuntimeProcedure
        {
            get
            {
                return m_runtimeProcedure;
            }
        }

        private class ReferenceBase : IProcedureReference
        {
            protected FileProcedure m_reference;

            public ReferenceBase(FileProcedure reference)
            {
                m_reference = reference;
            }

            public IProcedureReference BaseProcedure
            {
                get
                {
                    if (m_reference.BaseElement != null)
                    {
                        return (m_reference.BaseElement as FileProcedure).ProcedureReference;
                    }
                    return null;
                }
            }
            public IProcedureReference ParentProcedure
            {
                get
                {
                    if (m_reference.ParentElement != null)
                    {
                        return (m_reference.ParentElement as FileProcedure).ProcedureReference;
                    }
                    return null;
                }
            }

            public IFileProcedure ProcedureData
            {
                get
                {
                    return m_reference;
                }
            }

            public string Name { get { return m_reference.Name; } }

            public string FullName { get { return m_reference.FullName; } }

            IInheritable IInheritable.Base { get { return this.BaseProcedure; } }
        }
        private class Reference<T> : ReferenceBase, IProcedureReference<T> where T : class
        {
            public Reference(FileProcedure reference) : base(reference)
            {
                if (typeof(T).IsGenericTypeDefinition || !typeof(T).IsDelegate())
                {
                    throw new Exception("");
                }
            }

            public T RuntimeProcedure
            {
                get
                {
                    return m_reference.RuntimeProcedure as T;
                }
            }
        }

        public Type DelegateType
        {
            get
            {
                return m_delegateType;
            }
        }
        public Type ProcedureReferenceType
        {
            get
            {
                return m_referenceType;
            }
        }
        public IProcedureReference ProcedureReference
        {
            get
            {
                return m_reference;
            }
        }

        public ProcedureFlags Flags { get; internal set; }

        public ContextLogOption LogOption { get; set; }

        public bool HasBody { get; internal set; } = false;

        public bool DictatesParameters { get { return this.Flags.HasFlag(ProcedureFlags.FreeParameters) == false; } }

        public TypeReference ReturnType
        {
            get
            {
                return m_returnType;
            }
            set
            {
                m_returnType = value;
                if (value.Type != typeof(void))
                {
                    m_returnLabel = Expression.Label(m_returnType.Type);
                }
            }
        }

        public LabelTarget ReturnLabel { get { return m_returnLabel; } }

        internal static object DefaultValue(Type type)
        {
            if (type == typeof(bool)) return default(bool);
            if (type == typeof(long)) return default(long);
            if (type == typeof(double)) return default(double);
            if (type == typeof(string)) return default(string);
            if (type == typeof(bool)) return default(bool);
            if (type == typeof(TimeSpan)) return default(TimeSpan);     // For speed ...
            if (type == typeof(DateTime)) return default(DateTime);     // For speed ...
            if (type == typeof(Verdict)) return default(Verdict);       // For speed ...
            if (type.IsEnum || type.IsValueType) return Activator.CreateInstance(type);
            return null;
        }

        public IEnumerable<IdentifierInfo> Parameters()
        {
            return m_parametersInternal;
        }

        public List<ParameterData> GetFormalParameters()
        {
            return new List<ParameterData>(m_formalParameters);
        }

        public ParameterExpression ContextReference
        {
            get
            {
                return m_callContextParameter;
            }
        }

        public Expression ContextReferenceInternal
        {
            get
            {
                return Expression.Convert(m_callContextParameter, typeof(Execution.IScriptCallContext));
            }
        }

        public bool CheckForPrototypeChange(IList<ParameterData> parameters, Tuple<string, Antlr4.Runtime.IToken> returnType)
        {
            var changed = false;
            if (m_returnTypeData == null || !String.Equals(m_returnTypeData.Item1, returnType.Item1, StringComparison.InvariantCulture))
            {
                changed = true;
            }
            else
            {
                if (parameters.Count != m_formalParameters.Count)
                {
                    changed = true;
                }
                else
                {
                    for (int i = 0; i < parameters.Count; i++)
                    {
                        if (!String.Equals(m_formalParameters[i].Name, parameters[i].Name, StringComparison.InvariantCulture) ||
                            !String.Equals(m_formalParameters[i].TypeName, parameters[i].TypeName, StringComparison.InvariantCulture))
                        {
                            changed = true;
                            break;
                        }

                    }
                }
            }
            if (changed)
            {
                m_returnTypeData = returnType;
                m_formalParameters.Clear();
                foreach (var p in parameters)
                {
                    m_formalParameters.Add(p);
                }
                m_returnType = null;
                m_parametersInternal = null;
                m_delegateType = null;
                m_dataType = null;
            }
            return changed;
        }

        public void AddParameter(ParameterData parameter)
        {
            m_formalParameters.Add(parameter);
        }

        internal override int ParseSignature(StepBroListener listener, bool reportErrors)
        {
            int unresolvedTypes = 0;

            if (m_returnType == null)
            {
                m_returnType = listener.ParseTypeString(m_returnTypeData.Item1, reportErrors: reportErrors, token: m_returnTypeData.Item2);
                if (m_returnType == null) unresolvedTypes++;
            }

            foreach (var p in m_formalParameters)
            {
                if (p.Type == null)
                {
                    var type = listener.ParseTypeString(p.TypeName, reportErrors: reportErrors, token: p.TypeToken);
                    if (type == null)
                    {
                        unresolvedTypes++;
                        continue;
                    }
                    else
                    {
                        p.SetType(type);
                    }
                }
            }

            if (unresolvedTypes == 0)
            {
                this.CreateDelegateType();
            }

            return unresolvedTypes;
        }

        public bool IsFirstParameterThisReference
        {
            get
            {
                return (m_formalParameters != null &&
                    m_formalParameters.Count > 0 &&
                    m_formalParameters[0].Modifiers != null &&
                    m_formalParameters[0].Modifiers.Length > 0 &&
                    m_formalParameters[0].Modifiers[0].Equals("this", StringComparison.InvariantCulture));
            }
        }

        internal void CreateDelegateType()
        {
            if (this.HasBody || m_formalParameters.Count > 0 || this.ReturnType.Equals(TypeReference.TypeVoid))
            {
                m_delegateType = ProcedureDelegateManager.CreateOrGetDelegateType(
                    this.ReturnType,
                    this.CreateFormalParametersData());
            }
            else
            {
                m_delegateType = null;
            }
            m_parametersInternal = new List<IdentifierInfo>();
            if (m_delegateType != null)
            {
                m_parametersInternal.Add(new IdentifierInfo("callcontext", "callcontext", IdentifierType.Parameter, (TypeReference)m_delegateType, m_callContextParameter));
                foreach (var p in m_formalParameters)
                {
                    var runtimePar = Expression.Parameter(p.Type.Type, p.Name);
                    m_parametersInternal.Add(new IdentifierInfo(p.Name, p.Name, IdentifierType.Parameter, p.Type, runtimePar));
                }

                m_referenceType = typeof(IProcedureReference<>).MakeGenericType(m_delegateType);
                var refObjectType = typeof(Reference<>).MakeGenericType(m_delegateType);
                m_reference = (IProcedureReference)Activator.CreateInstance(refObjectType, this);
                m_dataType = new TypeReference(m_referenceType, this);
            }
            else
            {
                m_referenceType = null;
                m_reference = new ReferenceBase(this);
                m_dataType = new TypeReference(typeof(IProcedureReference), this);
            }
        }

        protected override bool ParsePropertyBlockFlag(string name)
        {
            if (name.Equals("FreeParameters"))
            {
                this.Flags |= ProcedureFlags.FreeParameters;
                return true;
            }
            if (name.Equals("ContinueOnFail"))  // Default is 'SkipRestOnFail' (exit procedure)
            {
                this.Flags |= ProcedureFlags.ContinueOnFail;
                return true;
            }
            if (name.Equals("NoSubResultInheritance"))
            {
                this.Flags |= ProcedureFlags.NoSubResultInheritance;
                return true;
            }
            return base.ParsePropertyBlockFlag(name);
        }

        internal void SetProcedureBody(Expression body)
        {
            m_bodyCode = body;
        }

        public IEnumerable<int> ListBreakpoints()
        {
            if (m_breakpoints == null) yield break;
            else
            {
                foreach (var bp in m_breakpoints) yield return bp;
            }
        }

        public bool IsBreakpointOnLine(int line)
        {
            return (m_breakpoints != null && m_breakpoints.Contains(line));
        }

        public int GetNextStepIndex()
        {
            return m_nextTestStepIndex++;
        }

        public void SetStepIndex(int index)
        {
            m_nextTestStepIndex = index + 1;
        }
        //IEnumerable<IdentifierInfo> IParsingContext.KnownIdentifiers()
        //{
        //    IParsingContext parent = (this.ParentFile != null && this.ParentFile is IParsingContext) ? this.ParentFile as IParsingContext : null;
        //    if (parent != null)
        //    {
        //        foreach (var identifier in parent.KnownIdentifiers()) { yield return identifier; }
        //    }
        //}

        #region IIdentifierInfo

        string IIdentifierInfo.Name
        {
            get
            {
                return this.Name;
            }
        }

        IdentifierType IIdentifierInfo.Type
        {
            get
            {
                return IdentifierType.FileElement;
            }
        }

        object IIdentifierInfo.Reference
        {
            get
            {
                return this.ProcedureReference;
            }
        }

        NamedData<TypeReference>[] IFileProcedure.Parameters
        {
            get
            {
                return this.CreateFormalParametersData();
            }
        }

        #endregion

        private NamedData<TypeReference>[] CreateFormalParametersData()
        {
            return m_formalParameters.Select(t => new NamedData<TypeReference>(t.Name, t.Type)).ToArray();
        }

        internal void Compile()
        {
            if (m_delegateType != null)
            {
                try
                {
                    List<Expression> bodyCode = new List<Expression>();
                    if (m_bodyCode != null) bodyCode.Add(m_bodyCode);
                    if (this.ReturnType.Type == typeof(void))
                    {
                        bodyCode.Add(Expression.Label(this.ReturnLabel));
                    }
                    else
                    {
                        if (this.ReturnType.Type == typeof(string))
                        {
                            bodyCode.Add(Expression.Label(this.ReturnLabel, Expression.Constant(null, typeof(string))));
                        }
                        else
                        {
                            var ret = Expression.Constant(DefaultValue(this.ReturnType.Type), this.ReturnType.Type);
                            bodyCode.Add(Expression.Label(this.ReturnLabel, ret));
                        }
                    }

                    // TODO: add context variable and logging

                    Expression body = Expression.Block(bodyCode);
                    //System.Diagnostics.Trace.WriteLine(body.GetDebugView());
                    var lambdaExpr = Expression.Lambda(m_delegateType, body, m_parametersInternal.Select(p => (ParameterExpression)p.Reference));
                    m_runtimeProcedure = lambdaExpr.Compile();
                }
                catch
                {
                    m_runtimeProcedure = null;
                    throw;
                }
            }
            else
            {
                m_runtimeProcedure = null;
            }
        }

        protected override TypeReference GetDataType()
        {
            return m_dataType;
        }
    }
}
