using System.Collections.Generic;
using Newtonsoft.Json;

namespace Wamplash.Messages
{
    public class ResultMessage : WampMessage, IRequest, IDetails, IArguments
    {
        public override int MessageId
        {
            get { return MessageTypes.Result; }
        }


        public long RequestId { get; set; }
        public dynamic Details { get; set; }

        public override string ToString()
        {
            var jsonDetails = "{}";
            var jsonArguments = "[]";
            if (Details != null)
                jsonDetails = JsonConvert.SerializeObject(Details);
            if (Arguments != null)
                jsonArguments = JsonConvert.SerializeObject(Arguments);

            return "[" + MessageId + ", " + RequestId + ", " + jsonDetails + ", " + jsonArguments + "]";
        }

        public List<dynamic> Arguments { get; set; }
    }

    public class AbortMessage : WampMessage, IDetails
    {
        private readonly string reason;

        public AbortMessage(string reason)
        {
            this.reason = reason;
        }

        public override int MessageId
        {
            get { return MessageTypes.Abort; }
        }


        public override string ToString()
        {
            var jsonDetails = "{}";
            if (Details != null)
                jsonDetails = JsonConvert.SerializeObject(Details);


            return "[" + MessageId + ", " + jsonDetails + ", \"" + reason + "\"]";
        }

        public dynamic Details { get; set; }
    }

    public class CallMessage : WampMessage, IRequest, IOptions
    {
        public override int MessageId
        {
            get { return MessageTypes.Call; }
        }

        public CallMessage(dynamic json)
        {
            RequestId = json[1];
            Options = json[2];
        }

        public long RequestId { get; set; }
        public dynamic Options { get; set; }
    }

    public class AuthenticateMessage : WampMessage, IDetails
    {

        public AuthenticateMessage(dynamic json)
        {
            Token = json[1];
            Details = json[2];
        }

        public string Token { get; set; }

        public override int MessageId
        {
            get { return MessageTypes.Authenticate; }
        }

        public dynamic Details { get; set; }
    }
}