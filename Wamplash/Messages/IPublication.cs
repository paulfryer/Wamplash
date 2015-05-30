using System.Collections.Generic;

namespace Wamplash.Messages
{
    public interface IPublication
    {
        long PublicationId { get; set; }
        dynamic PublishArguments { get; set; } 
    }
}