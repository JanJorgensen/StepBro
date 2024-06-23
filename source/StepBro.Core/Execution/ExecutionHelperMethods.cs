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
using System.Reflection;
using System.Text;
using SBP = StepBro.Core.Parser.Grammar.StepBro;

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
                    var container = file.TryGetVariableContainer<T>(id);
                    if (container != null)
                    {
                        return container;
                    }
                }
            }
            else
            {
                var container = ((StepBro.Core.ScriptData.ScriptFile)context.Self.ParentFile).TryGetVariableContainer<T>(id);
                if (container != null)
                {
                    return container;
                }
            }
            context.ReportError(String.Format("INTERNAL ERROR: Could not find variable container with id = {0}.", id));
            return null;
        }

        public static void SetGlobalVariable<T>(IScriptCallContext context, int id, T value)
        {
            // GetGlobalVariable reports error if the variable can not be found
            GetGlobalVariable<T>(context, id)?.SetValue(value);
        }

        public static T UnaryOperatorGlobalVariable<T>(IScriptCallContext context, int id, int op, bool opOnLeft)
        {
            // GetGlobalVariable reports error if the variable can not be found
            var variable = GetGlobalVariable<T>(context, id);
            if (variable != null)
            {
                // We save value before for opOnRight unary operations
                var valueBefore = variable.GetTypedValue();
                try
                {
                    dynamic variableValue = variable.GetTypedValue();
                    switch (op)
                    {
                        case SBP.OP_INC:
                            variable.SetValue(variableValue + 1);
                            break;
                        case SBP.OP_DEC:
                            variable.SetValue(variableValue - 1);
                            break;
                        default:
                            context.ReportError("Operator not supported on global variable.");
                            break;
                    }

                    if (opOnLeft)
                    {
                        return variable.GetTypedValue();
                    }
                    else
                    {
                        return valueBefore;
                    }
                }
                catch (Exception ex)
                {
                    context.ReportError($"Error occured during unary operation of a global variable, with following exception message: {ex.Message}");
                }
            }

            return default;
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
                        context.ReportError(String.Format("INTERNAL ERROR: Could not find procedure with id = {0}.", procedureID));
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
                        context.ReportError(String.Format("INTERNAL ERROR: Could not find script file with id = {0}.", fileID));
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
                            context.ReportError(String.Format("INTERNAL ERROR: Could not find procedure with id = {0}.", procedureID));
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

        public static bool PostProcedureCallResultHandling(IScriptCallContext context, IScriptCallContext sub)
        {
            return context.SetResultFromSub(sub);
        }

        public static IFileElement GetFileElement(IScriptCallContext context, int id)
        {
            var element = ((ScriptFile)context.Self.ParentFile).GetFileElement(id);
            if (element == null)
            {
                context.ReportError(String.Format("INTERNAL ERROR: Could not find procedure with id = {0}.", id));
            }
            return element;
        }

        public static void LogList(IScriptCallContext context, IEnumerable<string> list)
        {
            if (list != null)
            {
                foreach (string s in list)
                {
                    context.Log(s);
                }
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
                subContext = context.EnterNewScriptContext(procedure, Logging.ContextLogOption.Normal, true, sequencialFirstArguments); // TODO: collect all arguments

                var arguments = new List<object>();
                arguments.Add(subContext);
                if (!SetupCallArguments((FileProcedure)procedure.ProcedureData, sequencialFirstArguments, arguments))
                {
                    throw new ArgumentException("Missing procedure arguments.");
                }

                Delegate runtimeProcedure = ((FileProcedure)procedure.ProcedureData).RuntimeProcedure;
                object returnValue = runtimeProcedure.DynamicInvoke(arguments.ToArray());
                if (PostProcedureCallResultHandling(context, subContext))
                {
                    throw new RequestEarlyExitException();
                }
                return returnValue;
            }
            catch (RequestEarlyExitException)
            {
                throw;  // Just pass this right through.
            }
            catch (TargetInvocationException e)
            {
                context.ReportError("Exception in dynamic procedure call.", exception: e.InnerException);
                return null;
            }
            catch (Exception ex)
            {
                context.ReportError("Exception in dynamic procedure call.", exception: ex);
                return null;
            }
            finally
            {
                subContext.InternalDispose();
            }
        }

        internal static bool SetupCallArguments(FileProcedure procedure, object[] sequencialFirstArguments, List<object> arguments)
        {
            bool success = true;
            var methodInfo = procedure.DelegateType.GetMethod("Invoke");
            var formalParameters = procedure.GetFormalParameters();
            ArgumentList args = null;
            if (sequencialFirstArguments != null && sequencialFirstArguments.Length == 1 && sequencialFirstArguments[0].GetType() == typeof(ArgumentList))
            {
                args = sequencialFirstArguments[0] as ArgumentList;
            }
            else
            {
                if (sequencialFirstArguments != null)
                {
                    arguments.AddRange(sequencialFirstArguments);
                }
            }
            foreach (var p in methodInfo.GetParameters().Skip(arguments.Count)) // Start from the first missing argument.
            {
                bool handled = false;

                if (args != null)       // If arguments were given
                {
                    var arg = args.FirstOrDefault(a => a.Name == p.Name);
                    if (arg != null)    // If argument with same name as parameter was found
                    {
                        arguments.Add(arg.Value);
                        handled = true;
                    }
                }

                if (!handled)
                {
                    var parameter = formalParameters.FirstOrDefault(fp => fp.Name == p.Name);
                    if (parameter != null && parameter.HasDefaultValue)  // If parameter found in formal parameters (should never fail...) and there is a default value/argument
                    {
                        // Use the default value from the formal parameters.
                        var value = parameter.DefaultValue;
                        if (value == null && p.ParameterType.IsValueType)
                        {
                            value = Activator.CreateInstance(p.ParameterType);
                        }
                        arguments.Add(value);
                        handled = true;
                    }
                }
                if (!handled)
                {
                    success = false;
                    break;
                }
            }
            return success;
        }

        public static object DynamicFunctionCall(
            this IProcedureReference procedure,
            IScriptCallContext context,
            object[] sequencialFirstArguments,
            ArgumentList namedArguments,
            object[] sequencialLastArguments)
        {
            if (procedure == null) throw new ArgumentNullException("procedure");
            if ((procedure.ProcedureData.Flags & ProcedureFlags.IsFunction) == ProcedureFlags.None) throw new ArgumentException("The procedure is not marked as being a function.");
            throw new NotImplementedException();
        }

        public static IProcedureReference<T> CastToSpecificProcedureType<T>(IScriptCallContext context, IProcedureReference reference)
        {
            if (reference is IProcedureReference<T>) return (IProcedureReference<T>)reference;
            else
            {
                context.ReportError("The procedure reference is not the expected type. Type: " + reference.GetType().Name);
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
            Type t = typeof(T);
            var e = element;
            while (e != null)
            {
                var partner = e.ListPartners().FirstOrDefault(p => p.Name.Equals(partnerName, StringComparison.InvariantCulture));
                if (partner != null)
                {
                    //System.Diagnostics.Debug.WriteLine("Expected type: " + typeof(T).FullName);
                    //System.Diagnostics.Debug.WriteLine("Found type:    " + partner.ProcedureReference.ProcedureReference.GetType().GetGenericArguments()[0].FullName);
                    if (partner.ProcedureReference.ProcedureReference is IProcedureReference<T>)
                    {
                        return partner.ProcedureReference.ProcedureReference as IProcedureReference<T>;
                    }
                    else
                    {
                        context.ReportError("The partner procedure is not the expected type.");
                        return null;
                    }
                }
                e = e.BaseElement;
            }
            context.ReportError($"The element \"{element.FullName}\" has no partner named \"{partnerName}\".");
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
            if (result == null)
            {
                context.ReportError("No async result data. Did the method fail to execute?");
                return default(T);
            }
            if (!result.IsCompleted)
            {
                if (!result.AsyncWaitHandle.WaitOne(20000))
                {
                    context.ReportError("Timeout");
                    return default(T);
                }
            }
            if (result.IsFaulted)
            {
                if (result is IObjectFaultDescriptor)
                {
                    context.ReportError("Async operation failed. Fault: " + ((IObjectFaultDescriptor)result).FaultDescription);
                }
                else
                {
                    context.ReportError("Async operation failed.");
                }
                return default(T);
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
                // TODO: Register this "task" and replace with loop that waits a short while.
                // TODO: Ask the object itself what the timeout time should be.
                if (!result.AsyncWaitHandle.WaitOne(20000))
                {
                    throw new TimeoutException($"Timeout waiting for asynchronous result in line {context.CurrentScriptFileLine}.");
                }
            }
            if (result.IsFaulted)
            {
                if (result is IObjectFaultDescriptor)
                {
                    context.ReportError("Async operation failed. Fault: " + ((IObjectFaultDescriptor)result).FaultDescription);
                }
                else
                {
                    context.ReportError("Async operation failed.");
                }
                return default(T);
            }
            else
            {
                object v = result?.Result;
                if (v != null)
                {
                    var valueType = v.GetType();
                    if (typeof(T).IsAssignableFrom(valueType))
                    {
                        return (T)v;
                    }
                    else if (valueType == typeof(string))
                    {
                        try
                        {
                            object value = Convert.ChangeType(v, typeof(T));
                            return (T)value;
                        }
                        catch { }
                    }

                    throw new InvalidCastException($"The async result value type is \"{v.GetType().TypeName()}\", and cannot be converted into a \"{typeof(T).TypeName()}\".");
                }
            }
            return default(T);
        }

        #region Dynamic Object Access

        public static object ExecuteDynamicObjectMethod(
            IScriptCallContext context,
            IDynamicStepBroObject instance,
            string name,
            object[] arguments)
        {
            try
            {
                if (context != null && context.LoggingEnabled && context.Logger.IsDebugging)
                {
                    context.Log($"Calling dynamic method '{name}' on object of type '{instance.GetType().Name}'.");
                }
                return instance.InvokeMethod(context.EnterNewContext(instance.GetType().Name, false), name, arguments);
            }
            catch (DynamicMethodNotFoundException)
            {
                if (context != null)
                {
                    context.ReportError(
                        $"Method named '{name}' was not found on the object of type '{instance.GetType().Name}'.",
                        new DynamicMethodNotFoundError());
                }
                throw;
            }
            catch (Exception ex)
            {
                if (context != null)
                {
                    context.ReportError(
                        $"Exception executing method '{name}' on the object of type '{instance.GetType().Name}'.",
                        exception: ex);
                }
                throw;
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
                if (context != null && context.LoggingEnabled && context.Logger.IsDebugging)
                {
                    if (result != null)
                    {
                        context.Log($"Called method '{name}' on object of type '{instance.GetType().Name}'.");
                    }
                    else
                    {
                        context.Log($"Null value returned from method '{name}' on object of type '{instance.GetType().Name}'.");
                    }
                }
                return result;
            }
            catch (DynamicMethodNotFoundException)
            {
                if (context != null)
                {
                    context.ReportError(
                        $"Method named '{name}' was not found on the object of type '{instance.GetType().Name}'.",
                        new DynamicMethodNotFoundError());
                }
                throw;
            }
            catch (Exception ex)
            {
                if (context != null)
                {
                    context.ReportError(
                        $"Exception executing method '{name}' on the object of type '{instance.GetType().Name}'.",
                        exception: ex);
                }
                throw;
            }
        }

        public static TExpected DynamicObjectGetProperty<TExpected>(
            IScriptCallContext context,
            IDynamicStepBroObject instance,
            string name)
        {
            try
            {
                if (context != null && context.LoggingEnabled && context.Logger.IsDebugging)
                {
                    if (instance is INameable)
                    {
                        context.LogDetail($"Getting dynamic property '{name}' on '{((INameable)instance).Name}'.");
                    }
                    else if (instance is INamedObject)
                    {
                        context.LogDetail($"Getting dynamic property '{name}' on '{((INamedObject)instance).FullName}'.");
                    }
                    else
                    {
                        context.LogDetail($"Getting dynamic property '{name}' on object of type '{instance.GetType().Name}'.");
                    }
                }
                var value = instance.GetProperty(context.EnterNewContext(instance.GetType().Name, false), name);
                return (TExpected)System.Convert.ChangeType(value, typeof(TExpected));
            }
            catch (DynamicPropertyNotFoundException)
            {
                if (context != null)
                {
                    context.ReportError(
                        $"Property named '{name}' was not found on the object of type '{instance.GetType().Name}'.",
                        new DynamicPropertyNotFoundError());
                }
            }
            catch (Exception ex)
            {
                if (context != null)
                {
                    context.ReportError(
                        $"Exception getting property '{name}' on the object of type '{instance.GetType().Name}'. Expected type: '{typeof(TExpected).Name}'.",
                        exception: ex);
                }
            }
            return default(TExpected);
        }

        public static void DynamicObjectSetProperty(
            IScriptCallContext context,
            IDynamicStepBroObject instance,
            string name,
            object value)
        {
            try
            {
                if (context != null && context.LoggingEnabled && context.Logger.IsDebugging)
                {
                    if (instance is INameable)
                    {
                        context.LogDetail($"Setting dynamic property '{name}' on '{((INameable)instance).Name}'.");
                    }
                    else if (instance is INamedObject)
                    {
                        context.LogDetail($"Setting dynamic property '{name}' on '{((INamedObject)instance).FullName}'.");
                    }
                    else
                    {
                        context.LogDetail($"Setting dynamic property '{name}' on object of type '{instance.GetType().Name}'.");
                    }
                }
                instance.SetProperty(context.EnterNewContext(instance.GetType().Name, false), name, value);
            }
            catch (DynamicPropertyNotFoundException)
            {
                if (context != null)
                {
                    context.ReportError(
                        $"Property named '{name}' was not found on the object of type '{instance.GetType().Name}'.",
                        new DynamicPropertyNotFoundError());
                }
                throw;
            }
            catch (Exception ex)
            {
                if (context != null)
                {
                    context.ReportError(
                        $"Exception setting property '{name}' on the object of type '{instance.GetType().Name}'.",
                        exception: ex);
                }
                throw;
            }
        }

        #endregion

        public static TProcedure ProcedureReferenceAs<TProcedure>(
            IScriptCallContext context,
            IProcedureReference procedure,
            int targetFileID,
            int targetProcedureID) where TProcedure : class, IProcedureReference
        {
            var type = GetProcedure(context, targetFileID, targetProcedureID);
            if (type != null)
            {
                var targetType = type.ProcedureData.DataType;
                if (((FileProcedure)procedure.ProcedureData).IsA(type.ProcedureData))
                {
                    return (TProcedure)procedure;
                }
            }
            else
            {
                context.ReportError("Target procedure type was not found.");
            }
            return null;
        }

        public static bool ProcedureReferenceIs(
            IScriptCallContext context,
            IProcedureReference procedure,
            int fileID,
            int procedureID,
            bool isNot)
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

        /// <summary>
        /// Helper method to save the value for an expect statement, and then just return the value itself.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="value">Input value</param>
        /// <param name="position">Left, Middle, or Center</param>
        /// <param name="resetString">Whether to reset the string</param>
        /// <returns>Input value directly.</returns>
        public static T SaveExpectValueText<T>(IScriptCallContext context, T value, string position, bool resetString)
        {
            if (resetString)
            {
                context.ExpectStatementValue = "";
            }

            if (!String.IsNullOrEmpty(context.ExpectStatementValue))
            {
                context.ExpectStatementValue += ", ";
            }

            context.ExpectStatementValue += position + ": " + StringUtils.ObjectToString(value, false);
            return value;
        }

        public static void DisposeObject(IScriptCallContext context, IDisposable obj)
        {
            if (obj == null) throw new NotImplementedException();
            if (obj is IScriptDisposable)
            {
                (obj as IScriptDisposable).Dispose(context);
            }
            else
            {
                obj.Dispose();
            }
        }

        public static void SetupObjectWithPropertyBlock(IScriptFile file, ILogger logger, IValueContainerOwnerAccess containerOwner)
        {
            if (containerOwner != null && (containerOwner.Tags != null))
            {
                var @object = containerOwner.Container.GetValue(logger) as ISettableFromPropertyBlock;
                if (@object != null)
                {
                    object props;
                    if (containerOwner.Tags.TryGetValue(ScriptFile.VARIABLE_CUSTOM_PROPS_TAG, out props) && props is PropertyBlock)
                    {
                        @object.Setup(file, logger, props as PropertyBlock);
                    }
                }
            }
        }

        /// <summary>
        /// Executes the evaluation of an expect statement.
        /// </summary>
        /// <param name="context">Execution call context.</param>
        /// <param name="evaluation">The expect statement evaluation code.</param>
        /// <param name="negativeVerdict">The verdict for negative evaluation result.</param>
        /// <param name="title">The expect statement title.</param>
        /// <param name="expected">String representation of the expectation.</param>
        /// <returns>Whether the procedure should skip the rest and return immediately.</returns>
        public static bool ExpectStatement(
            IScriptCallContext context,
            bool success,
            Verdict negativeVerdict,
            string title,
            string expected)
        {
            try
            {
                context.ReportExpectResult(title, expected, success ? Verdict.Pass : negativeVerdict);

                if (success) return false;
                else if (negativeVerdict == Verdict.Error) return true;
                else return (context.Self.Flags & ProcedureFlags.ContinueOnFail) == ProcedureFlags.None;
            }
            catch (Exception ex)
            {
                if (context != null) context.ReportError($"Exception thrown during expect statement execution.", exception: ex);
                else
                {
                    throw;
                }

                return true; // Do exit the procedure
            }
        }

        public static bool PostExpressionStatement(IScriptCallContext context)
        {
            if (context.Result.Verdict >= Verdict.Error) return true;
            if (context.Result.Verdict >= Verdict.Fail)
            {
                return (context.Self.Flags & ProcedureFlags.ContinueOnFail) == ProcedureFlags.None;
            }
            return false;
        }

        public static ICallContext CreateMethodCallContext(IScriptCallContext context, string locationStatic, object locationObject, string locationFixed)
        {
            string location = (locationObject != null && locationObject is INamedObject) ? ((INamedObject)locationObject).ShortName : locationStatic;
            if (String.IsNullOrEmpty(location))
            {
                location = locationFixed;
            }
            else
            {
                location = location + "." + locationFixed;
            }
            var newContext = new CallContext(
                (ScriptCallContext)context,
                CallEntry.Subsequent,
                false,
                ((ScriptCallContext)context).GetDynamicLogLocation() + " " + location);  // TODO: set the last two arguments.
            return newContext;
        }
    }
}