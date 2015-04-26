namespace Wamplash.Messages
{
    public class HelloMessage : WampMessage, IRequest, IRealm, IDetails
    {
        public HelloMessage(dynamic json)
        {
            Realm = json[1];
            if (json.Count > 2)
                Details = json[2];
        }

        public override int MessageId
        {
            get { return MessageTypes.Hello; }
        }

        public dynamic Details { get; set; }
        public string Realm { get; set; }
        public long RequestId { get; set; }
    }
}