namespace Wamplash.Features
{
    [Feature("publisher_exclusion")]
    public interface IPublisherExclusion
    {
        bool PublisherExclusion { get; set; }
    }
}