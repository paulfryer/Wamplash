using System;
using System.Data;
using System.Text;
using Newtonsoft.Json;

namespace Wamplash.Messages
{
    public class SubscribeMessage : WampMessage, IRequest, IOptions, ITopic
    {
        public SubscribeMessage(string topic, long requestId = 0, dynamic options = null)
        {
            if (requestId == 0)
                requestId = UniqueIdGenerationService.GenerateUniqueId();

            RequestId = requestId;
            Topic = topic;
            Options = options;
        }

        public SubscribeMessage(dynamic json)
        {
            RequestId = json[1];
            Options = json[2];
            Topic = json[3];
        }

        public override int MessageId
        {
            get { return MessageTypes.Subscribe; }
        }

        public long RequestId { get; set; }
        public string Topic { get; set; }
        public dynamic Options { get; set; }

        public override string ToString()
        {
            var optionsJson = "{}";
            if (Options != null)
                optionsJson = JsonConvert.SerializeObject(Options);
            return "[" + MessageId + ", " + RequestId + ", " + optionsJson + ", \"" + Topic + "\"]";

        }
    }
}