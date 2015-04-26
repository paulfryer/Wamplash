using System.Collections.Generic;
using Wamplash.Redis.Handlers;
using Wamplash.Roles;

namespace Wamplash.Server.Handlers
{
    public class DemoHandler : RedisWampWebSocketHandler
    {
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
    }
}