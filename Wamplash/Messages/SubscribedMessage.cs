namespace Wamplash.Messages
{
    public class SubscribedMessage : WampMessage, IRequest, ISubscription
    {
        public SubscribedMessage(long requestId, long subscriptionId)
        {
            RequestId = requestId;
            SubscriptionId = subscriptionId;
        }

        public override int MessageId
        {
            get { return MessageTypes.Subscribed; }
        }

        public long RequestId { get; set; }
        public long SubscriptionId { get; set; }

        public override string ToString()
        {
            return "[" + MessageId + ", " + RequestId + ", " + SubscriptionId + "]";
        }
    }
}