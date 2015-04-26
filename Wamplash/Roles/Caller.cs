using Wamplash.Features;

namespace Wamplash.Roles
{
    public class Caller : Role, ICallerIdentification, ICallTimeout, ICallCanceling, IProgressiveCallResults
    {
        public bool CallCanceling { get; set; }
        public bool CallerIdentification { get; set; }
        public bool CallTimeout { get; set; }
        public bool ProgressiveCallResults { get; set; }
    }
}