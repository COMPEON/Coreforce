using Compos.Coreforce.Models.Authorization;
using System.Threading.Tasks;

namespace Compos.Coreforce.Authorization
{
    public interface ISalesforceOpenAuthorization
    {
        Task<AuthorizationResult> Authorize();
    }
}
