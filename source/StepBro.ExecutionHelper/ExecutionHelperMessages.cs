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
        CloseApplication,
        CloseConnection,
        Acknowledge
    }

    public class CreateOrSetVariable
    {
        public CreateOrSetVariable(string variableName, object value)
        {
            this.VariableName = variableName;
            this.Value = value;
        }

        public string VariableName { get; set; }
        public object Value { get; set; }
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

    public class SaveFile
    {
        public SaveFile(string fileName)
        {
            this.FileName = fileName;
        }

        public string FileName { get; set; }
    }

    public class LoadFile
    {
        public LoadFile(string fileName)
        {
            this.FileName = fileName;
        }

        public string FileName { get; set; }
    }
}
