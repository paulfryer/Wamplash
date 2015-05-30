using Newtonsoft.Json;

namespace Wamplash.Messages
{
    public class WelcomeMessage : WampMessage, ISession, IDetails
    {
        public WelcomeMessage(long sessionId, dynamic details = null)
        {
            SessionId = sessionId;
            if (details != null)
                Details = details;
        }

        public WelcomeMessage(dynamic json)
        {
            SessionId = json[1];
            if (json.Count > 2)
                Details = json[2];
        }


        public override int MessageId
        {
            get { return MessageTypes.Welcome; }
        }

        public dynamic Details { get; set; }
        public long SessionId { get; set; }

        public override string ToString()
        {
            var jd = JsonConvert.SerializeObject(Details);
            return "[" + MessageId + ", " + SessionId + ", " + jd + "]";
        }
    }
}