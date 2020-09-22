using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.General;
using StepBro.Core.Logging;
using StepBro.Core.ScriptData;
using StepBro.Core.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StepBro.Core.Execution
{
    internal static class ExecutionHelperMethods
    {
        internal const int StringMultiplierMaxLength = 1000 * 1000 * 10;
        public const double APPROX_TOL = 1.0E-14;
        public const long APPROX_TOL_TS = 10L;  // Set to 5. A better value?

        public static string StringMultiply(string input, long multiplier)
        {
            if (multiplier <= 0L)
            {
                return "";
            }
            var n = (int)multiplier;
            var len = input.Length * (int)multiplier;
            if (len <= StringMultiplierMaxLength)
            {
                var sb = new StringBuilder(len);
                for (var i = 0L; i < n; i++) sb.Append(input);
                return sb.ToString();
            }
            else
            {
                throw new ArgumentException("String multiplication results in a too large string (" + len.ToString() + " characters)");
            }
        }

        public static string ObjectToString(object value)
        {
            if (value == null) return "<null>";
            else if (value.GetType() != typeof(string) && value.GetType() != typeof(object) && typeof(IEnumerable).IsAssignableFrom(value.GetType()))
            {
                return StringUtils.ListToString((IEnumerable)value);
            }
            else
            {
                return Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        public static bool ApproximatelyEquals(double value, double expected)
        {
            return GreaterThanOrApprox(value, expected) && LessThanOrApprox(value, expected);
            //if (value == expected) return true;
            //if (value == 0.0)
            //{
            //    return expected > -APPROX_TOL && expected < APPROX_TOL;
            //}
            //else if (expected == 0.0)
            //{
            //    return value > -APPROX_TOL && value < APPROX_TOL;
            //}
            //else if (Math.Abs(value) > Math.Abs(expected))
            //{
            //    var tol = Math.Abs(expected) * APPROX_TOL;
            //    return value > (expected - tol) && value < (expected + tol);
            //}
            //else
            //{
            //    var tol = Math.Abs(value) * APPROX_TOL;
            //    return expected > (value - tol) && expected < (value + tol);
            //}
        }

        public static bool GreaterThanOrApprox(double value, double limit)
        {
            if (value > limit) return true;
            if (limit == 0.0)
            {
                return value > -APPROX_TOL;
            }
            else
            {
                var tol = Math.Abs(limit) * APPROX_TOL;
                return value > (limit - tol);
            }
        }
        public static bool GreaterThanOrApprox(long value, double limit)
        {
            return GreaterThanOrApprox((double)value, limit);
        }
        public static bool GreaterThanOrApprox(double value, long limit)
        {
            return GreaterThanOrApprox(value, (double)limit);
        }

        public static bool LessThanOrApprox(double value, double limit)
        {
            if (value < limit) return true;
            if (limit == 0.0)
            {
                return value < APPROX_TOL;
            }
            else
            {
                var tol = Math.Abs(limit) * APPROX_TOL;
                return value < (limit + tol);
            }
        }
        public static bool LessThanOrApprox(long value, double limit)
        {
            return LessThanOrApprox((double)value, limit);
        }
        public static bool LessThanOrApprox(double value, long limit)
        {
            return LessThanOrApprox(value, (double)limit);
        }


        public static bool IsBetweenIntegerExpression(IScriptCallContext context, long lower, NumericLimitType op1, long value, NumericLimitType op2, long upper)
        {
            bool result = true;

            switch (op1)
            {
                case NumericLimitType.Exclude:
                    result &= (value > lower);
                    break;
                case NumericLimitType.Include:
                case NumericLimitType.Approx:
                    result &= (value >= lower);
                    break;
                default:
                    throw new NotImplementedException();
            }
            switch (op1)
            {
                case NumericLimitType.Exclude:
                    result &= (value < upper);
                    break;
                case NumericLimitType.Include:
                case NumericLimitType.Approx:
                    result &= (value <= upper);
                    break;
                default:
                    throw new NotImplementedException();
            }

            return result;
        }

        public static bool IsBetweenDecimalExpression(IScriptCallContext context, double lower, NumericLimitType op1, double value, NumericLimitType op2, double upper)
        {
            double tolerance = (upper - lower) * APPROX_TOL;

            bool result = true;

            switch (op1)
            {
                case NumericLimitType.Exclude:
                    result &= (value > lower);
                    break;
                case NumericLimitType.Include:
                    result &= (value >= lower);
                    break;
                case NumericLimitType.Approx:
                    result &= (value > (lower - tolerance));
                    break;
                default:
                    throw new NotImplementedException();
            }
            switch (op1)
            {
                case NumericLimitType.Exclude:
                    result &= (value < upper);
                    break;
                case NumericLimitType.Include:
                    result &= (value <= upper);
                    break;
                case NumericLimitType.Approx:
                    result &= (value < (upper + tolerance));
                    break;
                default:
                    throw new NotImplementedException();
            }

            return result;
        }

        public static bool IsBetweenTimespanExpression(IScriptCallContext context, TimeSpan lower, NumericLimitType op1, TimeSpan value, NumericLimitType op2, TimeSpan upper)
        {
            long tolerance = APPROX_TOL_TS;

            bool result = true;

            switch (op1)
            {
                case NumericLimitType.Exclude:
                    result = (value > lower);
                    break;
                case NumericLimitType.Include:
                    result = (value >= lower);
                    break;
                case NumericLimitType.Approx:
                    result = (value.Ticks > (lower.Ticks - tolerance));
                    break;
                default:
                    throw new NotImplementedException();
            }
            switch (op1)
            {
                case NumericLimitType.Exclude:
                    result &= (value < upper);
                    break;
                case NumericLimitType.Include:
                    result &= (value <= upper);
                    break;
                case NumericLimitType.Approx:
                    result &= (value.Ticks < (upper.Ticks + tolerance));
                    break;
                default:
                    throw new NotImplementedException();
            }

            return result;
        }

        public static bool EqualsWithToleranceIntegerExpression(IScriptCallContext context, long value, long expected, long tolerance)
        {
            return (value >= (expected - tolerance) && value <= (expected + tolerance));
        }

        public static bool EqualsWithToleranceDecimalExpression(IScriptCallContext context, bool isApprox, double value, double expected, double tolerance)
        {
            if (tolerance < 0.0)
            {
                throw new ArgumentException("The tolerance must be a positive number.");
            }
            var tol = tolerance;
            if (isApprox)
            {
                if (tol == 0.0)
                {
                    tol = APPROX_TOL;
                }
                else
                {
                    tol += tolerance * APPROX_TOL;
                }
            }
            return (value >= (expected - tol) && value <= (expected + tol));
        }

        public static bool EqualsWithToleranceTimespanExpression(IScriptCallContext context, bool isApprox, TimeSpan value, TimeSpan expected, TimeSpan tolerance)
        {
            if (tolerance < TimeSpan.FromTicks(0))
            {
                throw new ArgumentException("The tolerance must be a positive value.");
            }
            var tol = tolerance;
            if (isApprox) tol = TimeSpan.FromTicks(tolerance.Ticks + APPROX_TOL_TS);
            return (value >= (expected - tolerance) && value <= (expected + tolerance));
        }

        public static bool EqualsWithToleranceDateTimeExpression(IScriptCallContext context, bool isApprox, DateTime value, DateTime expected, TimeSpan tolerance)
        {
            if (tolerance < TimeSpan.FromTicks(0))
            {
                throw new ArgumentException("The tolerance must be a positive value.");
            }
            var tol = tolerance;
            if (isApprox) tol = TimeSpan.FromTicks(tolerance.Ticks + APPROX_TOL_TS);
            var lower = expected - tol;
            var upper = expected + tol;
            return (value >= lower && value <= upper);
        }

        public static IValueContainer<T> GetGlobalVariable<T>(IScriptCallContext context, int id)
        {
            if (context == null)
            {
                foreach (var file in ServiceManager.Global.Get<ILoadedFilesManager>().ListFiles<ScriptFile>())
                {
                    var container = file.GetVariableContainer<T>(id);
                    if (container != null)
                    {
                        return container;
                    }
                }
            }
            else
            {
                var container = ((StepBro.Core.ScriptData.ScriptFile)context.Self.ParentFile).GetVariableContainer<T>(id);
                if (container != null)
                {
                    return container;
                }
            }
            context.ReportError(description: String.Format("INTERNAL ERROR: Could not find variable container with id = {0}.", id));
            return null;
        }

        public static IProcedureReference GetProcedure(IScriptCallContext context, int fileID, int procedureID)
        {
            IProcedureReference procedure = null;
            if (fileID < 0)
            {
                procedure = ((ScriptFile)context.Self.ParentFile).GetProcedure(procedureID);
                if (procedure == null)
                {
                    if (context != null)
                    {
                        context.ReportError(description: String.Format("INTERNAL ERROR: Could not find procedure with id = {0}.", procedureID));
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
            }
            else
            {
                ScriptFile file = context.LoadedFiles.ListFiles<ScriptFile>().FirstOrDefault(f => f.UniqueID == fileID);
                if (file == null)
                {
                    if (context != null)
                    {
                        context.ReportError(description: String.Format("INTERNAL ERROR: Could not find script file with id = {0}.", fileID));
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                else
                {
                    procedure = file.GetProcedure(procedureID);
                    if (procedure == null)
                    {
                        if (context != null)
                        {
                            context.ReportError(description: String.Format("INTERNAL ERROR: Could not find procedure with id = {0}.", procedureID));
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    }
                }
            }
            return procedure;
        }

        public static IProcedureReference<T> GetProcedureTyped<T>(IScriptCallContext context, int fileID, int procedureID) where T : class
        {
            return GetProcedure(context, fileID, procedureID) as IProcedureReference<T>;
        }

        public static IFileElement GetFileElement(IScriptCallContext context, int id)
        {
            var element = ((ScriptFile)context.Self.ParentFile).GetFileElement(id);
            if (element == null)
            {
                context.ReportError(description: String.Format("INTERNAL ERROR: Could not find procedure with id = {0}.", id));
            }
            return element;
        }

        public static void LogList(IScriptCallContext context, IEnumerable<string> list)
        {
            foreach (string s in list)
            {
                context.Log(s);
            }
        }

        public static object DynamicProcedureCall(
            this IProcedureReference procedure,
            IScriptCallContext context,
            PropertyBlock propertyBlock,
            object[] sequencialFirstArguments,
            ArgumentList namedArguments,
            object[] sequencialLastArguments)
        {
            if (procedure == null) throw new ArgumentNullException("procedure");


            IScriptCallContext subContext = null;
            try
            {
                subContext = context.EnterNewScriptContext(procedure, Logging.ContextLogOption.Normal, true);

                var methodInfo = ((FileProcedure)procedure.ProcedureData).DelegateType.GetMethod("Invoke");
                var arguments = new List<object>();
                int i = 0;
                foreach (var p in methodInfo.GetParameters())
                {
                    if (i == 0)
                    {
                        arguments.Add(subContext);
                    }
                    i++;
                }
                Delegate runtimeProcedure = ((FileProcedure)procedure.ProcedureData).RuntimeProcedure;
                return runtimeProcedure.DynamicInvoke(arguments.ToArray());
            }
            catch (Exception ex)
            {
                context.ReportError(description: "Exception in dynamic procedure call.", exception: ex);
                return null;
            }
            finally
            {
                subContext.InternalDispose();
            }
        }

        public static object DynamicFunctionCall(
            this IProcedureReference procedure,
            IScriptCallContext context,
            object[] sequencialFirstArguments,
            ArgumentList namedArguments,
            object[] sequencialLastArguments)
        {
            if (procedure == null) throw new ArgumentNullException("procedure");
            if (!procedure.ProcedureData.IsFunction) throw new ArgumentException("The procedure is not marked as being a function.");
            throw new NotImplementedException();
        }

        public static IProcedureReference<T> CastToSpecificProcedureType<T>(IScriptCallContext context, IProcedureReference reference)
        {
            if (reference is IProcedureReference<T>) return (IProcedureReference<T>)reference;
            else
            {
                context.ReportError(description: "The procedure reference is not the expected type. Type: " + reference.GetType().Name);
                return null;
            }
        }

        public static IProcedureReference<T> GetPartnerReferenceFromProcedureReference<T>(
            IScriptCallContext context, IProcedureReference procedure, string partnerName)
        {
            return GetPartnerReference<T>(context, procedure.ProcedureData, partnerName);
        }

        public static IProcedureReference<T> GetPartnerReference<T>(
            IScriptCallContext context, IFileElement element, string partnerName)
        {
            while (element != null)
            {
                var partner = element.ListPartners().FirstOrDefault(p => p.Name.Equals(partnerName, StringComparison.InvariantCulture));
                if (partner != null)
                {
                    if (partner.ProcedureReference.ProcedureReference is IProcedureReference<T>)
                    {
                        return partner.ProcedureReference.ProcedureReference as IProcedureReference<T>;
                    }
                    else
                    {
                        context.ReportError(description: "The partner procedure is not the expected type.");
                        return null;
                    }
                }
                element = element.BaseElement;
            }
            context.ReportError(description: $"The procedure has no partner named \"{partnerName}\".");
            return null;
        }

        public static void AwaitAsyncVoid(IScriptCallContext context, IAsyncResult result)
        {
            if (result.IsCompleted)
            {
                return;
            }
            result.AsyncWaitHandle.WaitOne();
        }

        public static T AwaitAsyncTyped<T>(IScriptCallContext context, Tasks.IAsyncResult<T> result)
        {
            if (!result.IsCompleted)
            {
                result.AsyncWaitHandle.WaitOne();
            }
            return result.Result;
        }

        public static T AwaitObjectToTyped<T>(IScriptCallContext context, object result)
        {
            if (result != null && result is Tasks.IAsyncResult<object>)
            {
                return AwaitAsyncToTyped<T>(context, (Tasks.IAsyncResult<object>)result);
            }
            else
            {
                if (result != null)
                {
                    throw new InvalidCastException($"The value type is \"{result.GetType().TypeName()}\", and cannot be converted into a \"{typeof(T).TypeName()}\".");
                }
                else
                {
                    throw new InvalidCastException($"The value is null, and therefore cannot be converted into a \"{typeof(T).TypeName()}\".");
                }
            }
        }

        public static T AwaitAsyncToTyped<T>(IScriptCallContext context, Tasks.IAsyncResult<object> result)
        {
            if (result != null && !result.IsCompleted)
            {
                if (!result.AsyncWaitHandle.WaitOne(20000))       // TODO: Register this "task" and replace with loop that waits a short while.
                {
                    throw new TimeoutException($"Timeout waiting for asynchronous result in line {context.CurrentScriptFileLine}.");
                }
            }
            if (result.IsFaulted)
            {
                if (result is IObjectFaultDescriptor)
                {
                    context.ReportError(description: "Async operation failed. Fault: " + ((IObjectFaultDescriptor)result).FaultDescription);
                }
                else
                {
                    context.ReportError(description: "Async operation failed.");
                }
                return default(T);
            }
            else
            {
                object v = result?.Result;
                if (v != null)
                {
                    if (typeof(T).IsAssignableFrom(v.GetType()))
                    {
                        return (T)v;
                    }
                    else
                    {
                        throw new InvalidCastException($"The async result value type is \"{v.GetType().TypeName()}\", and cannot be converted into a \"{typeof(T).TypeName()}\".");
                    }
                }
            }
            return default(T);
        }

        public static object ExecuteDynamicObjectMethod(
            IScriptCallContext context,
            IDynamicStepBroObject instance,
            string name,
            object[] arguments)
        {
            try
            {
                if (context != null && context.LoggingEnabled)
                {
                    context.Log($"Calling dynamic method \"{name}\".");
                }
                return instance.TryInvokeMethod(name, arguments);
            }
            catch (DynamicMethodNotFoundException)
            {
                if (context != null)
                {
                    context.ReportError(
                        new DynamicMethodNotFoundError(),
                        $"Method named '{name}' was not found on the object of type '{instance.GetType().Name}'.");
                }
                throw;
            }
            catch (Exception ex)
            {
                if (context != null)
                {
                    context.ReportError(
                        null,
                        $"Exception executing method '{name}' on the object of type '{instance.GetType().Name}'.",
                        ex);
                }
                throw ex;
            }
        }

        public static IAsyncResult<object> ExecuteDynamicAsyncObjectMethod(
            IScriptCallContext context,
            IDynamicAsyncStepBroObject instance,
            string name,
            object[] arguments)
        {
            try
            {
                var result = instance.TryInvokeMethod(name, arguments);
                if (result != null)
                {
                    if (context != null && context.LoggingEnabled)
                    {
                        context.Log($"Called method \"{name}\" on object of type '{instance.GetType().Name}'.");
                    }
                }
                else
                {
                    if (context != null && context.LoggingEnabled)
                    {
                        context.Log($"Null value returned from method \"{name}\" on object of type '{instance.GetType().Name}'.");
                    }
                }
                return result;
            }
            catch (DynamicMethodNotFoundException)
            {
                if (context != null)
                {
                    context.ReportError(
                        new DynamicMethodNotFoundError(),
                        $"Method named '{name}' was not found on the object of type '{instance.GetType().Name}'.");
                }
                throw;
            }
            catch (Exception ex)
            {
                if (context != null)
                {
                    context.ReportError(
                        null,
                        $"Exception executing method '{name}' on the object of type '{instance.GetType().Name}'.",
                        ex);
                }
                throw ex;
            }
        }

        public static bool ProcedureReferenceIs(IScriptCallContext context, IProcedureReference procedure, int fileID, int procedureID, bool isNot)
        {
            var type = GetProcedure(context, fileID, procedureID);
            if (Object.ReferenceEquals(procedure, type)) return !isNot;
            return false;
        }

        public static bool ObjectIsType(Type type, object obj, bool isNot)
        {
            if (obj == null) return !isNot;
            var ot = obj.GetType();
            return ot.IsAssignableFrom(type) == !isNot;
        }

        public static bool ResetFileVariable(IValueContainerOwnerAccess access, ILogger logger)
        {
            object o = access.Container.GetValue();
            if (o != null && o is IResettable)
            {
                return (o as IResettable).Reset(null);
            }
            return false;
        }
    }
}