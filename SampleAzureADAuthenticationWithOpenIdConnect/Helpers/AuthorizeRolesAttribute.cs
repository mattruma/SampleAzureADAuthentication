using Microsoft.AspNetCore.Authorization;

namespace SampleAzureADAuthenticationWithOpenIdConnect.Helpers
{
    public class AuthorizeRolesAttribute : AuthorizeAttribute
    {
        public AuthorizeRolesAttribute(params string[] roles) : base()
        {
            Roles = string.Join(",", roles);
        }
    }
}