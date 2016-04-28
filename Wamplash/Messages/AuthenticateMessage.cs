using Newtonsoft.Json;

namespace Wamplash.Messages
{
    public class AuthenticateMessage : WampMessage, IDetails
    {

        public AuthenticateMessage(dynamic json)
        {
            Token = json[1];
            Details = json[2];
        }

        public string Token { get; set; }

        public override int MessageId
        {
            get { return MessageTypes.Authenticate; }
        }

        public dynamic Details { get; set; }
    }
}