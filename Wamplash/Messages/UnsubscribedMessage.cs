namespace Wamplash.Messages
{
    public class UnsubscribedMessage : WampMessage, IRequest
    {
        public override int MessageId
        {
            get { return MessageTypes.Unsubscribed; }
        }

        public long RequestId { get; set; }

        public override string ToString()
        {
            return "[" + MessageId + ", " + RequestId + "]";
        }
    }
}