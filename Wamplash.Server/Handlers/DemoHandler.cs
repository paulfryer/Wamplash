using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using Wamplash.Messages;
using Wamplash.Redis.Handlers;
using Wamplash.Roles;

namespace Wamplash.Server.Handlers
{
    public class DemoHandler : RedisWampWebSocketHandler
    {
        public DemoHandler()
        {
            var events = Observable.FromEventPattern<EventMessage>(this, "Event");
            events.Subscribe(OnNext);
        }


        public override List<Role> Roles
        {
            get
            {
                return new List<Role>
                {
                    new Broker
                    {
                        PublisherIdentification = true
                    },
                    new Dealer
                    {
                        CallerIdentification = true
                    }
                };
            }
        }

        private void OnNext(EventPattern<EventMessage> eventPattern)
        {
            Debug.Print("Message: " + eventPattern.EventArgs.SubscriptionId);
        }
    }
}