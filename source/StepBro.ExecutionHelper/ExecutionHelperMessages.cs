using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.ExecutionHelper.Messages
{
    internal class ExecutionHelperMessages
    {
    }

    public enum ShortCommand
    {
        None,
        Close
    }

    public class CreateVariable
    {
        public CreateVariable(string variableName, object initialValue)
        {
            this.VariableName = variableName;
            this.InitialValue = initialValue;
        }

        public string VariableName { get; set; }
        public object InitialValue { get; set; }
    }

    public class IncrementVariable
    {
        public IncrementVariable(string variableName)
        {
            this.VariableName = variableName;
        }

        public string VariableName { get; set; }
    }

    public class GetVariable
    {
        public GetVariable(string variableName)
        {
            this.VariableName = variableName;
        }

        public string VariableName { get; set; }
    }

    public class SetVariable
    {
        public SetVariable(string variableName, object value)
        {
            this.VariableName = variableName;
            this.Value = value;
        }

        public string VariableName { get; set; }
        public object Value { get; set; }
    }

    public class SendVariable
    {
        public SendVariable(string variableName, object variable)
        {
            this.VariableName = variableName;
            this.Variable = variable;
        }

        public string VariableName { get; set; }
        public object Variable { get; set; }
    }
}
