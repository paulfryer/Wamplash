namespace Wamplash.Features
{
    [Feature("pattern_based_registration")]
    public interface IPatternBasedRegistration
    {
        bool PatternBasedRegistration { get; set; }
    }
}