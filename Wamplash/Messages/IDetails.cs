using System.Collections.Generic;

namespace Wamplash.Messages
{
    public interface IDetails
    {
        dynamic Details { get; set; }
    }

    public interface IArguments
    {
        List<dynamic> Arguments { get; set; }
}
}