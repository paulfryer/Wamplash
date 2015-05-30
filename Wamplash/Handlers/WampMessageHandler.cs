using Wamplash.Messages;

namespace Wamplash.Handlers
{
    public delegate void WampMessageHandler<in TMessage>(object sender, TMessage message) where TMessage : WampMessage;
}