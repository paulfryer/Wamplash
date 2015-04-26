using Newtonsoft.Json;

namespace Wamplash.Messages
{
    public class WelcomeMessage : WampMessage, ISession, IDetails
    {
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