using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json.Linq;
using SampleAzureADAuthenticationWithOpenIdConnect.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;

namespace SampleAzureADAuthenticationWithOpenIdConnect
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddAuthentication(auth =>
                {
                    auth.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    auth.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                    auth.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddCookie()
                .AddOpenIdConnect(options =>
                {
                    Configuration.Bind("OpenIdConnect", options);
                    options.Events = new OpenIdConnectEvents
                    {
                        OnAuthorizationCodeReceived = async ctx =>
                            {
                                if (ctx.Principal.Identity is ClaimsIdentity identity)
                                {
                                    if (ctx.Principal.Claims.Any(x => x.Type == "groups"))
                                    {
                                        var claims =
                                            ctx.Principal.Claims.Where(x => x.Type == "groups").ToList();

                                        foreach (var claim in claims)
                                        {
                                            identity.AddClaim(new Claim(ClaimTypes.Role, claim.Value));
                                        }
                                    }
                                    else if (ctx.Principal.Claims.Any(x => x.Type == "_claim_names"))
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
                                                ctx.Principal.Claims.Single(x => x.Type == "http://schemas.microsoft.com/identity/claims/tenantid").Value;
                                            var userId =
                                                ctx.Principal.Claims.Single(x => x.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier").Value;

                                            var httpResponse =
                                                await httpClient.PostAsJsonAsync(
                                                    $"https://graph.microsoft.com/v1.0/{tenantId}/users/{userId}/getMemberObjects",
                                                    new { SecurityEnabledOnly = false });

                                            httpResponse.EnsureSuccessStatusCode();

                                            var jsonResult =
                                                await httpResponse.Content.ReadAsAsync<dynamic>();
                                            var securityGroups =
                                                ((JArray)jsonResult.value).ToObject<List<string>>();
                                            var availableSecurityGroups =
                                                 new[] { Roles.ROLE_GROUP1, Roles.ROLE_GROUP2, Roles.ROLE_GROUP3 };

                                            foreach (var securityGroup in securityGroups.Intersect(availableSecurityGroups))
                                            {
                                                identity.AddClaim(new Claim(ClaimTypes.Role, securityGroup));
                                            }
                                        }
                                    }
                                }
                            }
                    };
                });

            services.AddMvc(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            })
            .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
