namespace Wamplash.Messages
{
    public class UnsubscribeMessage : WampMessage, IRequest, ISubscription
    {
        public UnsubscribeMessage(dynamic json)
        {
            RequestId = json[1];
            SubscriptionId = json[2];
        }

        public override int MessageId
        {
            get { return MessageTypes.Unsubscribed; }
        }


        public long RequestId { get; set; }
        public long SubscriptionId { get; set; }
    }
}