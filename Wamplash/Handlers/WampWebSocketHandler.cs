using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Web.WebSockets;
using Newtonsoft.Json;
using Wamplash.Features;
using Wamplash.Messages;

namespace Wamplash.Handlers
{
    public abstract class WampWebSocketHandler : WebSocketHandler
    {
        private readonly IRoleDescriber roleDescriber;

        protected WampWebSocketHandler(ISynchronizationPolicy synchronizationPolicy, IRoleDescriber roleDescriber)
        {
            this.roleDescriber = roleDescriber;
            SynchronizationPolicy = synchronizationPolicy;

            //Hello += OnHello;


            if (SynchronizationPolicy != null && SynchronizationPolicy.Synchronize)
            {
                Configure(SynchronizationPolicy.Endpoints);
                Subscribe += OnLocalSubscribe;
                Unsubscribe += OnLocalUnsubscribe;
            }
        }

        public Uri Endpoint { get; private set; }
        public long SessionId { get; private set; }
        public string AuthMethod { get; set; }
        public string AuthRole { get; set; }
        public string Realm { get; set; }

        public IAuditService AuditService { get; set; }
        public ISynchronizationPolicy SynchronizationPolicy { get; set; }

        #region Client section

        private readonly List<WampClient> clients = new List<WampClient>();

        private void OnRemoteEvent(object sender, EventMessage message)
        {
            var topic = (sender as WampClient).SubscribedTopics[message.SubscriptionId];
            var publishMessage = new PublishMessage(topic, message.PublishArguments);
            if (Publish != null)
                Publish(sender, publishMessage);
        }

        private async Task<WampClient> AddEndpoint(Uri endpoint)
        {
            var client = new WampClient();
            await client.Connect(endpoint, "SubscriptionSynchronizer");
            clients.Add(client);
            return client;
        }

        private async Task RemoveEndpoint(Uri endpoint)
        {
            foreach (var client in clients)
                if (client.Endpoint.AbsoluteUri == endpoint.AbsoluteUri)
                {
                    await client.Disconnect();
                    clients.Remove(client);
                }
        }

        private async Task Configure(List<Uri> endpointsList)
        {
            //var endpointsList = endpoints.Split(',').Select(ep => new Uri(ep));
            foreach (var endpoint in endpointsList)
            {
                var client = await AddEndpoint(endpoint);
                client.Event += OnRemoteEvent;
            }
            // TODO: Deal with authentication here..
        }

        private void OnLocalSubscribe(object sender, SubscribeMessage message)
        {
            foreach (var client in clients)
                client.Subscribe(message).Wait();
        }

        private void OnLocalUnsubscribe(object sender, UnsubscribeMessage message)
        {
            // TODO: remove client.


            //throw new NotImplementedException();
        }

        #endregion

        //public abstract List<Role> Roles { get; }

        public void Send(WampMessage message)
        {
            var messageString = message.ToString();

            if (AuditService != null)
                AuditService.StoreMessage(message);

            Send(messageString);
        }

        public event WampMessageHandler<HelloMessage> Hello;
        public event WampMessageHandler<SubscribeMessage> Subscribe;
        public event WampMessageHandler<SubscribedMessage> Subscribed;
        public event WampMessageHandler<PublishMessage> Publish;
        public event WampMessageHandler<UnsubscribeMessage> Unsubscribe;
        public event WampMessageHandler<EventMessage> Event;
        public event WampMessageHandler<AuthenticateMessage> Authenticate;
        public event WampMessageHandler<CallMessage> Call; 

        public void RaiseEvent(EventMessage eventMessage)
        {
            if (Event != null)
                Event(this, eventMessage);
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

            if (AuditService != null)
                AuditService.StoreMessage(m);

            var eventDelegate =
                (MulticastDelegate)
                    typeof (WampWebSocketHandler).GetField(messageName, BindingFlags.Instance | BindingFlags.NonPublic)
                        .GetValue(this);
            if (eventDelegate != null)
                foreach (var ed in eventDelegate.GetInvocationList())
                    ed.Method.Invoke(eventDelegate.Target, new object[] {this, m});
            else throw new Exception("Message not implemented : " + messageType);
        }

        protected virtual long GetSessionId()
        {
            return UniqueIdGenerationService.GenerateUniqueId();
        }

        protected void SendWelcome()
        {

            var details = new Dictionary<string, object>
            {
                {"authrole", AuthRole},
                {"authmethod", AuthMethod},
                {"roles", new Dictionary<string, object>()}
            };

            foreach (var role in roleDescriber.Roles)
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

            var sessionId =GetSessionId();
            SessionId = sessionId;
            var welcome = new WelcomeMessage(sessionId, details);
            Send(welcome);
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
        }
    }
}