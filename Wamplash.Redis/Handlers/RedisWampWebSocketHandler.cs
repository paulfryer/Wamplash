using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using CacheSharp;
using CacheSharp.Redis;
using Newtonsoft.Json;
using Wamplash.Handlers;
using Wamplash.Messages;

namespace Wamplash.Redis.Handlers
{
    public abstract class RedisWampWebSocketHandler : WampWebSocketHandler
    {
        private readonly RedisCache cache = new RedisCache();
        private readonly Dictionary<string, long> subscriptions = new Dictionary<string, long>();

        protected RedisWampWebSocketHandler() : this(
            ConfigurationManager.AppSettings.Get("Redis.Endpoint"),
            ConfigurationManager.AppSettings.Get("Redis.Key"),
            ConfigurationManager.AppSettings.Get("Redis.UseSsl")
            )
        {
        }

        protected RedisWampWebSocketHandler(string endpoint, string key, string useSsl)
        {
            cache.Initialize(
                new Dictionary<string, string>
                {
                    {"Endpoint", endpoint},
                    {"Key", key},
                    {"UseSsl", useSsl}
                });
            cache.MessageReceived += OnMessageReceived;
            Subscribe += OnSubscribe;
            Unsubscribe += OnUnsubscribe;
            Publish += OnPublish;
            Event += OnEvent;
        }

        private void OnUnsubscribe(UnsubscribeMessage message)
        {
            foreach (var subscription in subscriptions)
                if (subscription.Value == message.SubscriptionId)
                {
                    cache.UnsubscribeAsync(subscription.Key);
                }
            Send(new UnsubscribedMessage
            {
                RequestId = message.RequestId
            });
        }

        private void OnEvent(EventMessage message)
        {
            if (subscriptions.ContainsValue(message.SubscriptionId))
                Send(message);
        }

        private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            var publishMessage = JsonConvert.DeserializeObject<PublishMessage>(e.Value);

            foreach (var subscription in subscriptions.Where(s => s.Key == publishMessage.Topic))
            {
                RaiseEvent(new EventMessage
                {
                    Details = publishMessage.Details,
                    SubscriptionId = subscription.Value,
                    PublicationId = DateTime.UtcNow.Ticks
                });
            }
        }

        private void OnPublish(PublishMessage message)
        {
            var value = JsonConvert.SerializeObject(message);
            cache.PublishAsync(message.Topic, value);
        }

        private void OnSubscribe(SubscribeMessage message)
        {
            // todo: find a btter way to make this.
            var subscriptionId = long.MaxValue - DateTime.UtcNow.Ticks;

            subscriptions.Add(message.Topic, subscriptionId);

            cache.SubscribeAsync(message.Topic);
            Send(new SubscribedMessage(message.RequestId, subscriptionId));
        }
    }
}