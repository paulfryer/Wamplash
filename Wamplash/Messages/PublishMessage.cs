using Newtonsoft.Json;

namespace Wamplash.Messages
{
    public class PublishMessage : WampMessage, IRequest, ITopic, IDetails
    {
        public PublishMessage()
        {
            
        }

        
        public PublishMessage(string topic, dynamic details, int requestId = 0, bool acknowledge = false, bool excludeMe = true, dynamic arguments = null)
        {
            RequestId = requestId != 0 ? requestId : UniqueIdGenerationService.GenerateUniqueId();
            Acknowledge = acknowledge;
            ExcludeMe = excludeMe;
            Topic = topic;
            Details = details;
            Arguments = arguments;
        }

        public PublishMessage(dynamic json)
        {
            RequestId = json[1];
            Acknowledge = json[2].acknowledge;
            ExcludeMe = json[2].exclude_me;
            Topic = json[3];
            Details = json[4];
            Arguments = json[5];
        }

        public bool ExcludeMe { get; set; }
        public bool Acknowledge { get; set; }

        public override int MessageId
        {
            get { return MessageTypes.Publish; }
        }

        public dynamic Details { get; set; }

        public dynamic Arguments { get; set; }

        public long RequestId { get; set; }
        public string Topic { get; set; }

        public override string ToString()
        {
            var detailsJson = "{}";
            if (Details != null)
                detailsJson = JsonConvert.SerializeObject(Details);

            var argumentsJson = "{}";
            if (Arguments != null)
                argumentsJson = JsonConvert.SerializeObject(Arguments);

            var json =  "[" + MessageId + "," + RequestId + ",{\"acknowledge\":" + Acknowledge.ToString().ToLower() + ",\"exclude_me\":" +
                   ExcludeMe.ToString().ToLower() + "},\"" + Topic + "\"," + detailsJson + "," + argumentsJson + "]";

            return json;

        }
    }
}