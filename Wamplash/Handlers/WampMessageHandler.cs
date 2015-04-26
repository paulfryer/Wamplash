using Wamplash.Messages;

namespace Wamplash.Handlers
{
    public delegate void WampMessageHandler<in TMessage>(TMessage message) where TMessage : WampMessage;
}