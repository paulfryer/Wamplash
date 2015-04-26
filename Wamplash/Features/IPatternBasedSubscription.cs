namespace Wamplash.Features
{
    [Feature("pattern_based_subscription")]
    public interface IPatternBasedSubscription
    {
        bool PatternBasedSubscription { get; set; }
    }
}