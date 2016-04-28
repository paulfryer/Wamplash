using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wamplash.Handlers;
using Wamplash.Messages;

namespace Wamplash
{
    public static class XXXSubscriptionSynchronizer
    {
        private static readonly List<WampClient> Clients = new List<WampClient>();

        public static event WampMessageHandler<PublishMessage> Publish;

        public static async Task Configure(string endpoints)
        {
            var endpointsList = endpoints.Split(',').Select(ep => new Uri(ep));
            foreach (var endpoint in endpointsList)
            {
                var client = await AddEndpoint(endpoint);
                client.Event += OnRemoteEvent;
            }
            // TODO: Deal with authentication here..
        }

        public static void OnLocalSubscribe(object sender, SubscribeMessage message)
        {
            foreach (var client in Clients)
                client.Subscribe(message).Wait();
        }

        private static void OnRemoteEvent(object sender, EventMessage message)
        {
            var topic = (sender as WampClient).SubscribedTopics[message.SubscriptionId];
            var publishMessage = new PublishMessage(topic, message.PublishArguments);
            
            // TODO: rearch this, might want to actually just send event, instead of publish because publish is going to pub/sub the message which 
            // will result in duplicates.
            if (Publish != null)
                Publish(sender, publishMessage);
        
            

        /*
            foreach (var subscription in subscriptions.Where(s => s.Key == publishMessage.Topic))
            {
                var @event = new EventMessage(subscription.Value, publishMessage.RequestId, null, publishMessage.Details);
                RaiseEvent(@event);
            }*/
        
        }

        public static async Task Unsubscribe(UnsubscribeMessage message)
        {
            foreach (var client in Clients)
                await client.Unsubscribe(message.SubscriptionId);
        }

        public static async Task<WampClient> AddEndpoint(Uri endpoint)
        {
            var client = new WampClient();
            await client.Connect(endpoint, "SubscriptionSynchronizer");
            Clients.Add(client);
            return client;
        }

        public static async Task RemoveEndpoint(Uri endpoint)
        {
            foreach (var client in Clients)
                if (client.Endpoint.AbsoluteUri == endpoint.AbsoluteUri)
                {
                    await client.Disconnect();
                    Clients.Remove(client);
                }
        }

        public static void Unsubscribe(object sender, UnsubscribeMessage message)
        {
            // TODO: implement this.
            //throw new NotImplementedException();
        }
    }
}