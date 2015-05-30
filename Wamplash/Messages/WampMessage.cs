using Newtonsoft.Json;

namespace Wamplash.Messages
{
    public abstract class WampMessage
    {
        public abstract int MessageId { get; }
    }
}