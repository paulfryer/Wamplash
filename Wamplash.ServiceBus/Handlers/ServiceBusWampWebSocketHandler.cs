using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using Wamplash.Handlers;
using Wamplash.Messages;

namespace Wamplash.ServiceBus.Handlers
{
    public abstract class ServiceBusWampWebSocketHandler : WampWebSocketHandler
    {
        private readonly string connectionString = ConfigurationManager.AppSettings.Get("ServiceBusConnectionString");
        private readonly NamespaceManager namespaceManager;

        private readonly Dictionary<long, SubscriptionClient> subscriptionClients =
            new Dictionary<long, SubscriptionClient>();

        private readonly Dictionary<long, Thread> subscriptionThreads = new Dictionary<long, Thread>();
        private readonly Dictionary<string, TopicClient> topicClients = new Dictionary<string, TopicClient>();

        public ServiceBusWampWebSocketHandler()
        {
            namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);

            Subscribe += OnSubscribe;
            Unsubscribe += OnUnsubscribe;
            Publish += OnPublish;
            Event += OnEvent;
        }

        private void OnEvent(object sender, EventMessage message)
        {
            Send(message);
        }


        private void OnUnsubscribe(object sender, UnsubscribeMessage message)
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

        private void OnPublish(object sender, PublishMessage message)
        {
            if (!topicClients.ContainsKey(message.Topic))
            {
                if (!namespaceManager.TopicExists(message.Topic))
                    namespaceManager.CreateTopic(message.Topic);
                var topicClient = TopicClient.CreateFromConnectionString(connectionString, message.Topic);
                topicClients.Add(message.Topic, topicClient);
            }

            var payload = JsonConvert.SerializeObject(message.Details);
            var bm = new BrokeredMessage(payload)
            {
                MessageId = message.RequestId.ToString()
            };
            topicClients[message.Topic].Send(bm);
        }

        private void OnSubscribe(object sender, SubscribeMessage message)
        {
            if (!namespaceManager.TopicExists(message.Topic))
                namespaceManager.CreateTopic(message.Topic);
            var gb = Guid.NewGuid().ToByteArray();
            var subscriptionId = BitConverter.ToInt32(gb, 0);
            namespaceManager.CreateSubscription(message.Topic, subscriptionId.ToString());
            var subscriptionClient = SubscriptionClient.CreateFromConnectionString(connectionString, message.Topic,
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
    }
}