namespace Wamplash.Features
{
    [Feature("caller_identification")]
    public interface ICallerIdentification
    {
        bool CallerIdentification { get; set; }
    }
}