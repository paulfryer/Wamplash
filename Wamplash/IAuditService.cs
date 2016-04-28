using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Wamplash.Messages;

namespace Wamplash
{
    public interface IAuditService
    {
        Task StoreMessage(WampMessage wampMessage);
    }

   
}