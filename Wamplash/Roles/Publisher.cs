using Wamplash.Features;

namespace Wamplash.Roles
{
    public class Publisher : Role, IPublisherIdentification, ISubscriberBlackwhiteListing, IPublisherExclusion
    {
        public bool PublisherExclusion { get; set; }
        public bool PublisherIdentification { get; set; }
        public bool SubscriberBlackwhiteListing { get; set; }
    }
}