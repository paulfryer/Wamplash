namespace Wamplash.Features
{
    [Feature("event_history")]
    public interface IEventHistory
    {
        bool EventHistory { get; set; }
    }
}