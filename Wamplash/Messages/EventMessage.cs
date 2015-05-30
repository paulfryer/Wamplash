using Newtonsoft.Json;

namespace Wamplash.Messages
{
    public class EventMessage : WampMessage, ISubscription, IPublication, IDetails
    {
        public EventMessage(long subscriptionId, long publicationId, dynamic details, dynamic publishArguments)
        {
            SubscriptionId = subscriptionId;
            PublicationId = publicationId;
            Details = details;
            PublishArguments = publishArguments;
        }

        public EventMessage(dynamic json)
        {
            SubscriptionId = json[1];
            PublicationId = json[2];
            Details = json[3];
            PublishArguments = json[4];
        }

        public override int MessageId
        {
            get { return MessageTypes.Event; }
        }

        public dynamic Details { get; set; }
        public long PublicationId { get; set; }
        public dynamic PublishArguments { get; set; }
        public long SubscriptionId { get; set; }

        public override string ToString()
        {
            var jsonDetails = "{}";
            if (Details != null)
                jsonDetails = JsonConvert.SerializeObject(Details);
            var jsonPublishArguments = JsonConvert.SerializeObject(PublishArguments);
            return "[" + MessageId + ", " + SubscriptionId + ", " + PublicationId + ", " + jsonDetails + ", " +
                   jsonPublishArguments + "]";
        }
    }
}