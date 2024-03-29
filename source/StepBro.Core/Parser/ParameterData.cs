﻿using Antlr4.Runtime;
using StepBro.Core.Data;
using System;

namespace StepBro.Core.Parser
{
    public sealed class ParameterData
    {
        private readonly string[] m_modifiers;
        private readonly string m_name;
        private readonly string m_typeName;
        private TypeReference m_type;
        private readonly IToken m_typeToken;
        private object m_defaultValue = null;
        private IToken m_defaultValueToken = null;

        public ParameterData(string[] modifiers, string name, string typeName, TypeReference type = null, IToken typeToken = null)
        {
            m_modifiers = modifiers ?? new string[] { };
            m_name = name;
            m_typeName = typeName;
            m_type = type;
            m_typeToken = typeToken;
        }

        internal void SetType(TypeReference type)
        {
            if (m_type != null) throw new InvalidOperationException("The type has already been set.");
            m_type = type;
        }

        internal void SetDefaultValue(object value, IToken token)
        {
            if (token == null) throw new ArgumentNullException("token");
            m_defaultValue = value;
            m_defaultValueToken = token;
        }

        public string[] Modifiers { get { return m_modifiers; } }
        public string Name { get { return m_name; } }
        public string TypeName { get { return m_typeName; } }
        public TypeReference Type { get { return m_type; } }
        public IToken TypeToken { get { return m_typeToken; } }
        public object DefaultValue { get { return m_defaultValue; } }
        public IToken DefaultValueToken { get { return m_defaultValueToken; } }
        public bool HasDefaultValue {  get { return m_defaultValueToken != null; } }
    }
}
