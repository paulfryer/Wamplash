using Newtonsoft.Json;

namespace Wamplash.Messages
{
    public class ChallengeMessage : WampMessage, ISession, IDetails
    {
        public string AuthMethod { get; set; }

        public ChallengeMessage(string authMethod)
        {
            AuthMethod = authMethod;
        }

        public override int MessageId
        {
            get { return MessageTypes.Challenge; }
        }

        public long SessionId { get; set; }
        public dynamic Details { get; set; }

        public override string ToString()
        {
            var jd = JsonConvert.SerializeObject(Details);
            return "[" + MessageId + ", \"" + AuthMethod + "\", " + jd + "]";
        }
    }
}