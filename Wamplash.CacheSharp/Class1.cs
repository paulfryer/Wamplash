using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using CacheSharp;
using CacheSharp.Redis;
using Newtonsoft.Json;

namespace Wamplash.Redis
{
    public class CacheSharpWampWebSocketHandler : WampWebSocketHandler
    {
        private readonly RedisCache cache = new RedisCache();
        private readonly Dictionary<string, long> subscriptions = new Dictionary<string, long>();

        public CacheSharpWampWebSocketHandler()
        {
            var endpoint = ConfigurationManager.AppSettings.Get("Redis.Endpoint");
            var key = ConfigurationManager.AppSettings.Get("Redis.Key");
            var useSsl = ConfigurationManager.AppSettings.Get("Redis.UseSsl");
            cache.Initialize(
                new Dictionary<string, string>
                {
                    {"Endpoint", endpoint},
                    {"Key", key},
                    {"UseSsl", useSsl}
                });
            cache.MessageReceived += OnMessageReceived;

            Hello += OnHello;
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
            get
            {
                return new List<Role>
            {
                new Broker
                {
                    PublisherIdentification = true,
                    PublisherExclusion = true
                },
                new Publisher
                {
                    PublisherIdentification = true
                }
            };
            }
        }
    }
}
