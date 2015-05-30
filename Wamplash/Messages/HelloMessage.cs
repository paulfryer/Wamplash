using System.Text;

namespace Wamplash.Messages
{
    public class HelloMessage : WampMessage, IRequest, IRealm, IDetails
    {
        public HelloMessage(string realm, dynamic details = null)
        {
            Realm = realm;
            if (details != null)
                Details = details;
        }

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

        public override string ToString()
        {
            var sb = new StringBuilder("[" + MessageId + ", \"" + Realm + "\"");
            if (Details != null)
                sb.Append(", " + Details);
            sb.Append("]");
            var s = sb.ToString();
            return s;
        }
    }
}