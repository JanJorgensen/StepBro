using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Execution
{
    public class UnhandledExceptionInScriptException : Exception
    {
        private ScriptCallContext m_context;
        private string[] m_scriptCallStack;
        internal UnhandledExceptionInScriptException(Exception innerException, ScriptCallContext context) :
            base("Unhandled exception in script execution.", innerException)
        {
            m_context = context;
            m_scriptCallStack = context.GetCallStack();
        }

        internal ScriptCallContext Context { get { return m_context; } }

        public string[] ScriptCallStack { get { return m_scriptCallStack; } }

        public string[] GetPrintableCallStack()
        {
            return CreatePrintableCallStack(this.InnerException, m_scriptCallStack);
        }

        public static string[] CreatePrintableCallStack(Exception ex, string[] scriptCallStack)
        {
            List<string> callstack = new List<string>();
            if (ex.StackTrace != null)
            {
                foreach (string line in ex.StackTrace.Split("\r\n"))
                {
                    if (line.Contains("at lambda_method") || line.Contains("(Closure"))
                    {
                        break;
                    }
                    callstack.Add(line);
                }
            }
            if (scriptCallStack != null)
            {
                callstack.AddRange(scriptCallStack);
            }
            return callstack.ToArray();
        }
    }
}
