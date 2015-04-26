using Wamplash.Features;

namespace Wamplash.Roles
{
    public class Subscriber : Role, IPublisherIdentification, IPublicationTrustLevels, IPatternBasedSubscription,
        IEventHistory
    {
        public bool EventHistory { get; set; }
        public bool PatternBasedSubscription { get; set; }
        public bool PublicationTrustLevels { get; set; }
        public bool PublisherIdentification { get; set; }
    }
}