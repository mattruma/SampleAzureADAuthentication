using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using SampleAzureADBlazorApp.Data;
using SampleAzureADBlazorApp.Helpers;

namespace SampleAzureADBlazorApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddAuthentication(AzureADDefaults.AuthenticationScheme)
            //    .AddAzureAD(options => Configuration.Bind("AzureAd", options));

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

                                            dynamic jsonResult =
                                                await httpResponse.Content.ReadAsAsync<dynamic>();

                                            foreach (var value in jsonResult.value)
                                            {
                                                identity.AddClaim(new Claim(ClaimTypes.Role, value.ToString()));
                                            }
                                        }
                                }
                            }
                        }
                    };
                });

            services.AddControllersWithViews(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            });

            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddSingleton<WeatherForecastService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
