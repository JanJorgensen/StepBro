using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Execution;
using StepBro.Core.General;
using System;

namespace StepBroCoreTest.Data
{
    public class DummyDynamicObject : StepBro.Core.Api.DynamicStepBroObject
    {
        private bool m_initialized = false;
        private string m_lastValue = "";

        public string LastValue { get { return m_lastValue; } }

        public override DynamicSupport HasProperty(string name, out Type type, out bool isReadOnly)
        {
            if (m_initialized)
            {
                if (name == "Anna")
                {
                    type = typeof(long);
                    isReadOnly = false;
                    return DynamicSupport.Yes;
                }
                else if (name == "Berit")
                {
                    type = typeof(bool);
                    isReadOnly = false;
                    return DynamicSupport.Yes;
                }

                type = null;
                isReadOnly = false;
                return DynamicSupport.No;
            }
            else
            {
                type = null;
                isReadOnly = false;
                return DynamicSupport.KnownAtRuntimeOnly;
            }
        }

        public static DummyDynamicObject New()
        {
            return new DummyDynamicObject();
        }
        public static DummyDynamicObject NewInitialized()
        {
            var obj = new DummyDynamicObject();
            obj.Initialize();
            return obj;
        }

        public void Initialize()
        {
            m_initialized = true;
        }

        public override object GetProperty([Implicit] ICallContext context, string name)
        {
            var nameparts = name.Split('.');
            if (nameparts.Length == 1)
            {
                if (name == "Anna")
                {
                    return 9927L;
                }
                else if (name == "Berit")
                {
                    return true;
                }
                else
                {
                    throw new DynamicPropertyNotFoundException(name);
                }
            }
            else
            {
                if (name == "Aee.Bee.Cee.Dee.Anna")
                {
                    return 663L;
                }
                else
                {
                    throw new DynamicPropertyNotFoundException(name);
                }
            }
        }

        public override void SetProperty([Implicit] ICallContext context, string name, object value)
        {
            var nameparts = name.Split('.');
            if (nameparts.Length == 1)
            {
                if (name == "Anna")
                {
                    var v = (long)value;
                    m_lastValue = v.ToString();
                }
                else if (name == "Berit")
                {
                    var v = (bool)value;
                    m_lastValue = v.ToString();
                }
                else
                {
                    throw new DynamicPropertyNotFoundException(name);
                }
            }
            else
            {
                if (name == "Aee.Bee.Cee.Dee.Anna")
                {
                    var v = (long)value;
                    m_lastValue = v.ToString();
                }
                else
                {
                    throw new DynamicPropertyNotFoundException(name);
                }
            }
        }

        public override DynamicSupport HasMethod(string name, out NamedData<Type>[] parameters, out Type returnType)
        {
            parameters = null;
            returnType = null;
            if (m_initialized)
            {
                if (name == "Anderson")
                {
                    parameters = new NamedData<Type>[]
                    {
                        new NamedData<Type>("a", typeof(long)),
                        new NamedData<Type>("b", typeof(string)),
                        new NamedData<Type>("c", typeof(TimeSpan))
                    };
                    returnType = typeof(long);
                    return DynamicSupport.Yes;
                }
                else if (name == "Bengtson")
                {
                    parameters = new NamedData<Type>[]
                    {
                        new NamedData<Type>("g", typeof(Identifier)),
                        new NamedData<Type>("h", typeof(long))
                    };
                    returnType = typeof(bool);
                    return DynamicSupport.Yes;
                }
                else if (name == "Christianson")
                {
                    parameters = new NamedData<Type>[] { };
                    returnType = typeof(Identifier);
                    return DynamicSupport.Yes;
                }
                else if (name == "Dengson")
                {
                    parameters = new NamedData<Type>[]
                    {
                        new NamedData<Type>("n", typeof(string)),
                        new NamedData<Type>("q", typeof(long))
                    };
                    returnType = typeof(string[]);
                    return DynamicSupport.Yes;
                }
                else
                {
                    return DynamicSupport.No;
                }
            }
            else
            {
                return DynamicSupport.KnownAtRuntimeOnly;
            }
        }

        public override object InvokeMethod([Implicit] ICallContext context, string name, object[] args)
        {
            if (!m_initialized) throw new InvalidOperationException("Object has really not been initialized yet !!");
            if (name == "Anderson")
            {
                if (args.Length != 3) throw new ArgumentException("Wrong number of arguments.");
                if (args[0].GetType() != typeof(long)) throw new ArgumentException("Wrong type", "a");
                if (args[1].GetType() != typeof(string)) throw new ArgumentException("Wrong type", "b");
                if (args[2].GetType() != typeof(TimeSpan)) throw new ArgumentException("Wrong type", "c");
                return 726L;
            }
            else if (name == "Bengtson")
            {
                if (args.Length != 2) throw new ArgumentException("Wrong number of arguments.");
                if (args[0].GetType() != typeof(Identifier)) throw new ArgumentException("Wrong type", "g");
                if (args[1].GetType() != typeof(long)) throw new ArgumentException("Wrong type", "h");
                return true;
            }
            else if (name == "Christianson")
            {
                if (args.Length != 0) throw new ArgumentException("Wrong number of arguments.");
                return (Identifier)"NoFlight";
            }
            else if (name == "Dengson")
            {
                if (args.Length != 2) throw new ArgumentException("Wrong number of arguments.");
                if (args[0].GetType() != typeof(string)) throw new ArgumentException("Wrong type", "n");
                if (args[1].GetType() != typeof(long)) throw new ArgumentException("Wrong type", "q");
                return new string[] { "Luffe", "Sjanne", "Clarke", "Louis" };
            }
            else
            {
                throw new DynamicMethodNotFoundException(name);
            }
        }
    }
}
