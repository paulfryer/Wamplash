namespace Wamplash.Features
{
    [Feature("call_canceling")]
    public interface ICallCanceling
    {
        bool CallCanceling { get; set; }
    }
}