using System.Collections.Generic;
using System.Configuration;
using Wamplash.Azure;
using Wamplash.Handlers;
using Wamplash.Redis.Handlers;
using Wamplash.Roles;

namespace Wamplash.Server.Handlers
{

    public class DemoRoleDescriber : IRoleDescriber
    {
        public List<Role> Roles
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

    /*
    public class DemoHandler : RedisWampWebSocketHandler
    {
      
        public DemoHandler(ISynchronizationPolicy synchronizationPolicy): base(synchronizationPolicy)
        {
            if (ConfigurationManager.AppSettings["StorageAccountName"] != null &&
                ConfigurationManager.AppSettings["StorageAccountKey"] != null)
                AuditService = new AzureTableStorageAuditService();

   

        }



    }*/
}