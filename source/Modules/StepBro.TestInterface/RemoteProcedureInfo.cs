using System;
using System.Collections.Generic;

namespace StepBro.TestInterface
{
    public class RemoteProcedureInfo
    {
        private ProcedureParameterInfo[] m_parameters = null;
        private bool m_locked = false;

        public RemoteProcedureInfo(string name, int id = -1, string description = null, Type returnType = null, string lists = null, string error = null)
        {
            this.Name = name;
            this.ID = id;
            this.Description = description;
            this.Lists = lists;
            this.Error = error;
            if (!String.IsNullOrEmpty(lists) && !lists.Equals("null", StringComparison.InvariantCultureIgnoreCase))
            {
                this.ReturnType = typeof(List<string>);
            }
            else
            {
                this.ReturnType = returnType ?? typeof(string);
            }
        }

        public void AddParameter(string name, string description, Type type, bool hasDefaultValue = false, bool isParamsArray = false)
        {
            if (m_locked) throw new InvalidOperationException("The parameter list cannot be changed.");
            List<ProcedureParameterInfo> parameters;
            if (m_parameters == null) parameters = new List<ProcedureParameterInfo>();
            else parameters = new List<ProcedureParameterInfo>(m_parameters);
            parameters.Add(new ProcedureParameterInfo(name, description, type, hasDefaultValue, isParamsArray));
            m_parameters = parameters.ToArray();
        }

        public string Name { get; private set; }
        public int ID { get; private set; }
        public string Description { get; private set; }
        public Type ReturnType { get; private set; }
        public string Lists { get; private set; }
        public string Error { get; private set; }

        public IEnumerable<ProcedureParameterInfo> Parameters
        {
            get
            {
                m_locked = true;
                // Note: Implemented this way to make it readonly.
                if (m_parameters == null) yield break;
                else
                {
                    foreach (var p in m_parameters)
                    {
                        yield return p;
                    }
                }
            }
        }
    }
}
