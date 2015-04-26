namespace Wamplash.Features
{
    [Feature("call_timeout")]
    public interface ICallTimeout
    {
        bool CallTimeout { get; set; }
    }
}