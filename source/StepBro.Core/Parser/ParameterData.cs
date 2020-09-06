﻿using Antlr4.Runtime;
using System;
using StepBro.Core.Data;

namespace StepBro.Core.Parser
{
    public sealed class ParameterData
    {
        private readonly string[] m_modifiers;
        private readonly string m_name;
        private readonly string m_typeName;
        private TypeReference m_type;
        private IToken m_typeToken;
        public ParameterData(string[] modifiers, string name, string typeName, TypeReference type = null, IToken typeToken = null)
        {
            m_modifiers = modifiers ?? new string[] { };
            m_name = name;
            m_typeName = typeName;
            m_type = type;
            m_typeToken = typeToken;
        }

        public void SetType(TypeReference type)
        {
            if (m_type != null) throw new InvalidOperationException("The type has already been set.");
            m_type = type;
        }

        public string[] Modifiers { get { return m_modifiers; } }
        public string Name { get { return m_name; } }
        public string TypeName { get { return m_typeName; } }
        public TypeReference Type { get { return m_type; } }
    }
}
