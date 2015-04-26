namespace Wamplash.Features
{
    [Feature("shared_registration")]
    public interface ISharedRegistration
    {
        bool SharedRegistration { get; set; }
    }
}