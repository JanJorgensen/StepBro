using StepBro.Core.Data;
using StepBro.Core.Data.SerializationHelp;
using StepBro.Core.Logging;
using StepBro.Core.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StepBro.Sidekick.Messages
{
    public enum ShortCommand
    {
        None,
        RequestClose,   // From sidekick to console; this can be rejected/ignored.
        Close,          // Force closing.
        ClearDisplay,
        EndFileElements,
        Parse,
        ExecutionStarted,
        ExecutionStopped,
        GetPanelDefinitions,
        GetObjectExtraData
    }

    public class Log
    {
        public enum Type { Normal, Error }
        public Type LogType { get; set;} = Type.Normal;
        public string Text { get; set; }
    }

    public class CommandObjectsList
    {
        public string[] Objects { get; set; }
    }

    public enum VariableInterfaces { None = 0, Command = 0x01, MenuCreator = 0x02, ToolBarCreator = 0x04, PanelCreator = 0x08 }
    public class DataType
    {
        public DataType() { }
        public DataType(string name, string baseType)
        {
            this.Name = name;
            this.BaseType = baseType;
        }
        public string Name { get; set; }
        public string BaseType { get; set; }
    }
    public class Parameter
    {
        public Parameter() { }
        public Parameter(string name, string type, string baseType = null)
        {
            this.Name = name;
            this.Type = new DataType(type, baseType);
        }
        public string Name { get; set; }
        public DataType Type { get; set; }
    }

    [JsonDerivedType(typeof(Element), typeDiscriminator: "base")]
    [JsonDerivedType(typeof(Procedure), typeDiscriminator: "procedure")]
    [JsonDerivedType(typeof(TestList), typeDiscriminator: "testlist")]
    [JsonDerivedType(typeof(Variable), typeDiscriminator: "variable")]
    [JsonDerivedType(typeof(PanelDefinitionVariable), typeDiscriminator: "panelVariable")]
    [JsonDerivedType(typeof(ToolBarDefinitionVariable), typeDiscriminator: "toolbarVariable")]
    public class Element
    {
        public int File { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public string BaseElement { get; set; } = null;
        public Partner[] Partners { get; set; } = null;
    }
    public class TestList : Element
    {
    }
    public class Procedure : Element
    {
        public bool FirstParameterIsInstanceReference { get; set; } = false;
        public string ReturnType { get; set; } = null;
        public Parameter[] Parameters { get; set; } = null;
        public string[] CompatibleObjectInstances { get; set; } = null;
    }
    public class Partner
    {
        public string Name { get; set; }
        public string ProcedureReference { get; set; }
    }
    public class Variable : Element
    {
        public string DataType { get; set; }
        public VariableInterfaces Interfaces { get; set; }

        public override string ToString()
        {
            return this.Name;
        }
    }
    public class PanelDefinitionVariable : Variable
    {
        public string Title { get; set; } = null;
        public SerializablePropertyBlockEntry PanelDefinition { get; set; } = null;
    }
    public class ToolBarDefinitionVariable : Variable
    {
        public string Title { get; set; } = null;
        public SerializablePropertyBlockEntry ToolBarDefinition { get; set; } = null;
    }

    public class FileElement
    {
        public FileElement(Element data) { this.Data = data; }

        public Element Data { get; set; }
    }

    public class StartFileElements
    {
        public string TopFile { get; set; }
        public string[] Files { get; set; }
    }

    //public class RequestElementInfo
    //{
    //    public string File { get; set; }
    //    public string Element { get; set; }
    //}

    //public class ElementInfo
    //{
    //    public string Element { get; set; }
    //    public string[] Partners { get; set; }
    //}

    public class RequestOrResponse
    {
        public RequestOrResponse()
        {
            this.RequestID = UniqueInteger.GetLongProtected();
        }
        public RequestOrResponse(ulong id)
        {
            this.RequestID = id;
        }
        public ulong RequestID { get; set; }
        public string ExecutionNote { get; set; } = null;
    }

    public class ObjectCommand : RequestOrResponse
    {
        public ObjectCommand(string @object, string command)
        {
            this.Object = @object;
            this.Command = command;
        }
        public string Object { get; set; }
        public string Command { get; set; }
    }

    public class ReleaseRequest : RequestOrResponse
    {
        public ReleaseRequest(ulong requestID) : base(requestID)
        {
        }
    }

    public class RunScriptRequest : RequestOrResponse
    {
        public RunScriptRequest(ulong requestID, bool silent, string element, string partner, string objectReference, List<TypedValue> arguments = null) : base(requestID)
        {
            this.Silent = silent;
            this.Element = element;
            this.Partner = partner;
            this.ObjectReference = objectReference;
            this.Arguments = (arguments != null) ? arguments.ToList() : null;
        }
        public bool Silent { get; set; }
        public string Element { get; set; }
        public string Partner { get; set; }
        public string ObjectReference { get; set; }
        public List<TypedValue> Arguments { get; set; } = null;
    }

    public class StopExecutionRequest : RequestOrResponse
    {
        public StopExecutionRequest(ulong requestID) : base(requestID)
        {
        }
    }

    public class ExecutionStateUpdate : RequestOrResponse
    {
        public ExecutionStateUpdate(ulong requestID, TaskExecutionState state) : base(requestID)
        {
            this.State = state;
        }
        public TaskExecutionState State { get; set; }
    }
}
