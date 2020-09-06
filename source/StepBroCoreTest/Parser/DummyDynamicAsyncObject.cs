using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.General;
using StepBro.Core.Tasks;
using System;

namespace StepBroCoreTest.Parser
{
    public class DummyDynamicAsyncObject : StepBro.Core.Api.DynamicAsyncStepBroObject
    {
        private bool m_initialized = false;

        public long InvokeCount { get; private set; } = 0L;

        public static DummyDynamicAsyncObject New()
        {
            return new DummyDynamicAsyncObject();
        }
        public static DummyDynamicAsyncObject NewInitialized()
        {
            var obj = new DummyDynamicAsyncObject();
            obj.Initialize();
            return obj;
        }

        public void Initialize()
        {
            m_initialized = true;
        }

        public override DynamicSupport HasProperty(string name, out Type type, out bool isReadOnly)
        {
            if (m_initialized)
            {
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
        public override IAsyncResult<object> TryGetProperty(string name)
        {
            return base.TryGetProperty(name);
        }
        public override IAsyncResult<bool> TrySetProperty(string name, object value)
        {
            return base.TrySetProperty(name, value);
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
                else if (name == "Esildro")
                {
                    parameters = new NamedData<Type>[] { };
                    returnType = typeof(void);
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

        public override IAsyncResult<object> TryInvokeMethod(string name, object[] args)
        {
            if (!m_initialized) throw new InvalidOperationException("Object has really not been initialized yet !!");
            if (name == "Anderson")
            {
                if (args.Length != 3) throw new ArgumentException("Wrong number of arguments.");
                if (args[0].GetType() != typeof(long)) throw new ArgumentException("Wrong type", "a");
                if (args[1].GetType() != typeof(string)) throw new ArgumentException("Wrong type", "b");
                if (args[2].GetType() != typeof(TimeSpan)) throw new ArgumentException("Wrong type", "c");

                this.InvokeCount++;
                return (TaskToAsyncResult<object>)System.Threading.Tasks.Task<object>.Run<object>(() =>
                {
                    System.Threading.Thread.Sleep(250);
                    return 726L;
                });
            }
            else if (name == "Bengtson")
            {
                if (args.Length != 2) throw new ArgumentException("Wrong number of arguments.");
                if (args[0].GetType() != typeof(Identifier)) throw new ArgumentException("Wrong type", "g");
                if (args[1].GetType() != typeof(long)) throw new ArgumentException("Wrong type", "h");

                this.InvokeCount++;
                return (TaskToAsyncResult<object>)System.Threading.Tasks.Task<object>.Run<object>(() =>
                {
                    System.Threading.Thread.Sleep(250);
                    return true;
                });
            }
            else if (name == "Christianson")
            {
                if (args.Length != 0) throw new ArgumentException("Wrong number of arguments.");

                this.InvokeCount++;
                return (TaskToAsyncResult<object>)System.Threading.Tasks.Task<object>.Run<object>(() =>
                {
                    System.Threading.Thread.Sleep(250);
                    return (Identifier)"NoFlight";
                });
            }
            else if (name == "Dengson")
            {
                if (args.Length != 2) throw new ArgumentException("Wrong number of arguments.");
                if (args[0].GetType() != typeof(string)) throw new ArgumentException("Wrong type", "n");
                if (args[1].GetType() != typeof(long)) throw new ArgumentException("Wrong type", "q");

                this.InvokeCount++;
                return (TaskToAsyncResult<object>)System.Threading.Tasks.Task<object>.Run<object>(() =>
                {
                    System.Threading.Thread.Sleep(250);
                    return new string[] { "Luffe", "Sjanne", "Clarke", "Louis" };
                });
            }
            else if (name == "Esildro")
            {
                if (args != null && args.Length != 0) throw new ArgumentException("Wrong number of arguments.");

                this.InvokeCount++;
                return (TaskToAsyncResult<object>)System.Threading.Tasks.Task<object>.Run<object>(() =>
                {
                    System.Threading.Thread.Sleep(250);
                    return new object();        // TODO: investigate why I can't return null.
                });
            }
            else
            {
                throw new DynamicMethodNotFoundException(name);
            }
        }
    }
}
