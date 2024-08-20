using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Execution;
using StepBro.Core.Logging;
using StepBro.Core.ScriptData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using static StepBro.Core.Data.PropertyBlockDecoder;

namespace StepBro.StateMachine
{
    public class StateMachineDefinition : INameable, INamedObject, ISettableFromPropertyBlock
    {
        private string m_name = "StateMachine";

        private PropertyBlockDecoder.Block<object, StateMachineDefinition> m_decoder = null;
        private List<NamedData<Tuple<Type, object>>> m_variables = new List<NamedData<Tuple<Type, object>>>();
        private List<NamedData<IProcedureReference>> m_states = new List<NamedData<IProcedureReference>>();

        public StateMachineDefinition([ObjectName] string objectName = "StateMachine")
        {
            m_name = objectName;
        }

        [ObjectName]
        public string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                m_name = value;
            }
        }

        public string ShortName { get { return this.Name; } }

        public string FullName { get { return this.Name; } }

        public IEnumerable<NamedData<IProcedureReference>> ListStates()
        {
            foreach (var sd in m_states)
            {
                yield return sd;
            }
        }

        public IEnumerable<NamedData<object>> ListVariables()
        {
            foreach (var v in m_variables)
            {
                yield return new NamedData<object>(v.Name, v.Value.Item2);
            }
        }

        #region Parsing and Setup

        public void PreScanData(IScriptFile file, PropertyBlock data, List<Tuple<int, string>> errors)
        {
            var root = new PropertyBlockDecoder.Block<object, object>(
                new PropertyBlockDecoder.Array<object>("states"),
                new PropertyBlockDecoder.Value<object>());
            root.DecodeData(data, null, errors);
        }

        public void Setup(IScriptFile file, ILogger logger, PropertyBlock data)
        {
            m_states = new List<NamedData<IProcedureReference>>();
            m_variables = new List<NamedData<Tuple<Type, object>>>();

            m_decoder = new Block<object, StateMachineDefinition>
                (
                    new PropertyBlockDecoder.Array<object>("states", (t, a) =>
                    {
                        if (m_states.Count > 0)
                        {
                            return "States are already defined.";
                        }
                        else
                        {
                            foreach (var v in a)
                            {
                                if (!(v is Identifier))
                                {
                                    return "All states must be a simple identifier.";
                                }
                                m_states.Add(new NamedData<IProcedureReference>(((Identifier)v).Name, null));
                            }
                            return null;
                        }
                    }),
                    new Value<StateMachineDefinition>((t, v) =>
                    {
                        if (!v.HasTypeSpecified)
                        {
                            return "Variable type is not specified.";
                        }
                        if (v.SpecifiedTypeName == "var")
                        {
                            if (v.Value == null)
                            {
                                return "A variable of type 'var' cannot be set to 'null'.";
                            }
                            else
                            {
                                var type = v.Value.GetType();
                                if (type == typeof(long) ||
                                    type == typeof(bool) ||
                                    type == typeof(string) ||
                                    type == typeof(TimeSpan) ||
                                    type == typeof(DateTime))
                                {
                                    m_variables.Add(new NamedData<Tuple<Type, object>>(v.Name, new Tuple<Type, object>(type, v.Value)));
                                }
                                else
                                {
                                    return "Unknown or unsupported variable type '" + type.Name + "'.";
                                }
                            }
                        }
                        else if (v.SpecifiedTypeName == "int")
                        {
                            m_variables.Add(new NamedData<Tuple<Type, object>>(v.Name, new Tuple<Type, object>(typeof(long), v.Value)));
                        }
                        else if (v.SpecifiedTypeName == "bool")
                        {
                            m_variables.Add(new NamedData<Tuple<Type, object>>(v.Name, new Tuple<Type, object>(typeof(bool), v.Value)));
                        }
                        else if (v.SpecifiedTypeName == "string")
                        {
                            m_variables.Add(new NamedData<Tuple<Type, object>>(v.Name, new Tuple<Type, object>(typeof(string), v.Value)));
                        }
                        else if (v.SpecifiedTypeName == "datetime")
                        {
                            DateTime value = DateTime.MinValue;
                            if (v.Value.GetType() == typeof(Identifier) && ((Identifier)v.Value).Name.Equals("default"))
                            {
                                value = DateTime.MinValue;
                            }
                            else
                            {
                                return "Not implemented: value for datetime.";
                            }
                            m_variables.Add(new NamedData<Tuple<Type, object>>(v.Name, new Tuple<Type, object>(typeof(DateTime), value)));
                        }
                        else if (v.SpecifiedTypeName == "timespan")
                        {
                            TimeSpan value = TimeSpan.Zero;
                            if (v.Value.GetType() == typeof(TimeSpan))
                            {
                                value = (TimeSpan)v.Value;
                            }
                            else if (v.Value.GetType() == typeof(Identifier) && ((Identifier)v.Value).Name.Equals("default"))
                            {
                                value = TimeSpan.Zero;
                            }
                            else
                            {
                                return "Not implemented: value for timespan.";
                            }
                            m_variables.Add(new NamedData<Tuple<Type, object>>(v.Name, new Tuple<Type, object>(typeof(TimeSpan), value)));
                        }
                        else
                        {
                            return "Unknown or unsupported variable type '" + v.SpecifiedTypeName + "'.";
                        }
                        return null;    // No errors
                    })
                );

            var errors = new List<Tuple<int, string>>();
            m_decoder.DecodeData(data, this, errors);

            if (errors.Count > 0)
            {
                foreach (var error in errors)
                {
                    logger.LogError("Line " + error.Item1 + ": " + error.Item2);
                }
            }
            else
            {
                for (int i = 0; i < m_states.Count; i++)
                {
                    string name = this.Name + "_" + m_states[i].Name;
                    var proc = file.ListElements().FirstOrDefault(e => e.ElementType == FileElementType.ProcedureDeclaration && String.Equals(e.Name, name)) as IFileProcedure;
                    if (proc != null)
                    {
                        var parameters = proc.Parameters;
                        if (parameters.Length != 3 ||
                            parameters[0].Value.Type != typeof(IStateMachine) ||
                            parameters[1].Value.Type != typeof(Event) ||
                            parameters[2].Value.Type != typeof(EventData) ||
                            proc.ReturnType.Type != typeof(void))
                        {
                            logger.LogError("Procedure '" + name + "' does not have the correct parameters.");
                        }
                        else
                        {
                            m_states[i] = new NamedData<IProcedureReference>(m_states[i].Name, proc.ProcedureReference);
                        }
                    }
                    else
                    {
                        logger.LogError("No state procedure found. Looking for: void " + name + "(IStateMachine context, Event machineEvent, EventData data)");
                    }
                }
            }
        }

        #endregion
    }
}
