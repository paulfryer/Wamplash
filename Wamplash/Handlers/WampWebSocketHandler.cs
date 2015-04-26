using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Web.WebSockets;
using Newtonsoft.Json;
using Wamplash.Features;
using Wamplash.Messages;
using Wamplash.Roles;

namespace Wamplash.Handlers
{
    public abstract class WampWebSocketHandler : WebSocketHandler
    {
        protected WampWebSocketHandler()
        {
            Hello += OnHello;
        }

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

        protected virtual long GetSessionId(HelloMessage helloMessage)
        {
            return (int) DateTime.UtcNow.Ticks;
        }

        protected virtual string GetAuthRole(HelloMessage helloMessage)
        {
            return "anonymous";
        }

        protected virtual string GetAuthMethod(HelloMessage helloMessage)
        {
            return "anonymous";
        }

        private void OnHello(HelloMessage message)
        {
            var details = new Dictionary<string, object>();

            details.Add("authrole", GetAuthRole(message));
            details.Add("authmethod", GetAuthMethod(message));

            details.Add("roles", new Dictionary<string, object>());

            foreach (var role in Roles)
            {
                var features = new Dictionary<string, object>
                {
                    {"features", new Dictionary<string, bool>()}
                };

                (details["roles"] as Dictionary<string, object>)
                    .Add(role.GetType().Name.ToLower(), features);


                // TODO: this part needs work. Have to find the interfaces members then see if they are set to true.

                foreach (
                    var featureAttribute in
                        role.GetType()
                            .GetInterfaces()
                            .Where(i => i.GetCustomAttributes(typeof (FeatureAttribute)).Any())
                            .Select(
                                i => i.GetCustomAttributes(typeof (FeatureAttribute)).Cast<FeatureAttribute>().Single())
                    )
                {
                   
                    (features["features"] as Dictionary<string, bool>).Add(featureAttribute.FeatureCode, true);
                }
            }

            var welcome = new WelcomeMessage
            {
                SessionId = GetSessionId(message),
                Details = details
            };


            /*
            var welcome = new WelcomeMessage
            {
                SessionId = (int)DateTime.UtcNow.Ticks,
                Details = new Dictionary<string, object>
                {
                    {"authrole", "anonymous"},
                    {"authmethod", "anonymous"},
                    {
                        "roles", new Dictionary<string, object>
                        {
                            {
                                "broker", new Dictionary<string, object>
                                {
                                    {
                                        "features", new Dictionary<string, object>
                                        {
                                            {"publisher_identification", true},
                                            {"publisher_exclusion", true},
                                            {"subscriber_blackwhite_listing", true}
                                        }
                                    }
                                }
                            },
                        }
                    }
                }
            };*/
            Send(welcome);
        }
    }
}