using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{


    public interface IDataNode
    {
        bool IsArray();
        bool IsValue();
        bool IsInteger();
        bool IsString();

        bool Has(string child);

        IDataNode Get(string name);
        IDataValueNode GetValue(string name);
        IDataArrayNode GetArray(string name);

        long GetInteger(string name);
        string GetString(string name);

        IDataArrayNode AsArray();
    }

    public interface IDataValueNode : IDataNode
    {
    }

    public interface IDataArrayNode : IDataNode
    {
        long GetInteger(long index);
        string GetString(long index);
    }


    internal class JsonDataNode : IDataNode
    {
        private JsonNode m_node;

        public JsonDataNode(JsonNode node)
        {
            m_node = node;
        }

        public IDataArrayNode AsArray()
        {
            var array = m_node.AsArray();
            return new JsonDataArrayNode(array);
        }

        public IDataNode Get(string name)
        {
            throw new NotImplementedException();
        }

        public IDataArrayNode GetArray(string name)
        {
            throw new NotImplementedException();
        }

        public long GetInteger(string name)
        {
            throw new NotImplementedException();
        }

        public string GetString(string name)
        {
            throw new NotImplementedException();
        }

        public IDataValueNode GetValue(string name)
        {
            throw new NotImplementedException();
        }

        public bool Has(string child)
        {
            throw new NotImplementedException();
        }

        public bool IsArray()
        {
            return m_node is JsonArray;
        }

        public bool IsInteger()
        {
            throw new NotImplementedException();
        }

        public bool IsString()
        {
            throw new NotImplementedException();
        }

        public bool IsValue()
        {
            return m_node is JsonValue;
        }
    }

    internal class JsonDataArrayNode : JsonDataNode, IDataArrayNode
    {
        private JsonArray m_arrayNode;

        public JsonDataArrayNode(JsonArray node) : base(node)
        {
            m_arrayNode = node;
        }

        public long GetInteger(long index)
        {
            throw new NotImplementedException();
        }

        public string GetString(long index)
        {
            throw new NotImplementedException();
        }
    }
}
