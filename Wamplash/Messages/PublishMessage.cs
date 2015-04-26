namespace Wamplash.Messages
{
    public class PublishMessage : WampMessage, IRequest, ITopic, IDetails
    {
        public PublishMessage()
        {
        }

        public PublishMessage(dynamic json)
        {
            RequestId = json[1];
            Acknowledge = json[2].acknowledge;
            ExcludeMe = json[2].exclude_me;
            Topic = json[3];
            Details = json[4];
        }

        public bool ExcludeMe { get; set; }
        public bool Acknowledge { get; set; }

        public override int MessageId
        {
            get { return MessageTypes.Publish; }
        }

        public dynamic Details { get; set; }

        public long RequestId { get; set; }
        public string Topic { get; set; }
    }
}