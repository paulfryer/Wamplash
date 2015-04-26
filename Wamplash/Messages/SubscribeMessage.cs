namespace Wamplash.Messages
{
    public class SubscribeMessage : WampMessage, IRequest, ITopic
    {
        public SubscribeMessage(dynamic json)
        {
            RequestId = json[1];
            Topic = json[3];
        }

        public override int MessageId
        {
            get { return MessageTypes.Subscribe; }
        }

        public long RequestId { get; set; }
        public string Topic { get; set; }
    }
}