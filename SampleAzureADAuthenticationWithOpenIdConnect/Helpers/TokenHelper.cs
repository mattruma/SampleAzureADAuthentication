using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SampleAzureADAuthenticationWithOpenIdConnect.Helpers
{
    public class TokenHelper
    {
        public async Task<String> GetOnBehalfOfAccessToken(string resourceId, string replyUrl)
        {
            string accessToken = null;
            AuthenticationResult result = null;

            //      The Resource ID of the service we want to call.
            //      The current user's access token, from the current request's authorization header.
            //      The credentials of this application.
            //      The username (UPN or email) of the user calling the API
            //
            ClientCredential clientCred = new ClientCredential("d66c5935-48b3-4d43-8d58-e4cc699c5b03", "vk/6h3f@h6zJFI?Twt_coWK1q57[TvHK");

            if (ClaimsPrincipal.Current.Identities.First().BootstrapContext == null)
            {
                throw new Exception("BootstrapContext is null. Please modify the config 'saveSignInToken = true' to ensure that the original token is preserved.");
            }

            var bootstrapContext = new System.IdentityModel.Tokens.Jwt(ClaimsPrincipal.Current.Identities.First().BootstrapContext.ToString());
            if (bootstrapContext == null || string.IsNullOrWhiteSpace(bootstrapContext.Token))
            {
                throw new Exception("Failed to obtain the BootstrapContext token. Please modify the config 'saveSignInToken = true' to ensure that the original token is preserved.");
            }

            string userAccessToken = bootstrapContext.Token;

            // The username (UPN or email) of the user calling the API
            string userName = ClaimsPrincipal.Current.FindFirst(ClaimTypes.Upn) != null ? ClaimsPrincipal.Current.FindFirst(ClaimTypes.Upn).Value : ClaimsPrincipal.Current.FindFirst(ClaimTypes.Email).Value;
            if (string.IsNullOrWhiteSpace(userName))
            {
                throw new Exception("Failed to obtain the signed-in user's name or upn in the ClaimsPrincipal.Current.Claims collection.");
            }

            UserAssertion userAssertion = new UserAssertion(userAccessToken, OAuth2GrantTypeJwtBearer, userName);

            string userId = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;
            AuthenticationContext authContext = new AuthenticationContext(this.Authority, this.TokenCache);

            try
            {
                result = await authContext.AcquireTokenSilentAsync(resourceId, clientCred, new UserIdentifier(Util.GetSignedInUsersObjectIdFromClaims(), UserIdentifierType.UniqueId));
            }
            catch (AdalException ex)
            {
                result = await authContext.AcquireTokenAsync(resourceId, clientCred, userAssertion);
            }

            accessToken = result.AccessToken;
            return accessToken;
        }

        public async Task<string> GetAccessTokenForAppAsync(string resourceId)
        {
            AuthenticationContext authContext = new AuthenticationContext(this.Authority);
            AuthenticationResult authResult = null;
            ClientCredential creds = new ClientCredential(ConfigHelper.ClientId, ConfigHelper.AppKey);

            try
            {
                authResult = await authContext.AcquireTokenAsync(resourceId, creds);
            }
            catch (AdalException ex)
            {
                throw ex;
            }

            return authResult.AccessToken;
        }
    }
}
