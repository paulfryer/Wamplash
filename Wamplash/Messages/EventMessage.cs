namespace Wamplash.Messages
{
    public class EventMessage : WampMessage, ISubscription, IPublication, IDetails
    {
        public override int MessageId
        {
            get { return MessageTypes.Event; }
        }

        public dynamic Details { get; set; }
        public long PublicationId { get; set; }
        public long SubscriptionId { get; set; }

        public override string ToString()
        {
            return "[" + MessageId + ", " + SubscriptionId + ", " + PublicationId + ", {}, " + Details + "]";
        }
    }
}