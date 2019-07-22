using Microsoft.Graph;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Net.Http;
using System.Threading.Tasks;

namespace SampleAzureADAuthenticationWithOpenIdConnect.Helpers
{
    public class AzureAuthenticationProvider : IAuthenticationProvider
    {
        // https://www.jonathanhuss.com/intro-to-the-microsoft-graph-net-client-library/

        public async Task AuthenticateRequestAsync(
            HttpRequestMessage request)
        {
            string clientId = "d66c5935-48b3-4d43-8d58-e4cc699c5b03";
            string clientSecret = "vk/6h3f@h6zJFI?Twt_coWK1q57[TvHK";

            var authContext = new AuthenticationContext(
                "https://login.microsoftonline.com/common");

            var creds = new ClientCredential(clientId, clientSecret);

            var authResult = await authContext.AcquireTokenAsync("https://graph.microsoft.com/", creds);

            request.Headers.Add("Authorization", "Bearer " + authResult.AccessToken);
        }
    }
}