using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;

namespace Wamplash.ServiceBus
{
    public class ServiceBusWampWebSocketHandler : WampWebSocketHandler
    {
        private const string ConnectionString =
            "Endpoint=sb://wamptastic.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=cjC/ZHnSdG+npz8O8/1gMK2yfrokK1Y+L6sGctVPJyk=";

        private readonly Dictionary<long, SubscriptionClient> subscriptionClients = new Dictionary<long, SubscriptionClient>();
        private readonly Dictionary<long, Thread> subscriptionThreads = new Dictionary<long, Thread>();
        private readonly Dictionary<string, TopicClient> topicClients = new Dictionary<string, TopicClient>();

        private readonly NamespaceManager namespaceManager;

        public ServiceBusWampWebSocketHandler()
        {
            namespaceManager = NamespaceManager.CreateFromConnectionString(ConnectionString);

            Hello += OnHello;
            Subscribe += OnSubscribe;
            Unsubscribe += OnUnsubscribe;
            Publish += OnPublish;
            Event += OnEvent;
        }

        private void OnEvent(EventMessage message)
        {
            Send(message);
        }


        private void OnUnsubscribe(UnsubscribeMessage message)
        {
            subscriptionThreads[message.SubscriptionId].Abort();
            subscriptionThreads.Remove(message.SubscriptionId);

            subscriptionClients[message.SubscriptionId].Close();
            var topic = subscriptionClients[message.SubscriptionId].TopicPath;
            subscriptionClients.Remove(message.SubscriptionId);


            namespaceManager.DeleteSubscription(topic, message.SubscriptionId.ToString());
            if (namespaceManager.GetTopic(topic).SubscriptionCount <= 0)
            {
                namespaceManager.DeleteTopic(topic);
                topicClients.Remove(topic);
            }

            Send(new UnsubscribedMessage
            {
                RequestId = message.RequestId
            });
        }

        private void OnPublish(PublishMessage message)
        {
            if (!topicClients.ContainsKey(message.Topic))
            {
                if (!namespaceManager.TopicExists(message.Topic))
                    namespaceManager.CreateTopic(message.Topic);
                var topicClient = TopicClient.CreateFromConnectionString(ConnectionString, message.Topic);
                topicClients.Add(message.Topic, topicClient);
            }

            var payload = JsonConvert.SerializeObject(message.Details);
            var bm = new BrokeredMessage(payload)
            {
                MessageId = message.RequestId.ToString()
            };
            topicClients[message.Topic].Send(bm);
        }

        private void OnSubscribe(SubscribeMessage message)
        {
            if (!namespaceManager.TopicExists(message.Topic))
                namespaceManager.CreateTopic(message.Topic);
            var gb = Guid.NewGuid().ToByteArray();
            var subscriptionId = BitConverter.ToInt32(gb, 0);
            namespaceManager.CreateSubscription(message.Topic, subscriptionId.ToString());
            var subscriptionClient = SubscriptionClient.CreateFromConnectionString(ConnectionString, message.Topic,
                subscriptionId.ToString()
                , ReceiveMode.ReceiveAndDelete);
            subscriptionClients.Add(subscriptionId, subscriptionClient);
            var thread = new Thread(() =>
            {
                while (true)
                {
                    var sbMessage = subscriptionClient.Receive();
                    if (sbMessage != null)
                    {
                        var publicationId = long.Parse(sbMessage.MessageId);
                        var payload = sbMessage.GetBody<string>();
                        RaiseEvent(new EventMessage
                        {
                            Details = payload,
                            PublicationId = publicationId,
                            SubscriptionId = subscriptionId
                        });
                    }
                }
            });

            subscriptionThreads.Add(subscriptionId, thread);
            thread.Start();

            Send(new SubscribedMessage(message.RequestId, subscriptionId));
        }

        private void OnHello(HelloMessage message)
        {
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
            };
            Send(welcome);
        }

        public override List<Role> Roles
        {
            get { throw new NotImplementedException(); }
        }
    }
}
