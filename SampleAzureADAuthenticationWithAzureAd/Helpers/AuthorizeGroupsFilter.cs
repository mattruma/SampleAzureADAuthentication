using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;

namespace SampleAzureADAuthenticationWithAzureAd.Helpers
{
    public class AuthorizeGroupsFilter : IAuthorizationFilter
    {
        readonly string[] _groups;

        public AuthorizeGroupsFilter(string[] groups)
        {
            _groups = groups;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var hasGroup = context.HttpContext.User.Claims.Any(
                x => x.Type == "groups" && _groups.Contains(x.Value));

            if (!hasGroup)
            {
                context.Result = new RedirectToPageResult("AccessDenied");
            }
        }
    }
}