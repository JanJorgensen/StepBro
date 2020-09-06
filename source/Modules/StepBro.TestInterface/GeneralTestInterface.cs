using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Tasks;
using System;
using System.Linq;

namespace StepBro.TestInterface
{
    public class GeneralTestInterface : DynamicAsyncStepBroObject
    {
        private IConnection m_connection = null;

        public GeneralTestInterface()
        {
            System.Diagnostics.Debug.WriteLine("GeneralTestInterface()");
        }

        #region Properties

        public IConnection Connection { get { return m_connection; } set { m_connection = value; } }

        #endregion

        #region Dynamic Interface

        public override DynamicSupport HasProperty(string name, out Type type, out bool isReadOnly)
        {
            var par = m_connection.Parameters.FirstOrDefault(p => String.Equals(p.Name, name, StringComparison.InvariantCulture));
            if (par != null)
            {
                type = par.DataType;
                isReadOnly = !par.HasWriteAccess;
                return DynamicSupport.Yes;
            }
            else
            {
                type = null;
                isReadOnly = false;
                return DynamicSupport.No;
            }
        }

        public override IAsyncResult<object> TryGetProperty(string name)
        {
            var par = m_connection.Parameters.FirstOrDefault(p => String.Equals(p.Name, name, StringComparison.InvariantCulture));
            if (par != null)
            {
                return m_connection.Parameters.GetParameterValue(name);
            }
            else
            {
                var result = new AsyncResult<object>();
                result.SetFaulted(new ObjectNotFoundError());
                return result;
            }
        }

        public override IAsyncResult<bool> TrySetProperty(string name, object value)
        {
            var par = m_connection.Parameters.FirstOrDefault(p => String.Equals(p.Name, name, StringComparison.InvariantCulture));
            if (par != null)
            {
                return m_connection.Parameters.SetParameterValue(name, value);
            }
            else
            {
                var result = new AsyncResult<bool>();
                result.SetFaulted(new ObjectNotFoundError());
                return result;
            }
        }

        public override DynamicSupport HasMethod(string name, out NamedData<Type>[] parameters, out Type returnType)
        {
            var rp = m_connection.RemoteProcedures.Procedures.FirstOrDefault(p => String.Equals(p.Name, name, StringComparison.InvariantCulture));
            if (rp != null)
            {
                parameters = rp.Parameters.Select(par => new NamedData<Type>(par.Name, par.Type)).ToArray();
                returnType = rp.ReturnType;
                return DynamicSupport.Yes;
            }
            else
            {
                parameters = null;
                returnType = null;
                return DynamicSupport.No;
            }
        }

        public override IAsyncResult<object> TryInvokeMethod(string name, object[] args)
        {
            var rp = m_connection.RemoteProcedures.Procedures.FirstOrDefault(p => String.Equals(p.Name, name, StringComparison.InvariantCulture));
            if (rp != null)
            {
                if (((args != null) ? args.Length : 0) != rp.Parameters.Count())
                {

                }
                return m_connection.RemoteProcedures.Invoke(rp, args);
            }
            else
            {
                return m_connection.RemoteProcedures.Invoke(name, args);
            }
        }

        #endregion
    }
}
