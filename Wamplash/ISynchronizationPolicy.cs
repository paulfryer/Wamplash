using System;
using System.Collections.Generic;

namespace Wamplash
{
    public interface ISynchronizationPolicy
    {
        bool Synchronize { get;  }
        List<Uri> Endpoints { get; }
        //event EventHandler<List<Uri>> EndpointsUpdated;
    }
}