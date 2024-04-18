using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.VISABridge.Messages
{
    public enum ShortCommand
    {
        None,
        GetInstrumentList,
        SessionClosed,
        Receive
    }

    public class ConnectedInstruments
    {
        public ConnectedInstruments(string[] instruments)
        {
            this.Instruments = instruments;
        }

        public string[] Instruments { get; set; }
    }

    public class OpenSession
    {
        public OpenSession(string resource)
        {
            this.Resource = resource;
        }

        public string Resource { get; set; }
    }

    public class SessionOpened
    {
        public SessionOpened(string resource, int id)
        {
            this.Resouce = resource;
            this.Id = id;
        }

        public string Resouce { get; set; }
        public int Id { get; set; }
    }

    public class CloseSession
    {
        public CloseSession(string resource)
        {
            this.Resource = resource;
        }

        public string Resource { get; set; }
    }

    public class Send
    {
        public Send(string request)
        {
            this.Request = request;
        }

        public string Request { get; set; }
    }

    public class Received
    {
        public Received(string line)
        {
            this.Line = line;
        }

        public string Line { get; set; }
    }
}
