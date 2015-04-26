using Wamplash.Features;

namespace Wamplash.Roles
{
    public class Broker : Role, IPublisherIdentification, IPublicationTrustLevels, IPatternBasedSubscription,
        ISubscriptionMetaApi, ISubscriberBlackwhiteListing, IPublisherExclusion, IEventHistory
    {
        public bool EventHistory { get; set; }
        public bool PatternBasedSubscription { get; set; }
        public bool PublicationTrustLevels { get; set; }
        public bool PublisherExclusion { get; set; }
        public bool PublisherIdentification { get; set; }
        public bool SubscriberBlackwhiteListing { get; set; }
        public bool SubscriptionMetaApi { get; set; }
    }
}