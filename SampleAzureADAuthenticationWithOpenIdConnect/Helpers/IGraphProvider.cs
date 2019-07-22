using Microsoft.Identity.Client;
using System.Threading.Tasks;

namespace SampleAzureADAuthenticationWithOpenIdConnect.Helpers
{
    public interface IGraphAuthProvider
    {
        string Authority { get; }

        Task<string> GetUserAccessTokenAsync(string userId);

        Task<AuthenticationResult> GetUserAccessTokenByAuthorizationCode(string authorizationCode);
    }
}
