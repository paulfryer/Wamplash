using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Web.WebSockets;
using Newtonsoft.Json;
using Wamplash.Messages;
using Wamplash.Roles;

namespace Wamplash.Handlers
{
    public abstract class WampWebSocketHandler : WebSocketHandler
    {
        public abstract List<Role> Roles { get; }

        public void Send(WampMessage message)
        {
            var messageString = message.ToString();
            Send(messageString);
        }

        public event WampMessageHandler<HelloMessage> Hello;
        public event WampMessageHandler<SubscribeMessage> Subscribe;
        public event WampMessageHandler<PublishMessage> Publish;
        public event WampMessageHandler<UnsubscribeMessage> Unsubscribe;
        public event WampMessageHandler<EventMessage> Event;

        public void RaiseEvent(EventMessage eventMessage)
        {
            if (Event != null)
                Event(eventMessage);
        }


        public string FindConstantName<T>(Type containingType, T value)
        {
            var comparer = EqualityComparer<T>.Default;
            return (from field in containingType.GetFields(BindingFlags.Static | BindingFlags.Public)
                where field.FieldType == typeof (T)
                      && comparer.Equals(value, (T) field.GetValue(null))
                select field.Name)
                .FirstOrDefault();
        }

        public override void OnMessage(string message)
        {
            var json = JsonConvert.DeserializeObject<dynamic>(message);
            int messageType = json[0];
            var messageName = FindConstantName(typeof (MessageTypes), messageType);
            var messageT = Assembly.GetExecutingAssembly().GetTypes().Single(t => t.Name == messageName + "Message");
            var m = Activator.CreateInstance(messageT, json);
            var eventDelegate =
                (MulticastDelegate)
                    typeof (WampWebSocketHandler).GetField(messageName, BindingFlags.Instance | BindingFlags.NonPublic)
                        .GetValue(this);
            if (eventDelegate != null)
                eventDelegate.Method.Invoke(eventDelegate.Target, new object[] {m});
            else throw new Exception("Message not implemented : " + messageType);
        }
    }
}