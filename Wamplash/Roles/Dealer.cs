using Wamplash.Features;

namespace Wamplash.Roles
{
    public class Dealer : Role, ICallerIdentification, ICallTrustLevels, IPatternBasedRegistration, IRegistrationMetaApi,
        ISharedRegistration, ICallTimeout, ICallCanceling, IProgressiveCallResults
    {
        public bool CallCanceling { get; set; }
        public bool CallerIdentification { get; set; }
        public bool CallTimeout { get; set; }
        public bool CallTrustLevels { get; set; }
        public bool PatternBasedRegistration { get; set; }
        public bool ProgressiveCallResults { get; set; }
        public bool RegistrationMetaApi { get; set; }
        public bool SharedRegistration { get; set; }
    }
}