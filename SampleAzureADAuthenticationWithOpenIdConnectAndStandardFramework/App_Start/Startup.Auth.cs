using System;
using System.Configuration;
using System.Linq;
using Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using System.Security.Claims;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Net.Http;
using SampleAzureADAuthenticationWithOpenIdConnectAndStandardFramework.Helpers;

namespace SampleAzureADAuthenticationWithOpenIdConnectAndStandardFramework
{
    public partial class Startup
    {
        private static string _clientId = ConfigurationManager.AppSettings["OpenIdConnect:ClientId"];
        private static string _clientSecret = ConfigurationManager.AppSettings["OpenIdConnect:ClientSecret"];
        private static string _aadInstance = EnsureTrailingSlash(ConfigurationManager.AppSettings["OpenIdConnect:AADInstance"]);
        private static string _tenantId = ConfigurationManager.AppSettings["OpenIdConnect:TenantId"];
        private static string _postLogoutRedirectUri = ConfigurationManager.AppSettings["OpenIdConnect:PostLogoutRedirectUri"];
        private static string _authority = _aadInstance + _tenantId;

        public void ConfigureAuth(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions());

            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions
                {
                    ClientId = _clientId,
                    ClientSecret = _clientSecret,
                    Authority = _authority,
                    PostLogoutRedirectUri = _postLogoutRedirectUri,
                    Notifications = new OpenIdConnectAuthenticationNotifications()
                    {
                        AuthorizationCodeReceived = async ctx =>
                        {
                            if (ctx.AuthenticationTicket.Identity is ClaimsIdentity identity)
                            {
                                if (ctx.AuthenticationTicket.Identity.Claims.Any(x => x.Type == "groups"))
                                {
                                    var claims =
                                        ctx.AuthenticationTicket.Identity.Claims.Where(x => x.Type == "groups").ToList();

                                    foreach (var claim in claims)
                                    {
                                        identity.AddClaim(new Claim(ClaimTypes.Role, claim.Value));
                                    }
                                }
                                else if (ctx.AuthenticationTicket.Identity.Claims.Any(x => x.Type == "_claim_names"))
                                {
                                    var authenticationContext =
                                        new AuthenticationContext(ctx.Options.Authority);
                                    var clientCredentials =
                                        new ClientCredential(ctx.Options.ClientId, ctx.Options.ClientSecret);

                                    var result =
                                        await authenticationContext.AcquireTokenAsync("https://graph.microsoft.com", clientCredentials);

                                    using (var httpClient = new HttpClient())
                                    {
                                        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {result.AccessToken}");

                                        var tenantId =
                                            ctx.AuthenticationTicket.Identity.Claims.Single(x => x.Type == "http://schemas.microsoft.com/identity/claims/tenantid").Value;
                                        var userId =
                                            ctx.AuthenticationTicket.Identity.Claims.Single(x => x.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier").Value;

                                        var httpResponse =
                                            await httpClient.PostAsJsonAsync(
                                                $"https://graph.microsoft.com/v1.0/{tenantId}/users/{userId}/getMemberObjects",
                                                new { SecurityEnabledOnly = false });

                                        httpResponse.EnsureSuccessStatusCode();

                                        var jsonResult =
                                            await httpResponse.Content.ReadAsAsync<dynamic>();

                                        foreach (var value in jsonResult.value)
                                        {
                                            identity.AddClaim(new Claim(ClaimTypes.Role, value.ToString()));
                                        }
                                    }
                                }
                            }
                        }
                    }
                });
        }

        private static string EnsureTrailingSlash(string value)
        {
            if (value == null)
            {
                value = string.Empty;
            }

            if (!value.EndsWith("/", StringComparison.Ordinal))
            {
                return value + "/";
            }

            return value;
        }
    }
}
