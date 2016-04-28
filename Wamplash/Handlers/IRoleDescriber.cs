using System.Collections.Generic;
using Wamplash.Roles;

namespace Wamplash.Handlers
{
    public interface IRoleDescriber
    {
        List<Role> Roles { get; }
    }
}